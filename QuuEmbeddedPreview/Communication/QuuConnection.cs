using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS8618
#pragma warning disable CS8600

namespace QuuEmbeddedPreview.Communication
{
    class QuuConnection
    {
        public QuuConnection(QuuConnectionConfig config, string stationId)
        {
            this.config = config;
            this.stationId = stationId;
            status = QuuConnectionStatus.OFFLINE;
        }

        private readonly QuuConnectionConfig config;
        private readonly string stationId;
        private QuuConnectionStatus status;

        public event EventReceived OnEventReceived;
        public event StatusChanged OnStatusChanged;

        public QuuConnectionStatus Status
        {
            get => status;
            private set
            {
                status = value;
                OnStatusChanged?.Invoke(this, status);
            }
        }

        public delegate void EventReceived(QuuConnection connection, QuuEvent evt);
        public delegate void StatusChanged(QuuConnection connection, QuuConnectionStatus status);

        public async Task RunAsync()
        {
            while (true)
            {
                try
                {
                    using (ClientWebSocket sock = new ClientWebSocket())
                    {
                        //Change status
                        Status = QuuConnectionStatus.CONNECTING;

                        //Connect to endpoint
                        await sock.ConnectAsync(new Uri(config.WsUrl), CancellationToken.None);

                        //Change status
                        Status = QuuConnectionStatus.CONNECTED;

                        //Enter receive loop
                        int offset = 0;
                        byte[] buffer = new byte[65536];
                        while (true)
                        {
                            //Recieve from socket
                            WebSocketReceiveResult rec = await sock.ReceiveAsync(new ArraySegment<byte>(buffer, offset, buffer.Length), CancellationToken.None);

                            //Check if this is only a partial message
                            if (!rec.EndOfMessage)
                            {
                                //Expand buffer
                                byte[] newBuffer = new byte[buffer.Length * 2];
                                buffer.CopyTo(newBuffer, 0);
                                offset += rec.Count;
                                continue;
                            }

                            //Get full length
                            int len = offset + rec.Count;
                            offset = 0;

                            //Hande closures
                            if (rec.MessageType == WebSocketMessageType.Close)
                            {
                                await sock.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
                                break;
                            }

                            //Decode as text
                            string data = Encoding.UTF8.GetString(buffer, 0, len);

                            //Process
                            await ProcessIncomingMessage(sock, data);
                        }
                    }
                } catch
                {
                    //Do nothing
                }

                //Update status and wait
                Status = QuuConnectionStatus.OFFLINE;
                await Task.Delay(5000);
            }
        }

        private async Task ProcessIncomingMessage(ClientWebSocket sock, string data)
        {
            //First, decode as basic web payload to extract info
            JObject payload = JsonConvert.DeserializeObject<JObject>(data);
            if (payload == null)
                return;
            BaseWebPayload payloadBase = payload.ToObject<BaseWebPayload>();
            if (payloadBase == null)
                return;

            //Switch on message type
            switch (payloadBase.Action)
            {
                case 4: //Initialization?
                    //Send initialization message
                    byte[] initMsg = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new SelectChannelMessage
                    {
                        Action = 10,
                        Channel = "hdinput"
                    }));
                    await sock.SendAsync(new ArraySegment<byte>(initMsg), WebSocketMessageType.Text, true, CancellationToken.None);
                    break;
                case 15: //Message
                    //Decode as message payload
                    WebPayloadMessages messagePayload = payload.ToObject<WebPayloadMessages>();
                    if (messagePayload == null)
                        break;

                    //Process messages
                    foreach (var m in messagePayload.Messages)
                    {
                        //Filter
                        if (m.Name != stationId)
                            continue;

                        //Deserialize embedded data
                        QuuEvent evt = JsonConvert.DeserializeObject<QuuEvent>(m.Data);
                        if (evt == null)
                            continue;

                        //Send
                        OnEventReceived?.Invoke(this, evt);
                    }
                    break;
            }

        }

        class SelectChannelMessage : BaseWebPayload
        {
            [JsonProperty("channel")]
            public string Channel { get; set; }
        }

        class BaseWebPayload
        {
            [JsonProperty("action")]
            public int Action { get; set; }
        }

        class WebPayloadMessages : BaseWebPayload
        {
            [JsonProperty("connectionSerial")]
            public int ConnectionSerial { get; set; }
            [JsonProperty("channel")]
            public string Channel { get; set; }
            [JsonProperty("channelSerial")]
            public string ChannelSerial { get; set; }
            [JsonProperty("id")]
            public string Id { get; set; }
            [JsonProperty("messages")]
            public WebMessage[] Messages { get; set; }
            [JsonProperty("timestamp")]
            public ulong Timestamp { get; set; }
        }

        class WebMessage
        {
            [JsonProperty("data")]
            public string Data { get; set; }
            [JsonProperty("encoding")]
            public string Encoding { get; set; }
            [JsonProperty("name")]
            public string Name { get; set; }
        }
    }
}

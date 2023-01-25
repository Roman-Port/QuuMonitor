using QuuEmbeddedPreview.Drawing.Components;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuuEmbeddedPreview.Drawing.Views
{
    class MainView : IDrawingView
    {
        public MainView(WebImageDownloadManager downloadManager, RayMemoryAsset fontMain, RayMemoryAsset imgQuuBackground, Image aeDefaultImg, Image logoImg)
        {
            root = new LinearLayoutVertComponent();

            //HEADER

            header = new LinearLayoutHorizComponent();
            root.AddChild(header, new LinearLayoutComponent.LinearLayoutSettings
            {
                SizePercent = 0.12f
            });
            header.SetMargins(0.15f);

            clockDate = new TextComponent(fontMain, new Color(CLOCK_BRIGHTNESS, CLOCK_BRIGHTNESS, CLOCK_BRIGHTNESS, 0xFF));
            clockDate.MarginTop = CLOCK_MARGIN;
            clockDate.MarginBottom = CLOCK_MARGIN;
            clockDate.AlignmentHorizontal = TextComponent.HorizAlignment.Left;
            header.AddChild(clockDate, new LinearLayoutComponent.LinearLayoutSettings
            {
                AllowGrow = true,
                AutoSize = true
            });

            logo = new ImageComponent(logoImg);
            header.AddChild(logo, new LinearLayoutComponent.LinearLayoutSettings
            {
                AutoSize = true,
                AllowGrow = false
            });

            clockTime = new TextComponent(fontMain, new Color(CLOCK_BRIGHTNESS, CLOCK_BRIGHTNESS, CLOCK_BRIGHTNESS, 0xFF));
            clockTime.MarginTop = CLOCK_MARGIN;
            clockTime.MarginBottom = CLOCK_MARGIN;
            clockTime.AlignmentHorizontal = TextComponent.HorizAlignment.Right;
            header.AddChild(clockTime, new LinearLayoutComponent.LinearLayoutSettings
            {
                AllowGrow = true,
                AutoSize = true
            });

            // CONTENT

            container = new ImageComponent(imgQuuBackground.LoadAsImage());
            root.AddChild(container, new LinearLayoutComponent.LinearLayoutSettings
            {
                AutoSize = true,
                AllowGrow = true
            });

            containerContent = new LinearLayoutHorizComponent();
            container.AddChild(containerContent);
            containerContent.MarginTop = 0.234862385f;
            containerContent.MarginBottom = 0.317431193f;
            containerContent.MarginLeft = 0.148370497f;
            containerContent.MarginRight = 0.0471698113f;

            aeIcon = new WebImageComponent(downloadManager, aeDefaultImg);
            containerContent.AddChild(aeIcon);

            aeText = new LinearLayoutVertComponent();
            aeText.MarginLeft = 0.025f;
            containerContent.AddChild(aeText);

            aeTextStation = new TextComponent(fontMain, new Color(0x2f, 0x87, 0xff, 0xff));
            aeTextStation.MarginBottom = 0.1f;
            aeTextStation.AlignmentHorizontal = TextComponent.HorizAlignment.Left;
            aeText.AddChild(aeTextStation, new LinearLayoutComponent.LinearLayoutSettings
            {
                AutoSize = false,
                SizePercent = 0.2f
            });

            aeTextArtist = new TextComponent(fontMain, new Color(0xe0, 0xe0, 0xe0, 0xff));
            aeTextArtist.AlignmentHorizontal = TextComponent.HorizAlignment.Left;
            aeText.AddChild(aeTextArtist, new LinearLayoutComponent.LinearLayoutSettings
            {
                AutoSize = false,
                SizePercent = 0.2f
            });

            aeTextTitle = new TextComponent(fontMain, new Color(0xe0, 0xe0, 0xe0, 0xff));
            aeTextTitle.AlignmentHorizontal = TextComponent.HorizAlignment.Left;
            aeText.AddChild(aeTextTitle, new LinearLayoutComponent.LinearLayoutSettings
            {
                AutoSize = false,
                SizePercent = 0.2f
            });

            //CONNECTING MESSAGE

            connecting = new DrawingComponent();
            root.AddChild(connecting, new LinearLayoutComponent.LinearLayoutSettings
            {
                AllowGrow = false,
                AutoSize = false,
                SizePercent = 0.1f
            });
            connecting.BackgroundColor = new Color(252, 81, 81, 0xFF);

            connectingText = new TextComponent(fontMain, new Color(0xff, 0xff, 0xff, 0xff));
            connecting.AddChild(connectingText);
            connectingText.SetMargins(0.3f);
            connectingText.AlignmentHorizontal = TextComponent.HorizAlignment.Center;
            connectingText.AlignmentVertical = TextComponent.VertAlignment.Middle;
            connectingText.Text = "CONNECTION LOST - RECONNECTING...";
        }

        private const float CLOCK_MARGIN = 0.15f;
        private const int CLOCK_BRIGHTNESS = 225;

        public DrawingComponent RootComponent => root;

        private LinearLayoutVertComponent root;

        private LinearLayoutHorizComponent header;
        private TextComponent clockDate;
        private ImageComponent logo;
        private TextComponent clockTime;

        private ImageComponent container;
        private LinearLayoutHorizComponent containerContent;
        private WebImageComponent aeIcon;
        private LinearLayoutVertComponent aeText;
        private TextComponent aeTextStation;
        private TextComponent aeTextArtist;
        private TextComponent aeTextTitle;

        private DrawingComponent connecting;
        private TextComponent connectingText;

        public string AeIconUrl
        {
            get => aeIcon.Url;
            set => aeIcon.Url = value;
        }

        public string AeStation
        {
            get => aeTextStation.Text;
            set => aeTextStation.Text = value;
        }

        public string AeArtist
        {
            get => aeTextArtist.Text;
            set => aeTextArtist.Text = value;
        }

        public string AeTitle
        {
            get => aeTextTitle.Text;
            set => aeTextTitle.Text = value;
        }

        public bool IsConnected
        {
            get => !connecting.Visible;
            set => connecting.Visible = !value;
        }

        public void ProcessFrame()
        {
            //Update clock time
            string timeString = DateTime.Now.ToString("h:mm:ss tt").ToUpper();
            if (clockTime.Text != timeString)
                clockTime.Text = timeString;

            //Update clock date
            string dateString = DateTime.Now.ToString("ddd M/dd/yyyy");
            if (clockDate.Text != dateString)
                clockDate.Text = dateString;
        }
    }
}

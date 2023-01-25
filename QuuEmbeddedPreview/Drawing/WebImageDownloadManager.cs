using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace QuuEmbeddedPreview.Drawing
{
    class WebImageDownloadManager
    {
        public WebImageDownloadManager(DirectoryInfo cacheDir)
        {
            this.cacheDir = cacheDir;

            //Clear and create the cache directory
            if (cacheDir.Exists)
                cacheDir.Delete(true);
            cacheDir.Create();

            //Make client
            client = new WebClient();
        }

        private readonly DirectoryInfo cacheDir;
        private readonly WebClient client;

        private int nextCacheId = 0;
        private ConcurrentQueue<Tuple<string, object>> queuedUrls = new ConcurrentQueue<Tuple<string, object>>();
        private ConcurrentDictionary<string, DownloadedImage> cache = new ConcurrentDictionary<string, DownloadedImage>();

        public event RequestedImageReady OnRequestedImageReady;

        public delegate void RequestedImageReady(string url, object requester, DownloadedImage image);

        public void RequestImage(string url, object sender)
        {
            queuedUrls.Enqueue(new Tuple<string, object>(url, sender));
        }

        public async Task RunAsync()
        {
            while (true)
            {
                //Wait for a request
                Tuple<string, object> request;
                while (!queuedUrls.TryDequeue(out request))
                    await Task.Delay(500);

                //Check if the image exists in the cache
                DownloadedImage image;
                if (!cache.TryGetValue(request.Item1, out image))
                {
                    //Determine a new cache filename
                    FileInfo file = new FileInfo(cacheDir.FullName + Path.DirectorySeparatorChar + nextCacheId++ + ".jpg");

                    //Attempt to download the file
                    bool successful = true;
                    try
                    {
                        Console.WriteLine("WEB: Beginning download of image: " + request.Item1);
                        await client.DownloadFileTaskAsync(request.Item1, file.FullName);
                    } catch (Exception ex)
                    {
                        successful = false;
                        Console.WriteLine("WEB: Failed to download image: " + ex.Message + ex.StackTrace);
                    }

                    //Wrap as an image object and insert it into the cache
                    image = new DownloadedImage(successful, file);
                    cache.TryAdd(request.Item1, image);
                }

                //Send out event
                OnRequestedImageReady?.Invoke(request.Item1, request.Item2, image);
            }
        }

        public class DownloadedImage
        {
            public DownloadedImage(bool success, FileInfo filenane)
            {
                this.success = success;
                this.filenane = filenane;
            }

            private readonly bool success;
            private readonly FileInfo filenane;

            public bool Success => success;
            public FileInfo Filename => filenane;
        }
    }
}

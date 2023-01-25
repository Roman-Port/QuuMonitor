using Raylib_cs;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace QuuEmbeddedPreview.Drawing.Components
{
    class WebImageComponent : ImageComponent
    {
        public WebImageComponent(WebImageDownloadManager manager, Image defaultImage) : base(defaultImage)
        {
            this.manager = manager;
            manager.OnRequestedImageReady += Manager_OnRequestedImageReady;
            this.defaultImage = defaultImage;
        }

        private readonly WebImageDownloadManager manager;
        private readonly object mutex = new object();

        //Following can only be modified while locking mutex
        private Image defaultImage;
        private string url = null;
        private bool imageStale = true;
        private bool downloadedImageReady = false;
        private WebImageDownloadManager.DownloadedImage downloadedImageFile;

        //The following can only be changed in DrawPre
        private byte[] downloadedImageData;
        private GCHandle downloadedImageDataHandle;

        public string Url
        {
            get => url;
            set
            {
                lock (mutex)
                {
                    //Ignore if it's the same
                    if (url == value)
                        return;

                    //Apply
                    url = value;
                    downloadedImageReady = false;
                    imageStale = true;
                    if (value != null)
                        manager.RequestImage(value, this);
                }
                
            }
        }

        public override void DrawPre()
        {
            //Update image if needed
            if (imageStale)
            {
                lock (mutex)
                {
                    //Determine if the image is ready
                    if (url == null)
                    {
                        //Apply default image
                        Image = defaultImage;
                    } else if (downloadedImageReady && downloadedImageFile != null)
                    {
                        //Unload the previous image, if any
                        Image = defaultImage;
                        if (downloadedImageData != null)
                        {
                            downloadedImageDataHandle.Free();
                            downloadedImageData = null;
                        }

                        //If the file was successfully fetched, process
                        if (downloadedImageFile.Success)
                        {
                            try
                            {
                                //Decode the image
                                using (Stream fs = downloadedImageFile.Filename.OpenRead())
                                    LoadDownloadedImageFromFile(fs);
                            } catch
                            {
                                //Ignore error...
                            }
                        }
                    }

                    //Reset flag
                    imageStale = false;
                }
            }

            base.DrawPre();
        }

        private unsafe void LoadDownloadedImageFromFile(Stream fs)
        {
            using (SixLabors.ImageSharp.Image<Rgba32> img = SixLabors.ImageSharp.Image.Load<Rgba32>(fs))
            {
                //Allocate buffer and pin it
                downloadedImageData = new byte[img.Width * img.Height * Unsafe.SizeOf<Rgba32>()];
                downloadedImageDataHandle = GCHandle.Alloc(downloadedImageData, GCHandleType.Pinned);

                //Read pixel data
                img.CopyPixelDataTo(downloadedImageData);

                //Create a new image object for this
                Console.WriteLine($"Created new image of size {img.Width}x{img.Height}");
                Image = new Image
                {
                    width = img.Width,
                    height = img.Height,
                    format = PixelFormat.PIXELFORMAT_UNCOMPRESSED_R8G8B8A8,
                    mipmaps = 1,
                    data = (void*)downloadedImageDataHandle.AddrOfPinnedObject()
                };
            }
        }

        private void Manager_OnRequestedImageReady(string url, object requester, WebImageDownloadManager.DownloadedImage image)
        {
            //See if we're the one who requested this
            if (requester != this)
                return;

            //Apply
            lock (mutex)
            {
                //Make sure the URL matches the one we are requesting
                if (url != this.url)
                    return;

                //Apply
                downloadedImageReady = true;
                downloadedImageFile = image;
                imageStale = true;
            }
        }
    }
}

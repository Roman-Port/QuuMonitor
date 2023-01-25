using Newtonsoft.Json;
using QuuEmbeddedPreview.Communication;
using QuuEmbeddedPreview.Drawing;
using QuuEmbeddedPreview.Drawing.Components;
using QuuEmbeddedPreview.Drawing.Views;
using Raylib_cs;

namespace QuuEmbeddedPreview
{
    internal class Program
    {
        static DirectoryInfo homePath;
        static FileInfo configPath;
        static FileInfo defaultAeIconPath;
        static FileInfo logoPath;
        static DirectoryInfo cachePath;

        static PreviewConfig config;
        static QuuConnectionConfig quuConfig;
        static Image imageDefault;
        static Image imageLogo;

        static RayMemoryAsset fontMain;
        static RayMemoryAsset imgQuuBackground;

        static QuuConnection quu;
        static WebImageDownloadManager downloader;

        static ViewRenderer renderer;
        static MainView viewMain;

        static Task communicationTask;
        static Task downloadTask;

        static void Main(string[] args)
        {
            //Find files from the home directory
            if (args.Length != 1)
            {
                Console.WriteLine("You must pass exactly one argument to a new directory that'll be used for the application data.");
                return;
            }
            homePath = new DirectoryInfo(args[0]);
            configPath = homePath.GetFile("config.json");
            defaultAeIconPath = homePath.GetFile("default.png");
            logoPath = homePath.GetFile("logo.png");

            //Check
            if (!configPath.Exists)
            {
                Console.WriteLine("The configuration file doesn't exist!");
                return;
            }
            if (!defaultAeIconPath.Exists || !logoPath.Exists)
            {
                Console.WriteLine("Missing default and logo images. Try setting up the program again.");
                return;
            }

            //Load config
            config = JsonConvert.DeserializeObject<PreviewConfig>(File.ReadAllText(configPath.FullName));
            if (config.CacheDirectory == null || config.CacheDirectory.Length == 0)
            {
                Console.WriteLine("No cache directory is set in the configuration file.");
                return;
            }
            cachePath = new DirectoryInfo(config.CacheDirectory);

            //Load Quu configuration file
            if (config.QuuConfigFile == null || !File.Exists(config.QuuConfigFile))
            {
                Console.WriteLine("The Quu configuration file isn't set or doesn't exist.");
                return;
            }
            quuConfig = JsonConvert.DeserializeObject<QuuConnectionConfig>(File.ReadAllText(config.QuuConfigFile));

            //Create connection to Quu
            quu = new QuuConnection(quuConfig, config.QuuStationId);
            quu.OnEventReceived += Quu_OnEventReceived;
            quu.OnStatusChanged += Quu_OnStatusChanged;

            //Create renderer
            renderer = new ViewRenderer("Quu Monitor", config.DisplayWidth, config.DisplayHeight);

            //Load images from disk
            imageDefault = Raylib.LoadImage(defaultAeIconPath.FullName);
            imageLogo = Raylib.LoadImage(logoPath.FullName);

            //Load asset data stored as resources
            fontMain = RayMemoryAsset.FromResource("QuuEmbeddedPreview.Assets.font.ttf", ".ttf");
            imgQuuBackground = RayMemoryAsset.FromResource("QuuEmbeddedPreview.Assets.quu_background.png", ".png");

            //Create and start download manager
            downloader = new WebImageDownloadManager(cachePath);
            downloadTask = downloader.RunAsync();

            //Create views
            viewMain = new MainView(downloader, fontMain, imgQuuBackground, imageDefault, imageLogo);
            ClearAe();

            //Connect to Quu in the background
            communicationTask = quu.RunAsync();

            //Initialize display
            renderer.ChangeRoot(viewMain);
            renderer.Work();
        }

        private static void ClearAe()
        {
            viewMain.AeStation = config.CallLetters;
            viewMain.AeArtist = config.CallLetters;
            viewMain.AeTitle = config.Branding;
            viewMain.AeIconUrl = null;
        }

        private static void Quu_OnEventReceived(QuuConnection connection, QuuEvent evt)
        {
            renderer.QueueCommand((ViewRenderer r) =>
            {
                viewMain.AeIconUrl = evt.Image;
                viewMain.AeArtist = evt.Artist;
                viewMain.AeTitle = evt.Title;
            });
        }

        private static void Quu_OnStatusChanged(QuuConnection connection, QuuConnectionStatus status)
        {
            Console.WriteLine($"Quu connection status changed to {status}");
            renderer.QueueCommand((ViewRenderer r) =>
            {
                if (status == QuuConnectionStatus.CONNECTED)
                {
                    //Set state
                    viewMain.IsConnected = true;
                } else
                {
                    //Set state
                    viewMain.IsConnected = false;

                    //Reset to default
                    ClearAe();
                }
            });
        }
    }
}
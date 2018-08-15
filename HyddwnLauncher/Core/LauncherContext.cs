using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HyddwnLauncher.Util;

namespace HyddwnLauncher.Core
{
    public class LauncherContext
    {
        private readonly string[] _images =
        {
            "bangor.png",
            "damage.png",
            "duncan.png",
            "leehuga.png",
            "lucifer.png",
            "uniqueflyingpuppet.png",
            "vans.png"
        };

        public LauncherContext()
        {
            Log.Info("LauncherContext: Loading Settings...");
            LauncherSettingsManager = new LauncherSettingsManager();
            Log.Info("LauncherContext: Load Complete!");
        }

        public LauncherSettingsManager LauncherSettingsManager { get; protected set; }

        public ImageSource HostImage
        {
            get
            {
                var rnd = new Random();
                return GetImage(_images[rnd.Next(_images.Length)]);
            }
        }

        private static ImageSource GetImage(string imageName)
        {
            var sri = Application.GetResourceStream(new Uri("pack://application:,,,/Images/" + imageName));
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.StreamSource = sri.Stream;
            bmp.EndInit();

            return bmp;
        }
    }
}
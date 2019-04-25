using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using HyddwnLauncher.Util.Commands;
using MahApps.Metro;

namespace HyddwnLauncher.Util
{
    public class AccentColorMenuData
    {
        private ICommand _changeAccentCommand;
        public string Name { get; set; }
        public Brush BorderColorBrush { get; set; }
        public Brush ColorBrush { get; set; }

        public ICommand ChangeAccentCommand
        {
            get
            {
                return _changeAccentCommand ??
                       (_changeAccentCommand =
                           new SimpleCommand
                           {
                               CanExecuteDelegate = x => true,
                               ExecuteDelegate = x => DoChangeTheme(x)
                           });
            }
        }

        protected virtual void DoChangeTheme(object sender)
        {
            ThemeManager.DetectAppStyle(Application.Current);
            var accent = ThemeManager.GetAccent(Name);
            MainWindow.Instance.Settings.LauncherSettings.Accent = accent.Name;
            ThemeManager.ChangeAppStyle(Application.Current, accent,
                ThemeManager.GetAppTheme(MainWindow.Instance.Settings.LauncherSettings.Theme));
        }
    }

    public class AppThemeMenuData : AccentColorMenuData
    {
        protected override void DoChangeTheme(object sender)
        {
            ThemeManager.DetectAppStyle(Application.Current);
            var appTheme = ThemeManager.GetAppTheme(Name);
            MainWindow.Instance.Settings.LauncherSettings.Theme = appTheme.Name;
            ThemeManager.ChangeAppStyle(Application.Current,
                ThemeManager.GetAccent(MainWindow.Instance.Settings.LauncherSettings.Accent), appTheme);
        }
    }
}
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shell;
using System.Windows.Threading;
using HyddwnLauncher.Extensibility;
using HyddwnLauncher.Patcher.NxLauncher;
using HyddwnLauncher.Util.Helpers;

namespace HyddwnLauncher.Util
{
    public static class Extentions
    {
        public static void SetVisibilitySafe(this UIElement uiElement, Visibility visibility)
        {
            Application.Current.Dispatcher.Invoke((Action) (() => uiElement.Visibility = visibility), DispatcherPriority.Send);
        }

        public static void SetMetroProgressSafe(this ProgressBar progressBar, double value)
        {
            Application.Current.Dispatcher.Invoke((Action) (() => progressBar.Value = value), DispatcherPriority.Send);
        }

        public static void SetProgressValueSafe(this TaskbarItemInfo taskbarItemInfo, double value)
        {
            Application.Current.Dispatcher.Invoke((Action)(() => taskbarItemInfo.ProgressValue = MathHelper.Normalize(value)));
        }

        public static void SetProgressStateSafe(this TaskbarItemInfo taskbarItemInfo, TaskbarItemProgressState value)
        {
            Application.Current.Dispatcher.Invoke((Action)(() => taskbarItemInfo.ProgressState = value));
        }

        public static void SetCheckBoxIsCheckedSafe(this CheckBox checkBox, bool isChecked)
        {
            Application.Current.Dispatcher.Invoke((Action) (() => checkBox.IsChecked = isChecked));
        }

        public static void SetBoolSafe(this bool value3, bool p)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (value3 == p)
                    return;
                value3 = p;
            });
        }

        public static void AddChildrenSafe(this WrapPanel panel, UIElement child)
        {
            Application.Current.Dispatcher.Invoke((Action) (() => panel.Children.Add(child)));
        }

        public static void SetTextBoxTextSafe(this TextBox textBox, string text)
        {
            Application.Current.Dispatcher.Invoke((Action) (() => textBox.Text = text));
        }

        public static void SetTextBoxTextSafe(this TextBox textBox, string format, params object[] args)
        {
            textBox.SetTextBoxTextSafe(string.Format(format, args));
        }

        public static void SetTextBlockSafe(this TextBlock textBlock, string text)
        {
            Application.Current.Dispatcher.Invoke((Action) (() => textBlock.Text = text));
        }

        public static void SetTextBlockSafe(this TextBlock textBlock, string format, params object[] args)
        {
            textBlock.SetTextBlockSafe(string.Format(format, args));
        }

        public static void SetRunSafe(this Run run, string text)
        {
            Application.Current.Dispatcher.Invoke((Action)(() => run.Text = text));
        }

        public static void SetRunSafe(this Run run, string format, params object[] args)
        {
            run.SetRunSafe(string.Format(format, args));
        }

        public static void SetMetroProgressIndeterminateSafe(this ProgressBar progressBar, bool value)
        {
            Application.Current.Dispatcher.Invoke((Action) (() => progressBar.IsIndeterminate = value));
        }

        public static void SetForegroundSafe(this Control element, Brush brush)
        {
            Application.Current.Dispatcher.Invoke((Action) (() => element.Foreground = brush));
        }

        public static string LocalizedThemeColor(this string theme)
        {
            switch (theme)
            {
                case "BaseDark":
                    return Properties.Resources.BaseDark;
                case "BaseLight":
                    return Properties.Resources.BaseLight;
                case "Red":
                    return Properties.Resources.Red;
                case "Green":
                    return Properties.Resources.Green;
                case "Blue":
                    return Properties.Resources.Blue;
                case "Purple":
                    return Properties.Resources.Purple;
                case "Orange":
                    return Properties.Resources.Orange;
                case "Lime":
                    return Properties.Resources.Lime;
                case "Emerald":
                    return Properties.Resources.Emerald;
                case "Teal":
                    return Properties.Resources.Teal;
                case "Cyan":
                    return Properties.Resources.Cyan;
                case "Cobalt":
                    return Properties.Resources.Cobalt;
                case "Indigo":
                    return Properties.Resources.Indigo;
                case "Violet":
                    return Properties.Resources.Violet;
                case "Pink":
                    return Properties.Resources.Pink;
                case "Magenta":
                    return Properties.Resources.Magenta;
                case "Crimson":
                    return Properties.Resources.Crimson;
                case "Amber":
                    return Properties.Resources.Amber;
                case "Yellow":
                    return Properties.Resources.Yellow;
                case "Brown":
                    return Properties.Resources.Brown;
                case "Olive":
                    return Properties.Resources.Olive;
                case "Steel":
                    return Properties.Resources.Steel;
                case "Mauve":
                    return Properties.Resources.Mauve;
                case "Taupe":
                    return Properties.Resources.Taupe;
                case "Sienna":
                    return Properties.Resources.Sienna;
                default:
                    return theme;
            }
        }

        public static string LocalizedLocalization(this string localization)
        {
            switch (localization)
            {
                case ClientLocalization.NorthAmerica:
                    return Properties.Resources.NorthAmerica;
                case ClientLocalization.Japan:
                    return Properties.Resources.Japan;
                case ClientLocalization.JapanHangame:
                    return Properties.Resources.JapanHangame;
                //case ClientLocalization.Korea:
                //    return Properties.Resources.Korea;
                //case ClientLocalization.KoreaTest:
                //    return Properties.Resources.KoreaTest;
                //case ClientLocalization.Taiwan:
                //    return Properties.Resources.Taiwan;
                default:
                    return localization;
            }
        }

        public static string LocalizedPatchReason(this PatchReason reason)
        {
            switch (reason)
            {
                case PatchReason.None:
                    return Properties.Resources.None;
                case PatchReason.Modified:
                    return Properties.Resources.Modified;
                case PatchReason.Older:
                    return Properties.Resources.Older;
                case PatchReason.DoesNotExist:
                    return Properties.Resources.DoesNotExist;
                case PatchReason.SizeNotMatch:
                    return Properties.Resources.SizeNotMatch;
                case PatchReason.Repair:
                    return Properties.Resources.Repair;
                case PatchReason.Force:
                    return Properties.Resources.Force;
                default:
                    return reason.ToString();
            }
        }

        public static string ToExtendedLaunchArguments(this string localization)
        {
            switch (localization)
            {
                case ClientLocalization.Japan:
                    return "setting:\"file://data/features.xml=Regular, Japan\"";
                case ClientLocalization.JapanHangame:
                    return "setting:\"file://data/features.xml=Regular, Japan\" sublocale:nhnjapan";
                default:
                    return "setting:file://data/features.xml locale:USA env:Regular";
            }
        }

        public static string LocalizedLogLevel(this LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Info:
                    return Properties.Resources.Info;
                case LogLevel.Warning:
                    return Properties.Resources.Warning;
                case LogLevel.Error:
                    return Properties.Resources.Error;
                case LogLevel.Debug:
                    return Properties.Resources.Debug;
                case LogLevel.Status:
                    return Properties.Resources.Status;
                case LogLevel.Exception:
                    return Properties.Resources.Exception;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }

        /// <summary>
        ///     Raises event with thread and null-ref safety.
        /// </summary>
        public static void Raise<T>(this EventHandler<T> handler, object sender, T args) where T : EventArgs
        {
            handler?.Invoke(sender, args);
        }

        /// <summary>
        ///     Raises event with thread and null-ref safety.
        /// </summary>
        public static void Raise(this Action handler)
        {
            handler?.Invoke();
        }

        /// <summary>
        ///     Raises event with thread and null-ref safety.
        /// </summary>
        public static void Raise<T>(this Action<T> handler, T args)
        {
            handler?.Invoke(args);
        }

        /// <summary>
        ///     Raises event with thread and null-ref safety.
        /// </summary>
        public static void Raise<T1, T2>(this Action<T1, T2> handler, T1 args1, T2 args2)
        {
            handler?.Invoke(args1, args2);
        }

        /// <summary>
        ///     Raises event with thread and null-ref safety.
        /// </summary>
        public static void Raise<T1, T2, T3>(this Action<T1, T2, T3> handler, T1 args1, T2 args2, T3 args3)
        {
            handler?.Invoke(args1, args2, args3);
        }

        /// <summary>
        ///     Raises event with thread and null-ref safety.
        /// </summary>
        public static void Raise<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> handler, T1 args1, T2 args2, T3 args3,
            T4 args4)
        {
            handler?.Invoke(args1, args2, args3, args4);
        }

        /// <summary>
        ///     Raises event with thread and null-ref safety.
        /// </summary>
        public static void Raise<T1, T2, T3, T4, T5>(this Action<T1, T2, T3, T4, T5> handler, T1 args1, T2 args2,
            T3 args3, T4 args4, T5 args5)
        {
            handler?.Invoke(args1, args2, args3, args4, args5);
        }

        /// <summary>
        ///     StackOverflow
        ///     https://stackoverflow.com/questions/25366534/file-writealltext-not-flushing-data-to-disk
        ///     When saving the configuration, it ti first written to a temp file.
        ///     If something happens to cause the write to the temp file to fail, the save is lost
        ///     however the original data is untouched. This should reduce loss of configuration data
        ///     due to write failure significantly
        /// </summary>
        /// <param name="path"></param>
        /// <param name="contents"></param>
        public static void WriteAllTextWithBackup(this string contents, string path)
        {
            if (!File.Exists(path))
            {
                File.WriteAllText(path, contents);
                return;
            }

            // use the same folder so that they are always on the same drive!
            var tempPath = Path.Combine(Path.GetDirectoryName(path), Guid.NewGuid().ToString());

            // create the backup name
            var backup = path + ".backup";

            // delete any existing backups
            if (File.Exists(backup))
                File.Delete(backup);

            // get the bytes
            var data = Encoding.UTF8.GetBytes(contents);

            // write the data to a temp file
            using (var tempFile = File.Create(tempPath, 4096, FileOptions.WriteThrough))
            {
                tempFile.Write(data, 0, data.Length);
            }

            // replace the contents
            File.Replace(tempPath, path, backup);
        }

        /// <summary>
        ///     StackOverflow
        ///     https://stackoverflow.com/questions/25366534/file-writealltext-not-flushing-data-to-disk
        ///     When saving the configuration, it ti first written to a temp file.
        ///     If something happens to cause the write to the temp file to fail, the save is lost
        ///     however the original data is untouched. This should reduce loss of configuration data
        ///     due to write failure significantly
        /// </summary>
        /// <param name="path"></param>
        /// <param name="contents"></param>
        public static void WriteAllBytesWithBackup(this byte[] contents, string path)
        {
            if (!File.Exists(path))
            {
                File.WriteAllBytes(path, contents);
                return;
            }

            // use the same folder so that they are always on the same drive!
            var tempPath = Path.Combine(Path.GetDirectoryName(path), Guid.NewGuid().ToString());

            // create the backup name
            var backup = path + ".backup";

            // delete any existing backups
            if (File.Exists(backup))
                File.Delete(backup);

            // write the data to a temp file
            using (var tempFile = File.Create(tempPath, 4096, FileOptions.WriteThrough))
            {
                tempFile.Write(contents, 0, contents.Length);
            }

            // replace the contents
            File.Replace(tempPath, path, backup);
        }

        public static bool IsWithin(this int value, int minimum, int maximum)
        {
            return value >= minimum && value <= maximum;
        }
    }
}
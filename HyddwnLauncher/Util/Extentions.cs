using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace HyddwnLauncher.Util
{
    public static class Extentions
    {
        public static void SetVisibilitySafe(this UIElement uiElement, Visibility visibility)
        {
            Application.Current.Dispatcher.Invoke((Action) (() => uiElement.Visibility = visibility));
        }

        public static void SetMetroProgressSafe(this ProgressBar progressBar, double value)
        {
            Application.Current.Dispatcher.Invoke((Action) (() => progressBar.Value = value));
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
            Application.Current.Dispatcher.Invoke((Action) (() => textBlock.Text = text), DispatcherPriority.Send);
        }

        public static void SetTextBlockSafe(this TextBlock textBlock, string format, params object[] args)
        {
            textBlock.SetTextBlockSafe(string.Format(format, args));
        }

        public static void SetMetroProgressIndeterminateSafe(this ProgressBar progressBar, bool value)
        {
            Application.Current.Dispatcher.Invoke((Action) (() => progressBar.IsIndeterminate = value));
        }

        public static void SetForegroundSafe(this Control element, Brush brush)
        {
            Application.Current.Dispatcher.Invoke((Action) (() => element.Foreground = brush));
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
        public static void Raise<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> handler, T1 args1, T2 args2, T3 args3, T4 args4)
        {
            handler?.Invoke(args1, args2, args3, args4);
        }

        /// <summary>
        ///     Raises event with thread and null-ref safety.
        /// </summary>
        public static void Raise<T1, T2, T3, T4, T5>(this Action<T1, T2, T3, T4, T5> handler, T1 args1, T2 args2, T3 args3, T4 args4, T5 args5)
        {
            handler?.Invoke(args1, args2, args3, args4, args5);
        }

        /// <summary>
        /// StackOverflow
        /// https://stackoverflow.com/questions/25366534/file-writealltext-not-flushing-data-to-disk
        ///
        /// When saving the configuration, it ti first written to a temp file.
        /// If something happens to cause the write to the temp file to fail, the save is lost
        /// however the original data is untouched. This should reduce loss of configuration data
        /// due to write failure significantly
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

            // generate a temp filename
            var tempPath = Path.GetTempFileName();

            // create the backup name
            var backup = path + ".backup";

            // delete any existing backups
            if (File.Exists(backup))
                File.Delete(backup);

            // get the bytes
            var data = Encoding.UTF8.GetBytes(contents);

            // write the data to a temp file
            using (var tempFile = File.Create(tempPath, 4096, FileOptions.WriteThrough))
                tempFile.Write(data, 0, data.Length);

            // replace the contents
            File.Replace(tempPath, path, backup);
        }
    }
}
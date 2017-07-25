using System;
using System.Windows;
using System.Windows.Controls;
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
    }
}
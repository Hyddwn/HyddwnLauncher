using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using HyddwnLauncher.Util;

namespace HyddwnLauncher
{
    /// <summary>
    ///     Interaction logic for ExceptionReporter.xaml
    /// </summary>
    public partial class ExceptionReporter
    {
        private readonly Exception _ex;

        public bool Result { get; set; } = true;

        public ExceptionReporter(Exception ex)
        {
            _ex = ex;
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ExceptionBox.Text = string.Format(
                Properties.Resources.ExceptionReporterString,
                Assembly.GetExecutingAssembly().GetName().Version, DateTime.Now, Environment.OSVersion,
                AppDomain.CurrentDomain.BaseDirectory, Environment.CurrentDirectory, Environment.SystemDirectory,
                RuntimeEnvironment.GetSystemVersion(), App.IsAdministrator() ? "Admin" : "Non-Admin", _ex);

            Log.Info(ExceptionBox.Text);
        }

        private void SendButtonClick(object sender, RoutedEventArgs e)
        {
            SetResultAndClose(true);
        }

        private void DontSentButtonClick(object sender, RoutedEventArgs e)
        {
            SetResultAndClose(false);
        }

        private void SetResultAndClose(bool result)
        {
            Result = result;
            Close();
        }
    }
}
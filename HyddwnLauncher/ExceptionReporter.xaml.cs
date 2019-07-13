using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using HyddwnLauncher.Util;

namespace HyddwnLauncher
{
    /// <summary>
    ///     Interaction logic for ExceptionReporter.xaml
    /// </summary>
    public partial class ExceptionReporter
    {
        private readonly Exception _ex;

        public ExceptionReporter(Exception ex)
        {
            _ex = ex;
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ExceptionBox.Text = string.Format(
                HyddwnLauncher.Properties.Resources.ExceptionReporterString,
                Assembly.GetExecutingAssembly().GetName().Version, DateTime.Now, Environment.OSVersion,
                AppDomain.CurrentDomain.BaseDirectory, Environment.CurrentDirectory, Environment.SystemDirectory,
                RuntimeEnvironment.GetSystemVersion(), _ex);

            Log.Info(ExceptionBox.Text);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HyddwnLauncher.Patcher.Legacy.Core.Controls
{
    /// <summary>
    /// Interaction logic for ProgressReporter.xaml
    /// </summary>
    public partial class ProgressReporter
    {
        public ProgressReporter(ProgressReporterViewModel progressReporterViewModel)
        {
            // This should just make the bindings automagically work!
            this.DataContext = progressReporterViewModel;
            InitializeComponent();
        }
    }
}

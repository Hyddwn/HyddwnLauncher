using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HyddwnLauncher.Extensibility.Interfaces;
using HyddwnLauncher.Patcher.Legacy.Annotations;

namespace HyddwnLauncher.Patcher.Legacy.Core
{
    public class ProgressReporterViewModel : IProgressIndicator, INotifyPropertyChanged
    {
        private double _progressBarPercent;
        private bool _isIndeterminate;
        private string _leftText;
        private string _rightText;

        public double ProgressBarPercent
        {
            get => _progressBarPercent;
            set
            {
                if (value.Equals(_progressBarPercent)) return;
                _progressBarPercent = value;
                OnPropertyChanged();
            }
        }

        public bool IsIndeterminate
        {
            get => _isIndeterminate;
            set
            {
                if (value == _isIndeterminate) return;
                _isIndeterminate = value;
                OnPropertyChanged();
            }
        }

        public string LeftText
        {
            get => _leftText;
            set
            {
                if (value == _leftText) return;
                _leftText = value;
                OnPropertyChanged();
            }
        }

        public string RightText
        {
            get => _rightText;
            set
            {
                if (value == _rightText) return;
                _rightText = value;
                OnPropertyChanged();
            }
        }

        public ProgressReporterViewModel(string leftText = "", string rightText = "")
        {
            LeftText = leftText;
            RightText = rightText;
        }

        public void SetIsIndeterminate(bool value)
        {
            IsIndeterminate = value;
        }

        public void SetLeftText(string text)
        {
            LeftText = text;
        }

        [StringFormatMethod("format")]
        public void SetLeftText(string format, params object[] args)
        {
            SetLeftText(string.Format(format, args));
        }

        public void SetProgressBar(double value)
        {
            ProgressBarPercent = value;
        }

        public void SetRightText(string text)
        {
            RightText = text;
        }

        [StringFormatMethod("format")]
        public void SetRightText(string format, params object[] args)
        {
            SetRightText(string.Format(format, args));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

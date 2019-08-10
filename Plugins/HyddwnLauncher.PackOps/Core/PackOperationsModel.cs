using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace HyddwnLauncher.PackOps.Core
{
	public class PackOperationsViewModel : INotifyPropertyChanged
	{
		// Works but probably not exactly what I was looking for.
		private const string PackFileTestRegex = @"(\S+\d*_full|\S+\d*_to_\S+\d*).pack";

		private string _filePath;
		private bool _isSequenceTargetable;
		private string _packName;
		private int _packVersion;
	    private int _fromPackVersion;

		public PackOperationsViewModel()
		{
		}

		public PackOperationsViewModel(string packFilePath)
		{
			FilePath = packFilePath;
			PackName = Path.GetFileName(FilePath);

			var match = Regex.Match(PackName, PackFileTestRegex).Value;
			IsSequenceTargetable = match == PackName;

		    if (PackName.Contains("_to_"))
		    {
		        var matchRegex = @"(_\d+)";
		        match = Regex.Match(PackName, matchRegex).Value;
		        var toMatch = match.Replace("_", "");

		        var packVersion = 0;

		        int.TryParse(toMatch, out packVersion);

		        PackVersion = packVersion;
		        return;
		    }

            if (PackName.Contains("full"))
			{
				var matchRegex = @"(\d+_)";
				match = Regex.Match(PackName, matchRegex).Value;
				var fromMatch = match.Replace("_", "");

				var packVersion = 0;

				int.TryParse(fromMatch, out packVersion);

				PackVersion = packVersion;
				return;
			}

			PackVersion = -1;
		}

		public string FilePath
		{
			get => _filePath;
			set
			{
				if (_filePath == value) return;
				_filePath = value;
				OnPropertyChanged();
			}
		}


		public string PackName
		{
			get => _packName;
			set
			{
				if (_packName == value) return;
				_packName = value;
				OnPropertyChanged();
			}
		}

		public int PackVersion
		{
			get => _packVersion;
			protected set
			{
				if (_packVersion == value) return;
				_packVersion = value;
				OnPropertyChanged();
			}
		}

		public bool IsSequenceTargetable
		{
			get => _isSequenceTargetable;
			set
			{
				if (_isSequenceTargetable == value) return;
				_isSequenceTargetable = value;
				OnPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
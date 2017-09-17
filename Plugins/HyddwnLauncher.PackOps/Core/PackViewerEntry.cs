using System.Collections.ObjectModel;

namespace HyddwnLauncher.PackOps.Core
{
	public class PackViewerEntry
	{
		public PackViewerEntry()
		{
			SubItems = new ObservableCollection<PackViewerEntry>();
		}

		public int Level { get; set; }

		public string Name { get; set; }

		public ObservableCollection<PackViewerEntry> SubItems { get; set; }
	}
}
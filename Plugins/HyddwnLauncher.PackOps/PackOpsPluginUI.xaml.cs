using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using HyddwnLauncher.Extensibility;
using HyddwnLauncher.Extensibility.Interfaces;
using HyddwnLauncher.PackOps.Core;
using HyddwnLauncher.PackOps.Pack;
using HyddwnLauncher.PackOps.Util;

namespace HyddwnLauncher.PackOps
{
	/// <summary>
	///     Interaction logic for PackOpsPluginUI.xaml
	/// </summary>
	public partial class PackOpsPluginUI : UserControl
	{
		public static readonly DependencyProperty MaximumPackVersionProperty = DependencyProperty.Register(
			"MaximumPackVersion", typeof(int), typeof(PackOpsPluginUI), new PropertyMetadata(0));

		public static readonly DependencyProperty MinimumPackVersionProperty = DependencyProperty.Register(
			"MinimumPackVersion", typeof(int), typeof(PackOpsPluginUI), new PropertyMetadata(0));

		public static readonly DependencyProperty FromValueProperty = DependencyProperty.Register(
			"FromValue", typeof(int), typeof(PackOpsPluginUI), new PropertyMetadata(0));

		public static readonly DependencyProperty ToValueProperty = DependencyProperty.Register(
			"ToValue", typeof(int), typeof(PackOpsPluginUI), new PropertyMetadata(0));

		private IClientProfile _clientProfile;
		private readonly PluginContext _pluginContext;
		private IServerProfile _serverProfile;

		public PackOpsPluginUI(PluginContext pluginContext, IClientProfile clientProfile, IServerProfile serverProfile)
		{
			_pluginContext = pluginContext;
			_clientProfile = clientProfile;
			_serverProfile = serverProfile;

			PackOpsSettings = PackOpsSettingsManager.Instance.PackOpsSettings;

			PackViewEntries = new ObservableCollection<PackViewerEntry>();
			PackOperationsViewModels = new ObservableCollection<PackOperationsViewModel>();

			MinimumPackVersion = MaximumPackVersion = FromValue = ToValue = 0;

			InitializeComponent();
		}

		public int MaximumPackVersion
		{
			get => (int) GetValue(MaximumPackVersionProperty);
			set => SetValue(MaximumPackVersionProperty, value);
		}

		public int MinimumPackVersion
		{
			get => (int) GetValue(MinimumPackVersionProperty);
			set => SetValue(MinimumPackVersionProperty, value);
		}

		public int FromValue
		{
			get => (int) GetValue(FromValueProperty);
			set => SetValue(FromValueProperty, value);
		}

		public int ToValue
		{
			get => (int) GetValue(ToValueProperty);
			set => SetValue(ToValueProperty, value);
		}

		public PackOpsSettings PackOpsSettings { get; protected set; }
		public List<PackListEntry> PackFileEntries { get; private set; }
		public ObservableCollection<PackViewerEntry> PackViewEntries { get; private set; }
		public ObservableCollection<PackOperationsViewModel> PackOperationsViewModels { get; }

		public void ClientProfileChangedAsync(IClientProfile clientProfile)
		{
			_clientProfile = clientProfile;

			GetPacksForClientProfile();
		}

		internal async void GetPacksForClientProfile()
		{
			if (_clientProfile == null) return;

			PackOperationsViewModels.Clear();

			await Task.Run(() =>
			{
				foreach (var packFilePath in Directory.EnumerateFiles($"{Path.GetDirectoryName(_clientProfile.Location)}\\package",
					"*.pack", SearchOption.TopDirectoryOnly).ToList().OrderBy(a => a))
					Dispatcher.Invoke(() => PackOperationsViewModels.Add(new PackOperationsViewModel(packFilePath)));
			});

		    var packViewModelWorkingSet = PackOperationsViewModels.Where(pvm => pvm.IsSequenceTargetable)
		        .OrderBy(pvm => pvm.PackVersion).ToList();

            MaximumPackVersion = packViewModelWorkingSet.LastOrDefault()?.PackVersion ?? 0;
		    MinimumPackVersion = FromValue = ToValue = packViewModelWorkingSet.FirstOrDefault()?.PackVersion ?? 0;
        }

		public void ServerProfileChanged(IServerProfile serverProfile)
		{
			_serverProfile = serverProfile;

			GetPacksForClientProfile();
		}

		private async void PackViewerRefreshOnClick(object sender, RoutedEventArgs e)
		{
			PackViewLoader.IsOpen = true;
			await Task.Delay(500);

			await GetPackEntries();
			await Refresh();
		}

		private async Task GetPackEntries()
		{
			if (_clientProfile == null) return;

			var packagePath = $"{Path.GetDirectoryName(_clientProfile.Location)}\\package";

			await Task.Run(() =>
			{
				using (var packReader = new PackReader(packagePath))
				{
					PackFileEntries = packReader.GetEntries().OrderBy(g => g.PackFilePath).ToList();
				}
			});
		}

		private async Task Refresh()
		{
			if (_clientProfile == null) return;
			if (string.IsNullOrWhiteSpace(_clientProfile.Location)) return;

			await Populate();

			PackViewLoader.IsOpen = false;
		}

		private async Task Populate()
		{
			await Task.Run(() =>
			{
				Dispatcher.Invoke(() =>
				{
					PackViewEntries.Clear();
					PackViewTreeView.Items.SortDescriptions.Clear();

					foreach (var packFileEntry in PackFileEntries)
					{
						var root =
							PackViewEntries.FirstOrDefault(
								x => x.Name.Equals(Path.GetFileName(packFileEntry.PackFilePath)) && x.Level.Equals(1));
						if (root == null)
						{
							root = new PackViewerEntry
							{
								Level = 1,
								Name = Path.GetFileName(packFileEntry.PackFilePath)
							};
							PackViewEntries.Add(root);
							PackViewEntries.OrderBy(p => p.Name);
						}

						var fileItem = packFileEntry.FullName.Split('\\');
						if (fileItem.Any())
						{
							var subRoot =
								root.SubItems.FirstOrDefault(x => x.Name.Equals(fileItem[0]) && x.Level.Equals(2));
							if (subRoot == null)
							{
								subRoot = new PackViewerEntry
								{
									Level = 2,
									Name = fileItem[0]
								};
								root.SubItems.Add(subRoot);
								root.SubItems.OrderBy(p => p.Name);
							}

							if (fileItem.Length > 1)
							{
								var parentItem = subRoot;
								var level = 3;
								for (var i = 1; i < fileItem.Length; ++i)
								{
									var subItem =
										parentItem.SubItems.FirstOrDefault(
											x => x.Name.Equals(fileItem[i]) && x.Level.Equals(level));
									if (subItem == null)
									{
										subItem = new PackViewerEntry
										{
											Name = fileItem[i],
											Level = level
										};
										parentItem.SubItems.Add(subItem);
										parentItem.SubItems.OrderBy(p => p.Name);
									}

									parentItem = subItem;
									level++;
								}
							}
						}
					}

					PackViewTreeView.Items.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
					PackViewTreeView.Items.Refresh();
				});
			});
		}

		private async void PackOperationsMergePacksOnClick(object sender, RoutedEventArgs e)
		{
			List<PackListEntry> packEntryCollection;
			var selectedPackViewModels = PackOperationsViewModels
				.Where(pvm => pvm.IsSequenceTargetable && pvm.PackVersion >= FromValue && pvm.PackVersion <= ToValue)
				.OrderBy(pvm => pvm.PackVersion).ToList();
			if (selectedPackViewModels.Count < 2) return;

			// Reader must be kept open to pull the data streams
			using (var packReader = new PackReader())
			{
				_pluginContext.SetPatcherState(true);
				PackOperationsLoader.IsOpen = true;
				await Task.Delay(500);

				ProgressBar.Value = 0;
				ProgressBar.IsIndeterminate = true;
				ProgressText.Text = "Getting Entries...";

				await Task.Run(() =>
				{
					// This will then only take the overrides between the selected pack files you would like to merge
					foreach (var packViewModel in selectedPackViewModels)
						try
						{
							packReader.Load(packViewModel.FilePath);
						}
						catch (Exception ex)
						{
							_pluginContext.LogException(ex, false);
						}
				});

				packEntryCollection = packReader.GetEntries().OrderBy(ple => ple.PackFilePath).ThenBy(ple => ple.FullName).ToList();

				const string packagePath = "package";

				await Task.Run(() =>
				{
					double entries = packEntryCollection.Count;
					var progress = 0;

					var bytes = packEntryCollection.Sum(p => p.DecompressedSize);

					Dispatcher.Invoke(() =>
					{
						ProgressBar.Value = 0;
						ProgressBar.IsIndeterminate = false;
						ProgressText.Text = $"0 of {entries} ({ByteSizeHelper.ToString(bytes)} left)";
					});

					var packName = "";

					var beginningPack = selectedPackViewModels.FirstOrDefault().PackName;

					packName = beginningPack.EndsWith("full.pack", StringComparison.OrdinalIgnoreCase)
						? $"{selectedPackViewModels.LastOrDefault().PackVersion}_full.pack"
						: $"{selectedPackViewModels.FirstOrDefault().PackVersion}_to_{selectedPackViewModels.LastOrDefault().PackVersion}.pack";

					using (var pw = new PackWriter($"{packagePath}\\{packName}", selectedPackViewModels.LastOrDefault().PackVersion))
					{
						foreach (var entry in packEntryCollection)
						{
							var fileStream = entry.GetCompressedDataAsStream();

							pw.WriteDirect(fileStream, entry.FullName, (int) entry.Seed, (int) entry.CompressedSize,
								(int) entry.DecompressedSize, entry.IsCompressed, entry.CreationTime, entry.LastWriteTime,
								entry.LastAccessTime);
							fileStream.Dispose();

							progress++;
							bytes -= entry.DecompressedSize;

							var progressLocal = progress;
							var bytesLocal = bytes;
							Dispatcher.Invoke(() =>
							{
								ProgressBar.Value = progressLocal / entries * 100;
								ProgressText.Text = $"{progressLocal} of {entries} ({ByteSizeHelper.ToString(bytesLocal)} left)";
							});
						}

						Dispatcher.Invoke(() =>
						{
							ProgressBar.Value = 0;
							ProgressBar.IsIndeterminate = true;
							ProgressText.Text = "Creating Pack File...";
						});

						pw.Pack();
					}
				});
			}

			await Task.Run(() =>
			{
				if (!PackOpsSettingsManager.Instance.PackOpsSettings.DeletePackFilesAfterMerge) return;
				Dispatcher.Invoke(() =>
				{
					ProgressBar.Value = 0;
					ProgressBar.IsIndeterminate = true;
					ProgressText.Text = "Cleaning Up...";
				});

				foreach (var model in selectedPackViewModels)
					try
					{
						File.Delete(model.FilePath);
					}
					catch (Exception ex)
					{
						_pluginContext.LogString($"Unable to delete {model.PackName}", false);
						_pluginContext.LogException(ex, false);
					}
			});

			ProgressBar.Value = 0;
			ProgressBar.IsIndeterminate = true;
			ProgressText.Text = "Getting Packs...";

			GetPacksForClientProfile();

			PackOperationsLoader.IsOpen = false;

			_pluginContext.SetPatcherState(false);
		}
	}
}
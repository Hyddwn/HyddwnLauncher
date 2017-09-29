using System;

namespace HyddwnLauncher.Extensibility.Interfaces
{
	public interface ISettingsManager
	{
		T Load<T>() where T : class, new();
		T LoadSection<T>() where T : class, new();
		object Load(Type type);
		object LoadSection(Type type);
		void Save<T>(T typeObject) where T : class, new();
		void SaveSection<T>(T typeObject) where T : class, new();
		void Save(Type type, object value);
		void SaveSection(Type type, object value);
	}
}

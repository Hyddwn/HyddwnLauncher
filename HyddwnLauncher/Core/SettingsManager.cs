using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using HyddwnLauncher.Extensibility.Interfaces;
using HyddwnLauncher.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace HyddwnLauncher.Core
{
	public class SettingsManager : ISettingsManager
	{
		private readonly string _configurationFilePath;
		private readonly string _sectionNameSuffix;

		private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
		{
			ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
			ContractResolver = new SettingsManagerContractResolver(),
			ReferenceLoopHandling = ReferenceLoopHandling.Ignore
		};

		public SettingsManager(string configurationFilePath, string sectionNameSuffix = "Settings")
		{
			_configurationFilePath = configurationFilePath;
			_sectionNameSuffix = sectionNameSuffix;
		}

		public T Load<T>() where T : class, new() => Load(typeof(T)) as T;

		public T LoadSection<T>() where T : class, new() => LoadSection(typeof(T)) as T;

		public object Load(Type type)
		{
			if (!File.Exists(_configurationFilePath))
				return Activator.CreateInstance(type);

			var jsonFile = File.ReadAllText(_configurationFilePath);

			return JsonConvert.DeserializeObject(jsonFile, type, JsonSerializerSettings);
		}

		public object LoadSection(Type type)
		{
			if (!File.Exists(_configurationFilePath))
				return Activator.CreateInstance(type);

			var jsonFile = File.ReadAllText(_configurationFilePath);
			var section = ToCamelCase(type.Name.Replace(_sectionNameSuffix, string.Empty));
			var settingsData = JsonConvert.DeserializeObject<dynamic>(jsonFile, JsonSerializerSettings);
			var settingsSection = settingsData[section];

			return settingsSection == null
				? Activator.CreateInstance(type)
				: JsonConvert.DeserializeObject(JsonConvert.SerializeObject(settingsSection), type,
					JsonSerializerSettings);
		}

		public void Save<T>(T settingsObject) where T : class, new() => Save(typeof(T), settingsObject);

		public void SaveSection<T>(T settingsSectionObject) where T : class, new() =>
			SaveSection(typeof(T), settingsSectionObject);

		public void Save(Type type, object value)
		{
			var jsonFile = JsonConvert.SerializeObject(value, type, Formatting.Indented, JsonSerializerSettings);

            jsonFile.WriteAllTextWithBackup(_configurationFilePath);
        }

		public void SaveSection(Type type, object value)
		{
			string jsonFile;
			dynamic settingsData = new Dictionary<string, string>();

			var sectionName = ToCamelCase(type.Name.Replace(_sectionNameSuffix, string.Empty));

			if (File.Exists(_configurationFilePath))
			{
				jsonFile = File.ReadAllText(_configurationFilePath);
				settingsData = JsonConvert.DeserializeObject<dynamic>(jsonFile, JsonSerializerSettings);
				settingsData[sectionName] = JsonConvert.SerializeObject(value, type, Formatting.Indented, JsonSerializerSettings);
				jsonFile = JsonConvert.SerializeObject(settingsData, Formatting.Indented);
			    jsonFile.WriteAllTextWithBackup(_configurationFilePath);
                return;
			}

			settingsData[sectionName] = JsonConvert.SerializeObject(value, type, Formatting.Indented, JsonSerializerSettings);
			jsonFile = JsonConvert.SerializeObject(settingsData, Formatting.Indented);
		    jsonFile.WriteAllTextWithBackup(_configurationFilePath);
        }

		private class SettingsManagerContractResolver : DefaultContractResolver
		{
			protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
			{
				var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
					.Select(p => CreateProperty(p, memberSerialization))
					.ToList();
				props.ForEach(p =>
				{
					p.Writable = true;
					p.Readable = true;
				});

				return props;
			}
		}

		private static string ToCamelCase(string text)
			=> string.IsNullOrWhiteSpace(text)
				? string.Empty
				: $"{text[0].ToString().ToLowerInvariant()}{text.Substring(1)}";
    }
}

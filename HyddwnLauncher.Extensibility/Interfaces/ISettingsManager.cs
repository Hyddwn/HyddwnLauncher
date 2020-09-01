using System;

namespace HyddwnLauncher.Extensibility.Interfaces
{
	/// <summary>
	///		Represents an object capable of managing settings
	/// </summary>
	public interface ISettingsManager
	{
		/// <summary>
		///		Loads settings deserialized as the specified type
		/// </summary>
		/// <typeparam name="T">The type that the object should be deserialized as</typeparam>
		/// <returns></returns>
		T Load<T>() where T : class, new();

		/// <summary>
		///		Loads a section of the settings deserialized as the specified type
		/// </summary>
		/// <typeparam name="T">The type that the object should be deserialized as</typeparam>
		/// <returns></returns>
		T LoadSection<T>() where T : class, new();

		/// <summary>
		///		Loads settings
		/// </summary>
		/// <param name="type">The type that the object should be deserialized as</param>
		/// <returns></returns>
		object Load(Type type);

		/// <summary>
		///		Loads a section of the settings
		/// </summary>
		/// <param name="type">The type that the object should be deserialized as</param>
		/// <returns></returns>
		object LoadSection(Type type);

		/// <summary>
		///		Saves an object serialized as json
		/// </summary>
		/// <typeparam name="T">The type that the object should be serialized as</typeparam>
		/// <param name="typeObject">An instance of the type to serialize</param>
		void Save<T>(T typeObject) where T : class, new();

		/// <summary>
		///		Saves an object serialized as json as a section
		/// </summary>
		/// <typeparam name="T">The type that the object should be serialized as</typeparam>
		/// <param name="typeObject"></param>
		void SaveSection<T>(T typeObject) where T : class, new();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <param name="value"></param>
		void Save(Type type, object value);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <param name="value"></param>
		void SaveSection(Type type, object value);
	}
}

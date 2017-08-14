using System;
using System.Diagnostics;
using Microsoft.Win32;

namespace HyddwnLauncher.UOTiara.Util
{
    public class RegistryHelper
    {
        /// <summary>
        ///     A property to set the SubKey value
        ///     (default = "SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\UO Tiaras Moonshine Mod")
        /// </summary>
        public string SubKey { get; set; } =
            "SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\UO Tiaras Moonshine Mod";

        /// <summary>
        ///     A property to set the BaseRegistryKey value.
        ///     (default = Registry.LocalMachine)
        /// </summary>
        public RegistryKey BaseRegistryKey { get; set; } = Registry.LocalMachine;

        /// <summary>
        ///     To read a registry key.
        ///     input: KeyName (string)
        ///     output: value (string)
        /// </summary>
        public string Read(string KeyName)
        {
            // Opening the registry key
            var rk = BaseRegistryKey;
            // Open a subKey as read-only
            var sk1 = rk.OpenSubKey(SubKey);
            // If the RegistrySubKey doesn't exist -> (null)
            if (sk1 == null)
                return null;
            try
            {
                // If the RegistryKey exists I get its value
                // or null is returned.
                return (string) sk1.GetValue(KeyName.ToUpper());
            }
            catch (Exception e)
            {
                // AAAAAAAAAAARGH, an error!
                return null;
            }
        }

        /// <summary>
        /// Reads (Default) key as a string
        /// </summary>
        /// <returns></returns>
        public string Read()
        {
            // Opening the registry key
            var rk = BaseRegistryKey;
            // Open a subKey as read-only
            var sk1 = rk.OpenSubKey(SubKey);
            // If the RegistrySubKey doesn't exist -> (null)
            if (sk1 == null)
                return null;
            try
            {
                // If the RegistryKey exists I get its value
                // or null is returned.
                return (string)sk1.GetValue(null);
            }
            catch (Exception e)
            {
                // AAAAAAAAAAARGH, an error!
                return null;
            }
        }

        /// <summary>
        /// Reads key as string then converts to int
        /// </summary>
        public int ReadInt(string KeyName)
        {
            // Opening the registry key
            var rk = BaseRegistryKey;
            // Open a subKey as read-only
            var sk1 = rk.OpenSubKey(SubKey);
            // If the RegistrySubKey doesn't exist -> (null)
            if (sk1 == null)
                return default(int);
            try
            {
                // If the RegistryKey exists I get its value
                // or null is returned.
                var intString = (string)sk1.GetValue(KeyName.ToUpper());
                int value;
                return !int.TryParse(intString, out value) ? 0 : value;
            }
            catch (Exception e)
            {
                // AAAAAAAAAAARGH, an error!
                return default(int);
            }
        }

        /// <summary>
        /// Reads (Default) key value as int
        /// </summary>
        /// <returns></returns>
        public int ReadInt()
        {
            // Opening the registry key
            var rk = BaseRegistryKey;
            // Open a subKey as read-only
            var sk1 = rk.OpenSubKey(SubKey);
            // If the RegistrySubKey doesn't exist -> (null)
            if (sk1 == null)
                return default(int);
            try
            {
                // If the RegistryKey exists I get its value
                // or null is returned.
                var intString = (string)sk1.GetValue(null);
                int value;
                return !int.TryParse(intString, out value) ? 0 : value;
            }
            catch (Exception e)
            {
                // AAAAAAAAAAARGH, an error!
                return default(int);
            }
        }

        public T Read<T>(string KeyName)
        {
            // Opening the registry key
            var rk = BaseRegistryKey;
            // Open a subKey as read-only
            var sk1 = rk.OpenSubKey(SubKey);
            // If the RegistrySubKey doesn't exist -> (null)
            if (sk1 == null)
                return default(T);
            try
            {
                // If the RegistryKey exists I get its value
                // or null is returned.
                return (T)sk1.GetValue(KeyName.ToUpper());
            }
            catch (Exception e)
            {
                // AAAAAAAAAAARGH, an error!
                return default(T);
            }
        }

        /// <summary>
        /// Reads (Default) key value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Read<T>()
        {
            // Opening the registry key
            var rk = BaseRegistryKey;
            // Open a subKey as read-only
            var sk1 = rk.OpenSubKey(SubKey);
            // If the RegistrySubKey doesn't exist -> (null)
            if (sk1 == null)
                return default(T);
            try
            {
                // If the RegistryKey exists I get its value
                // or null is returned.
                return (T)sk1.GetValue(null);
            }
            catch (Exception e)
            {
                // AAAAAAAAAAARGH, an error!
                return default(T);
            }
        }

        /// <summary>
        ///     To write into a registry key.
        ///     input: KeyName (string) , Value (object)
        ///     output: true or false
        /// </summary>
        public bool Write(string KeyName, object Value)
        {
            try
            {
                // Setting
                var rk = BaseRegistryKey;
                // I have to use CreateSubKey 
                // (create or open it if already exits), 
                // 'cause OpenSubKey open a subKey as read-only
                var sk1 = rk.CreateSubKey(SubKey);
                // Save the value
                sk1.SetValue(KeyName.ToUpper(), Value);

                return true;
            }
            catch (Exception e)
            {
                // AAAAAAAAAAARGH, an error!
                return false;
            }
        }

        /// <summary>
        ///     To delete a registry key.
        ///     input: KeyName (string)
        ///     output: true or false
        /// </summary>
        public bool DeleteKey(string KeyName)
        {
            try
            {
                // Setting
                var rk = BaseRegistryKey;
                var sk1 = rk.CreateSubKey(SubKey);
                // If the RegistrySubKey doesn't exists -> (true)
                if (sk1 == null)
                    return true;
                sk1.DeleteValue(KeyName);

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        /// <summary>
        ///     To delete a sub key and any child.
        ///     input: void
        ///     output: true or false
        /// </summary>
        public bool DeleteSubKeyTree()
        {
            try
            {
                // Setting
                var rk = BaseRegistryKey;
                var sk1 = rk.OpenSubKey(SubKey);
                // If the RegistryKey exists, I delete it
                if (sk1 != null)
                    rk.DeleteSubKeyTree(SubKey);

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        /// <summary>
        ///     Retrive the count of subkeys at the current key.
        ///     input: void
        ///     output: number of subkeys
        /// </summary>
        public int SubKeyCount()
        {
            try
            {
                // Setting
                var rk = BaseRegistryKey;
                var sk1 = rk.OpenSubKey(SubKey);
                // If the RegistryKey exists...
                if (sk1 != null)
                    return sk1.SubKeyCount;
                return 0;
            }
            catch (Exception e)
            {
                // AAAAAAAAAAARGH, an error!
                return 0;
            }
        }

        /// <summary>
        ///     Retrive the count of values in the key.
        ///     input: void
        ///     output: number of keys
        /// </summary>
        public int ValueCount()
        {
            try
            {
                // Setting
                var rk = BaseRegistryKey;
                var sk1 = rk.OpenSubKey(SubKey);
                // If the RegistryKey exists...
                if (sk1 != null)
                    return sk1.ValueCount;
                return 0;
            }
            catch (Exception e)
            {
                return 0;
            }
        }
    }
}
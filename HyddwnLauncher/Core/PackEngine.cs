using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using HyddwnLauncher.Extensibility.Interfaces;
using HyddwnLauncher.Util;
using Ionic.Zip;
using MabinogiResource;

namespace HyddwnLauncher.Core
{
    public class PackEngine : IPackEngine
    {
        private static readonly string Assembly = System.Reflection.Assembly.GetExecutingAssembly().Location;
        private static readonly string Assemblypath = Path.GetDirectoryName(Assembly);

        /// <exception cref="ArgumentNullException"><paramref name="serverProfile" /> is <see langword="null" />.</exception>
        public bool BuildServerPack(ServerProfile serverProfile, int mabiVersion)
        {
            if (serverProfile == null) throw new ArgumentNullException(nameof(serverProfile));

            // No url means no pack url, we can return success and skip build
            if (string.IsNullOrWhiteSpace(serverProfile.PackDataUrl)) return true;

            var extractDirectory = $"{Assemblypath}\\PackData\\{serverProfile.Name}-{{{serverProfile.Guid}}}";
            var serverPackDataFile = $"{Assemblypath}\\PackData\\{serverProfile.Guid}.zip";
            var serverPackDataPath = extractDirectory + "\\data";
            var packFileName = $"package\\hl_{serverProfile.Name}-{serverProfile.Guid}.pack";

            if (!Directory.Exists(extractDirectory))
                Directory.CreateDirectory(extractDirectory);

            // Download the packdata from the url provided
            using (var wc = new WebClient())
            {
                try
                {
                    wc.DownloadFile(serverProfile.PackDataUrl, serverPackDataFile);
                }
                catch (ArgumentNullException)
                {
                    Log.Error("No pack data url specified, pack aborted");
                    return false;
                }
                catch (Exception ex)
                {
                    Log.Exception(ex, "Unable to download pack file data, pack aborted");
                    return false;
                }
            }

            try
            {
                // Extract the pack data
                using (var zipFile = ZipFile.Read(serverPackDataFile))
                {
                    zipFile.ExtractExistingFile = ExtractExistingFileAction.OverwriteSilently;
                    zipFile.ExtractAll(extractDirectory);
                }
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "Failed to extract pack data, pack aborted.");
                return false;
            }

            var textString = "";

            //Edit Urls.xml
            try
            {
                using (var fs = new FileStream(extractDirectory + "\\data\\db\\urls.xml", FileMode.Open,
                    FileAccess.Read))
                using (var sr = new StreamReader(fs))
                {
                    textString = sr.ReadToEnd();

                    textString = serverProfile.UrlsXmlOptions[0].Aggregate(textString,
                        (current, urlsXmlOption) =>
                            current.Replace($"=={urlsXmlOption.Key}==",
                                urlsXmlOption.Value.Replace("[WebPort]", serverProfile.WebPort.ToString())
                                    .Replace("[WebHost]", serverProfile.WebHost)));

                    sr.Close();
                }

                File.WriteAllText(extractDirectory + "\\data\\db\\urls.xml", textString, Encoding.Unicode);
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "Failed to extract pack data, pack aborted.");
                return false;
            }

            var version = (uint)mabiVersion;

            Pack(serverPackDataPath, packFileName, ++version);

            TryDeleteDirectory(extractDirectory);

            return true;
        }

        public void Pack(string inputDir, string outputFile, uint version, int level = 9)
        {
            if (File.Exists(outputFile))
                File.Delete(outputFile);

            var files = Directory.GetFiles(inputDir, "*", SearchOption.AllDirectories);
            Array.Sort(files);

            using (var pack = new PackResourceSetCreater(version, level))
            {
                foreach (var filePath in files)
                {
                    var fileName = filePath.Replace(inputDir + "\\", "");
                    pack.AddFile(fileName, filePath);
                }

                pack.CreatePack(outputFile);
            }
        }

        public static void TryDeleteDirectory(string p)
        {
            try
            {
                Directory.Delete(p, true);
            }
            catch (Exception ex)
            {
                Log.Warning("Can't delete {0}: {1}", p, ex.Message);
            }
        }
    }
}
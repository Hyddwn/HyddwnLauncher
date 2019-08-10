using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace HyddwnLauncher.Util.Helpers
{
    public class BrowserHelper
    {
        public static string GetDefaultBrowserPath()
        {
            const string urlAssociation = @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http";
            const string browserPathKey = @"$BROWSER$\shell\open\command";

            try
            {
                //Read default browser path from userChoiceLKey
                var userChoiceKey = Registry.CurrentUser.OpenSubKey(urlAssociation + @"\UserChoice", false);

                //If user choice was not found, try machine default
                if (userChoiceKey == null)
                {
                    //Read default browser path from Win XP registry key
                    var browserKey = Registry.ClassesRoot.OpenSubKey(@"HTTP\shell\open\command", false) ?? Registry.CurrentUser.OpenSubKey(
                                         urlAssociation, false);

                    //If browser path wasn’t found, try Win Vista (and newer) registry key
                    var path = CleanBrowserPath(browserKey.GetValue(null) as string);
                    browserKey.Close();
                    return path;
                }
                else
                {
                    // user defined browser choice was found
                    var progId = (userChoiceKey.GetValue("ProgId").ToString());
                    userChoiceKey.Close();

                    // now look up the path of the executable
                    var concreteBrowserKey = browserPathKey.Replace("$BROWSER$", progId);
                    var kp = Registry.ClassesRoot.OpenSubKey(concreteBrowserKey, false);
                    var browserPath = CleanBrowserPath(kp.GetValue(null) as string);
                    kp.Close();
                    return browserPath;
                }
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "Failed to acquire the default browser.");
                return "";
            }
        }

        private static string CleanBrowserPath(string p)
        {
            var url = p.Split('"');
            var clean = url[1];
            return clean;
        }
    }
}

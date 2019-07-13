using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HyddwnLauncher.Util;

namespace HyddwnLauncher.Core
{
    public class MabiLocalizationData
    {
        public string Name { get; set; }
        public string TranslatedName => Name?.LocalizedLocalization();
    }
}

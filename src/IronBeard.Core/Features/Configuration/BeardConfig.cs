using System.Collections.Generic;

namespace IronBeard.Core.Features.Configuration
{
    public class BeardConfig
    {
        public string SiteTitle { get; set; } = "My IrondBeard Site";
        public string IndexFileName { get; set; } = "Index";
        public string LayoutFileName { get; set; } = "_Layout";
        public List<string> StaticExtensionIgnoreList { get; set; } = new List<string> { ".cshtml", ".md", ".DS_Store", ".json" };
    }
}
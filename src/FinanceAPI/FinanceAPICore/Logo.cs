using System.Collections.Generic;

namespace FinanceAPICore
{
    public class Logo
    {
        public string Url { get; set; }
        public bool ForceOverride { get; set; } = false;
        public List<string> Types { get; set; } = new List<string>();

        public Logo(){}

        public Logo(string url, bool forceOverride, List<string> types)
        {
            Url = url;
            ForceOverride = forceOverride;
            Types = types;
        }
    }
}
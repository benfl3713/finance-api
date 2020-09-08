using FinanceAPIData.Datafeeds.APIs;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceAPIData.Datafeeds
{
    public static class DatafeedManager
    {
        public static IDatafeedAPI ResolveApiType(string datafeedId)
        {
            Type datafeedType = datafeedApis[datafeedId];
            if (datafeedType == null)
                return null;

            return (IDatafeedAPI)Activator.CreateInstance(datafeedType);
        }

        private static Dictionary<string, Type> datafeedApis = new Dictionary<string, Type>
        {
            { "TRUELAYER", typeof(TrueLayerAPI) }
        };
    }
}

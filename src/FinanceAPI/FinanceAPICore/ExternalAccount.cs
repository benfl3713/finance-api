using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceAPICore
{
    public class ExternalAccount
    {
        public string AccountID;
        public string AccountName;
        public string VendorID;
        public string VendorName;
        public string Provider;
        public bool Mapped;
        public string MappedAccount;

        public ExternalAccount(string ID, string accountName, string vendorId, string vendorName, string provider, bool mapped, string mappedAccount = null)
        {
            AccountID = ID;
            AccountName = accountName;
            VendorID = vendorId;
            VendorName = vendorName;
            Provider = provider;
            Mapped = mapped;
            MappedAccount = mappedAccount;
        }
    }
}

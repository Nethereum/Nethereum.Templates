using System;
using System.Collections.Generic;
using System.Text;

using Wonka.MetaData;

namespace WonkaDeFi.Wonka
{
    class WonkaDeFiDefaultMetadata : IMetadataRetrievable
    {
        public WonkaDeFiDefaultMetadata()
        { }

        #region Standard Metadata Cache (Minimum Set)

        public List<WonkaRefAttr> GetAttrCache()
        {
            List<WonkaRefAttr> AttrCache = new List<WonkaRefAttr>();

            AttrCache.Add(new WonkaRefAttr() { AttrId = 1, AttrName = "AccountCurrValue", FieldId = 1, GroupId = 1, IsAudited = true, IsDecimal = true });
            AttrCache.Add(new WonkaRefAttr() { AttrId = 2, AttrName = "VaultYieldRate", FieldId = 2, GroupId = 1, IsAudited = true, IsDecimal = true });
            AttrCache.Add(new WonkaRefAttr() { AttrId = 3, AttrName = "TokenType", FieldId = 3, GroupId = 1, IsAudited = true, MaxLength = 3 });
            AttrCache.Add(new WonkaRefAttr() { AttrId = 4, AttrName = "VaultAddress", FieldId = 4, GroupId = 1, IsAudited = true, MaxLength = 44 });
            AttrCache.Add(new WonkaRefAttr() { AttrId = 5, AttrName = "AccountThreshold", FieldId = 5, GroupId = 1, IsAudited = true, IsNumeric = true });
            AttrCache.Add(new WonkaRefAttr() { AttrId = 6, AttrName = "TokenTransferAmt", FieldId = 6, GroupId = 1, IsAudited = true, IsNumeric = true });
            AttrCache.Add(new WonkaRefAttr() { AttrId = 7, AttrName = "OwnerAddress", FieldId = 7, GroupId = 1, IsAudited = true, MaxLength = 44 });
            AttrCache.Add(new WonkaRefAttr() { AttrId = 8, AttrName = "Result", FieldId = 8, GroupId = 1, IsAudited = true, MaxLength = 32 });

            return AttrCache;
        }

        public List<WonkaRefCurrency> GetCurrencyCache()
        {
            List<WonkaRefCurrency> CurrencyCache = new List<WonkaRefCurrency>();

            CurrencyCache.Add(new WonkaRefCurrency() { CurrencyId = 1, CurrencyCd = "USD", USDCost = 1, USDList = 1 });
            CurrencyCache.Add(new WonkaRefCurrency() { CurrencyId = 2, CurrencyCd = "EUR", USDCost = 1.24f, USDList = 1.24f });
            CurrencyCache.Add(new WonkaRefCurrency() { CurrencyId = 3, CurrencyCd = "CNY", USDCost = 0.16f, USDList = 0.16f });
            CurrencyCache.Add(new WonkaRefCurrency() { CurrencyId = 4, CurrencyCd = "BTC", USDCost = 18722.73f, USDList = 18722.73f });

            return CurrencyCache;
        }

        public List<WonkaRefCadre> GetCadreCache()
        {
            List<WonkaRefCadre> CadreCache = new List<WonkaRefCadre>();

            // NOT NEEDED FOR NOW

            return CadreCache;
        }

        public List<WonkaRefGroup> GetGroupCache()
        {
            List<WonkaRefGroup> GroupCache = new List<WonkaRefGroup>();

            GroupCache.Add(new WonkaRefGroup() { GroupId = 1, GroupName = "TransactionData", Description = "All data needed for the transaction" });

            return GroupCache;
        }

        public List<WonkaRefSource> GetSourceCache()
        {
            List<WonkaRefSource> SourceCache = new List<WonkaRefSource>();

            // NOT NEEDED FOR NOW

            return SourceCache;
        }

        public List<WonkaRefSourceCadre> GetSourceCadreCache()
        {
            List<WonkaRefSourceCadre> SourceCadreCache = new List<WonkaRefSourceCadre>();

            // NOT NEEDED FOR NOW

            return SourceCadreCache;
        }

        public List<WonkaRefStandard> GetStandardCache()
        {
            List<WonkaRefStandard> StandardCache = new List<WonkaRefStandard>();

            // NOT NEEDED FOR NOW

            return StandardCache;
        }

        #endregion

        #region Extended Metadata Cache

        public List<WonkaRefAttrCollection> GetAttrCollectionCache()
        {
            List<WonkaRefAttrCollection> AttrCollCache = new List<WonkaRefAttrCollection>();

            // NOT NEEDED FOR NOW

            return AttrCollCache;
        }

        #endregion
    }
}

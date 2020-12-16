using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Text;

using Nethereum.Web3.Accounts;

using Wonka.BizRulesEngine;
using Wonka.BizRulesEngine.Reporting;
using Wonka.BizRulesEngine.RuleTree;
using Wonka.Eth.Init;
using Wonka.Eth.Extensions;
using Wonka.MetaData;
using Wonka.Product;
using Wonka.Storage.Extensions;

namespace WonkaDeFi.Wonka
{
    public partial class WonkaERC20Service
    {

        public const string CONST_DEFAULT_WONKA_DEFI_RULES =
@"<?xml version=""1.0""?>
<RuleTree xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">

   <if description=""Move Tokens to Vault"">
      <criteria op=""AND"">
         <eval id=""pop1"">(N.OwnerAddress) POPULATED</eval>
		 <eval id=""arl1"">(N.AccountCurrValue) ERC20_GET_BALANCE (N.OwnerAddress)</eval>
      </criteria>

      <if description="""">
           <criteria op=""OR"">
               <eval id=""chk1"">(O.VaultYieldRate) GT (0.1)</eval>
               <eval id=""chk2"">(N.AccountCurrValue) GE (N.TokenTransferAmt)</eval>
           </criteria>

           <validate err=""severe"">
               <criteria op=""AND"">
                   <eval id=""tfr1"">(N.Result) ERC20_TRANSFER (N.VaultAddress, N.TokenTransferAmt)</eval>
               </criteria>

               <failure_message/>
               <success_message/>
           </validate>           
       </if>

   </if>    
    
</RuleTree>
";

		public const string CONST_INFURA_IPFS_GATEWAY_URL       = "https://ipfs.infura.io/ipfs";
        public const string CONST_INFURA_IPFS_WRITE_GATEWAY_URL = "https://ipfs.infura.io:5001";

		public const string CONST_CONTRACT_FUNCTION_EXEC_RPT     = "executeWithReport";
        public const string CONST_CONTRACT_FUNCTION_GET_LAST_RPT = "getLastRuleReport";

		private readonly bool mbInitChainEnv;

		private string msContractAddress;

        private string                             msRulesContents  = null;
        private IMetadataRetrievable               moMetadataSource = null;
        private Dictionary<string, WonkaBizSource> moSourceMap      = null;

        private string msSenderAddress           = "";
        private string msPassword                = "";

		private WonkaEthEngineInitialization moEthEngineInit = null;

		public WonkaERC20Service(string psSenderAddress, 
                                 string psPassword, 
                                 string psContractAddress,
                                 string psRulesContentsHttpUrl,
                   IMetadataRetrievable poMetadata = null,
     Dictionary<string, WonkaBizSource> poSourceMap = null,
                                   bool pbInitChainEnv = false)
        {
            msSenderAddress = psSenderAddress;
            msPassword      = psPassword;

			mbInitChainEnv     = pbInitChainEnv;
			msContractAddress  = psContractAddress;

            // Create an instance of the class that will provide us with PmdRefAttributes (i.e., the data domain)
            // that define our data record, if one is not provided
            if (poMetadata != null)
                moMetadataSource = poMetadata;

            moSourceMap = poSourceMap;

            if (!String.IsNullOrEmpty(psRulesContentsHttpUrl))
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    msRulesContents = client.GetStringAsync(psRulesContentsHttpUrl).Result;
                }
            }

            WonkaRefEnvironment.CreateInstance(false, moMetadataSource);

            var bInitSuccess = Init().Result;
		}

        public WonkaERC20Service(string psSenderAddress,
                                 string psPassword,
                                 string psContractAddress,
                          StringBuilder poRulesContentsBuilder = null,
                   IMetadataRetrievable poMetadata = null,
     Dictionary<string, WonkaBizSource> poSourceMap = null,
                                   bool pbInitChainEnv = false)
        {
            msSenderAddress = psSenderAddress;
            msPassword      = psPassword;

			mbInitChainEnv     = pbInitChainEnv;
			msContractAddress  = psContractAddress;

            // Create an instance of the class that will provide us with PmdRefAttributes (i.e., the data domain)
            // that define our data record, if one is not provided
            if (poMetadata != null)
                moMetadataSource = poMetadata;

            moSourceMap = poSourceMap;

            if ((poRulesContentsBuilder != null) && (poRulesContentsBuilder.Length > 0))
            {
                msRulesContents = poRulesContentsBuilder.ToString();
            }

            var bInitSuccess = Init().Result;
		}

		private async Task<bool> Init()
		{
			bool bResult = true;

            if (String.IsNullOrEmpty(msRulesContents))
                msRulesContents = CONST_DEFAULT_WONKA_DEFI_RULES;

            if (moMetadataSource == null)
                moMetadataSource = new WonkaDeFiDefaultMetadata();

            WonkaRefEnvironment.CreateInstance(false, moMetadataSource);

            await InitEngineAsync(mbInitChainEnv).ConfigureAwait(false);

            return bResult;
		}

        public bool Execute(WonkaProduct poNewTrxRecord)
        {
			bool bResult = false;

			var RulesEngine = moEthEngineInit.Engine.RulesEngine;

            // Validate that the .NET implementation and the rules markup are both working properly
            WonkaBizRuleTreeReport Report = RulesEngine.Validate(poNewTrxRecord);

            if (Report.OverallRuleTreeResult == ERR_CD.CD_SUCCESS)
                bResult = true;
            else if (Report.GetRuleSetFailureCount() > 0)
            {
                bResult = false;
                // NOTE: Notification about the error should be returned
            }
            else
                bResult = false;

			return bResult;
        }

        // NOTE: Not needed for now
        public Dictionary<string, WonkaBizSource> GetDefaultSourceMap()
        {
            Dictionary<string, WonkaBizSource> SourceMap = new Dictionary<string, WonkaBizSource>();

            /*
            WonkaBizSource TempSource =
                new WonkaBizSource(poEngineInitData.StorageDefaultSourceId,
                                   poEngineInitData.EthSenderAddress,
                                   poEngineInitData.EthPassword,
                                   poEngineInitData.StorageContractAddress,
                                   poEngineInitData.StorageContractABI,
                                   poEngineInitData.StorageGetterMethod,
                                   poEngineInitData.StorageSetterMethod,
                                   EngineProps.DotNetRetrieveMethod);
            */

            return SourceMap;
        }

        private async Task<bool> InitEngineAsync(bool pbInitChainEnv)
        {
			bool bResult = false;

			moEthEngineInit = new WonkaEthEngineInitialization();

            moEthEngineInit.Engine.MetadataSource       = moMetadataSource;
			moEthEngineInit.Engine.RulesMarkupXml       = msRulesContents;
			moEthEngineInit.Engine.DotNetRetrieveMethod = RetrieveValueMethod;
			moEthEngineInit.EthSenderAddress            = moEthEngineInit.EthRuleTreeOwnerAddress = msSenderAddress;
			moEthEngineInit.EthPassword                 = msPassword;
			// moEthEngineInit.Web3HttpUrl                 = CONST_ONLINE_TEST_CHAIN_URL;
            moEthEngineInit.ERC20ContractAddress        = msContractAddress;

            if ((moSourceMap == null) || (moSourceMap.Count <= 0))
                moSourceMap = GetDefaultSourceMap();

            moEthEngineInit.Engine.RulesEngine =
                new WonkaEthRulesEngine(new StringBuilder(msRulesContents),
                                        moSourceMap,
                                        moEthEngineInit,
                                        moMetadataSource,
                                        false);

			await moEthEngineInit.InitEngineAsync().ConfigureAwait(false);

            /* 
             * NOTE: Not needed for now
			// Serialize the data domain to the blockchain
			if (pbInitChainEnv)
            {
				await moEthEngineInit.SerializeAsync().ConfigureAwait(false);
            }
             */

			return bResult;
        }

        public string RetrieveValueMethod(WonkaBizSource poTargetSource, string psAttrName)
        {
            string sValue = "";

            if (psAttrName != "VaultYieldRate")
            {
                sValue = poTargetSource.GetAttrValue(psAttrName);
            }
            // NOTE: Only useful in case of demo
            else
            {
                sValue = "0.12";
            }

            return sValue;
		}

    }
}

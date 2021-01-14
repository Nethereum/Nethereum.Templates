using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using Nethereum.Web3.Accounts;
using Nethereum.Web3;
using Nethereum.Contracts;

using Wonka.MetaData;
using Wonka.Product;

using WonkaDeFi.Contracts.ERC20Token;
using WonkaDeFi.Contracts.ERC20Token.ContractDefinition;
using WonkaDeFi.Wonka;

namespace WonkaDeFiConsoleApp
{
    class Program
    {
        public const string CONST_ONLINE_TEST_CHAIN_URL = "http://testchain.nethereum.com:8545";

        public const string CONST_WONKA_DEFI_RULES =
@"<?xml version=""1.0""?>
<RuleTree xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">

   <if description=""Move Tokens to Vault"">
      <criteria op=""AND"">
         <eval id=""pop1"">(N.OwnerAddress) POPULATED</eval>
		 <eval id=""arl1"">(N.AccountCurrValue) ERC20_GET_BALANCE (N.OwnerAddress)</eval>
      </criteria>

      <if description=""Check for greater than 0.1"">
           <criteria op=""OR"">
               <eval id=""chk1"">(N.VaultYieldRate) GT (0.1)</eval>
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

      <if description=""Check for greater than 0.2"">
           <criteria op=""OR"">
               <eval id=""chk1"">(N.VaultYieldRate) GT (0.2)</eval>
               <eval id=""chk2"">(N.AccountCurrValue) GE (N.TokenTransferAmt)</eval>
           </criteria>

           <validate err=""severe"">
               <criteria op=""AND"">
                   <eval id=""tfr2"">(N.Result) ERC20_TRANSFER (N.VaultAddress, N.TokenTransferAmt)</eval>
                   <eval id=""tfr3"">(N.Result) ERC20_TRANSFER (N.VaultAddress, N.TokenTransferAmt)</eval>
               </criteria>

               <failure_message/>
               <success_message/>
           </validate>           
      </if>

   </if>    
    
</RuleTree>
";

        private static Random RANDO = new Random();

        static void Main(string[] args)
        {
            var ownerAddress       = "0x12890d2cce102216644c59daE5baed380d84830c";
            var password           = "0xb5b1870957d373ef0eeffecc6e4812c0fd08f554b37b233526acc331bf1544f7";
            var destinationAddress = "0x6C547791C3573c2093d81b919350DB1094707011";

            var web3 = GetWeb3(ownerAddress, password, CONST_ONLINE_TEST_CHAIN_URL);

            var erc20TokenDeployment = 
                new ERC20TokenDeployment() { DecimalUnits = 18, TokenName = "TST", TokenSymbol = "TST", InitialAmount = Web3.Convert.ToWei(10000) };

            //Deploy our custom token
            var tokenDeploymentReceipt = ERC20TokenService.DeployContractAndWaitForReceiptAsync(web3, erc20TokenDeployment).Result;

            //Creating a new service
            var tokenService = new ERC20TokenService(web3, tokenDeploymentReceipt.ContractAddress);

            //Creating the rules engine, using the default set of rules
            var wonkaERC20Service =
                new WonkaERC20Service(ownerAddress,
                                      password,
                                      tokenDeploymentReceipt.ContractAddress,
                                      new StringBuilder(CONST_WONKA_DEFI_RULES),
                                      new WonkaDeFiDefaultMetadata(),
                                      null,
                                      false,
                                      CONST_ONLINE_TEST_CHAIN_URL);

            var RefEnv = WonkaRefEnvironment.GetInstance();

            WonkaRefAttr OwnerAddressAttr   = RefEnv.GetAttributeByAttrName("OwnerAddress");
            WonkaRefAttr VaultAddressAttr   = RefEnv.GetAttributeByAttrName("VaultAddress");
            WonkaRefAttr TokenTrxAmtAttr    = RefEnv.GetAttributeByAttrName("TokenTransferAmt");
            WonkaRefAttr VaultYieldRateAttr = RefEnv.GetAttributeByAttrName("VaultYieldRate");

            var trxData = new WonkaProduct();
            trxData.SetAttribute(OwnerAddressAttr, ownerAddress);
            trxData.SetAttribute(VaultAddressAttr, destinationAddress);
            trxData.SetAttribute(TokenTrxAmtAttr,  "12"); // Must specify the amount in hex form

            var currentBalance = tokenService.BalanceOfQueryAsync(ownerAddress).Result;

            for (int i=0; (i < 1000) && (currentBalance.CompareTo(0) > 0); ++i)
            {
                trxData.SetAttribute(VaultYieldRateAttr, GetCurrentInterestRate());

                wonkaERC20Service.Execute(trxData);

                currentBalance = tokenService.BalanceOfQueryAsync(ownerAddress).Result;

                var vaultBalance = tokenService.BalanceOfQueryAsync(destinationAddress).Result;

                Thread.Sleep(5000);
            }
        }

        public static string GetCurrentInterestRate()
        {
            string sInterestRate = "";

            var RandoNum = RANDO.Next();

            if ((RandoNum % 3) == 0)
                sInterestRate = "0.21";
            else if ((RandoNum % 2) == 0)
                sInterestRate = "0.11";
            else
                sInterestRate = "0.01";

            return sInterestRate;
        }

        public static Nethereum.Web3.Web3 GetWeb3(string psAccount, string psPassword, string psChainHttpUrl = "")
        {
            var account = new Account(psPassword);

            var web3 = 
                !String.IsNullOrEmpty(psChainHttpUrl) ? new Nethereum.Web3.Web3(account, psChainHttpUrl) : new Nethereum.Web3.Web3(account);

            return web3;
        }

    }
}

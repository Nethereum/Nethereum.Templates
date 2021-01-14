using System.Numerics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

using Nethereum.Signer;
using Nethereum.Util;
using Nethereum.Web3.Accounts;
using Nethereum.XUnitEthereumClients;
using Nethereum.Web3;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.BlockchainProcessing.ProgressRepositories;
using Wonka.MetaData;
using Wonka.Product;
using Xunit;
 
using WonkaDeFi.Contracts.ERC20Token;
using WonkaDeFi.Contracts.ERC20Token.ContractDefinition;
using WonkaDeFi.Wonka;

namespace WonkaDeFi.Testing
{
    [Collection(EthereumClientIntegrationFixture.ETHEREUM_CLIENT_COLLECTION_DEFAULT)]
    public class WonkaErc20Tests
    {
        private readonly EthereumClientIntegrationFixture _ethereumClientIntegrationFixture;

        public WonkaErc20Tests(EthereumClientIntegrationFixture ethereumClientIntegrationFixture)
        {
            _ethereumClientIntegrationFixture = ethereumClientIntegrationFixture;
        }

        [Fact]
        public async void ShouldTransferToken()
        {

            var destinationAddress = "0x6C547791C3573c2093d81b919350DB1094707011";
            //Using ropsten infura if wanted for only a tests
            //var web3 = _ethereumClientIntegrationFixture.GetInfuraWeb3(InfuraNetwork.Ropsten);
            var web3 = _ethereumClientIntegrationFixture.GetWeb3();

            var erc20TokenDeployment = new ERC20TokenDeployment() { DecimalUnits = 18, TokenName = "TST", TokenSymbol = "TST", InitialAmount = Web3.Convert.ToWei(10000) };

            //Deploy our custom token
            var tokenDeploymentReceipt = await ERC20TokenService.DeployContractAndWaitForReceiptAsync(web3, erc20TokenDeployment);
            
            //Creating a new service
            var tokenService = new ERC20TokenService(web3, tokenDeploymentReceipt.ContractAddress);

            //using Web3.Convert.ToWei as it has 18 decimal places (default)
            var transferReceipt = await tokenService.TransferRequestAndWaitForReceiptAsync(destinationAddress, Web3.Convert.ToWei(10, 18));
            
            //validate the current balance
            var balance = await tokenService.BalanceOfQueryAsync(destinationAddress);
            Assert.Equal(10, Web3.Convert.FromWei(balance));

            //retrieving the event from the receipt
            var eventTransfer = transferReceipt.DecodeAllEvents<TransferEventDTO>()[0];

            Assert.Equal(10, Web3.Convert.FromWei(eventTransfer.Event.Value));
            Assert.True(destinationAddress.IsTheSameAddress(eventTransfer.Event.To));
        }


        [Fact]
        public async void ShouldGetTransferEventLogs()
        {

            var destinationAddress = "0x6C547791C3573c2093d81b919350DB1094707011";
            //Using ropsten infura if wanted for only a tests
            //var web3 = _ethereumClientIntegrationFixture.GetInfuraWeb3(InfuraNetwork.Ropsten);
            var web3 = _ethereumClientIntegrationFixture.GetWeb3();

            var erc20TokenDeployment = new ERC20TokenDeployment() { DecimalUnits = 18, TokenName = "TST", TokenSymbol = "TST", InitialAmount = Web3.Convert.ToWei(10000) };

            //Deploy our custom token
            var tokenDeploymentReceipt = await ERC20TokenService.DeployContractAndWaitForReceiptAsync(web3, erc20TokenDeployment);
            
            //Creating a new service
            var tokenService = new ERC20TokenService(web3, tokenDeploymentReceipt.ContractAddress);

            //using Web3.Convert.ToWei as it has 18 decimal places (default)
            var transferReceipt1 = await tokenService.TransferRequestAndWaitForReceiptAsync(destinationAddress, Web3.Convert.ToWei(10, 18));
            var transferReceipt2 = await tokenService.TransferRequestAndWaitForReceiptAsync(destinationAddress, Web3.Convert.ToWei(10, 18));

            var transferEvent = web3.Eth.GetEvent<TransferEventDTO>();
            var transferFilter = transferEvent.GetFilterBuilder().AddTopic(x => x.To, destinationAddress).Build(tokenService.ContractHandler.ContractAddress,
                new BlockRange(transferReceipt1.BlockNumber, transferReceipt2.BlockNumber));
         
            var transferEvents = await transferEvent.GetAllChanges(transferFilter);

            Assert.Equal(2, transferEvents.Count); 

        }


        [Fact]
        public async void ShouldInvokeERC20WonkaService()
        {
            var ownerAddress       = "0x12890d2cce102216644c59daE5baed380d84830c";
            var destinationAddress = "0x6C547791C3573c2093d81b919350DB1094707011";

            //Using ropsten infura if wanted for only a tests
            //var web3 = _ethereumClientIntegrationFixture.GetInfuraWeb3(InfuraNetwork.Ropsten);
            var web3 = _ethereumClientIntegrationFixture.GetWeb3();

            var erc20TokenDeployment = new ERC20TokenDeployment() { DecimalUnits = 18, TokenName = "TST", TokenSymbol = "TST", InitialAmount = Web3.Convert.ToWei(10000) };

            //Deploy our custom token
            var tokenDeploymentReceipt = await ERC20TokenService.DeployContractAndWaitForReceiptAsync(web3, erc20TokenDeployment);

            //Creating a new service
            var tokenService = new ERC20TokenService(web3, tokenDeploymentReceipt.ContractAddress);

            //Creating the rules engine, using the default set of rules
            var wonkaERC20Service =
                new WonkaERC20Service("0x12890d2cce102216644c59daE5baed380d84830c",
                                      "0xb5b1870957d373ef0eeffecc6e4812c0fd08f554b37b233526acc331bf1544f7",
                                      tokenDeploymentReceipt.ContractAddress);

            var RefEnv = WonkaRefEnvironment.GetInstance();

            WonkaRefAttr OwnerAddressAttr = RefEnv.GetAttributeByAttrName("OwnerAddress");
            WonkaRefAttr VaultAddressAttr = RefEnv.GetAttributeByAttrName("VaultAddress");
            WonkaRefAttr TokenTrxAmtAttr  = RefEnv.GetAttributeByAttrName("TokenTransferAmt");

            var trxData = new WonkaProduct();
            trxData.SetAttribute(OwnerAddressAttr, ownerAddress);
            trxData.SetAttribute(VaultAddressAttr, destinationAddress);
            trxData.SetAttribute(TokenTrxAmtAttr,  "12"); // Must specify the amount in hex form

            var ownerBalance = await tokenService.BalanceOfQueryAsync(ownerAddress);

            var beforeBalance = await tokenService.BalanceOfQueryAsync(destinationAddress);

            wonkaERC20Service.Execute(trxData);

            //validate the current balance (in decimal form)
            var afterBalance = await tokenService.BalanceOfQueryAsync(destinationAddress);
            Assert.Equal(18, afterBalance);
        }

    }
}
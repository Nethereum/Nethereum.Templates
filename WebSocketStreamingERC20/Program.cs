using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.JsonRpc.WebSocketStreamingClient;
using Nethereum.RPC.Reactive.Eth.Subscriptions;
using Nethereum.Web3;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace WebSocketStreamingERC20
{
    class Program
    {
        public static async Task Main()
        {
            //DAI contract address
            var contractAddress = "0x6b175474e89094c44da98b954eedeac495271d0f";

            using (var client = new StreamingWebSocketClient("wss://mainnet.infura.io/ws/v3/7238211010344719ad14a89db874158c"))
            {

                try { 
                    var eventSubscription = new EthLogsObservableSubscription(client);
                    eventSubscription.GetSubscriptionDataResponsesAsObservable().Subscribe(log =>
                    {
                        var transfer = log.DecodeEvent<TransferEventDTO>();
                        Console.WriteLine($@"{transfer.Log.TransactionHash}, From:{transfer.Event.From}, To:{transfer.Event.To}, Value:{Web3.Convert.FromWei(transfer.Event.Value)}");
                    } );

                    eventSubscription.GetSubscribeResponseAsObservable().Subscribe(id => Console.WriteLine($"Subscribed with id: {id}"));

                    var filterAuction = Event<TransferEventDTO>.GetEventABI().CreateFilterInput(contractAddress);

                    await client.StartAsync();

                    await eventSubscription.SubscribeAsync(filterAuction);

                    Console.ReadLine();

                    await eventSubscription.UnsubscribeAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }


    [Event("Transfer")]
    public class TransferEventDTO : IEventDTO
    {
        [Parameter("address", "_from", 1, true)]
        public string From { get; set; }

        [Parameter("address", "_to", 2, true)]
        public string To { get; set; }

        [Parameter("uint256", "_value", 3, false)]
        public BigInteger Value { get; set; }
    }
}

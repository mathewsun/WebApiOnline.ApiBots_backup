using Binance.Net;
using Binance.Net.Objects;

using CryptoExchange.Net.Authentication;

using System;
using System.Threading.Tasks;

using WebApiOnline.ApiBots.Services;

namespace WebApiOnline.ApiBots
{
    class Program
    {
        static void Main(string[] args)
        {
            BinanceSocketClient.SetDefaultOptions(new BinanceSocketClientOptions()
            {
                ApiCredentials = new ApiCredentials(
                    "3WVntMaeHayWsLCrXh4pnzvM3xnqjiO89hSrquoJfP0MTfI4bDHRnKDfNXhPKAS2",
                    "tIcwcg1AI3If1NQkyMGltYQtAALKhh1mieEzgCwPw2UnurP5821tdCWtQw2Wmgsc")
            });

            OrdersService orderService = new OrdersService();

            orderService.StartSendingOrders();

            Task.Run(async() =>
            {
                while (true)
                {
                    Console.Clear();

                    await Task.Delay(10000);
                }
            });
           

            Console.ReadLine();
        }
    }
}

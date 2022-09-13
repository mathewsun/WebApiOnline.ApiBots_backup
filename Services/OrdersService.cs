using Binance.Net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Binance.Net.Enums;
using Flurl.Http;
using WebApiOnline.ApiBots.Helpers;
using WebApiOnline.ApiBots.Models;
using WebApiOnline.ApiBots.Models.Constants;

namespace WebApiOnline.ApiBots.Services
{
    public class OrdersService
    {
        private readonly Random rnd;
        
        private List<PairModel> pairs = new()
        {
            new PairModel()
            {
                PairName = "BTCUSDT",
                AmountFrom = 0.000001m,
                AmountTo = 0.00001m
            },
            new PairModel()
            {
                PairName = "ETHUSDT",
                AmountFrom = 0.00001m,
                AmountTo = 0.0001m
            },
            //new PairModel()
            //{
            //    PairName = "DOGEUSDT",
            //    AmountFrom = 0.1m,
            //    AmountTo = 0.99m
            //},
            new PairModel()
            {
                PairName = "LTCUSDT",
                AmountFrom = 0.001m,
                AmountTo = 0.01m
            },
            new PairModel()
            {
                PairName = "DASHUSDT",
                AmountFrom = 0.001m,
                AmountTo = 0.01m
            },
            new PairModel()
            {
                PairName = "ETHBTC",
                AmountFrom = 0.00001m,
                AmountTo = 0.00024m
            },
            new PairModel()
            {
                PairName = "LTCBTC",
                AmountFrom = 0.00001m,
                AmountTo = 0.00024m
            }
            //,
            //new PairModel()
            //{
            //    PairName = "DASHBTC",
            //    AmountFrom = 0.00001m,
            //    AmountTo = 0.0001m
            //},
            //new PairModel()
            //{
            //    PairName = "BCHUSDT",
            //    AmountFrom = 0.0001m,
            //    AmountTo = 0.001m
            //},
            //new PairModel()
            //{
            //    PairName = "BCHBTC",
            //    AmountFrom = 0.0001m,
            //    AmountTo = 0.001m
            //},
            //new PairModel() {
            //    PairName = "DOGEBTC",
            //    AmountFrom = 0.1m,
            //    AmountTo = 16m
            //}
        };

        public Dictionary<string, OrderModel> newOrders = new();

        public OrdersService()
        {
            rnd = new Random();
            
            //Создание сокет клиента для последующих
            //запросов на подписки к бирже Binance
            var socketClient = new BinanceSocketClient();
            
            //Подписка всех пар записанных в списоке pairs
            //К бинансу на получение новых ордеров
            foreach (var pair in pairs)
            {
                /*
                 * Нижнее подчеркивание перед знаком равно -
                 * Это переменные-заполнители, которые намеренно не используются в коде приложения.
                 * Пустые переменные эквивалентны переменным, которым не присвоены значения;
                 * пустые переменные не имеют значений.
                 * Пустые переменные объявляют свое намерение компилятору и другим разработчикам,
                 * которые читают ваш код. Вы намерены игнорировать результат выражения. 
                 */
                _ = socketClient.Spot.SubscribeToTradeUpdatesAsync(pair.PairName, data =>
                {
                    pair.IsBuy = !pair.IsBuy;

                    //Заполнение модели ордера, для добавление в список новых ордеров
                    var order = new OrderModel()
                    {
                        Price = data.Data.Price.ToString(),
                        Amount = rnd.NextDecimal(pair.AmountFrom, pair.AmountTo)
                            .ToString(CultureInfo.InvariantCulture),
                        IsBuy = pair.IsBuy,
                        Pair = pair.PairName,
                        BotAuthCode = BotAuthCodeConstant.Binance
                    };

                    if (newOrders.ContainsKey(pair.PairName))
                    {
                        newOrders[pair.PairName] = order;
                    }
                    else
                    {
                        newOrders.Add(pair.PairName, order);
                    }
                    
                });
            }
        }
        
        //Список в котором записан последний отправленный ордер
        //по всем парам
        private Dictionary<string, OrderModel> oldOrders = new();
        
        public void StartSendingOrders()
        {
            //Создаем таймер, который с интервалом 1 сек
            //Будет вызывать событие Elapsed
            Timer aTimer = new Timer(1000);
            //Привязываем к событию Elapsed наш обработчик событий 
            //OnATimerOnElapsed
            aTimer.Elapsed += OnATimerOnElapsed;
            //Включаем таймер.
            aTimer.Enabled = true;
        }
        
        //Метод, который будет вызываться каждую секунду
        private void OnATimerOnElapsed(object o, ElapsedEventArgs elapsedEventArgs)
        {
            foreach (var pair in pairs)
            {
                
                //Если в старых ордерах и новых ордерах есть объейкт с ключём текущей пары
                //И эти два объекта одинаковы спустя 1 секунду (Следовательно новых ордеров по этой
                //паре не пришло) то выполняем If
                //Иначе, если в новых ордерах есть объект с ключем текущей пары, то выполняем Else If
                if (oldOrders.ContainsKey(pair.PairName) && newOrders.ContainsKey(pair.PairName)
                    && newOrders[pair.PairName].Equals(oldOrders[pair.PairName]))
                {
                    var updatedOldOrder = oldOrders[pair.PairName];
                    
                    //Изменяем IsBuy и обновляем Amount
                    updatedOldOrder.IsBuy = !updatedOldOrder.IsBuy;
                    updatedOldOrder.Amount = rnd.NextDecimal(pair.AmountFrom, pair.AmountTo).ToString(CultureInfo.InvariantCulture);

                    newOrders[pair.PairName] = updatedOldOrder;
                    
                    "https://ecats.online/trade/crypto/createorder"
                        .PostJsonAsync(updatedOldOrder)
                        .Wait();
                    
                    Console.WriteLine($"BINANCE: ({pair.PairName}): {updatedOldOrder.Price}-{(updatedOldOrder.IsBuy ? "BUY" : "SELL")}");
                }
                else if (newOrders.ContainsKey(pair.PairName))
                {
                    //Если данный объект в списке не используется другим потоком (Например:
                    //с сервера бинанса не пришёл новый ордер в этот момент) то выполняем if
                    if (newOrders.TryGetValue(pair.PairName, out var newOrder))
                    {
                        try
                        {
                            "https://ecats.online/trade/crypto/createorder"
                                .PostJsonAsync(newOrder)
                                .Wait();

                            Console.WriteLine($"BINANCE: ({pair.PairName}): {newOrder.Price}-{(newOrder.IsBuy ? "BUY" : "SELL")}");
                        }
                        catch
                        {

                        }
                    }
                }
            }
        }


        private async Task SendOrder(string symbol, OrderSide orderSide, decimal quantity, decimal price,
            decimal stopPrice)
        {
            var socketClient = new BinanceClient();

            var res = await socketClient.Spot.Order.PlaceOcoOrderAsync(symbol, orderSide, quantity, price, stopPrice);

            if (res.Success && res.ResponseHeaders != null)
            {
                var orderId = res.ResponseHeaders.FirstOrDefault(x => x.Key == "orderId").Value;
            }
        }

        private async Task CancelOrder(string symbol, long orderId)
        {
            var socketClient = new BinanceClient();

            var res = await socketClient.Spot.Order.CancelOrderAsync(symbol, orderId);

            if (res.Success)
            {
                //TODO: smth
            }
        }
    }
}
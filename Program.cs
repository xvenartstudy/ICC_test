﻿using System;
using System.Linq;
using System.Net;
using System.Threading;
using TankCommon;
using TankServer;

namespace ICC_Tank
{
    class Program
    {
        static uint ParseOrDefault(string v, uint defaultValue)
        {
            uint u;

            if (string.IsNullOrWhiteSpace(v))
            {
                return defaultValue;
            }

            if (uint.TryParse(v, out u))
            {
                return u;
            }

            return defaultValue;
        }

        static void Main(string[] args)
        {
            var map = MapManager.LoadMap(15,'с', 20, 50);
            Console.WriteLine($"Сгенерирована карта");

            var port = ParseOrDefault(System.Configuration.ConfigurationManager.AppSettings["port"], 2000);
            var maxBotsCount = (uint)5;
            //var maxBotsCount = ParseOrDefault(System.Configuration.ConfigurationManager.AppSettings["maxBotsCount"], 1000);
            var coreUpdateMs = ParseOrDefault(System.Configuration.ConfigurationManager.AppSettings["coreUpdateMs"], 100);
            var spectatorUpdateMs = ParseOrDefault(System.Configuration.ConfigurationManager.AppSettings["spectatorUpdateMs"], 100);
            var botUpdateMs = ParseOrDefault(System.Configuration.ConfigurationManager.AppSettings["botUpdateMs"], 250);

            var strHostName = Dns.GetHostName();
            var ipEntry = Dns.GetHostEntry(strHostName);
            var ipAddresses = ipEntry.AddressList;

            Console.WriteLine($"Соединение по имени: ws://{strHostName}:{port}");
            Console.WriteLine("Или по IP адресу(ам):");
            foreach (var ipAddress in ipAddresses)
            {
                Console.WriteLine($"\tws://{ipAddress}:{port}");
            }
            
            Console.WriteLine("Нажмите Escape для выхода");

            var tokenSource = new CancellationTokenSource();
            var server = new Server(map, port, maxBotsCount, coreUpdateMs, spectatorUpdateMs, botUpdateMs);
            var serverTask = server.Run(tokenSource.Token);

            try
            {
                while (!serverTask.IsCompleted)
                {
                    Thread.Sleep(100);

                    if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                    {
                        tokenSource.Cancel();
                    }
                }
            }
            finally
            {
                server.Dispose();

                Console.WriteLine("Завершение работы сервера. Нажмите Enter для выхода");
                Console.ReadLine();
            }
        }
    }
}

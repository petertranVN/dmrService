﻿using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SingnalrClient
{
    class Program
    {
        static DateTime lastSend;
        static async Task Main(string[] args)
        {
          

            await Client("http://10.4.4.224:1009/ec-hub", "Client 1", "E");

        }


        static async Task Client(string path, string name, string building)
        {
            var _connection = new HubConnectionBuilder()
              .WithUrl("http://10.4.4.224:1009/ec-hub")
             .Build();

            _connection.On<string, string, string>("Welcom", (scalingMachineID, amount, unit) =>
            {
                string text = unit != "g" ? $"{name} The big one: " : $"{name} The small one: ";
                string newMessage = $"#### ### {text} {scalingMachineID}: {amount}{unit} {building}";
                Console.ForegroundColor = unit != "g" ? ConsoleColor.Green : ConsoleColor.White;
                Console.WriteLine(newMessage);
            });
            await _connection.StartAsync();
            Console.WriteLine(_connection.State);

        
            while (true)
            {
                Parallel.Invoke(
                async () =>
                {
                    double kg = Math.Round(RandomNumber(100, 134), 2);
                    await _connection.InvokeAsync("Welcom", "3", kg + "", "g");
                },
                async () =>
                {
                    double kg = Math.Round(RandomNumber(3.5, 4.2), 2);
                    await _connection.InvokeAsync("Welcom", "4", kg + "", "k");
                },



                 async () =>
                 {
                     double kg = Math.Round(RandomNumber(100, 124), 2);
                     await _connection.InvokeAsync("Welcom", "2", kg + "", "k");
                 },
                 async () =>
                 {
                     double kg = Math.Round(RandomNumber(3, 3.2), 2);
                     await _connection.InvokeAsync("Welcom", "2", kg + "", "k");
                 },
                 
                 async () =>
                 {
                     double g = Math.Round(RandomNumber(940, 950), 2);
                     await _connection.InvokeAsync("Welcom", "1", g.ToString(), "g");
                     await Task.Delay(2000);
                 }
                );

                await Task.Delay(100);

            }
        }
    
        public static double RandomNumber(double minimum, double maximum)
        {
            Random random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }
    }
}

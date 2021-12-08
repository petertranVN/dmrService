using System;
using System.IO;
using System.IO.Ports;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using scalingmachine.Helpers;

namespace bigcalingmachine
{
    class Program
    {
        static bool _continue;
        static SerialPort _serialPort;
        static HubConnection _connection;
        static float before2;
        public static async Task Main(string[] args)
        {
      
            var currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var appsettings = ReadConfiguration(currentPath);

            _connection = new HubConnectionBuilder()
            .WithUrl(appsettings.SignalrUrl)
            .Build();

            _connection.On<string, string, string>("Welcom", (scalingMachineID, message, unit) =>
            {
                if (scalingMachineID == appsettings.MachineID.ToString())
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    var newMessage = $"#### ### Welcom Big Scaling Machine Data: {scalingMachineID}: {message}: {unit}";
                    Console.WriteLine(newMessage);

                }
            });
            await _connection.StartAsync();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"#### ### ClientId Welcom Big Scaling Machine: " + _connection.ConnectionId);

            string name;
            string message;
            StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
            Thread readThread = new Thread(async () => await Read());

            // Create a new SerialPort object with default settings.
            _serialPort = new SerialPort();

            // Allow the user to set the appropriate properties.
            _serialPort.PortName = ReadConfiguration(currentPath).portName;
            //_serialPort.BaudRate = SetPortBaudRate(_serialPort.BaudRate);
            //_serialPort.Parity = SetPortParity(_serialPort.Parity);
            //_serialPort.DataBits = SetPortDataBits(_serialPort.DataBits);
            //_serialPort.StopBits = SetPortStopBits(_serialPort.StopBits);
            //_serialPort.Handshake = SetPortHandshake(_serialPort.Handshake);

            // Set the read/write timeouts
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;

            _serialPort.Open();
            _continue = true;
            readThread.Start();

            //Console.Write("Name: ");
            //name = Console.ReadLine();

            Console.WriteLine("Type QUIT to exit");
            while (_continue)
            {
                // emit ve client
                message = Console.ReadLine();

                if (stringComparer.Equals("quit", message))
                {
                    _continue = false;
                }
                else
                {

                    //_serialPort.WriteLine(
                    //    String.Format("<{0}>: {1}", name, message));
                }
            }

            readThread.Join();
            _serialPort.Close();

        }

        public static async Task Read()
        {
            while (_continue)
            {
                try
                {

                    string message2 = _serialPort.ReadLine();
                    if (message2.Trim() != string.Empty)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        var unit2 = new Regex(@"[a-zA-Z]").Match(message2).Value.ToSafetyString();
                        var messages2 = new Regex(@"[+-]?([0-9]*[.])?[0-9]+").Match(message2).Value.ToFloat();
                        if (before2 != messages2)
                        {
                            if (unit2 == "k")
                            {
                                before2 = messages2;
                                await _connection.InvokeAsync("Welcom", "2", messages2.ToString(), "kg");
                            }
                        }

                    }

                }
                catch (TimeoutException) { }
            }
        }

        // Display Port values and prompt user to enter a port.
        public static string SetPortName(string defaultPortName)
        {
            var currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            string portName = ReadConfiguration(currentPath).portName;
            portName = defaultPortName;
            return portName;
        }
        // Display BaudRate values and prompt user to enter a value.
        public static int SetPortBaudRate(int defaultPortBaudRate)
        {
            string baudRate;

            Console.Write("Baud Rate(default:{0}): ", defaultPortBaudRate);
            baudRate = Console.ReadLine();

            if (baudRate == "")
            {
                baudRate = defaultPortBaudRate.ToString();
            }

            return int.Parse(baudRate);
        }
        public void LogError(Exception ex)
        {
            string message = string.Format("Time: {0}", DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"));
            message += Environment.NewLine;
            message += "-----------------------------------------------------------";
            message += Environment.NewLine;
            message += string.Format("Message: {0}", ex.Message);
            message += Environment.NewLine;
            message += string.Format("StackTrace: {0}", ex.StackTrace.Substring(ex.StackTrace.Length - 7, 7));
            message += Environment.NewLine;
            message += string.Format("Source: {0}", ex.Source);
            message += Environment.NewLine;
            message += string.Format("extype: {0}", ex.GetType().ToString());
            message += Environment.NewLine;
            //message += string.Format("ErrorLocation: {0}", ex.Message.ToString());
            //message += Environment.NewLine;

            //message += string.Format("TargetSite: {0}", ex.TargetSite.ToString());
            //message += Environment.NewLine;

            message += "-----------------------------------------------------------";
            message += Environment.NewLine;
            // string path = @"C:\Users\sy.pham\Desktop\ScalingConsoleApp\LogError\BigScaleError.txt";
            string path = $@"C:\ScalingConsoleApp\Log\";

            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine(message);
                writer.Close();
            }

        }
        // Display PortParity values and prompt user to enter a value.
        public static Parity SetPortParity(Parity defaultPortParity)
        {
            string parity;

            Console.WriteLine("Available Parity options:");
            foreach (string s in Enum.GetNames(typeof(Parity)))
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Enter Parity value (Default: {0}):", defaultPortParity.ToString(), true);
            parity = Console.ReadLine();

            if (parity == "")
            {
                parity = defaultPortParity.ToString();
            }

            return (Parity)Enum.Parse(typeof(Parity), parity, true);
        }
        // Display DataBits values and prompt user to enter a value.
        public static int SetPortDataBits(int defaultPortDataBits)
        {
            string dataBits;

            Console.Write("Enter DataBits value (Default: {0}): ", defaultPortDataBits);
            dataBits = Console.ReadLine();

            if (dataBits == "")
            {
                dataBits = defaultPortDataBits.ToString();
            }

            return int.Parse(dataBits.ToUpperInvariant());
        }

        // Display StopBits values and prompt user to enter a value.
        public static StopBits SetPortStopBits(StopBits defaultPortStopBits)
        {
            string stopBits;

            Console.WriteLine("Available StopBits options:");
            foreach (string s in Enum.GetNames(typeof(StopBits)))
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Enter StopBits value (None is not supported and \n" +
             "raises an ArgumentOutOfRangeException. \n (Default: {0}):", defaultPortStopBits.ToString());
            stopBits = Console.ReadLine();

            if (stopBits == "")
            {
                stopBits = defaultPortStopBits.ToString();
            }

            return (StopBits)Enum.Parse(typeof(StopBits), stopBits, true);
        }
        public static Handshake SetPortHandshake(Handshake defaultPortHandshake)
        {
            string handshake;

            Console.WriteLine("Available Handshake options:");
            foreach (string s in Enum.GetNames(typeof(Handshake)))
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Enter Handshake value (Default: {0}):", defaultPortHandshake.ToString());
            handshake = Console.ReadLine();

            if (handshake == "")
            {
                handshake = defaultPortHandshake.ToString();
            }

            return (Handshake)Enum.Parse(typeof(Handshake), handshake, true);
        }
        /// <summary>
        ///     Reads the configuration.
        /// </summary>
        /// <returns>A <see cref="Config" /> object.</returns>
        private static Config ReadConfiguration(string currentPath)
        {
            var filePath = $"{currentPath}\\config.json";

            Config config = null;

            // ReSharper disable once InvertIf
            if (File.Exists(filePath))
            {
                using var r = new StreamReader(filePath);
                var json = r.ReadToEnd();
                config = JsonConvert.DeserializeObject<Config>(json);
            }

            return config;
        }

    }
}

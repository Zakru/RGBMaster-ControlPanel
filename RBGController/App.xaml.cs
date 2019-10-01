using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Net;
using System.IO.Ports;
using System.Threading;

namespace RBGController
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public static HttpListener httpListener;
        public static SerialPort serialPort;

        private static App app;
        private static List<string> previousPorts = new List<string>();

        [STAThread]
        public static void Main()
        {
            httpListener = new HttpListener();
            httpListener.Prefixes.Add("http://localhost:8081/");
            httpListener.Start();
            HttpListenerLoop();

            Console.WriteLine("asd");
            foreach (string port in SerialPort.GetPortNames())
            {
                Console.WriteLine(port);
            }
            serialPort = new SerialPort();
            //serialPort.PortName = "";
            serialPort.BaudRate = 57600;

            PortListenerLoop();

            app = new App();
            app.InitializeComponent();
            app.Run();
        }

        private static Task PortListenerLoop()
        {
            return Task.Run(() =>
            {
                while (true)
                {
                    string[] portNames = SerialPort.GetPortNames();

                    if (portNames.Length != previousPorts.Count) UpdatePorts();
                    else
                    {
                        for (int i = 0; i < portNames.Length; i++)
                        {
                            if (!portNames[i].Equals(previousPorts[i]))
                            {
                                UpdatePorts();
                                break;
                            }
                        }
                    }

                    Thread.Sleep(100);
                }
            });
        }

        private static void UpdatePorts()
        {
            string[] ports = SerialPort.GetPortNames();
            previousPorts.Clear();
            previousPorts.AddRange(ports);
            ((MainWindow)app.MainWindow).portSelectorItems.Clear();
            foreach (string port in ports) ((MainWindow)app.MainWindow).portSelectorItems.Add(port);
        }

        private static async void HttpListenerLoop()
        {
            while (httpListener.IsListening)
            {
                try
                {
                    HttpListenerContext context = await httpListener.GetContextAsync();
                    if (context.Request.HttpMethod == "POST")
                    {
                        context.Response.StatusCode = 200;
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }
                    context.Response.Close();
                }
                catch
                {

                }
            }
        }
    }
}

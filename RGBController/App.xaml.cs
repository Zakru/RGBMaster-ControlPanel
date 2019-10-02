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
using System.Management;
using System.Text;

namespace RGBController
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

        private static float[] hues = new float[240];

        [STAThread]
        public static void Main()
        {
            for (int i = 0; i < 240; i++)
            {
                float h = i * 6f / 240;
                if (h < 1)
                {
                    hues[i] = 255;
                }
                else if (h < 2)
                {
                    hues[i] = 255 * (2 - h);
                }
                else if (h < 4)
                {
                    hues[i] = 0;
                }
                else if (h < 5)
                {
                    hues[i] = 255 * (h - 4);
                }
                else
                {
                    hues[i] = 255;
                }
                Console.WriteLine(hues[i]);
            }

            httpListener = new HttpListener();
            httpListener.Prefixes.Add("http://localhost:8081/");
            httpListener.Start();
            HttpListenerLoop();
            serialPort = new SerialPort();
            //serialPort.PortName = "";
            serialPort.BaudRate = 57600;

            PortListenerLoop();

            SendPixelsLoop();

            app = new App();
            app.InitializeComponent();
            app.Run();
        }

        private static Task PortListenerLoop()
        {
            return Task.Run(() =>
            {
                while (app == null);
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

            app.Dispatcher.Invoke(() => {
                ((MainWindow)app.MainWindow).portSelectorItems.Clear();
                foreach (string port in ports) ((MainWindow)app.MainWindow).portSelectorItems.Add(port);
            });
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
        
        private static Task SendPixelsLoop()
        {
            return Task.Run(() =>
            {
                int i = 0;
                byte[] bytes = new byte[181];
                while (true)
                {
                    i = (i + 1) % 240;
                    while (!serialPort.IsOpen);
                    for(int j=0;j<181;j++)
                    {
                        if (j == 0) bytes[j] = 0;
                        else if (j < 61) bytes[j] = (byte)hues[(240 + (j-1) * 4 - i) % 240];
                        else if (j < 121) bytes[j] = (byte)hues[(240 + (j-61) * 4 - i + 80) % 240];
                        else bytes[j] = (byte)hues[(240 + (j - 121) * 4 - i + 160) % 240];
                    }
                    serialPort.Write(bytes, 0, 181);
                }
            });
        }
    }
}

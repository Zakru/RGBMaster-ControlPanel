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
        public static SerialPort serialPort;

        public static bool closing = false;

        private static App app;
        private static readonly List<string> previousPorts = new List<string>();

        private static readonly RendererStack renderers = new RendererStack();

        [STAThread]
        public static void Main()
        {
            serialPort = new SerialPort();
            //serialPort.PortName = "";
            serialPort.BaudRate = 57600;

            PortListenerLoop();

            SendPixelsLoop();

            app = new App();
            app.InitializeComponent();
            app.Run();
        }

        public static void Cleanup()
        {
            closing = true;
            serialPort.Close();
            foreach (RGBRenderer r in renderers)
            {
                r.Cleanup();
            }
        }

        private static Task PortListenerLoop()
        {
            return Task.Run(async () =>
            {
                while (app == null);
                while (true)
                {
                    if (closing) return;

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

                    await Task.Delay(100);
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
        
        private static Task SendPixelsLoop()
        {
            return Task.Run(() =>
            {
                while (!serialPort.IsOpen);
                renderers.Add(new CounterStrikeRenderer(60));
                renderers.Add(new HueRenderer(60));
                byte[] bytes = new byte[181];
                byte[] pixelBuffer = new byte[180];
                while (true)
                {
                    if (closing) return;
                    renderers.RenderFirstOnto(pixelBuffer);
                    for (int i = 0; i < pixelBuffer.Length; i++) bytes[i + 1] = pixelBuffer[i];
                    while (!serialPort.IsOpen);
                    serialPort.Write(bytes, 0, 181);
                }
            });
        }
    }
}

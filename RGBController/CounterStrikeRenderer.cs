using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RGBController
{
    internal class CounterStrikeRenderer : RGBRenderer
    {
        private readonly HueRenderer hueRenderer;

        private HttpListener httpListener = new HttpListener();

        private Dictionary<string, dynamic> gameState;

        public CounterStrikeRenderer(int pixelCount) : base(pixelCount)
        {
            hueRenderer = new HueRenderer(pixelCount);

            httpListener.Prefixes.Add("http://localhost:8081/");
            httpListener.Start();
            HttpListenerLoop();
        }

        public override bool RenderOnto(byte[] pixelBuffer)
        {
            if (DateTime.Now.Second % 2 == 0)
            {
                return hueRenderer.RenderOnto(pixelBuffer);
            }
            if (gameState != null && gameState.ContainsKey("player") && gameState["player"].ContainsKey("state"))
            {
                DrawLine(pixelBuffer, 0, 255, 0, 0, pixelCount * (int)gameState["player"]["state"]["health"] / 100f);
                Console.WriteLine(pixelCount * (int)gameState["player"]["state"]["health"] / 100f);
            }
            return true;
        }

        public override void Cleanup()
        {
            httpListener.Close();
        }

        private Task HttpListenerLoop()
        {
            return Task.Run(async () => {
                while (httpListener.IsListening)
                {
                    try
                    {
                        HttpListenerContext context = await httpListener.GetContextAsync();
                        if (context.Request.HttpMethod == "POST")
                        {
                            string body = new StreamReader(context.Request.InputStream).ReadToEnd();
                            gameState = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(body);
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
            });
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGBController
{
    class HueRenderer : RGBRenderer
    {
        private static readonly int AntiAliasing = 4;

        private readonly float[] hues;

        public HueRenderer(int pixelCount) : base(pixelCount)
        {
            hues = new float[pixelCount * AntiAliasing];
            for (int i = 0; i < hues.Length; i++)
            {
                float h = i * 6f / hues.Length;
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
            }
        }

        public override bool RenderOnto(byte[] pixelBuffer)
        {
            float offset = (float)(DateTime.Now - Process.GetCurrentProcess().StartTime).TotalSeconds * 0.3f * pixelCount * AntiAliasing % (pixelCount * AntiAliasing);
            for (int i = 0; i < pixelCount; i++)
            {
                pixelBuffer[i*3] = (byte)hues[(int)(i * 4 + offset) % (pixelCount * AntiAliasing)];
                pixelBuffer[i*3+1] = (byte)hues[(int)(i * 4 + offset + pixelCount * AntiAliasing / 3) % (pixelCount * AntiAliasing)];
                pixelBuffer[i*3+2] = (byte)hues[(int)(i * 4 + offset + pixelCount * AntiAliasing * 2 / 3) % (pixelCount * AntiAliasing)];
            }
            return true;
        }
    }
}

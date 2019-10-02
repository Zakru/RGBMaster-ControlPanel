using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGBController
{
    abstract class RGBRenderer
    {
        protected int pixelCount;
        public RGBRenderer(int pixelCount)
        {
            this.pixelCount = pixelCount;
        }

        public abstract bool RenderOnto(byte[] pixelBuffer);

        public virtual void Cleanup()
        {

        }

        protected void DrawLine(byte[] pixelBuffer, byte r, byte g, byte b, float start, float length)
        {
            for (int i=(int)start; i<(int)(start+length)+1; i++)
            {
                if (i >= pixelBuffer.Length/3) break;
                float s, e;
                if (start < i) s = 0;
                else s = start - i;
                if (start + length > i + 1) e = 1;
                else e = start + length - i;

                pixelBuffer[i*3] = (byte)(r * (e - s));
                pixelBuffer[i*3+1] = (byte)(g * (e - s));
                pixelBuffer[i*3+2] = (byte)(b * (e - s));
            }
        }
    }
}

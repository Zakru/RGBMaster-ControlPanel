using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGBController
{
    class RendererStack : List<RGBRenderer>
    {
        public bool RenderFirstOnto(byte[] pixelBuffer)
        {
            foreach (RGBRenderer renderer in this)
            {
                if (renderer.RenderOnto(pixelBuffer))
                {
                    return true;
                }
            }
            return false;
        }
    }
}

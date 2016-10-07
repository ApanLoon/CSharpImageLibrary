using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsefulThings;
using Microsoft.IO;
using System.IO;
using System.Windows.Media.Imaging;

namespace CSharpImageLibrary
{
    /// <summary>
    /// Represents a mipmap of an image.
    /// </summary>
    public class MipMap
    {
        /// <summary>
        /// Pixels.
        /// </summary>
        public byte[] Pixels
        {
            get; set;
        }

        /// <summary>
        /// Mipmap width.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Mipmap height.
        /// </summary>
        public int Height { get; set; }


        public MipMap(byte[] pixels)
        {
            Pixels = pixels;
        }

        public MipMap(BitmapSource source)
        {
            int stride = source.PixelWidth * 4;
            Pixels = new byte[source.PixelHeight * stride];
            source.CopyPixels(Pixels, stride, 0);
            Width = source.PixelWidth;
            Height = source.PixelHeight;
        }
    }
}

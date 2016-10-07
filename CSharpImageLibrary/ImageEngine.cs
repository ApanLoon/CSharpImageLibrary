using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpImageLibrary
{
    internal static class ImageEngine
    {
        public static bool WICCodecsAvailable { get; private set; }
        public static bool GPUAvailable { get; private set; }
        public static bool EnableThreading { get; set; } = true;
        public static bool EnableGPUAcceleration { get; set; } = true;

        static ImageEngine()
        {
            // TODO: WIC and GPU detection
            WICCodecsAvailable = true;
        }

        public static List<MipMap> Load(MemoryStream stream, ImageEngineFormat format, int maxDimension = 0)
        {
            List<MipMap> MipMaps = null;
            switch (format)
            {
                case ImageEngineFormat.BMP:
                case ImageEngineFormat.JPG:
                case ImageEngineFormat.PNG:
                    MipMaps = WIC_Codecs.LoadWithCodecs(stream, maxDimension, false);
                    break;
                case ImageEngineFormat.DDS_DXT1:
                case ImageEngineFormat.DDS_DXT2:
                case ImageEngineFormat.DDS_DXT3:
                case ImageEngineFormat.DDS_DXT4:
                case ImageEngineFormat.DDS_DXT5:
                    if (WICCodecsAvailable)
                        MipMaps = WIC_Codecs.LoadWithCodecs(stream, maxDimension, true);
                    /*else
                        MipMaps = DDSGeneral.LoadDDS(stream, header, Format, maxHeight > maxWidth ? maxHeight : maxWidth);*/
                    break;
                case ImageEngineFormat.DDS_ARGB:
                case ImageEngineFormat.DDS_A8L8:
                case ImageEngineFormat.DDS_RGB:
                case ImageEngineFormat.DDS_ATI1:
                case ImageEngineFormat.DDS_ATI2_3Dc:
                case ImageEngineFormat.DDS_G8_L8:
                case ImageEngineFormat.DDS_V8U8:
                    //MipMaps = DDSGeneral.LoadDDS(stream, header, Format, maxHeight > maxWidth ? maxHeight : maxWidth);
                    break;
                case ImageEngineFormat.TGA:
                    /*var img = new TargaImage(stream);
                    byte[] pixels = UsefulThings.WinForms.Imaging.GetPixelDataFromBitmap(img.Image);
                    WriteableBitmap wbmp = UsefulThings.WPF.Images.CreateWriteableBitmap(pixels, img.Image.Width, img.Image.Height);
                    var mip1 = new MipMap(wbmp);
                    MipMaps = new List<MipMap>() { mip1 };
                    img.Dispose();*/
                    break;
                default:
                    throw new InvalidDataException("Image format is unknown.");
            }

            return MipMaps;
        }
    }
}

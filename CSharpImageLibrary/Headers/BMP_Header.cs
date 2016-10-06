using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpImageLibrary.Headers
{
    internal class BMP_Header : AbstractHeader
    {
        #region Header Structs
        /// <summary>
        /// Detailed Image Information.
        /// </summary>
        static class DIBHeaders
        {
            public enum CompressionMethod
            {
                /// <summary>
                /// Most common. Uncompressed.
                /// </summary>
                BI_RGB = 0,

                /// <summary>
                /// RLE 8-bit/pixel. Must only be used with 8bpp images.
                /// </summary>
                BI_RLE8 = 1,

                /// <summary>
                /// RLE 4-bit/pixel. Must only be used with 4bpp images.
                /// </summary>
                BI_RLE4 = 2,

                /// <summary>
                /// OS22XBITMAPHEADER: Huffman 1D
                /// BITMAPV2INFOHEADER: RGB Bit Field Masks,
                /// BITMAPV3INFOHEADER+: RGBA
                /// </summary>
                BI_BITFIELDS = 3,

                /// <summary>
                /// OS22XBITMAPHEADER: RLE-24
                /// </summary>
                BI_JPEG = 4,

                /// <summary>
                /// PNG I guess...
                /// </summary>
                BI_PNG = 5,

                /// <summary>
                /// RGBA Bit field masks. Only .NET 4.0 or later.
                /// </summary>
                BI_ALPHABITFIELDS = 6,

                /// <summary>
                /// Only Windows metafile CMYK.
                /// </summary>
                BI_CMYK = 11,

                /// <summary>
                /// Only Windows metafile CMYK.
                /// </summary>
                BI_CMYKRLE8 = 12,

                /// <summary>
                /// Only Windows metafile CMYK.
                /// </summary>
                BI_CMYKRLE4 = 13,
            }

            /// <summary>
            /// Original bmp header. Also known as OS21XBITMAPHEADER.
            /// </summary>
            public struct BITMAPCOREHEADER
            {
                public const int HeaderSize = 12;

                public ushort Width;
                public ushort Height;
                public const ushort ColourPlanes = 1;
                public ushort BitsPerPixel;
            }


            // Not supporting OS/2

            /// <summary>
            /// MOST COMMON NOW (2016)
            /// Extension: Adds 16bpp and 32bpp, RLE compression.
            /// </summary>
            public struct BITMAPINFOHEADER
            {
                public const int HeaderSize = 40;

                public int Width;
                public int Height;
                public const ushort ColourPlanes = 1;
                public ushort BitsPerPixel;
                public CompressionMethod Compression;
                public int DataLength; // 0 can be given for BI_RGB bitmaps i.e. Most common, uncompressed.
                public int HorizontalResolution;
                public int VerticalResolution;
                public int NumColoursInPalette;  // 0 for default, 2^n.
                public int NumImportantColours;  // Normally ignored. 0 when every colour is important.

                public override string ToString()
                {
                    StringBuilder sb = new StringBuilder();

                    sb.AppendLine($"Header Size: {HeaderSize}");
                    sb.AppendLine($"Width: {Width}");
                    sb.AppendLine($"Height: {Height}");
                    sb.AppendLine($"Colour Planes: {ColourPlanes}");
                    sb.AppendLine($"Bits Per Pixel: {BitsPerPixel}");
                    sb.AppendLine($"Compression: {Compression.ToString()}");
                    sb.AppendLine($"Data Length: {DataLength}");
                    sb.AppendLine($"Horizontal Resolution: {HorizontalResolution}");
                    sb.AppendLine($"Vertical Resolution: {VerticalResolution}");
                    sb.AppendLine($"Number of Colours in Palette: {NumColoursInPalette}");
                    sb.AppendLine($"Number of Important Colours: {NumImportantColours}");

                    return sb.ToString();
                }
            }
        }

        /// <summary>
        /// Header for BMP file.
        /// </summary>
        struct FileHeader
        {
            /// <summary>
            /// File Identifier.
            /// BM = Windows, BA, CI, CP, IC, PT = OS/2 variations.
            /// </summary>
            public string Identifier;

            /// <summary>
            /// Length of file
            /// </summary>
            public int Length;

            /// <summary>
            /// Depends on application
            /// </summary>
            public short Reserved1;

            /// <summary>
            /// Depends on application
            /// </summary>
            public short Reserved2;


            /// <summary>
            /// Offset at which Pixel Data begins.
            /// </summary>
            public int DataOffset;

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Identifier: {Identifier}");
                sb.AppendLine($"Length: {Length}");
                sb.AppendLine($"Reserved1: {Reserved1}");
                sb.AppendLine($"Reserved2: {Reserved2}");
                sb.AppendLine($"Offset: {DataOffset}");

                return sb.ToString();
            }
        }

        // Not including information about Color Table, ICC Profile, or alignment.
        #endregion Header Structs


        public const int MaxHeaderSize = 54; // Only considering default header.
        FileHeader fileHeader = new FileHeader();
        DIBHeaders.BITMAPINFOHEADER BMPInfoHeader = new DIBHeaders.BITMAPINFOHEADER();

        public override int HeaderSize
        {
            get
            {
                return 14 + 40;
            }
        }

        public override int Width
        {
            get
            {
                return BMPInfoHeader.Width;
            }
        }

        public override int Height
        {
            get
            {
                return BMPInfoHeader.Height;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("--- BMP Header information ---");

            // File Header
            sb.AppendLine("- File Header -");
            sb.AppendLine(fileHeader.ToString());
            sb.AppendLine("- End File Header -");

            // Info Header
            sb.AppendLine("- Info Header -");
            sb.AppendLine(BMPInfoHeader.ToString());
            sb.AppendLine("- End Info Header -");

            sb.AppendLine("----------End Header----------");

            return sb.ToString();
        }

        public BMP_Header()
        {

        }
    }
}

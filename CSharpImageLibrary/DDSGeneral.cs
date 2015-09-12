﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsefulThings;

namespace CSharpImageLibrary
{
    /// <summary>
    /// Provides general functions specific to DDS format
    /// </summary>
    internal static class DDSGeneral
    {
        private readonly static byte[] RedBlueDecompressionConstants = new byte[] { 0, 8, 16, 25, 33, 41, 49, 58, 66, 74, 82, 90, 99, 107, 115, 123, 132, 140, 148, 156, 165, 173, 181, 189, 197, 206, 214, 222, 230, 239, 247, 255, 7, 15, 24, 32, 40, 48, 57, 65, 73, 81, 89, 98, 106, 114, 122, 131, 139, 147, 155, 164, 172, 180, 188, 196, 205, 213, 221, 229, 238, 246, 254, 6, 14, 23, 31, 39, 47, 56, 64, 72, 80, 88, 97, 105, 113, 121, 130, 138, 146, 154, 163, 171, 179, 187, 195, 204, 212, 220, 228, 237, 245, 253, 5, 13, 22, 30, 38, 46, 55, 63, 71, 79, 87, 96, 104, 112, 120, 129, 137, 145, 153, 162, 170, 178, 186, 194, 203, 211, 219, 227, 236, 244, 252, 4, 12, 21, 29, 37, 45, 54, 62, 70, 78, 86, 95, 103, 111, 119, 128, 136, 144, 152, 161, 169, 177, 185, 193, 202, 210, 218, 226, 235, 243, 251, 3, 11, 20, 28, 36, 44, 53, 61, 69, 77, 85, 94, 102, 110, 118, 127, 135, 143, 151, 160, 168, 176, 184, 192, 201, 209, 217, 225, 234, 242, 250, 2, 10, 19, 27, 35, 43, 52, 60, 68, 76, 84, 93, 101, 109, 117, 126, 134, 142, 150, 159, 167, 175, 183, 191, 200, 208, 216, 224, 233, 241, 249, 1, 9, 18, 26, 34, 42, 51, 59, 67, 75, 83, 92, 100, 108, 116, 125, 133, 141, 149, 158, 166, 174, 182, 190, 199, 207, 215, 223, 232, 240, 248, 0, 8, 17, 25, 33, 41, 50 };
        private readonly static byte[] GreenDecompressionConstants = new byte[] { 0, 4, 8, 12, 16, 20, 24, 28, 32, 36, 40, 45, 49, 53, 57, 61, 65, 69, 73, 77, 81, 85, 89, 93, 97, 101, 105, 109, 113, 117, 121, 125, 130, 134, 138, 142, 146, 150, 154, 158, 162, 166, 170, 174, 178, 182, 186, 190, 194, 198, 202, 206, 210, 215, 219, 223, 227, 231, 235, 239, 243, 247, 251, 255, 3, 7, 11, 15, 19, 23, 27, 31, 35, 39, 44, 48, 52, 56, 60, 64, 68, 72, 76, 80, 84, 88, 92, 96, 100, 104, 108, 112, 116, 120, 124, 129, 133, 137, 141, 145, 149, 153, 157, 161, 165, 169, 173, 177, 181, 185, 189, 193, 197, 201, 205, 209, 214, 218, 222, 226, 230, 234, 238, 242, 246, 250, 254, 2, 6, 10, 14, 18, 22, 26, 30, 34, 38, 43, 47, 51, 55, 59, 63, 67, 71, 75, 79, 83, 87, 91, 95, 99, 103, 107, 111, 115, 119, 123, 128, 132, 136, 140, 144, 148, 152, 156, 160, 164, 168, 172, 176, 180, 184, 188, 192, 196, 200, 204, 208, 213, 217, 221, 225, 229, 233, 237, 241, 245, 249, 253, 1, 5, 9, 13, 17, 21, 25, 29, 33, 37, 42, 46, 50, 54, 58, 62, 66, 70, 74, 78, 82, 86, 90, 94, 98, 102, 106, 110, 114, 118, 122, 127, 131, 135, 139, 143, 147, 151, 155, 159, 163, 167, 171, 175, 179, 183, 187, 191, 195, 199, 203, 207, 212, 216, 220, 224, 228, 232, 236, 240, 244, 248, 252, 0, 4, 8 };
        private readonly static byte[] RedBlueCompressionConstants = new byte[] { 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 4, 4, 4, 5, 5, 5, 5, 5, 5, 5, 5, 6, 6, 6, 6, 6, 6, 6, 6, 7, 7, 7, 7, 7, 7, 7, 7, 8, 8, 8, 8, 8, 8, 8, 8, 9, 9, 9, 9, 9, 9, 9, 9, 9, 10, 10, 10, 10, 10, 10, 10, 10, 11, 11, 11, 11, 11, 11, 11, 11, 12, 12, 12, 12, 12, 12, 12, 12, 13, 13, 13, 13, 13, 13, 13, 13, 13, 14, 14, 14, 14, 14, 14, 14, 14, 15, 15, 15, 15, 15, 15, 15, 15, 16, 16, 16, 16, 16, 16, 16, 16, 17, 17, 17, 17, 17, 17, 17, 17, 18, 18, 18, 18, 18, 18, 18, 18, 18, 19, 19, 19, 19, 19, 19, 19, 19, 20, 20, 20, 20, 20, 20, 20, 20, 21, 21, 21, 21, 21, 21, 21, 21, 22, 22, 22, 22, 22, 22, 22, 22, 22, 23, 23, 23, 23, 23, 23, 23, 23, 24, 24, 24, 24, 24, 24, 24, 24, 25, 25, 25, 25, 25, 25, 25, 25, 26, 26, 26, 26, 26, 26, 26, 26, 27, 27, 27, 27, 27, 27, 27, 27, 27, 28, 28, 28, 28, 28, 28, 28, 28, 29, 29, 29, 29, 29, 29, 29, 29, 30, 30, 30, 30, 30, 30, 30, 30, 31, 31, 31, 31, 31 };
        private readonly static byte[] GreenCompressionConstants = new byte[] { 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 6, 6, 6, 6, 7, 7, 7, 7, 8, 8, 8, 8, 9, 9, 9, 9, 10, 10, 10, 10, 11, 11, 11, 11, 12, 12, 12, 12, 13, 13, 13, 13, 14, 14, 14, 14, 15, 15, 15, 15, 16, 16, 16, 16, 17, 17, 17, 17, 18, 18, 18, 18, 19, 19, 19, 19, 20, 20, 20, 20, 21, 21, 21, 21, 21, 22, 22, 22, 22, 23, 23, 23, 23, 24, 24, 24, 24, 25, 25, 25, 25, 26, 26, 26, 26, 27, 27, 27, 27, 28, 28, 28, 28, 29, 29, 29, 29, 30, 30, 30, 30, 31, 31, 31, 31, 32, 32, 32, 32, 33, 33, 33, 33, 34, 34, 34, 34, 35, 35, 35, 35, 36, 36, 36, 36, 37, 37, 37, 37, 38, 38, 38, 38, 39, 39, 39, 39, 40, 40, 40, 40, 41, 41, 41, 41, 42, 42, 42, 42, 42, 43, 43, 43, 43, 44, 44, 44, 44, 45, 45, 45, 45, 46, 46, 46, 46, 47, 47, 47, 47, 48, 48, 48, 48, 49, 49, 49, 49, 50, 50, 50, 50, 51, 51, 51, 51, 52, 52, 52, 52, 53, 53, 53, 53, 54, 54, 54, 54, 55, 55, 55, 55, 56, 56, 56, 56, 57, 57, 57, 57, 58, 58, 58, 58, 59, 59, 59, 59, 60, 60, 60, 60, 61, 61, 61, 61, 62, 62, 62, 62, 63, 63, 63 };


        #region Header Stuff
        /// <summary>
        /// Reads DDS header from file.
        /// </summary>
        /// <param name="h">Header struct.</param>
        /// <param name="r">File reader.</param>
        internal static void Read_DDS_HEADER(DDS_HEADER h, BinaryReader r)
        {
            h.dwSize = r.ReadInt32();
            h.dwFlags = r.ReadInt32();
            h.dwHeight = r.ReadInt32();
            h.dwWidth = r.ReadInt32();
            h.dwPitchOrLinearSize = r.ReadInt32();
            h.dwDepth = r.ReadInt32();
            h.dwMipMapCount = r.ReadInt32();
            for (int i = 0; i < 11; ++i)
                h.dwReserved1[i] = r.ReadInt32();
            Read_DDS_PIXELFORMAT(h.ddspf, r);
            h.dwCaps = r.ReadInt32();
            h.dwCaps2 = r.ReadInt32();
            h.dwCaps3 = r.ReadInt32();
            h.dwCaps4 = r.ReadInt32();
            h.dwReserved2 = r.ReadInt32();
        }

        /// <summary>
        /// Reads DDS pixel format.
        /// </summary>
        /// <param name="p">Pixel format struct.</param>
        /// <param name="r">File reader.</param>
        private static void Read_DDS_PIXELFORMAT(DDS_PIXELFORMAT p, BinaryReader r)
        {
            p.dwSize = r.ReadInt32();
            p.dwFlags = r.ReadInt32();
            p.dwFourCC = r.ReadInt32();
            p.dwRGBBitCount = r.ReadInt32();
            p.dwRBitMask = r.ReadUInt32();
            p.dwGBitMask = r.ReadUInt32();
            p.dwBBitMask = r.ReadUInt32();
            p.dwABitMask = r.ReadUInt32();
        }

        /// <summary>
        /// Contains information about DDS Headers. 
        /// </summary>
        internal class DDS_HEADER
        {
            public int dwSize;
            public int dwFlags;
            public int dwHeight;
            public int dwWidth;
            public int dwPitchOrLinearSize;
            public int dwDepth;
            public int dwMipMapCount;
            public int[] dwReserved1 = new int[11];
            public DDS_PIXELFORMAT ddspf = new DDS_PIXELFORMAT();
            public int dwCaps;
            public int dwCaps2;
            public int dwCaps3;
            public int dwCaps4;
            public int dwReserved2;

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("--DDS_HEADER--");
                sb.AppendLine($"dwSize: {dwSize}");
                sb.AppendLine($"dwFlags: 0x{dwFlags.ToString("X")}");  // As hex
                sb.AppendLine($"dwHeight: {dwHeight}");
                sb.AppendLine($"dwWidth: {dwWidth}");
                sb.AppendLine($"dwPitchOrLinearSize: {dwPitchOrLinearSize}");
                sb.AppendLine($"dwDepth: {dwDepth}");
                sb.AppendLine($"dwMipMapCount: {dwMipMapCount}");
                sb.AppendLine($"ddspf: ");
                sb.AppendLine(ddspf.ToString());
                sb.AppendLine($"dwCaps: 0x{dwCaps.ToString("X")}");
                sb.AppendLine($"dwCaps2: {dwCaps2}");
                sb.AppendLine($"dwCaps3: {dwCaps3}");
                sb.AppendLine($"dwCaps4: {dwCaps4}");
                sb.AppendLine($"dwReserved2: {dwReserved2}");
                sb.AppendLine("--END DDS_HEADER--");
                return sb.ToString();
            }
        }

        
        /// <summary>
        /// Contains information about DDS Pixel Format.
        /// </summary>
        internal class DDS_PIXELFORMAT
        {
            public int dwSize;
            public int dwFlags;
            public int dwFourCC;
            public int dwRGBBitCount;
            public uint dwRBitMask;
            public uint dwGBitMask;
            public uint dwBBitMask;
            public uint dwABitMask;

            public DDS_PIXELFORMAT()
            {
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("--DDS_PIXELFORMAT--");
                sb.AppendLine($"dwSize: {dwSize}");
                sb.AppendLine($"dwFlags: 0x{dwFlags.ToString("X")}");  // As hex
                sb.AppendLine($"dwFourCC: 0x{dwFourCC.ToString("X")}");  // As Hex
                sb.AppendLine($"dwRGBBitCount: {dwRGBBitCount}");
                sb.AppendLine($"dwRBitMask: 0x{dwRBitMask.ToString("X")}");  // As Hex
                sb.AppendLine($"dwGBitMask: 0x{dwGBitMask.ToString("X")}");  // As Hex
                sb.AppendLine($"dwBBitMask: 0x{dwBBitMask.ToString("X")}");  // As Hex
                sb.AppendLine($"dwABitMask: 0x{dwABitMask.ToString("X")}");  // As Hex
                sb.AppendLine("--END DDS_PIXELFORMAT--");
                return sb.ToString();
            }
        }


        /// <summary>
        /// Builds a header for DDS file format using provided information.
        /// </summary>
        /// <param name="Mips">Number of mips in image.</param>
        /// <param name="Height">Image Height.</param>
        /// <param name="Width">Image Width.</param>
        /// <param name="surfaceformat">DDS FourCC.</param>
        /// <returns>Header for DDS file.</returns>
        public static DDS_HEADER Build_DDS_Header(int Mips, int Height, int Width, ImageEngineFormat surfaceformat)
        {
            DDS_HEADER header = new DDS_HEADER();
            header.dwSize = 124;
            header.dwFlags = 0x1 | 0x2 | 0x4 | 0x0 | 0x1000 | (Mips != 1 ? 0x20000 : 0x0) | 0x0 | 0x0;  // Flags to denote fields: DDSD_CAPS = 0x1 | DDSD_HEIGHT = 0x2 | DDSD_WIDTH = 0x4 | DDSD_PITCH = 0x8 | DDSD_PIXELFORMAT = 0x1000 | DDSD_MIPMAPCOUNT = 0x20000 | DDSD_LINEARSIZE = 0x80000 | DDSD_DEPTH = 0x800000
            header.dwWidth = Width;
            header.dwHeight = Height;
            header.dwCaps = 0x1000 | (Mips != 1 ? 0 : (0x8 | 0x400000));  // Flags are: 0x8 = Optional: Used for mipmapped textures | 0x400000 = DDSCAPS_MIMPAP | 0x1000 = DDSCAPS_TEXTURE
            header.dwMipMapCount = Mips == 1 ? 1 : Mips;

            DDS_PIXELFORMAT px = new DDS_PIXELFORMAT();
            px.dwSize = 32;
            px.dwFourCC = (int)surfaceformat;
            px.dwFlags = 4;

            switch (surfaceformat)
            {
                case ImageEngineFormat.DDS_ATI2_3Dc:
                    px.dwFlags |= 0x80000;
                    header.dwPitchOrLinearSize = (int)(Width * Height);
                    break;
                case ImageEngineFormat.DDS_ATI1:
                    header.dwFlags |= 0x80000;  
                    header.dwPitchOrLinearSize = (int)(Width * Height / 2f);
                    break;
                case ImageEngineFormat.DDS_G8_L8:
                    px.dwFlags = 0x20000;
                    header.dwPitchOrLinearSize = Width * 8; // maybe?
                    header.dwFlags |= 0x8;
                    px.dwRGBBitCount = 8;
                    px.dwRBitMask = 0xFF;
                    px.dwFourCC = 0x0;
                    break;
                case ImageEngineFormat.DDS_ARGB:
                    px.dwFlags = 0x41;
                    px.dwFourCC = 0x0;
                    px.dwRGBBitCount = 32;
                    px.dwRBitMask = 0xFF0000;
                    px.dwGBitMask = 0xFF00;
                    px.dwBBitMask = 0xFF;
                    px.dwABitMask = 0xFF000000;
                    break;
                case ImageEngineFormat.DDS_V8U8:
                    px.dwFourCC = 0x0;
                    px.dwFlags = 0x80000;  // 0x80000 not actually a valid value....
                    px.dwRGBBitCount = 16;
                    px.dwRBitMask = 0xFF;
                    px.dwGBitMask = 0xFF00;
                    break;
            }
            

            header.ddspf = px;
            return header;
        }


        /// <summary>
        /// Write DDS header to stream via BinaryWriter.
        /// </summary>
        /// <param name="header">Populated DDS header by Build_DDS_Header.</param>
        /// <param name="writer">Stream to write to.</param>
        public static void Write_DDS_Header(DDS_HEADER header, BinaryWriter writer)
        {
            // KFreon: Write magic number ("DDS")
            writer.Write(0x20534444);

            // KFreon: Write all header fields regardless of filled or not
            writer.Write(header.dwSize);
            writer.Write(header.dwFlags);
            writer.Write(header.dwHeight);
            writer.Write(header.dwWidth);
            writer.Write(header.dwPitchOrLinearSize);
            writer.Write(header.dwDepth);
            writer.Write(header.dwMipMapCount);

            // KFreon: Write reserved1
            for (int i = 0; i < 11; i++)
                writer.Write(0);

            // KFreon: Write PIXELFORMAT
            DDS_PIXELFORMAT px = header.ddspf;
            writer.Write(px.dwSize);
            writer.Write(px.dwFlags);
            writer.Write(px.dwFourCC);
            writer.Write(px.dwRGBBitCount);
            writer.Write(px.dwRBitMask);
            writer.Write(px.dwGBitMask);
            writer.Write(px.dwBBitMask);
            writer.Write(px.dwABitMask);

            writer.Write(header.dwCaps);
            writer.Write(header.dwCaps2);
            writer.Write(header.dwCaps3);
            writer.Write(header.dwCaps4);
            writer.Write(header.dwReserved2);
        }
        #endregion Header Stuff


        #region Saving
        /// <summary>
        /// Writes a block compressed DDS to stream. Uses format specific function to compress and write blocks.
        /// </summary>
        /// <param name="MipMaps">List of MipMaps to save. Pixels only.</param>
        /// <param name="Destination">Stream to save to.</param>
        /// <param name="header">Header of DDS to use.</param>
        /// <param name="CompressBlock">Function to compress and write blocks with.</param>
        /// <returns>True on success.</returns>
        internal static bool WriteBlockCompressedDDS(List<MipMap> MipMaps, Stream Destination, DDS_HEADER header, Func<byte[], byte[]> CompressBlock)
        {
            Action<BinaryWriter, Stream, int, int> PixelWriter = (writer, pixels, width, height) =>
            {
                byte[] texel = DDSGeneral.GetTexel(pixels, width, height);
                byte[] CompressedBlock = CompressBlock(texel);
                writer.Write(CompressedBlock);
            };

            return DDSGeneral.WriteDDS(MipMaps, Destination, header, PixelWriter, true);
        }


        /// <summary>
        /// Writes a DDS file using a format specific function to write pixels.
        /// </summary>
        /// <param name="MipMaps">List of MipMaps to save. Pixels only.</param>
        /// <param name="Destination">Stream to save to.</param>
        /// <param name="header">Header to use.</param>
        /// <param name="PixelWriter">Function to write pixels. Optionally also compresses blocks before writing.</param>
        /// <param name="isBCd">True = Block Compressed DDS. Performs extra manipulation to get and order Texels.</param>
        /// <returns>True on success.</returns>
        internal static bool WriteDDS(List<MipMap> MipMaps, Stream Destination, DDS_HEADER header, Action<BinaryWriter, Stream, int, int> PixelWriter, bool isBCd)
        {
            try
            {
                using (BinaryWriter writer = new BinaryWriter(Destination, Encoding.Default, true))
                {
                    Write_DDS_Header(header, writer);
                    for (int m = 0; m < MipMaps.Count ; m++)
                    {
                        MemoryStream mipmap = MipMaps[m].Data;
                        mipmap.Seek(0, SeekOrigin.Begin);
                        WriteMipMap(mipmap, MipMaps[m].Width, MipMaps[m].Height, PixelWriter, isBCd, writer);
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
        }


        /// <summary>
        /// Write a mipmap to a stream using a format specific pixel writing function.
        /// </summary>
        /// <param name="pixelData">Pixels of mipmap.</param>
        /// <param name="Width">Mipmap Width.</param>
        /// <param name="Height">Mipmap Height.</param>
        /// <param name="PixelWriter">Function to write pixels with. Also compresses if block compressed texture.</param>
        /// <param name="isBCd">True = Block Compressed DDS.</param>
        /// <param name="writer">Stream to write to.</param>
        private static void WriteMipMap(Stream pixelData, int Width, int Height, Action<BinaryWriter, Stream, int, int> PixelWriter, bool isBCd, BinaryWriter writer)
        {
            int bitsPerScanLine = 4 * Width;  // KFreon: Bits per image line.

            // KFreon: Loop over rows and columns, doing extra moving if Block Compressed to accommodate texels.
            for (int h = 0; h < Height; h += (isBCd ? 4 : 1))
            {
                for (int w = 0; w < Width; w += (isBCd ? 4 : 1))
                {
                    PixelWriter(writer, pixelData, Width, Height);
                    if (isBCd && w != Width - 4 && Width > 4 && Height > 4)  // KFreon: Only do this if dimensions are big enough
                        pixelData.Seek(-(bitsPerScanLine * 4) + 4 * 4, SeekOrigin.Current);  // Not at an row end texel. Moves back up to read next texel in row.
                }

                if (isBCd && Width > 4 && Height > 4)  // Only do this jump if dimensions allow it
                    pixelData.Seek(-bitsPerScanLine + 4 * 4, SeekOrigin.Current);  // Row end texel. Just need to add 1.
            }
        }
        #endregion Save


        #region Loading
        /// <summary>
        /// Loads an uncompressed DDS image given format specific Pixel Reader
        /// </summary>
        /// <param name="stream">Stream containing entire image. NOT just pixels.</param>
        /// <param name="PixelReader">Function that knows how to read a pixel. Different for each format (V8U8, BGRA)</param>
        /// <returns></returns>
        internal static List<MipMap> LoadUncompressed(Stream stream, Func<Stream, List<byte>> PixelReader)
        {
            // KFreon: Necessary to move stream position along to pixel data.
            DDS_HEADER header = null;
            Format format = ImageFormats.ParseDDSFormat(stream, out header);

            List<MipMap> MipMaps = new List<MipMap>();

            int newWidth = header.dwWidth;
            int newHeight = header.dwHeight;

            // KFreon: Read data
            int estimatedMips = header.dwMipMapCount == 0 ? EstimateNumMipMaps(newWidth, newHeight) + 1 : header.dwMipMapCount;

            for (int m = 0; m < estimatedMips; m++)
            {
                // KFreon: Since mip count is an estimate, check if there are any mips left to read.
                if (stream.Position >= stream.Length)
                    break;

                int count = 0;
                byte[] mipmap = new byte[newHeight * newWidth * 4];
                for (int y = 0; y < newHeight; y++)
                {
                    for (int x = 0; x < newWidth; x++)
                    {
                        List<byte> bgr = PixelReader(stream);  // KFreon: Reads pixel using a method specific to the format as provided
                        mipmap[count++] = bgr[0];
                        mipmap[count++] = bgr[1];
                        mipmap[count++] = bgr[2];
                        mipmap[count++] = 0xFF;
                    }
                }
                MipMaps.Add(new MipMap(UsefulThings.RecyclableMemoryManager.GetStream(mipmap), newWidth, newHeight));

                newWidth /= 2;
                newHeight /= 2;
            }

            return MipMaps;
        }


        /// <summary>
        /// Loads a block compressed (BCx) texture.
        /// </summary>
        /// <param name="compressed">Compressed image data.</param>
        /// <param name="DecompressBlock">Format specific block decompressor.</param>
        /// <returns>16 pixel BGRA channels.</returns>
        internal static List<MipMap> LoadBlockCompressedTexture(Stream compressed, Func<Stream, List<byte[]>> DecompressBlock)
        {
            DDS_HEADER header;
            Format format = ImageFormats.ParseDDSFormat(compressed, out header);

            List<MipMap> MipMaps = new List<MipMap>();

            int mipWidth = header.dwWidth;
            int mipHeight = header.dwHeight;

            int estimatedMips = header.dwMipMapCount == 0 ? EstimateNumMipMaps(mipWidth, mipHeight) + 1 : header.dwMipMapCount;
            long mipOffset = 128;  // Includes header

            for (int m = 0; m < estimatedMips; m++)
            {
                MemoryStream mipmap = UsefulThings.RecyclableMemoryManager.GetStream(4 * (int)mipWidth * (int)mipHeight);

                // Loop over rows and columns NOT pixels
                int compressedLineSize = format.BlockSize * mipWidth / 4;
                int bitsPerScanline = 4 * (int)mipWidth;
                ParallelOptions po = new ParallelOptions();
                po.MaxDegreeOfParallelism = -1;
                int texelCount = mipHeight / 4;
                if (texelCount == 0)
                    mipmap.Write(new byte[format.BlockSize], 0, format.BlockSize);
                else
                {
                    Parallel.For(0, texelCount, po, (rowr, loopstate) =>
                    {
                        int row = rowr;
                        using (MemoryStream DecompressedLine = ReadBCMipLine(compressed, mipHeight, mipWidth, bitsPerScanline, mipOffset, compressedLineSize, row, DecompressBlock))
                        {
                            if (DecompressedLine != null)
                                lock (mipmap)
                                {
                                    mipmap.Position = rowr * bitsPerScanline * 4;
                                    DecompressedLine.WriteTo(mipmap);
                                }
                            else
                                loopstate.Break();
                        }
                    });
                }

                if (mipmap.Length == 0)
                    break;

                MipMaps.Add(new MipMap(mipmap, mipWidth, mipHeight));

                mipOffset += mipWidth * mipHeight * format.BlockSize / 16;
                mipWidth /= 2;
                mipHeight /= 2;
            }
            
            return MipMaps;
        }

        private static MemoryStream ReadBCMipLine(Stream compressed, int mipHeight, int mipWidth, int bitsPerScanLine, long mipOffset, int compressedLineSize, int rowIndex, Func<Stream, List<byte[]>> DecompressBlock)
        {
            int bitsPerPixel = 4;

            MemoryStream DecompressedLine = UsefulThings.RecyclableMemoryManager.GetStream(bitsPerScanLine * 4);

            // KFreon: Read compressed line into new stream for multithreading purposes
            MemoryStream CompressedLine = UsefulThings.RecyclableMemoryManager.GetStream(compressedLineSize);
            lock (compressed)
            {
                // KFreon: Seek to correct texel
                compressed.Position = mipOffset + rowIndex * compressedLineSize;  // +128 = header size

                // KFreon: since mip count is an estimate, check to see if there are any mips left to read.
                if (compressed.Position >= compressed.Length)
                    return null;

                // KFreon: Read compressed line
                CompressedLine.ReadFrom(compressed, compressedLineSize);
            }
            CompressedLine.Position = 0;

            // KFreon: Read texels in row
            for (int column = 0; column < mipWidth; column += 4)
            {
                // decompress 
                List<byte[]> decompressed = DecompressBlock(CompressedLine);
                byte[] blue = decompressed[0];
                byte[] green = decompressed[1];
                byte[] red = decompressed[2];
                byte[] alpha = decompressed[3];


                // Write texel
                int TopLeft = column * bitsPerPixel;// + rowIndex * 4 * bitsPerScanLine;  // Top left corner of texel IN BYTES (i.e. expanded pixels to 4 channels)
                DecompressedLine.Seek(TopLeft, SeekOrigin.Begin);
                byte[] block = new byte[16];
                for (int i = 0; i < 16; i += 4)
                {
                    // BGRA
                    for (int j = 0; j < 16; j += 4)
                    {
                        block[j] = blue[i + (j >> 2)];
                        block[j + 1] = green[i + (j >> 2)];
                        block[j + 2] = red[i + (j >> 2)];
                        block[j + 3] = alpha[i + (j >> 2)];
                    }
                    DecompressedLine.Write(block, 0, 16);

                    // Go one line of pixels down (bitsPerScanLine), then to the left side of the texel (4 pixels back from where it finished)
                    DecompressedLine.Seek(bitsPerScanLine - bitsPerPixel * 4, SeekOrigin.Current);
                }
            }
            return DecompressedLine;
        }
        #endregion Loading


        #region Block Decompression
        /// <summary>
        /// Decompresses an 8 bit channel.
        /// </summary>
        /// <param name="compressed">Compressed image data.</param>
        /// <param name="isSigned">true = use signed alpha range (-254 -- 255), false = 0 -- 255</param>
        /// <returns>Single channel decompressed (16 bits).</returns>
        internal static byte[] Decompress8BitBlock(Stream compressed, bool isSigned)
        {
            byte[] DecompressedBlock = new byte[16];

            // KFreon: Read min and max colours (not necessarily in that order)
            byte[] block = new byte[8];
            compressed.Read(block, 0, 8);

            byte min = block[0];
            byte max = block[1];

            byte[] Colours = Build8BitPalette(min, max, isSigned);

            // KFreon: Decompress pixels
            ulong bitmask = (ulong)block[2] << 0 | (ulong)block[3] << 8 | (ulong)block[4] << 16 |   // KFreon: Read all 6 compressed bytes into single 
                (ulong)block[5] << 24 | (ulong)block[6] << 32 | (ulong)block[7] << 40;


            // KFreon: Bitshift and mask compressed data to get 3 bit indicies, and retrieve indexed colour of pixel.
            for (int i = 0; i < 16; i++)
                DecompressedBlock[i] = (byte)Colours[bitmask >> (i * 3) & 0x7];

            return DecompressedBlock;
        }

        /// <summary>
        /// Decompresses a 3 channel (RGB) block.
        /// </summary>
        /// <param name="compressed">Compressed image data.</param>
        /// <param name="isDXT1">True = DXT1, otherwise false.</param>
        /// <returns>16 pixel BGRA channels.</returns>
        internal static List<byte[]> DecompressRGBBlock(Stream compressed, bool isDXT1)
        {
            int[] DecompressedBlock = new int[16];

            ushort min;
            ushort max;
            byte[] pixels;
            int[] Colours;
            using (BinaryReader reader = new BinaryReader(compressed, Encoding.Default, true))
            {
                // Read min max colours
                min = (ushort)reader.ReadInt16();
                max = (ushort)reader.ReadInt16();
                Colours = BuildRGBPalette(min, max, isDXT1);

                // Decompress pixels
                pixels = reader.ReadBytes(4);
            }

                
            for (int i = 0; i < 16; i += 4)
            {
                //byte bitmask = (byte)compressed.ReadByte();
                byte bitmask = pixels[i / 4];
                for (int j = 0; j < 4; j++)
                    DecompressedBlock[i + j] = Colours[bitmask >> (2 * j) & 0x03];
            }

            // KFreon: Decode into BGRA
            List<byte[]> DecompressedChannels = new List<byte[]>(4);
            byte[] red = new byte[16];
            byte[] green = new byte[16];
            byte[] blue = new byte[16];
            byte[] alpha = new byte[16];
            DecompressedChannels.Add(blue);
            DecompressedChannels.Add(green);
            DecompressedChannels.Add(red);
            DecompressedChannels.Add(alpha);

            for (int i = 0; i < 16; i++)
            {
                int colour = DecompressedBlock[i];
                var rgb = ReadDXTColour(colour);
                red[i] = rgb[0];
                green[i] = rgb[1];
                blue[i] = rgb[2];
                alpha[i] = 0xFF;//(byte)(colour == 0 && max > min ? 0x0 : 0xFF);
            }
            return DecompressedChannels;
        }
        #endregion


        #region Block Compression
        /// <summary>
        /// Compresses RGB texel into DXT colours.
        /// </summary>
        /// <param name="texel">4x4 Texel to compress.</param>
        /// <param name="min">Minimum Colour value.</param>
        /// <param name="max">Maximum Colour value.</param>
        /// <returns>DXT Colours</returns>
        internal static int[] CompressRGBFromTexel(byte[] texel, out int min, out int max)
        {
            int[] RGB = new int[16];
            int count = 0;
            for (int i = 0; i < 64; i += 16) // texel row
            {
                for (int j = 0; j < 16; j += 4)  // pixels in row incl BGRA
                {
                    int pixelColour = BuildDXTColour(texel[i + j + 2], texel[i + j + 1], texel[i + j]);
                    RGB[count++] = pixelColour;
                }
            }

            min = RGB.Min();
            max = RGB.Max();

            return RGB;
        }


        /// <summary>
        /// Compresses RGB channels using Block Compression.
        /// </summary>
        /// <param name="texel">16 pixel texel to compress.</param>
        /// <param name="isDXT1">Set true if DXT1.</param>
        /// <returns>8 byte compressed texel.</returns>
        public static byte[] CompressRGBBlock(byte[] texel, bool isDXT1)
        {
            byte[] CompressedBlock = new byte[8];

            // Get Min and Max colours
            int min = 0;
            int max = 0;
            int[] texelColours = CompressRGBFromTexel(texel, out min, out max);

            if (isDXT1)
            {
                // KFreon: Check alpha (every 4 bytes)
                for (int i = 3; i < texel.Length; i += 4)
                {
                    if (texel[i] != 0xFF) // Alpha found, switch min and max
                    {
                        int temp = min;
                        min = max;
                        max = temp;
                        break;
                    }
                }
            }


            // Write colours
            byte[] colour0 = BitConverter.GetBytes(min);
            byte[] colour1 = BitConverter.GetBytes(max);

            CompressedBlock[0] = colour0[0];
            CompressedBlock[1] = colour0[1];

            CompressedBlock[2] = colour1[0];
            CompressedBlock[3] = colour1[1];

            // Build interpolated palette
            int[] Colours = BuildRGBPalette(min, max, isDXT1);

            // Compress pixels
            for (int i = 0; i < 16; i += 4) // each "row" of 4 pixels is a single byte
            {
                byte fourIndicies = 0;
                for (int j = 0; j < 4; j++)
                {
                    int colour = texelColours[i + j];
                    int index = Colours.IndexOfMin(c => Math.Abs(colour - c));
                    fourIndicies |= (byte)(index << (2 * j));
                }
                CompressedBlock[i / 4 + 4] = fourIndicies;
            }

            return CompressedBlock;
        }

        /// <summary>
        /// Compresses single channel using Block Compression.
        /// </summary>
        /// <param name="texel">4 channel Texel to compress.</param>
        /// <param name="channel">0-3 (BGRA)</param>
        /// <param name="isSigned">true = uses alpha range -255 -- 255, else 0 -- 255</param>
        /// <returns>8 byte compressed texel.</returns>
        public static byte[] Compress8BitBlock(byte[] texel, int channel, bool isSigned)
        {
            // KFreon: Get min and max
            byte min = byte.MaxValue;
            byte max = byte.MinValue;
            int count = channel;
            for (int i = 0; i < 16; i++)
            {
                byte colour = texel[count];
                if (colour > max)
                    max = colour;
                else if (colour < min)
                    min = colour;

                count += 4; // skip to next entry in channel
            }

            // Build Palette
            byte[] Colours = Build8BitPalette(min, max, isSigned);

            // Compress Pixels
            ulong line = 0;
            count = channel;
            List<byte> indicies = new List<byte>();
            for (int i = 0; i < 16; i++)
            {
                byte colour = texel[count];
                byte index = (byte)Colours.IndexOfMin(c => Math.Abs(colour - c));
                indicies.Add(index);
                line |= (ulong)index << (i * 3); 
                count += 4;  // Only need 1 channel
            }

            byte[] CompressedBlock = new byte[8];
            byte[] compressed = BitConverter.GetBytes(line);
            CompressedBlock[0] = min;
            CompressedBlock[1] = max;
            for (int i = 2; i < 8; i++)
                CompressedBlock[i] = compressed[i - 2];

            return CompressedBlock;
        }
        #endregion Block Compression


        /// <summary>
        /// Gets 4x4 texel block from stream.
        /// </summary>
        /// <param name="pixelData">Image pixels.</param>
        /// <param name="Width">Width of image.</param>
        /// <param name="Height">Height of image.</param>
        /// <returns>4x4 texel.</returns>
        internal static byte[] GetTexel(Stream pixelData, int Width, int Height)
        {
            byte[] texel = new byte[16 * 4]; // 16 pixels, 4 bytes per pixel

            // KFreon: Edge case for when dimensions are too small for texel
            int count = 0;
            if (Width < 4 || Height < 4)
            {
                for (int h = 0; h < Height; h++)
                    for (int w = 0; w < Width; w++)
                        for (int i = 0; i < 4; i++)
                            texel[count++] = (byte)pixelData.ReadByte();

                return texel;
            }

            // KFreon: Normal operation. Read 4x4 texel row by row.
            int bitsPerScanLine = 4 * Width;
            for (int i = 0; i < 64; i += 16)  // pixel rows
            {
                pixelData.Read(texel, i, 16);
                /*for (int j = 0; j < 16; j += 4)  // pixels in row
                    for (int k = 0; k < 4; k++) // BGRA
                        texel[i + j + k] = (byte)pixelData.ReadByte();*/

                pixelData.Seek(bitsPerScanLine - 4 * 4, SeekOrigin.Current);  // Seek to next line of texel
            }
                

            return texel;
        }


        #region Palette/Colour
        /// <summary>
        /// Reads a packed DXT colour into RGB
        /// </summary>
        /// <param name="colour">Colour to convert to RGB</param>
        /// <returns>RGB bytes</returns>
        private static byte[] ReadDXTColour(int colour)
        {
            // Read RGB 5:6:5 data
            var b = (colour & 0x1F);
            var g = (colour & 0x7E0) >> 5;
            var r = (colour & 0xF800) >> 11;


            // Expand to 8 bit data
            /*byte r1 = (byte)Math.Round(r * 255f / 31f);
            byte g1 = (byte)Math.Round(g * 255f / 63f);
            byte b1 = (byte)Math.Round(b * 255f / 31f);*/

            byte r1 = RedBlueDecompressionConstants[r];
            byte g1 = GreenDecompressionConstants[g];
            byte b1 = RedBlueDecompressionConstants[b];

            return new byte[3] { r1, g1, b1 };
        }


        /// <summary>
        /// Creates a packed DXT colour from RGB.
        /// </summary>
        /// <param name="r">Red byte.</param>
        /// <param name="g">Green byte.</param>
        /// <param name="b">Blue byte.</param>
        /// <returns>DXT Colour</returns>
        private static int BuildDXTColour(byte r, byte g, byte b)
        {
            // Compress to 5:6:5
            /*byte r1 = (byte)(Math.Round(r * 31f / 255f));
            byte g1 = (byte)(Math.Round(g * 63f / 255f));
            byte b1 = (byte)(Math.Round(b * 31f / 255f));*/
            byte r1 = RedBlueCompressionConstants[r];
            byte g1 = GreenCompressionConstants[g];
            byte b1 = RedBlueCompressionConstants[b];

            return r1 << 11 | g1 << 5 | b1;
        }


        /// <summary>
        /// Builds palette for 8 bit channel.
        /// </summary>
        /// <param name="min">First main colour (often actually minimum)</param>
        /// <param name="max">Second main colour (often actually maximum)</param>
        /// <param name="isSigned">true = sets signed alpha range (-254 -- 255), false = 0 -- 255</param>
        /// <returns>8 byte colour palette.</returns>
        internal static byte[] Build8BitPalette(byte min, byte max, bool isSigned)
        {
            byte[] Colours = new byte[8];
            Colours[0] = min;
            Colours[1] = max;

            // KFreon: Choose which type of interpolation is required
            if (min > max)
            {
                // KFreon: Interpolate other colours
                for (int i = 2; i < 8; i++)
                {
                    double test = ((8 - i) * min + (i - 1) * max) / 7.0f;
                    Colours[i] = (byte)test;
                }
            }
            else
            {
                // KFreon: Interpolate other colours and add Opacity or something...
                for (int i = 2; i < 6; i++)
                {
                    double test = ((8 - i) * min + (i - 1) * max) / 5.0f;
                    Colours[i] = (byte)test;
                }
                Colours[6] = (byte)(isSigned ? -254 : 0);  // KFreon: snorm and unorm have different alpha ranges
                Colours[7] = 255;
            }

            return Colours;
        }

        

        /// <summary>
        /// Builds a palette for RGB channels (DXT only)
        /// </summary>
        /// <param name="min">First main colour. Often actually the minumum.</param>
        /// <param name="max">Second main colour. Often actually the maximum.</param>
        /// <param name="isDXT1">true = use DXT1 format (1 bit alpha)</param>
        /// <returns>4 Colours as integers.</returns>
        public static int[] BuildRGBPalette(int min, int max, bool isDXT1)
        {
            int[] Colours = new int[4];
            Colours[0] = min;
            Colours[1] = max;

            var minrgb = ReadDXTColour(min);
            var maxrgb = ReadDXTColour(max);

            // Interpolate other 2 colours
            if (min > max || !isDXT1)
            {
                var r = (byte)(2 / 3f * minrgb[0] + 1 / 3f * maxrgb[0]);
                var g = (byte)(2 / 3f * minrgb[1] + 1 / 3f * maxrgb[1]);
                var b = (byte)(2 / 3f * minrgb[2] + 1 / 3f * maxrgb[2]);

                Colours[2] = BuildDXTColour(r, g, b);

                r = (byte)(1 / 3f * minrgb[0] + 2 / 3f * maxrgb[0]);
                g = (byte)(1 / 3f * minrgb[1] + 2 / 3f * maxrgb[1]);
                b = (byte)(1 / 3f * minrgb[2] + 2 / 3f * maxrgb[2]);

                Colours[3] = BuildDXTColour(r, g, b);
            }
            else
            {
                // KFreon: Only for dxt1
                var r = (byte)(1 / 2f * minrgb[0] + 1 / 2f * maxrgb[0]);
                var g = (byte)(1 / 2f * minrgb[1] + 1 / 2f * maxrgb[1]);
                var b = (byte)(1 / 2f * minrgb[2] + 1 / 2f * maxrgb[2]);
            
                Colours[2] = BuildDXTColour(r, g, b);
                Colours[3] = 0;
            }

            return Colours;
        }
        #endregion Palette/Colour
        

        /// <summary>
        /// Estimates number of MipMaps for a given width and height EXCLUDING the top one.
        /// i.e. If output is 10, there are 11 mipmaps total.
        /// </summary>
        /// <param name="Width">Image Width.</param>
        /// <param name="Height">Image Height.</param>
        /// <returns>Number of mipmaps expected for image.</returns>
        internal static int EstimateNumMipMaps(int Width, int Height)
        {
            int limitingDimension = Width > Height ? Height : Width;
            return (int)Math.Log(limitingDimension, 2); // There's 10 mipmaps besides the main top one.
        }
    }
}

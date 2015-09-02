﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsefulThings;

namespace CSharpImageLibrary
{
    /// <summary>
    /// Provides Block Compressed 3 functionality. Also known as DXT4 and 5.
    /// </summary>
    public static class BC3
    {
        /// <summary>
        /// Load important information from image file.
        /// </summary>
        /// <param name="imageFile">Path to image file.</param>
        /// <returns>16 byte BGRA channels as stream.</returns>
        internal static List<MipMap> Load(string imageFile)
        {
            using (FileStream fs = new FileStream(imageFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                return Load(fs);
        }


        /// <summary>
        /// Load important information from image stream.
        /// </summary>
        /// <param name="compressed">Stream containing entire image file. NOT just pixels.</param>
        /// <returns>16 byte BGRA channels as stream.</returns>
        internal static List<MipMap> Load(Stream compressed)
        {
            return DDSGeneral.LoadBlockCompressedTexture(compressed, DecompressBC3);
        }


        /// <summary>
        /// Reads a 16 byte BC3 compressed block from stream.
        /// </summary>
        /// <param name="compressed">BC3 compressed image stream.</param>
        /// <returns>List of BGRA channels.</returns>
        private static List<byte[]> DecompressBC3(Stream compressed)
        {
            byte[] alpha = DDSGeneral.Decompress8BitBlock(compressed, false);
            List<byte[]> DecompressedBlock = DDSGeneral.DecompressRGBBlock(compressed, false);
            DecompressedBlock[3] = alpha;
            return DecompressedBlock;
        }


        /// <summary>
        /// Compress texel to 16 byte BC3 compressed block.
        /// </summary>
        /// <param name="texel">4x4 BGRA set of pixels.</param>
        /// <returns>16 byte BC3 compressed block.</returns>
        private static byte[] CompressBC3Block(byte[] texel)
        {
            // Compress Alpha
            byte[] Alpha = DDSGeneral.Compress8BitBlock(texel, 3, false);

            // Compress Colour
            byte[] RGB = DDSGeneral.CompressRGBBlock(texel, false);

            return Alpha.Concat(RGB).ToArray(Alpha.Length + RGB.Length);
        }


        /// <summary>
        /// Saves a texture using BC3 compression.
        /// </summary>
        /// <param name="Destination">Stream to save to.</param>
        /// <param name="MipMaps">List of MipMaps to save. Pixels only.</param>
        /// <returns>True if saved successfully.</returns>
        internal static bool Save(List<MipMap> MipMaps, Stream Destination)
        {
            DDSGeneral.DDS_HEADER header = DDSGeneral.Build_DDS_Header(MipMaps.Count, MipMaps[0].Height, MipMaps[0].Width, ImageEngineFormat.DDS_DXT5);
            return DDSGeneral.WriteBlockCompressedDDS(MipMaps, Destination, header, CompressBC3Block);
        }
    }
}

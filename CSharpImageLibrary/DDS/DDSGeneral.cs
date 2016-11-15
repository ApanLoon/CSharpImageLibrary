﻿using CSharpImageLibrary.Headers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using UsefulThings;

namespace CSharpImageLibrary.DDS
{
    /// <summary>
    /// Provides general functions specific to DDS format
    /// </summary>
    public static class DDSGeneral
    {
        /// <summary>
        /// Value at which alpha is included in DXT1 conversions. i.e. pixels lower than this threshold are made 100% transparent, and pixels higher are made 100% opaque.
        /// </summary>
        public static float DXT1AlphaThreshold = 0.2f;


        #region Loading
        private static MipMap ReadUncompressedMipMap(MemoryStream stream, int mipOffset, int mipWidth, int mipHeight, DDS_Header.DDS_PIXELFORMAT ddspf)
        {
            byte[] data = stream.GetBuffer();
            byte[] mipmap = new byte[mipHeight * mipWidth * 4];
            DDS_Decoders.ReadUncompressed(data, mipOffset, mipmap, mipWidth * mipHeight, ddspf);

            bool alphaPresent = ddspf.dwABitMask != 0;
            return new MipMap(mipmap, mipWidth, mipHeight, alphaPresent);
        }

        private static MipMap ReadCompressedMipMap(MemoryStream compressed, int mipWidth, int mipHeight, int blockSize, int mipOffset, Action<byte[], int, byte[], int, int> DecompressBlock)
        {
            // Gets stream as data. Note that this array isn't necessarily the correct size. Likely to have garbage at the end.
            // Don't want to use ToArray as that creates a new array. Don't want that.
            byte[] CompressedData = compressed.GetBuffer();

            byte[] decompressedData = new byte[4 * mipWidth * mipHeight];
            int decompressedRowLength = mipWidth * 4;
            int texelRowSkip = decompressedRowLength * 4;

            int texelCount = (mipWidth * mipHeight) / 16;
            int numTexelsInRow = mipWidth / 4;
            if (texelCount != 0)
            {
                Action<int, ParallelLoopState> action = new Action<int, ParallelLoopState>((texelIndex, loopstate) =>
                {
                    int compressedPosition = mipOffset + texelIndex * blockSize;
                    int decompressedStart = (int)(texelIndex / numTexelsInRow) * texelRowSkip + (texelIndex % numTexelsInRow) * 16;
                    DecompressBlock(CompressedData, compressedPosition, decompressedData, decompressedStart, decompressedRowLength);

                    // TODO: Cancellation
                    /*if (ImageEngine.EnableThreading)
                        loopstate.Break();
                    else if (!ImageEngine.EnableThreading)
                        return;*/
                });

                // Actually perform decompression using threading, no threading, or GPU.
                if (ImageEngine.EnableGPUAcceleration)
                    Debugger.Break();  // TODO: GPU acceleration
                else if (ImageEngine.EnableThreading)
                    Parallel.For(0, texelCount, (texelIndex, loopstate) => action(texelIndex, loopstate));
                else
                    for (int texelIndex = 0; texelIndex < texelCount; texelIndex++)
                        action(texelIndex, null);
            }
            // No else here cos the lack of texels means it's below texel dimensions (4x4). So the resulting block is set to 0. Not ideal, but who sees 2x2 mipmaps?

            return new MipMap(decompressedData, mipWidth, mipHeight, true);  // All DXT can contain alpha
        }

        

        internal static List<MipMap> LoadDDS(MemoryStream compressed, DDS_Header header, int desiredMaxDimension)
        {           
            List<MipMap> MipMaps = new List<MipMap>();

            int mipWidth = header.Width;
            int mipHeight = header.Height;
            ImageEngineFormat format = header.Format;
            int blockSize = ImageFormats.GetBlockSize(format);

            int estimatedMips = header.dwMipMapCount;
            int mipOffset = 128;  // Includes header. 
            // TODO: Incorrect mip offset for DX10

            if (!EnsureMipInImage(compressed.Length, mipWidth, mipHeight, 4, format, out mipOffset))  // Update number of mips too
                estimatedMips = 1;

            if (estimatedMips == 0)
                estimatedMips = EstimateNumMipMaps(mipWidth, mipHeight);

            mipOffset = 128;  // Needs resetting after checking there's mips in this image.

            // KFreon: Decide which mip to start loading at - going to just load a few mipmaps if asked instead of loading all, then choosing later. That's slow.
            if (desiredMaxDimension != 0 && estimatedMips > 1)
            {
                if (!EnsureMipInImage(compressed.Length, mipWidth, mipHeight, desiredMaxDimension, format, out mipOffset))  // Update number of mips too
                    throw new InvalidDataException($"Requested mipmap does not exist in this image. Top Image Size: {mipWidth}x{mipHeight}, requested mip max dimension: {desiredMaxDimension}.");

                // Not the first mipmap. 
                if (mipOffset > 128)
                {

                    double divisor = mipHeight > mipWidth ? mipHeight / desiredMaxDimension : mipWidth / desiredMaxDimension;
                    mipHeight = (int)(mipHeight / divisor);
                    mipWidth = (int)(mipWidth / divisor);

                    if (mipWidth == 0 || mipHeight == 0)  // Reset as a dimension is too small to resize
                    {
                        mipHeight = header.Height;
                        mipWidth = header.Width;
                        mipOffset = 128;
                    }
                    else
                    {
                        // Update estimated mips due to changing dimensions.
                        estimatedMips = EstimateNumMipMaps(mipWidth, mipHeight);
                    }
                }
                else  // The first mipmap
                    mipOffset = 128;

            }

            // Move to requested mipmap
            compressed.Position = mipOffset;

            // Block Compressed texture chooser.
            Action<byte[], int, byte[], int, int> DecompressBCBlock = null;
            switch (format)
            {
                case ImageEngineFormat.DDS_DXT1:
                    DecompressBCBlock = DDS_Decoders.DecompressBC1Block;
                    break;
                case ImageEngineFormat.DDS_DXT2:
                case ImageEngineFormat.DDS_DXT3:
                    DecompressBCBlock = DDS_Decoders.DecompressBC2Block;
                    break;
                case ImageEngineFormat.DDS_DXT4:
                case ImageEngineFormat.DDS_DXT5:
                    DecompressBCBlock = DDS_Decoders.DecompressBC3Block;
                    break;
                case ImageEngineFormat.DDS_ATI1:
                    DecompressBCBlock = DDS_Decoders.DecompressATI1;
                    break;
                case ImageEngineFormat.DDS_ATI2_3Dc:
                    DecompressBCBlock = DDS_Decoders.DecompressATI2Block;
                    break;
            }

            // KFreon: Read mipmaps
            for (int m = 0; m < estimatedMips; m++)
            {
                // KFreon: If mip is too small, skip out. This happens most often with non-square textures. I think it's because the last mipmap is square instead of the same aspect.
                if (mipWidth <= 0 || mipHeight <= 0)  // Needed cos it doesn't throw when reading past the end for some reason.
                {
                    Debugger.Break();
                    break;
                }

                MipMap mipmap = null;
                if (ImageFormats.IsBlockCompressed(format))
                    mipmap = ReadCompressedMipMap(compressed, mipWidth, mipHeight, blockSize, mipOffset, DecompressBCBlock);
                else
                    mipmap = ReadUncompressedMipMap(compressed, mipOffset, mipWidth, mipHeight, header.ddspf);

                MipMaps.Add(mipmap);

                mipOffset += mipWidth * mipHeight * blockSize / 16; // Only used for BC textures
                mipWidth /= 2;
                mipHeight /= 2;
            }

            if (MipMaps.Count == 0)
                Debugger.Break();
            return MipMaps;
        }
        #endregion Loading

        #region Saving
        internal static byte[] Save(List<MipMap> mipMaps, ImageEngineFormat saveFormat)
        {
            // Set compressor for Block Compressed textures
            Action<byte[], int, int, byte[], int> compressor = null;
            switch (saveFormat)
            {
                case ImageEngineFormat.DDS_ATI1:
                    compressor = DDS_Encoders.CompressBC4Block;
                    break;
                case ImageEngineFormat.DDS_ATI2_3Dc:
                    compressor = DDS_Encoders.CompressBC5Block;
                    break;
                case ImageEngineFormat.DDS_DX10:
                    Debugger.Break();
                    break; // TODO: NOT SUPPORTED YET. DX10
                case ImageEngineFormat.DDS_DXT1:
                    compressor = DDS_Encoders.CompressBC1Block;
                    break;
                case ImageEngineFormat.DDS_DXT2:
                case ImageEngineFormat.DDS_DXT3:
                    compressor = DDS_Encoders.CompressBC2Block;
                    break;
                case ImageEngineFormat.DDS_DXT4:
                case ImageEngineFormat.DDS_DXT5:
                    compressor = DDS_Encoders.CompressBC3Block;
                    break;
            }

            // +1 to get the full size, not just the offset of the last mip.
            int fullSize = GetMipOffset(mipMaps.Count + 1, saveFormat, mipMaps[0].Width, mipMaps[0].Height);
            byte[] destination = new byte[fullSize];

            // Create header and write to destination
            DDS_Header header = new DDS_Header(mipMaps.Count, mipMaps[0].Height, mipMaps[0].Width, saveFormat);
            header.WriteToArray(destination, 0);

            int mipOffset = 128;
            int blockSize = ImageFormats.GetBlockSize(saveFormat);
            foreach (MipMap mipmap in mipMaps)
            {
                if (ImageFormats.IsBlockCompressed(saveFormat))
                    mipOffset = WriteCompressedMipMap(destination, mipOffset, mipmap, blockSize, compressor);
                else
                    mipOffset = WriteUncompressedMipMap(destination, mipOffset, mipmap, saveFormat, header.ddspf);
            }

            return destination;
        }


        static int WriteCompressedMipMap(byte[] destination, int mipOffset, MipMap mipmap, int blockSize, Action<byte[], int, int, byte[], int> compressor)
        {
            int destinationTexelCount = mipmap.Width * mipmap.Height / 16;
            int sourceLineLength = mipmap.Width * 4;
            int numTexelsInLine = mipmap.Width / 4;

            var mipWriter = new Action<int>(texelIndex =>
            {
                // Since this is the top corner of the first texel in a line, skip 4 pixel rows (texel = 4x4 pixels) and the number of rows down the bitmap we are already.
                int sourceLineOffset = sourceLineLength * 4 * (texelIndex / numTexelsInLine);  // Length in bytes x 3 lines x texel line index (how many texel sized lines down the image are we). Index / width will truncate, so for the first texel line, it'll be < 0. For the second texel line, it'll be < 1 and > 0.

                int sourceTopLeftCorner = ((texelIndex % numTexelsInLine) * 16) + sourceLineOffset; // *16 since its 4 pixels with 4 channels each. Index % numTexels will effectively reset each line.
                compressor(mipmap.Pixels, sourceTopLeftCorner, sourceLineLength, destination, mipOffset + texelIndex * blockSize);
            });

            // Choose an acceleration method.
            if (ImageEngine.EnableGPUAcceleration)
                Debugger.Break(); // TODO: GPU Accelerated saving
            else if (ImageEngine.EnableThreading)
                Parallel.For(0, destinationTexelCount, new ParallelOptions { MaxDegreeOfParallelism = ImageEngine.NumThreads }, mipWriter);
            else
                for (int i = 0; i < destinationTexelCount; i++)
                    mipWriter(i);

            return mipOffset + destinationTexelCount * blockSize;
        }

        static int WriteUncompressedMipMap(byte[] destination, int mipOffset, MipMap mipmap, ImageEngineFormat saveFormat, DDS_Header.DDS_PIXELFORMAT ddspf)
        {
            return DDS_Encoders.WriteUncompressed(mipmap.Pixels, destination, mipOffset, ddspf);
        }
        #endregion Saving

        #region Mipmap Management
        /// <summary>
        /// Ensures all Mipmaps are generated in MipMaps.
        /// </summary>
        /// <param name="MipMaps">MipMaps to check.</param>
        /// <param name="mergeAlpha">True = flattens alpha, directly affecting RGB.</param>
        /// <returns>Number of mipmaps present in MipMaps.</returns>
        internal static int BuildMipMaps(List<MipMap> MipMaps, bool mergeAlpha)
        {
            if (MipMaps?.Count == 0)
                return 0;

            MipMap currentMip = MipMaps[0];

            // KFreon: Check if mips required
            int estimatedMips = DDSGeneral.EstimateNumMipMaps(currentMip.Width, currentMip.Height);
            if (MipMaps.Count > 1)
                return estimatedMips;

            // KFreon: Half dimensions until one == 1.
            MipMap[] newmips = new MipMap[estimatedMips];

            Action<int> action = new Action<int>(item =>
            {
                int index = item;
                MipMap newmip;
                newmip = ImageEngine.Resize(currentMip, 1f / Math.Pow(2, index), mergeAlpha);
                newmips[index - 1] = newmip;
            });

            // Start at 1 to skip top mip
            if (ImageEngine.EnableThreading)
                Parallel.For(1, estimatedMips + 1, item => action(item));
            else
                for (int item = 1; item < estimatedMips + 1; item++)
                    action(item);

            MipMaps.AddRange(newmips);
            return estimatedMips;
        }

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


        /// <summary>
        /// Checks image file size to ensure requested mipmap is present in image.
        /// Header mip count can be incorrect or missing. Use this method to validate the mip you're after.
        /// </summary>
        /// <param name="streamLength">Image file stream length.</param>
        /// <param name="mainWidth">Width of image.</param>
        /// <param name="mainHeight">Height of image.</param>
        /// <param name="desiredMipDimension">Max dimension of desired mip.</param>
        /// <param name="format">Format of image.</param>
        /// <param name="mipOffset">Offset of desired mipmap in image.</param>
        /// <returns></returns>
        public static bool EnsureMipInImage(long streamLength, int mainWidth, int mainHeight, int desiredMipDimension, ImageEngineFormat format, out int mipOffset)
        {
            if (mainWidth <= desiredMipDimension && mainHeight <= desiredMipDimension)
            {
                mipOffset = 128;
                return true; // One mip only
                // TODO: DX10 
            }

            int dependentDimension = mainWidth > mainHeight ? mainWidth : mainHeight;
            int mipIndex = (int)Math.Log((dependentDimension / desiredMipDimension), 2) - 1;
            if (mipIndex < -1)
                throw new InvalidDataException($"Invalid dimensions for mipmapping. Got desired: {desiredMipDimension} and dependent: {dependentDimension}");

            int requiredOffset = GetMipOffset(mipIndex, format, mainHeight, mainWidth);  // +128 for header

            // KFreon: Something wrong with the count here by 1 i.e. the estimate is 1 more than it should be 
            if (format == ImageEngineFormat.DDS_ARGB)
                requiredOffset -= 2;

            mipOffset = requiredOffset;

            // Should only occur when an image has no mips
            if (streamLength < requiredOffset)
                return false;

            return true;
        }

        internal static int GetMipOffset(double mipIndex, ImageEngineFormat format, int baseWidth, int baseHeight)
        {
            /*
                Mipmapping halves both dimensions per mip down. Dimensions are then divided by 4 if block compressed as a texel is 4x4 pixels.
                e.g. 4096 x 4096 block compressed texture with 8 byte blocks e.g. DXT1
                Sizes of mipmaps:
                    4096 / 4 x 4096 / 4 x 8
                    (4096 / 4 / 2) x (4096 / 4 / 2) x 8
                    (4096 / 4 / 2 / 2) x (4096 / 4 / 2 / 2) x 8

                Pattern: Each dimension divided by 2 per mip size decreased.
                Thus, total is divided by 4.
                    Size of any mip = Sum(1/4^n) x divWidth x divHeight x blockSize,  
                        where n is the desired mip (0 based), 
                        divWidth and divHeight are the block compress adjusted dimensions (uncompressed textures lead to just original dimensions, block compressed are divided by 4)

                Turns out the partial sum of the infinite sum: Sum(1/4^n) = 1/3 x (4 - 4^-n). Who knew right?
            */

            int divisor = 1;
            if (ImageFormats.IsBlockCompressed(format))
                divisor = 4;

            double sumPart = mipIndex == -1 ? 0 :
                (1 / 3f) * (4 - Math.Pow(4, -mipIndex));

            double totalSize = 128 + (sumPart * ImageFormats.GetBlockSize(format) * (baseWidth / divisor) * (baseHeight / divisor));

            return (int)totalSize;
        }
        #endregion Mipmap Management
    }
}

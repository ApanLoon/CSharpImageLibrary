﻿using CSharpImageLibrary.DDS;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UsefulThings;

namespace CSharpImageLibrary
{
    public partial class ImageFormats
    {
        /// <summary>
        /// Detailed representation of an image format.
        /// </summary>
        [DebuggerDisplay("Format: {Format}, ComponentSize: {ComponentSize}")]
        public class ImageEngineFormatDetails
        {
            /// <summary>
            /// Format of details.
            /// </summary>
            public ImageEngineFormat Format;

            /// <summary>
            /// Indicates whether format contains premultiplied alpha.
            /// </summary>
            public bool IsPremultipliedFormat;

            /// <summary>
            /// Number of bytes in colour.
            /// </summary>
            public int ComponentSize;

            /// <summary>
            /// Number of bits in colour.
            /// </summary>
            public int BitCount;

            /// <summary>
            /// Indicates whether supported format is Block Compressed.
            /// </summary>
            public bool IsBlockCompressed;

            /// <summary>
            /// Indicates whether format supports mipmaps.
            /// </summary>
            public bool IsMippable;

            /// <summary>
            /// Size of a discrete block in bytes. (e.g. 2 channel 8 bit colour = 2, DXT1 = 16). Block can mean texel (DXTn) or pixel (uncompressed)
            /// </summary>
            public int BlockSize;

            /// <summary>
            /// String representation of formats' file extension. No '.'.
            /// </summary>
            public string Extension;

            /// <summary>
            /// Enum version of formats' file extension.
            /// </summary>
            public SupportedExtensions Supported_Extension;

            /// <summary>
            /// Indicates whether format is a DDS format.
            /// </summary>
            public bool IsDDS;

            /// <summary>
            /// Max number of supported channels. Usually 4, but some formats are 1 (G8), 2 (V8U8), or 3 (RGB) channels.
            /// </summary>
            public int MaxNumberOfChannels;

            /// <summary>
            /// Writes the max value to array using the correct bit styles.
            /// </summary>
            public Action<byte[], int> SetMaxValue = null;

            /// <summary>
            /// Reads a byte from a source array using the correct bit styles.
            /// </summary>
            public Func<byte[], int, byte> ReadByte = null;

            /// <summary>
            /// Reads a ushort (int16) from a source array using the correct bit styles.
            /// </summary>
            public Func<byte[], int, ushort> ReadUShort = null;

            /// <summary>
            /// Reads a float from a source array using the correct bit styles.
            /// </summary>
            public Func<byte[], int, float> ReadFloat = null;

            Func<byte[], int, byte[]> ReadUShortAsArray = null;
            Func<byte[], int, byte[]> ReadFloatAsArray = null;

            public Action<byte[], int, int, byte[], int, AlphaSettings, ImageEngineFormatDetails> BlockEncoder = null;
            public Action<byte[], int, byte[], int, int, bool> BlockDecoder = null;

            /// <summary>
            /// Writes a colour from source to destination performing correct bit style conversions if requried.
            /// </summary>
            public Action<byte[], int, ImageEngineFormatDetails, byte[], int> WriteColour = null;

            /// <summary>
            /// Details the given format.
            /// </summary>
            /// <param name="format">Image Format to get details for.</param>
            public ImageEngineFormatDetails(ImageEngineFormat format)
            {
                Format = format;
                IsPremultipliedFormat = Format == ImageEngineFormat.DDS_DXT2 || Format == ImageEngineFormat.DDS_DXT4;
                IsDDS = Format.ToString().Contains("DDS");
                MaxNumberOfChannels = MaxNumberOfChannels(Format);

                {
                    var temp = Format.ToString();
                    if (temp.Contains("DDS"))
                        Supported_Extension = SupportedExtensions.DDS;
                    else
                        Supported_Extension = ParseExtension(temp);
                }

                Extension = Supported_Extension.ToString();

                BitCount = 8;
                {
                    switch (Format)
                    {
                        case ImageEngineFormat.DDS_G8_L8:
                        case ImageEngineFormat.DDS_A8:
                        case ImageEngineFormat.DDS_ATI1:
                            BitCount = 8;
                            break;
                        case ImageEngineFormat.DDS_A8L8:
                        case ImageEngineFormat.DDS_V8U8:
                        case ImageEngineFormat.DDS_ATI2_3Dc:
                            BitCount = 16;
                            break;
                        case ImageEngineFormat.BMP:
                        case ImageEngineFormat.DDS_ABGR_8:
                        case ImageEngineFormat.DDS_ARGB_8:
                        case ImageEngineFormat.GIF:
                        case ImageEngineFormat.PNG:
                        case ImageEngineFormat.TGA:
                        case ImageEngineFormat.TIF:
                        case ImageEngineFormat.DDS_DXT1:
                        case ImageEngineFormat.DDS_DXT2:
                        case ImageEngineFormat.DDS_DXT3:
                        case ImageEngineFormat.DDS_DXT4:
                        case ImageEngineFormat.DDS_DXT5:
                        case ImageEngineFormat.DDS_G16_R16:
                            BitCount = 32;
                            break;
                        case ImageEngineFormat.JPG:
                        case ImageEngineFormat.DDS_RGB_8:
                            BitCount = 24;
                            break;
                        case ImageEngineFormat.DDS_ARGB_32F:
                            BitCount = 128;
                            break;
                        case ImageEngineFormat.DDS_ARGB_4:
                        case ImageEngineFormat.DDS_R5G6B5:
                        case ImageEngineFormat.DDS_CUSTOM:
                        case ImageEngineFormat.DDS_DX10:
                            throw new InvalidDataException("Bitcount thing. I dunno yet.");
                    }
                }

                ComponentSize = (BitCount / 8) / MaxNumberOfChannels;
                BlockSize = GetBlockSize(Format, ComponentSize);
                IsBlockCompressed = IsBlockCompressed(Format);
                IsMippable = IsFormatMippable(Format);

                // Functions
                ReadByte = ReadByteFromByte;
                ReadUShort = ReadUShortFromByte;
                ReadFloat = ReadFloatFromByte;
                SetMaxValue = WriteByteMax;
                WriteColour = WriteByte;
                ReadUShortAsArray = ReadUShortFromByteAsArray;
                ReadFloatAsArray = ReadFloatFromByteOrUShortAsArray;

                if (ComponentSize == 2)
                {
                    ReadByte = ReadByteFromUShort;
                    ReadUShort = ReadUShortFromUShort;
                    ReadFloat = ReadFloatFromUShort;
                    SetMaxValue = WriteUShortMax;
                    WriteColour = WriteUShort;
                    ReadUShortAsArray = ReadUShortFromUShortAsArray;
                    // Don't need ReadFloatAsArray set here, as it's shared between byte and ushort reading.
                }
                else if (ComponentSize == 4)
                {
                    ReadByte = ReadByteFromFloat;
                    ReadUShort = ReadUShortFromFloat;
                    ReadFloat = ReadFloatFromFloat;
                    SetMaxValue = WriteFloatMax;
                    WriteColour = WriteFloat;
                    ReadUShortAsArray = ReadUShortFromFloatAsArray;
                    ReadFloatAsArray = ReadFloatFromFloatAsArray;
                }


                switch (Format)
                {
                    case ImageEngineFormat.DDS_ATI1:
                        BlockEncoder = DDS_Encoders.CompressBC4Block;
                        break;
                    case ImageEngineFormat.DDS_ATI2_3Dc:
                        BlockEncoder = DDS_Encoders.CompressBC5Block;
                        break;
                    case ImageEngineFormat.DDS_DX10:
                        //Debugger.Break();
                        break; // TODO: NOT SUPPORTED YET. DX10
                    case ImageEngineFormat.DDS_DXT1:
                        BlockEncoder = DDS_Encoders.CompressBC1Block;
                        break;
                    case ImageEngineFormat.DDS_DXT2:
                    case ImageEngineFormat.DDS_DXT3:
                        BlockEncoder = DDS_Encoders.CompressBC2Block;
                        break;
                    case ImageEngineFormat.DDS_DXT4:
                    case ImageEngineFormat.DDS_DXT5:
                        BlockEncoder = DDS_Encoders.CompressBC3Block;
                        break;
                }

                switch (format)
                {
                    case ImageEngineFormat.DDS_DXT1:
                        BlockDecoder = DDS_Decoders.DecompressBC1Block;
                        break;
                    case ImageEngineFormat.DDS_DXT2:
                    case ImageEngineFormat.DDS_DXT3:
                        BlockDecoder = DDS_Decoders.DecompressBC2Block;
                        break;
                    case ImageEngineFormat.DDS_DXT4:
                    case ImageEngineFormat.DDS_DXT5:
                        BlockDecoder = DDS_Decoders.DecompressBC3Block;
                        break;
                    case ImageEngineFormat.DDS_ATI1:
                        BlockDecoder = DDS_Decoders.DecompressATI1;
                        break;
                    case ImageEngineFormat.DDS_ATI2_3Dc:
                        BlockDecoder = DDS_Decoders.DecompressATI2Block;
                        break;
                }
            }
            #region Bit Conversions
            byte ReadByteFromByte(byte[] source, int sourceStart)
            {
                return source[sourceStart];
            }

            byte ReadByteFromUShort(byte[] source, int sourceStart)
            {
                return (byte)(((source[sourceStart + 1] << 8) | source[sourceStart]) * (255d / ushort.MaxValue));
            }

            byte ReadByteFromFloat(byte[] source, int sourceStart)
            {
                return (byte)(BitConverter.ToSingle(source, sourceStart) * 255f);
            }

            /************************************/

            ushort ReadUShortFromByte(byte[] source, int sourceStart)
            {
                return source[sourceStart];
            }

            ushort ReadUShortFromUShort(byte[] source, int sourceStart)
            {
                return BitConverter.ToUInt16(source, sourceStart);
            }

            ushort ReadUShortFromFloat(byte[] source, int sourceStart)
            {
                return (ushort)(BitConverter.ToSingle(source, sourceStart) * ushort.MaxValue);
            }

            /************************************/

            float ReadFloatFromByte(byte[] source, int sourceStart)
            {
                return source[sourceStart] / 255f;
            }

            float ReadFloatFromUShort(byte[] source, int sourceStart)
            {
                return BitConverter.ToUInt16(source, sourceStart) / (ushort.MaxValue * 1f);
            }

            float ReadFloatFromFloat(byte[] source, int sourceStart)
            {
                return BitConverter.ToSingle(source, sourceStart);
            }
            #endregion Bit Conversions

            #region Max Value Writers
            void WriteByteMax(byte[] source, int sourceStart)
            {
                source[sourceStart] = 255;
            }

            void WriteUShortMax(byte[] source, int sourceStart)
            {
                source[sourceStart] = 255;
                source[sourceStart + 1] = 255;
            }

            void WriteFloatMax(byte[] source, int sourceStart)
            {
                source[sourceStart] = 0;
                source[sourceStart + 1] = 0;
                source[sourceStart + 2] = 63;
                source[sourceStart + 3] = 127;
            }
            #endregion Max Value Writers

            #region Readers to arrays
            byte[] ReadUShortFromByteAsArray(byte[] source, int sourceStart)
            {
                return new byte[] { 0, source[sourceStart] };
            }

            byte[] ReadUShortFromUShortAsArray(byte[] source, int sourceStart)
            {
                return new byte[] { source[sourceStart], source[sourceStart + 1] };
            }

            byte[] ReadUShortFromFloatAsArray(byte[] source, int sourceStart)
            {
                return BitConverter.GetBytes(ReadUShortFromFloat(source, sourceStart));
            }

            /**/

            byte[] ReadFloatFromByteOrUShortAsArray(byte[] source, int sourceStart)
            {
                return BitConverter.GetBytes(ReadFloat(source, sourceStart));
            }
            byte[] ReadFloatFromFloatAsArray(byte[] source, int sourceStart)
            {
                return new byte[] { source[sourceStart], source[sourceStart + 1], source[sourceStart + 2], source[sourceStart + 3] };
            }
            #endregion Readers to arrays

            #region Writers
            void WriteByte(byte[] source, int sourceStart, ImageEngineFormatDetails sourceFormatDetails, byte[] destination, int destStart)
            {
                destination[destStart] = sourceFormatDetails.ReadByte(source, sourceStart);
            }

            void WriteUShort(byte[] source, int sourceStart, ImageEngineFormatDetails sourceFormatDetails, byte[] destination, int destStart)
            {
                byte[] bytes = sourceFormatDetails.ReadUShortAsArray(source, sourceStart);
                destination[destStart] = bytes[0];
                destination[destStart + 1] = bytes[1];
            }

            void WriteFloat(byte[] source, int sourceStart, ImageEngineFormatDetails sourceFormatDetails, byte[] destination, int destStart)
            {
                byte[] bytes = sourceFormatDetails.ReadFloatAsArray(source, sourceStart);
                destination[destStart] = bytes[0];
                destination[destStart + 1] = bytes[1];
                destination[destStart + 2] = bytes[2];
                destination[destStart + 3] = bytes[3];
            }
            #endregion Writers
        }
    }
}

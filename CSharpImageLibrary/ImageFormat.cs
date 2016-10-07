using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpImageLibrary
{
    /// <summary>
    /// Indicates image format.
    /// Use FORMAT struct.
    /// </summary>
    public enum ImageEngineFormat
    {
        // TODO: GIF

        /// <summary>
        /// Unknown image format. Using this as a save/load format will fail that operation.
        /// </summary>
        Unknown = 1,

        /// <summary>
        /// Standard JPEG image handled by everything.
        /// </summary>
        JPG = 2,

        /// <summary>
        /// Standard PNG image handled by everything. Uses alpha channel if available.
        /// </summary>
        PNG = 3,

        /// <summary>
        /// Standard BMP image handled by everything.
        /// </summary>
        BMP = 4,

        /// <summary>
        /// Targa image. Multipage format. Can be used for mipmaps.
        /// </summary>
        TGA = 5,

        /// <summary>
        /// (BC1) Block Compressed Texture. Compresses 4x4 texels.
        /// Used for Simple Non Alpha.
        /// </summary>
        DDS_DXT1 = 0x31545844,  // 1TXD i.e. DXT1 backwards

        /// <summary>
        /// (BC2) Block Compressed Texture. Compresses 4x4 texels.
        /// Used for Sharp Alpha. Premultiplied alpha. 
        /// </summary>
        DDS_DXT2 = 0x32545844,

        /// <summary>
        /// (BC2) Block Compressed Texture. Compresses 4x4 texels.
        /// Used for Sharp Alpha. 
        /// </summary>
        DDS_DXT3 = 0x33545844,

        /// <summary>
        /// (BC3) Block Compressed Texture. Compresses 4x4 texels.
        /// Used for Gradient Alpha. Premultiplied alpha.
        /// </summary>
        DDS_DXT4 = 0x34545844,

        /// <summary>
        /// (BC3) Block Compressed Texture. Compresses 4x4 texels.
        /// Used for Gradient Alpha. 
        /// </summary>
        DDS_DXT5 = 0x35545844,

        /// <summary>
        /// Uncompressed ARGB DDS.
        /// </summary>
        DDS_ARGB = 6,  // No specific value apparently

        /// <summary>
        /// (BC4) Block Compressed Texture. Compresses 4x4 texels.
        /// Used for Normal (bump) Maps. 8 bit single channel with alpha.
        /// </summary>
        DDS_ATI1 = 0x31495441,  // ATI1 backwards

        /// <summary>
        /// Uncompressed pair of 8 bit channels.
        /// Used for Normal (bump) maps.
        /// </summary>
        DDS_V8U8 = 8,

        /// <summary>
        /// Pair of 8 bit channels.
        /// Used for Luminescence.
        /// </summary>
        DDS_G8_L8 = 7,  // No specific value it seems

        /// <summary>
        /// Alpha and single channel luminescence.
        /// Uncompressed.
        /// </summary>
        DDS_A8L8 = 9,

        /// <summary>
        /// RGB. No alpha. 
        /// Uncompressed.
        /// </summary>
        DDS_RGB = 10,

        /// <summary>
        /// (BC5) Block Compressed Texture. Compresses 4x4 texels.
        /// Used for Normal (bump) Maps. Pair of 8 bit channels.
        /// </summary>
        DDS_ATI2_3Dc = 0x32495441  // ATI2 backwards
    }
}

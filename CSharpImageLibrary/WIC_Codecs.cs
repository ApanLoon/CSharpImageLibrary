using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;

namespace CSharpImageLibrary
{
    internal class WIC_Codecs
    {
        internal static List<MipMap> LoadWithCodecs(MemoryStream stream, int maxDimension, bool isMippable)
        {
            List<MipMap> mipmaps = new List<MipMap>();
            stream.Position = 0;

            bool decodeDimensionsSpecified = maxDimension != 0;

            var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnDemand);
            if (isMippable)
            {
                foreach (var frame in decoder.Frames)
                {
                    // Skip unwanted mipmaps
                    if (decodeDimensionsSpecified && (frame.PixelHeight > maxDimension || frame.PixelWidth > maxDimension))
                        continue;

                    mipmaps.Add(new MipMap(frame));
                }
            }
            else
            {
                // Only add if 
                var frame = decoder.Frames[0];
                if (!(decodeDimensionsSpecified && (frame.PixelHeight > maxDimension || frame.PixelWidth > maxDimension)))
                    mipmaps.Add(new MipMap(frame));
            }

            // No suitable mip sizes found. Resize top.
            if (mipmaps.Count == 0)
            {

            }

            return mipmaps;
        }
    }
}
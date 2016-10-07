using CSharpImageLibrary.Headers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using UsefulThings;

namespace CSharpImageLibrary
{
    /// <summary>
    /// Represents an image. Can use Windows codecs if available.
    /// </summary>
    public class ImageEngineImage
    {
        AbstractHeader header = null;
        List<MipMap> MipMaps = null;

        public ImageEngineFormat SurfaceFormat
        {
            get
            {
                // TESTING
                return ImageEngineFormat.DDS_DXT1;
                return header?.SurfaceFormat ?? ImageEngineFormat.Unknown;
            }
        }

        public ImageEngineImage(string filename) : this()
        {
            LoadFromFileAsync(filename);
        }

        public ImageEngineImage()
        {

        }

        internal BitmapSource GetWPFBitmap()
        {
            return UsefulThings.WPF.Images.CreateWriteableBitmap(MipMaps[0].Pixels, MipMaps[0].Width, MipMaps[0].Height);
        }

        public static async Task<ImageEngineImage> LoadAsync(string filename)
        {
            var img = new ImageEngineImage();
            await img.LoadFromFileAsync(filename);
            return img;
        }

        void LoadFromStream(MemoryStream stream)
        {
            header = Headers.Headers.GetHeader(stream);
            if (header == null)
                throw new InvalidDataException("Unknown header format.");

            // Load mipmaps
            MipMaps = ImageEngine.Load(stream, SurfaceFormat);
        }

        async Task LoadFromFileAsync(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Open);
            MemoryStream dataStream = new MemoryStream();
            using (MemoryStream ms = new MemoryStream())
            {
                // Read header
                ms.ReadFrom(fs, Headers.AbstractHeader.MaxHeaderSize);

                // Read in remaining image while header is being parsed
                fs.Position = 0;
                var fullLoad = fs.CopyToAsync(dataStream);

                // Parse header
                header = Headers.Headers.GetHeader(ms);
                if (header == null)
                    throw new InvalidDataException("Unknown header format.");

                // Complete loading
                await fullLoad;
                fs.Dispose();
            }

            // Load mipmaps
            MipMaps = await Task.Run(() => ImageEngine.Load(dataStream, SurfaceFormat));
            dataStream.Dispose();
        }
    }
}

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
    public class ImageEngineImage : IDisposable
    {
        AbstractHeader header = null;

        public ImageEngineImage()
        {

        }

        void Load(Stream stream)
        {
            header = Headers.Headers.GetHeader(stream);
            // Format in headers
            
            // TODO
            // Open filestream
            // Read chunk for header
            // Begin off thread reading into memory of entire file
            // Process header, format etc
            // await loading to memory
            // Load into mipmaps
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}

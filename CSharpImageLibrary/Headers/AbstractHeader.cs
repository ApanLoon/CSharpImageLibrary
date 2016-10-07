using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpImageLibrary.Headers
{
    public abstract class AbstractHeader
    {
        public abstract int Width { get; }
        public abstract int Height { get; }
        public abstract ImageEngineFormat SurfaceFormat { get; }
        
        public abstract int HeaderSize { get; }

        public AbstractHeader(Stream stream) { }
    }
}

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
        public static int MaxHeaderSize = 160;

        public abstract int Width { get; }
        public abstract int Height { get; }
        public ImageEngineFormat SurfaceFormat { get; internal set; }   
    }
}

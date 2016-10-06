using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpImageLibrary.Headers
{
    public abstract class AbstractHeader
    {
        public abstract int Width { get; }
        public abstract int Height { get; }
        // public format
        
        public abstract int HeaderSize { get; }
    }
}

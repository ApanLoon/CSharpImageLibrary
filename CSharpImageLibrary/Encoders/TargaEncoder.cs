using CSharpImageLibrary.Decoders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace CSharpImageLibrary.Encoders
{
    public partial class TargaImage
    {
        public void Save(MemoryStream ms, WriteableBitmap img)
        {
            TargaHeader header = new TargaHeader();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpImageLibrary.Headers
{
    public class DDS_Header : AbstractHeader
    {
        public override int Height
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override int Width
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public DDS_Header(Stream stream)
        {

        }
    }
}

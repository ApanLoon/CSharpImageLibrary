using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpImageLibrary.Headers
{
    public static class Headers
    {
        public static AbstractHeader GetHeader(Stream stream)
        {
            // Read File ID
            stream.Position = 0;
            byte[] ID = new byte[4];
            stream.Read(ID, 0, 4);

            AbstractHeader header = null;

            // Determine correct header to create
            // BMP
            if (ID[0] == 'B' && ID[1] == 'M')
                header = new BMP_Header(stream);

            // PNG
            if (ID[0] == 137 && ID[1] == 'P' && ID[2] == 'N' && ID[3] == 'G')
                header = new PNG_Header(stream);

            // JPG
            if (ID[0] == 0xFF && ID[1] == 0xD8 && ID[2] == 0xFF)
                header = new JPG_Header(stream);

            // DDS
            if (ID[0] == 'D' && ID[1] == 'D' && ID[2] == 'S')
                header = new DDS_Header(stream);

            // GIF

            // TIFF
            if (ID[0] == 'I' && ID[1] == 'I')
                header = new TIFF_Header(stream);

            // TGA
            if (header == null)
            {
                try
                {
                    header = new TGA_Header(stream);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                }
            }

            return header;
        }
    }
}

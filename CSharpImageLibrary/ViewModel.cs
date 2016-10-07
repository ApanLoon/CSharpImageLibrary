using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using UsefulThings.WPF;

namespace CSharpImageLibrary
{
    public class ViewModel : ViewModelBase
    {
        ImageEngineImage img = null;

        BitmapSource preview = null;
        public BitmapSource Preview
        {
            get
            {
                return preview;
            }
            set
            {
                SetProperty(ref preview, value);
            }
        }

        public ViewModel()
        {
            BeginLoading();
        }

        private async void BeginLoading()
        {
            var start = Environment.TickCount;
            img = await ImageEngineImage.LoadAsync(@"R:\Latest HD ME3 Textures\done\Anderson\done\MASSEFFECT3.EXE_0x77B54F3A_Anderson_diff.dds");
            var loading = Environment.TickCount;
            Preview = img.GetWPFBitmap();
            var preview = Environment.TickCount;
        }
    }
}

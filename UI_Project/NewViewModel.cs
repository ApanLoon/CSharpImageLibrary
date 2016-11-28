﻿using CSharpImageLibrary;
using CSharpImageLibrary.DDS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using UsefulThings.WPF;

namespace UI_Project
{
    public class NewViewModel : ViewModelBase
    {
        #region Commands
        CommandHandler closeCommand = null;
        public CommandHandler CloseCommand
        {
            get
            {
                if (closeCommand == null)
                    closeCommand = new CommandHandler(() => CloseImage(true));

                return closeCommand;
            }
        }

        CommandHandler saveCommand = null;
        public CommandHandler SaveCommand
        {
            get
            {
                if (saveCommand == null)
                    saveCommand = new CommandHandler(() =>
                    {
                        Task.Run(() =>
                        {
                            SaveAttempted = true;
                            try
                            {
                                LoadedImage.Save(SavePath, SaveFormat, SaveMipType, mergeAlpha: false); // TODO: Alpha
                            }
                            catch (Exception e)
                            {
                                SaveError = e.ToString();
                            }
                        });
                    });

                return saveCommand;

            }
        }
        #endregion Commands

        bool saveAttempted = false;
        public bool SaveAttempted
        {
            get
            {
                return saveAttempted; 
            }
            set
            {
                SetProperty(ref saveAttempted, value);
            }
        }

        string saveError = null;
        public string SaveError
        {
            get
            {
                return saveError;
            }
            set
            {
                SetProperty(ref saveError, value);
            }
        }

        #region Loaded Image Properties
        int mipIndex = 0;
        public int MipIndex
        {
            get
            {
                return mipIndex;
            }
            set
            {
                SetProperty(ref mipIndex, value);
            }
        }

        ImageEngineImage loadedImage = null;
        public ImageEngineImage LoadedImage
        {
            get
            {
                return loadedImage;
            }
            set
            {
                SetProperty(ref loadedImage, value);
                OnPropertyChanged(nameof(IsImageLoaded));
            }
        }

        WriteableBitmap preview = null;
        public WriteableBitmap Preview
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


        string windowTitle = "Image Engine";
        public string WindowTitle
        {
            get
            {
                return windowTitle;
            }
            set
            {
                SetProperty(ref windowTitle, value);
            }
        }


        public string LoadedPath
        {
            get
            {
                return LoadedImage?.FilePath;
            }
        }

        public ImageEngineFormat LoadedFormat
        {
            get
            {
                return LoadedImage?.Format ?? ImageEngineFormat.Unknown;
            }
        }

        public int Width
        {
            get
            {
                return LoadedImage?.Width ?? -1;
            }
        }

        public int Height
        {
            get
            {
                return LoadedImage?.Height ?? -1;
            }
        }

        public int UncompressedSize
        {
            get
            {
                if (LoadedImage == null)
                    return -1;

                return ImageFormats.GetUncompressedSizeWithMips(LoadedImage.Width, LoadedImage.Height, LoadedImage.NumberOfChannels);
            }
        }

        public int LoadedCompressedSize
        {
            get
            {
                return LoadedImage?.CompressedSize ?? -1;
            }
        }

        public int MipCount
        {
            get
            {
                return LoadedImage?.NumMipMaps ?? -1;
            }
        }

        public string HeaderDetails
        {
            get
            {
                return LoadedImage?.Header?.ToString();
            }
        }

        int alphaDisplaySetting = 0;
        public int AlphaDisplaySetting
        {
            get
            {
                return alphaDisplaySetting;
            }
            set
            {
                SetProperty(ref alphaDisplaySetting, value);
            }
        }
        #endregion Loaded Image Properties

        #region Save Properties
        WriteableBitmap savePreview = null;
        public WriteableBitmap SavePreview
        {
            get
            {
                return savePreview;
            }
            set
            {
                SetProperty(ref savePreview, value);
            }
        }

        public bool IsSaveSmaller
        {
            get
            {
                return SaveCompressedSize < UncompressedSize;
            }
        }

        public double SaveCompressionRatio
        {
            get
            {
                var ratio = (1d * SaveCompressedSize) / UncompressedSize;
                if (ratio < 1)
                    ratio = 1 / ratio;

                return ratio * 100d;
            }
        }

        public int SaveCompressedSize
        {
            get
            {
                int estimatedMips = DDSGeneral.EstimateNumMipMaps(Width, Height);
                return ImageFormats.GetCompressedSize(SaveFormat, Width, Height, 
                    SaveMipType == MipHandling.KeepTopOnly || (SaveMipType == MipHandling.KeepExisting && MipCount == 1) ? 
                    1 : estimatedMips);
            }
        }

        public string DefaultSavePath
        {
            get
            {
                if (LoadedImage?.FilePath == null)
                    return null;  // TODO: Maybe Desktop when path is unknown?

                string name = $"{UsefulThings.General.GetFullPathWithoutExtension(LoadedImage.FilePath)}.{ImageFormats.GetExtensionOfFormat(SaveFormat)}";
                return UsefulThings.General.FindValidNewFileName(name);
            }
        }

        string savePath = null;
        public string SavePath
        {
            get
            {
                return savePath ?? DefaultSavePath;
            }
            set
            {
                SetProperty(ref savePath, value);
                FixExtension();
            }
        }

        ImageEngineFormat saveFormat = ImageEngineFormat.Unknown;
        public ImageEngineFormat SaveFormat
        {
            get
            {
                return saveFormat;
            }
            set
            {
                bool changed = value != saveFormat;

                SetProperty(ref saveFormat, value);
                OnPropertyChanged(nameof(SaveCompressedSize));
                OnPropertyChanged(nameof(SaveCompressionRatio));
                OnPropertyChanged(nameof(IsSaveFormatMippable));
                OnPropertyChanged(nameof(IsSaveSmaller));

                if (SavePath == null)
                    return;

                // Test paths without extensions
                if (SavePath.Substring(0, SavePath.LastIndexOf('.')) == DefaultSavePath.Substring(0, DefaultSavePath.LastIndexOf('.')))
                    SavePath = DefaultSavePath;

                // Change extension as required
                FixExtension();


                // Regenerate save preview
                if (changed)
                    GenerateSavePreview();
            }
        }

        MipHandling saveMipType = MipHandling.Default;
        public MipHandling SaveMipType
        {
            get
            {
                return saveMipType;
            }
            set
            {
                SetProperty(ref saveMipType, value);
                OnPropertyChanged(nameof(SaveCompressedSize));
                OnPropertyChanged(nameof(SaveCompressionRatio));
                OnPropertyChanged(nameof(IsSaveSmaller));
            }
        }

        public bool IsSaveFormatMippable
        {
            get
            {
                return ImageFormats.IsFormatMippable(SaveFormat);
            }
        }

        public int DXT1AlphaThreshold
        {
            get
            {
                return (int)(CSharpImageLibrary.DDS.DDSGeneral.DXT1AlphaThreshold * 100f);
            }
            set
            {
                SetProperty(ref CSharpImageLibrary.DDS.DDSGeneral.DXT1AlphaThreshold, value / 100f);
            }
        }

        public int JPG_CompressionSetting
        {
            get
            {
                return WIC_Codecs.JPGCompressionSetting;
            }
            set
            {
                SetProperty(ref CSharpImageLibrary.WIC_Codecs.JPGCompressionSetting, value);
            }
        }
        #endregion Save Properties

        public bool IsImageLoaded
        {
            get
            {
                return LoadedImage != null;
            }
        }

        public NewViewModel() : base()
        {

        }

        internal async Task LoadImage(string path)
        {
            var bytes = File.ReadAllBytes(path);
            await LoadImage(bytes);
            LoadedImage.FilePath = path;
            SavePath = DefaultSavePath;
            OnPropertyChanged(nameof(LoadedPath));
        }

        internal async Task LoadImage(byte[] data)
        {
            CloseImage(false); // Don't need to update the UI here, it'll get updated after loading the image. But do need to reset some things
            WindowTitle = "Image Engine - View";

            // Full image
            var fullLoad = Task.Run(() => new ImageEngineImage(data));

            // Quick previews
            /*LoadedImage = await Task.Run(() => new ImageEngineImage(data, 512));
            UpdatePreview(LoadedImage.MipMaps[0]);*/

            // Wait for full load to be done
            LoadedImage = await fullLoad;
            UpdatePreview(LoadedImage.MipMaps[0]);

            SaveFormat = LoadedFormat;
        }

        void UpdatePreview(MipMap mip)
        {
            // Create Preview Object if required
            if (Preview == null || (Preview.PixelHeight != mip.Height || Preview.PixelWidth != mip.Width))
                Preview = UsefulThings.WPF.Images.CreateWriteableBitmap(mip.Pixels, mip.Width, mip.Height);
            else
                RedrawEitherPreview(Preview, mip.Pixels, mip.Width, mip.Height);

            OnPropertyChanged(nameof(Preview));
            UpdateUI();
        }

        public async Task GenerateSavePreview()
        {
            // Save and reload to give accurate depiction of what it'll look like when saved.
            ImageEngineImage img = await Task.Run(() =>
            {
                byte[] data = LoadedImage.Save(SaveFormat, MipHandling.KeepTopOnly, mergeAlpha: false);  // TODO: Alpha settings
                return new ImageEngineImage(data);
            });

            if (SavePreview == null)
                SavePreview = UsefulThings.WPF.Images.CreateWriteableBitmap(img.MipMaps[0].Pixels, img.Width, img.Height);
            else
                RedrawEitherPreview(SavePreview, img.MipMaps[0].Pixels, img.Width, img.Height);

            img.Dispose();

            // Update Properties
            OnPropertyChanged(nameof(SavePreview));
        }

        void RedrawEitherPreview(WriteableBitmap bmp, byte[] pixels, int width, int height)
        {
            var rect = new System.Windows.Int32Rect(0, 0, width, height);
            bmp.WritePixels(rect, pixels, width * 4, 0);
        }

        void UpdateUI()
        {
            // Update UI
            OnPropertyChanged(nameof(LoadedFormat));
            OnPropertyChanged(nameof(LoadedPath));
            OnPropertyChanged(nameof(LoadedCompressedSize));
            OnPropertyChanged(nameof(UncompressedSize));
            OnPropertyChanged(nameof(HeaderDetails));
            OnPropertyChanged(nameof(Height));
            OnPropertyChanged(nameof(Width));
            OnPropertyChanged(nameof(MipCount));
        }

        void FixExtension()
        {
            string requiredExtension = "." + ImageFormats.GetExtensionOfFormat(SaveFormat);

            var test = Path.GetExtension(savePath);
            if (test == "")  // No extension
                SavePath += requiredExtension;
            else if (Path.GetExtension(SavePath) != requiredExtension)  // Existing extension
                Path.ChangeExtension(SavePath, requiredExtension);
        }

        void CloseImage(bool updateUI)
        {
            // Clear things - should close panels when this happens
            LoadedImage = null;
            Preview = null;
            SavePreview = null;
            SavePath = null;
            SaveError = null;
            SaveAttempted = false;
            MipIndex = 0;
            WindowTitle = "Image Engine";

            // Notify
            if (updateUI)
                UpdateUI();
        }
    }
}

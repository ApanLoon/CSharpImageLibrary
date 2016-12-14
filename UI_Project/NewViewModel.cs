﻿using CSharpImageLibrary;
using CSharpImageLibrary.DDS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using UsefulThings.WPF;

namespace UI_Project
{
    public enum AlphaDisplaySettings
    {
        [Description("Don't show Alpha")]
        NoAlpha,

        [Description("Alpha 'merged' (premult) with RGB")]
        PremultiplyAlpha,

        [Description("Alpha Only")]
        AlphaOnly,
    }

    public class NewViewModel : ViewModelBase
    {
        Stopwatch timer = new Stopwatch();


        #region Settings Panel Properties
        bool settingsPanelOpen = false;
        public bool SettingsPanelOpen
        {
            get
            {
                return settingsPanelOpen;
            }
            set
            {
                SetProperty(ref settingsPanelOpen, value);
            }
        }

        public bool EnableThreading
        {
            get
            {
                return ImageEngine.EnableThreading;
            }
            set
            {
                ImageEngine.EnableThreading = value;
                OnPropertyChanged(nameof(EnableThreading));
            }
        }

        public bool UseWindowsCodecs
        {
            get
            {
                return ImageEngine.WindowsWICCodecsAvailable;
            }
            set
            {
                ImageEngine.WindowsWICCodecsAvailable = value;
                OnPropertyChanged(nameof(UseWindowsCodecs));
            }
        }

        public int NumThreads
        {
            get
            {
                return ImageEngine.NumThreads;
            }
            set
            {
                if (value < 1 && value != -1) // Allowed to be -1 for being infinite
                    return;

                ImageEngine.NumThreads = value;
                OnPropertyChanged(nameof(NumThreads));

                if (NumThreads == 1)
                    EnableThreading = false;
            }
        }
        #endregion Settings Panel Properties

        #region Info Panel Properties
        bool infoPanelOpen = false;
        public bool InfoPanelOpen
        {
            get
            {
                return infoPanelOpen;
            }
            set
            {
                SetProperty(ref infoPanelOpen, value);
            }
        }


        string cpuName = "Unknown";
        public string CPUName
        {
            get
            {
                if (cpuName == "Unknown")
                {
                    var searcher = new ManagementObjectSearcher("select * from Win32_Processor");
                    List<object> testting = new List<object>();
                    foreach (var item in searcher.Get())
                    {
                        cpuName = item["Name"].ToString();
                        break;
                    }
                }

                return cpuName;
            }
        }

        public int NumCores
        {
            get
            {
                return System.Environment.ProcessorCount;
            }
        }

        public bool Is64Bit
        {
            get
            {
                return System.Environment.Is64BitOperatingSystem;
            }
        }

        public bool IsRunning64Bit
        {
            get
            {
                return System.Environment.Is64BitProcess;
            }
        }

        string osVersion = "Unknown";
        public string OSVersion
        {
            get
            {
                if (osVersion == "Unknown")
                {
                    var name = (from x in new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem").Get().Cast<ManagementObject>()
                                select x.GetPropertyValue("Caption")).FirstOrDefault();
                    osVersion = name != null ? name.ToString() : "Unknown";
                }

                return osVersion;
            }
        }

        ulong ramSize = 0;
        public ulong RAMSize
        {
            get
            {
                if (ramSize == 0)
                {
                    var searcher = new ManagementObjectSearcher("select * from Win32_PhysicalMemory");
                    foreach (var item in searcher.Get())
                        ramSize += (ulong)item["Capacity"];
                }

                return ramSize;
            }
        }

        string gpuName = "Unknown";
        public string GPUName
        {
            get
            {
                if (gpuName == "Unknown")
                {
                    var searcher = new ManagementObjectSearcher("select * from Win32_VideoController");
                    List<object> testting = new List<object>();
                    foreach (var item in searcher.Get())
                    {
                        gpuName = item["Name"].ToString();
                        break;
                    }
                }

                return gpuName;
            }
        }
        #endregion Info Panel Properties

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
                        Task.Run(async () =>
                        {
                            Busy = true;
                            try
                            {
                                await LoadedImage.Save(SavePath, SaveFormat, SaveMipType, removeAlpha: GeneralRemovingAlpha, customMasks: customMasks);
                            }
                            catch (Exception e)
                            {
                                SaveError = e.ToString();
                            }
                            SaveAttempted = true;
                            Busy = false;
                            SavePath = UsefulThings.General.FindValidNewFileName(SavePath);  // Ensure save path is pointing to a new valid filepath
                        });
                    });

                return saveCommand;

            }
        }
        #endregion Commands

        #region General Properties
        bool busy = false;
        public bool Busy
        {
            get
            {
                return busy;
            }
            set
            {
                SetProperty(ref busy, value);
            }
        }

        #region Alpha and Colour related Properties
        bool GeneralRemovingAlpha
        {
            get
            {
                return SaveFormat == ImageEngineFormat.DDS_DXT1 ? DXT1AlphaRemove : RemoveGeneralAlpha;
            }
        }

        uint aMask = 0xFF000000;
        public uint AMask
        {
            get
            {
                return aMask;
            }
            set
            {
                SetProperty(ref aMask, value);
            }
        }

        uint rMask = 0x00FF0000;
        public uint RMask
        {
            get
            {
                return rMask;
            }
            set
            {
                SetProperty(ref rMask, value);
            }
        }

        uint gMask = 0x0000FF00;
        public uint GMask
        {
            get
            {
                return gMask;
            }
            set
            {
                SetProperty(ref gMask, value);
            }
        }

        uint bMask = 0x000000FF;
        public uint BMask
        {
            get
            {
                return bMask;
            }
            set
            {
                SetProperty(ref bMask, value);
            }
        }

        List<uint> customMasks
        {
            get
            {
                return new List<uint>() { AMask, RMask, GMask, BMask };
            }
        }
        #endregion Alpha and Colour related Properties

        #region Bulk Convert Properties
        public MTRangedObservableCollection<string> BulkConvertFiles { get; set; } = new MTRangedObservableCollection<string>();
        public MTRangedObservableCollection<string> BulkConvertFailed { get; set; } = new MTRangedObservableCollection<string>();

        bool bulkConvertOpen = false;
        public bool BulkConvertOpen
        {
            get
            {
                return bulkConvertOpen;
            }
            set
            {
                SetProperty(ref bulkConvertOpen, value);
            }
        }

        bool bulkFolderBrowseRecurse = true;
        public bool BulkFolderBrowseRecurse
        {
            get
            {
                return bulkFolderBrowseRecurse;
            }
            set
            {
                SetProperty(ref bulkFolderBrowseRecurse, value);
            }
        }

        bool bulkConvertRunning = false;
        public bool BulkConvertRunning
        {
            get
            {
                return bulkConvertRunning;
            }
            set
            {
                SetProperty(ref bulkConvertRunning, value);
            }
        }

        bool bulkConvertFinished = false;
        public bool BulkConvertFinished
        {
            get
            {
                return bulkConvertFinished;
            }
            set
            {
                SetProperty(ref bulkConvertFinished, value);
            }
        }

        string bulkSaveFolder = null;
        public string BulkSaveFolder
        {
            get
            {
                return bulkSaveFolder;
            }
            set
            {
                SetProperty(ref bulkSaveFolder, value);
            }
        }

        bool bulkUseSourceDestination = false;
        public bool BulkUseSourceDestination
        {
            get
            {
                return bulkUseSourceDestination;
            }
            set
            {
                SetProperty(ref bulkUseSourceDestination, value);
            }
        }

        string bulkStatus = "Ready";
        public string BulkStatus
        {
            get
            {
                return bulkStatus;
            }
            set
            {
                SetProperty(ref bulkStatus, value);
            }
        }

        int bulkProgressMax = 0;
        public int BulkProgressMax
        {
            get
            {
                return bulkProgressMax;
            }
            set
            {
                SetProperty(ref bulkProgressMax, value);
            }
        }

        int bulkProgressValue = 1;
        public int BulkProgressValue
        {
            get
            {
                return bulkProgressValue;
            }
            set
            {
                SetProperty(ref bulkProgressValue, value);
            }
        }
        #endregion Bulk Convert Properties

        #region Loaded Image Properties
        bool loadFailed = false;
        public bool LoadFailed
        {
            get
            {
                return loadFailed;
            }
            set
            {
                SetProperty(ref loadFailed, value);
            }
        }


        public bool IsImageLoaded
        {
            get
            {
                return LoadedImage != null;
            }
        }


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

                if (LoadedImage != null)
                    UpdateLoadedPreview();
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

                return ImageFormats.GetUncompressedSize(LoadedImage.Width, LoadedImage.Height, LoadedImage.NumberOfChannels, LoadedImage.NumMipMaps > 1);
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

        AlphaDisplaySettings alphaDisplaySetting = 0;
        public AlphaDisplaySettings AlphaDisplaySetting
        {
            get
            {
                return alphaDisplaySetting;
            }
            set
            {
                SetProperty(ref alphaDisplaySetting, value);

                if (LoadedImage != null)
                {
                    UpdateLoadedPreview();

                    if (SavePreview != null)
                        UpdateSavePreview(false); // Already have the image, just need to change some alpha bits around.
                }
            }
        }
        #endregion Loaded Image Properties

        #region Save Properties
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


        ImageEngineImage savePreviewIMG = null;
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

        int saveCompressedSize = 0;
        public int SaveCompressedSize
        {
            get
            {
                if (SaveFormat.ToString().Contains("_"))
                {
                    int estimatedMips = DDSGeneral.EstimateNumMipMaps(Width, Height);
                    return ImageFormats.GetCompressedSize(SaveMipType == MipHandling.KeepTopOnly || (SaveMipType == MipHandling.KeepExisting && MipCount == 1) ? 1 : estimatedMips, 
                        SaveFormat, Width, Height);
                }

                return saveCompressedSize;
            }
            set
            {
                SetProperty(ref saveCompressedSize, value);
                OnPropertyChanged(nameof(SaveCompressionRatio));
                OnPropertyChanged(nameof(IsSaveSmaller));
            }
        }

        public string DefaultSavePath
        {
            get
            {
                string name = null;
                if (LoadedImage?.FilePath == null)
                    name = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), $"ImageEngine_{SaveFormat}.{ImageFormats.GetExtensionOfFormat(SaveFormat)}");
                else
                    name = $"{UsefulThings.General.GetFullPathWithoutExtension(LoadedImage.FilePath)}.{ImageFormats.GetExtensionOfFormat(SaveFormat)}";

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
                if (ImageFormats.SaveUnsupported.Contains(value))
                    return;

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

                // Ensure SavePath doesn't already exist
                SavePath = UsefulThings.General.FindValidNewFileName(SavePath);

                // Regenerate save preview
                if (changed && SavePreview != null)
                    UpdateSavePreview();
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

        bool removeGeneralAlpha = false;
        public bool RemoveGeneralAlpha
        {
            get
            {
                return removeGeneralAlpha;
            }
            set
            {
                SetProperty(ref removeGeneralAlpha, value);
                OnPropertyChanged(nameof(SaveCompressedSize));
                OnPropertyChanged(nameof(SaveCompressionRatio));
                OnPropertyChanged(nameof(IsSaveSmaller));

                if (SavePreview != null)
                    UpdateSavePreview();
            }
        }

        public bool IsSaveFormatMippable
        {
            get
            {
                return ImageFormats.IsFormatMippable(SaveFormat);
            }
        }


        DispatcherTimer SliderTimer = new DispatcherTimer();

        public double DXT1AlphaThreshold
        {
            get
            {
                return CSharpImageLibrary.DDS.DDSGeneral.DXT1AlphaThreshold;
            }
            set
            {
                SetProperty(ref CSharpImageLibrary.DDS.DDSGeneral.DXT1AlphaThreshold, value);

                // Update Save Preview - Not every change though (that could be a bunch in 1 second).
                SliderTimer.Stop();
                SliderTimer.Start();
            }
        }

        bool dxt1AlphaRemove = true;
        public bool DXT1AlphaRemove
        {
            get
            {
                return dxt1AlphaRemove;
            }
            set
            {
                SetProperty(ref dxt1AlphaRemove, value);

                if (SavePreview != null)
                    UpdateSavePreview();
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

                // Update Save Preview - Not every change though (that could be a bunch in 1 second).
                SliderTimer.Stop();
                SliderTimer.Start();
            }
        }
        #endregion Save Properties
        #endregion General Properties


        public NewViewModel() : base()
        {
            // Space out the Output Window a bit
            Trace.WriteLine("");
            Trace.WriteLine("");

            SliderTimer.Interval = TimeSpan.FromSeconds(1);
            SliderTimer.Tick += (arg, arg2) =>
            {
                UpdateSavePreview();
                SliderTimer.Stop();
            };
        }

        internal async Task LoadImage(string path)
        {
            timer.Restart();
            byte[] bytes = null;
            try
            {
                bytes = File.ReadAllBytes(path);
            }
            catch (Exception e)
            {
                Trace.WriteLine($"Failed to read image from disk: {e.Message}");
                LoadFailed = true;
                return;
            }

            bool success = await LoadImage(bytes);
            if (!success)
                return;

            LoadedImage.FilePath = path;
            SavePath = DefaultSavePath;
            OnPropertyChanged(nameof(LoadedPath));
        }

        internal async Task<bool> LoadImage(byte[] data)
        {
            LoadFailed = false;

            if (!timer.IsRunning)
                timer.Restart();

            CloseImage(false); // Don't need to update the UI here, it'll get updated after loading the image. But do need to reset some things
            WindowTitle = "Image Engine - View";

            // Full image
            var fullLoad = Task.Run(() => new ImageEngineImage(data));

            // Quick previews
            /*LoadedImage = await Task.Run(() => new ImageEngineImage(data, 512));
            UpdatePreview(LoadedImage.MipMaps[0]);*/

            // Wait for full load to be done

            try
            {
                LoadedImage = await fullLoad;
            }
            catch(Exception e)
            {
                LoadFailed = true;
                return false;
            }

            UpdateLoadedPreview();

            SaveFormat = LoadedFormat;

            timer.Stop();
            Trace.WriteLine($"Loading of {LoadedFormat} ({Width}x{Height}, {(MipCount > 1 ? "Mips Present" : "No Mips")}) = {timer.ElapsedMilliseconds}ms.");
            return true;
        }

        void UpdateLoadedPreview()
        {
            if (MipIndex >= loadedImage.MipMaps.Count)   // Easy way out of handling indicies - Max is bound to mipcount e.g 12, but 0 based index, hence 12 = 13th value, out of range.
                return;

            var mip = LoadedImage.MipMaps[MipIndex];
            var pixels = GetPixels(mip);

            UpdatePreview(ref preview, mip.Width, mip.Height, pixels);

            OnPropertyChanged(nameof(Preview));
            UpdateUI();
        }

        byte[] GetPixels(MipMap mip)
        {
            var pixels = mip.Pixels;
            if (AlphaDisplaySetting == AlphaDisplaySettings.AlphaOnly)  // Get a different set of pixels
                pixels = mip.AlphaOnlyPixels;
            else if (AlphaDisplaySetting == AlphaDisplaySettings.NoAlpha)  // Other two are just an alpha channel different - Bitmap objects are BGRA32, so need to set the alpha to opaque when "don't want it".
                pixels = mip.RGBAOpaque;
            return pixels;
        }

        void UpdatePreview(ref WriteableBitmap bmp, int width, int height, byte[] pixels)
        {
            // Create Preview Object if required - need to recreate if alpha setting changes to premultiplied
            if (bmp == null || (bmp.PixelHeight != height || bmp.PixelWidth != width))
                bmp = UsefulThings.WPF.Images.CreateWriteableBitmap(pixels, width, height);
            else
                RedrawEitherPreview(bmp, pixels, width, height);
        }

        public async Task UpdateSavePreview(bool needRegenerate = true)
        {
            timer.Restart();
            if (needRegenerate)
                await Task.Run(() =>
                {
                    // Save and reload to give accurate depiction of what it'll look like when saved.
                    byte[] data = LoadedImage.Save(SaveFormat, MipHandling.KeepTopOnly, removeAlpha: GeneralRemovingAlpha, customMasks: customMasks);
                    SaveCompressedSize = data.Length;
                    savePreviewIMG = new ImageEngineImage(data);                    
                });

            byte[] pixels = GetPixels(savePreviewIMG.MipMaps[0]);

            UpdatePreview(ref savePreview, savePreviewIMG.Width, savePreviewIMG.Height, pixels);

            // Update Properties
            OnPropertyChanged(nameof(SavePreview));

            timer.Stop();
            Trace.WriteLine($"Save preview of {SaveFormat} ({Width}x{Height}, No Mips) = {timer.ElapsedMilliseconds}ms.");
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

        public void FixExtension(bool indicateSavePathPropertyChanged = false)
        {
            if (SavePath == null || ImageFormats.SaveUnsupported.Contains(SaveFormat))
                return;

            string requiredExtension = "." + ImageFormats.GetExtensionOfFormat(SaveFormat);

            var currentExt = Path.GetExtension(savePath);
            if (currentExt == "")  // No extension
                SavePath += requiredExtension;
            else if (currentExt != requiredExtension)  // Existing extension
                SavePath = Path.ChangeExtension(SavePath, requiredExtension);

            if (indicateSavePathPropertyChanged)
                OnPropertyChanged(nameof(SavePath));
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
            RemoveGeneralAlpha = false; // Other alpha settings not reset because they're specific, but this one spans formats.
            BulkProgressValue = 0;
            BulkProgressMax = 0;
            BulkStatus = "Ready";
            BulkConvertFinished = false;
            BulkConvertRunning = false;
            BulkConvertFiles.Clear();
            BulkConvertFailed.Clear();
            LoadFailed = false;

            // Notify
            if (updateUI)
                UpdateUI();
        }

        public async Task DoBulkConvert()
        {
            BulkProgressMax = BulkConvertFiles.Count;
            BulkProgressValue = 0;
            BulkStatus = $"Converting {BulkProgressValue}/{BulkProgressMax} images.";
            BulkConvertFinished = false;
            BulkConvertRunning = true;


            await Task.Run(async () =>
            {
                // Test if can parallelise uncompressed saving
                // Below says: Only formats that don't support mips or do but aren't block compressed - can be parallised.
                bool supportsParallel = !ImageFormats.IsFormatMippable(SaveFormat);
                supportsParallel |= !supportsParallel && !ImageFormats.IsBlockCompressed(SaveFormat);

                
                if (supportsParallel)
                    await DoBulkParallel();
                else
                    foreach (var file in BulkConvertFiles)
                    {
                        using (ImageEngineImage img = new ImageEngineImage(file))
                        {
                            string filename = Path.GetFileNameWithoutExtension(file) + "." + ImageFormats.GetExtensionOfFormat(SaveFormat);
                            string path = Path.Combine(BulkUseSourceDestination ? Path.GetDirectoryName(file) : BulkSaveFolder, filename);

                            path = UsefulThings.General.FindValidNewFileName(path);

                            try
                            {
                                await img.Save(path, SaveFormat, SaveMipType, removeAlpha: GeneralRemovingAlpha, customMasks: customMasks);
                            }
                            catch (Exception e)
                            {
                                BulkConvertFailed.Add(path + "  Reason: " + e.ToString());
                            }
                        }

                        BulkProgressValue++;
                        BulkStatus = $"Converting {BulkProgressValue}/{BulkProgressMax} images.";
                    }
            });


            BulkStatus = "Conversion complete! ";
            if (BulkConvertFailed.Count > 0)
                BulkStatus += $"{BulkConvertFailed.Count} failed to convert.";

            BulkProgressValue = BulkProgressMax;
            BulkConvertFinished = true;
            BulkConvertRunning = false;
        }

        Task DoBulkParallel()
        {
            BufferBlock<string> fileNameStore = new BufferBlock<string>();
            int maxParallelism = ImageEngine.NumThreads == 1 ? 1 :
                (ImageEngine.NumThreads == -1 ? Environment.ProcessorCount : ImageEngine.NumThreads);


            // Define block to perform each conversion
            var encoder = new TransformBlock<string, Tuple<byte[], string>>(file =>
            {
                byte[] data = null;

                string filename = Path.GetFileNameWithoutExtension(file) + "." + ImageFormats.GetExtensionOfFormat(SaveFormat);
                string path = Path.Combine(BulkUseSourceDestination ? Path.GetDirectoryName(file) : BulkSaveFolder, filename);
                path = UsefulThings.General.FindValidNewFileName(path);

                using (ImageEngineImage img = new ImageEngineImage(file))
                {
                    try
                    {
                        data = img.Save(SaveFormat, SaveMipType, removeAlpha: GeneralRemovingAlpha, customMasks: customMasks);
                    }
                    catch (Exception e)
                    {
                        BulkConvertFailed.Add(path + "  Reason: " + e.ToString());
                    }
                }

                BulkProgressValue++;
                BulkStatus = $"Converting {BulkProgressValue}/{BulkProgressMax} images.";
                return new Tuple<byte[], string>(data, path);
            }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxParallelism, BoundedCapacity = maxParallelism });

            // Define block to write converted data to disk
            var diskWriter = new ActionBlock<Tuple<byte[], string>>(tuple =>
            {
                File.WriteAllBytes(tuple.Item2, tuple.Item1);
            }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 2, BoundedCapacity = maxParallelism });  // Limit to 2 disk write operations at a time, but allow many to be stored in it's buffer.


            // Link blocks together
            fileNameStore.LinkTo(encoder, new DataflowLinkOptions { PropagateCompletion = true });
            encoder.LinkTo(diskWriter, new DataflowLinkOptions { PropagateCompletion = true });

            // Begin production
            new Action(async () =>
            {
                foreach (var file in BulkConvertFiles)
                    await fileNameStore.SendAsync(file);

                fileNameStore.Complete();
            }).Invoke();

            return diskWriter.Completion;
        }
    }
}

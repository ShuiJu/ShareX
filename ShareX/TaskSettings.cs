#region License Information (GPL v3)

/*
    ShareX - A program that allows you to take screenshots and share any file type
    Copyright (c) 2007-2026 ShareX Team

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)

using Newtonsoft.Json;
using ShareX.AvaloniaUI.Windows;
using ShareX.HelpersLib;
using ShareX.ImageEditor.Integration;
using ShareX.ImageEffectsLib;
using ShareX.ScreenCaptureLib;
using ShareX.Tools;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;

namespace ShareX
{
    [JsonConverter(typeof(LocalSettingsConverter))]
    public class TaskSettings
    {
        [JsonIgnore]
        public TaskSettings TaskSettingsReference { get; private set; }

        [JsonIgnore]
        public bool IsSafeTaskSettings => TaskSettingsReference != null;

        public string Description = "";

        public HotkeyType Job = HotkeyType.None;

        public bool UseDefaultAfterCaptureJob = true;
        public AfterCaptureTasks AfterCaptureJob = AfterCaptureTasks.CopyImageToClipboard | AfterCaptureTasks.SaveImageToFile;


        public bool OverrideScreenshotsFolder = false;
        public string ScreenshotsFolder = "";

        public bool UseDefaultGeneralSettings = true;
        public TaskSettingsGeneral GeneralSettings = new TaskSettingsGeneral();

        public bool UseDefaultImageSettings = true;
        public TaskSettingsImage ImageSettings = new TaskSettingsImage();

        [JsonIgnore]
        public TaskSettingsImage ImageSettingsReference
        {
            get
            {
                if (UseDefaultImageSettings)
                {
                    return ApplicationState.DefaultTaskSettings.ImageSettings;
                }

                return TaskSettingsReference.ImageSettings;
            }
        }

        public bool UseDefaultCaptureSettings = true;
        public TaskSettingsCapture CaptureSettings = new TaskSettingsCapture();

        [JsonIgnore]
        public TaskSettingsCapture CaptureSettingsReference
        {
            get
            {
                if (UseDefaultCaptureSettings)
                {
                    return ApplicationState.DefaultTaskSettings.CaptureSettings;
                }

                return TaskSettingsReference.CaptureSettings;
            }
        }


        public bool UseDefaultActions = true;
        public List<ExternalProgram> ExternalPrograms = new List<ExternalProgram>();

        public bool UseDefaultToolsSettings = true;
        public TaskSettingsTools ToolsSettings = new TaskSettingsTools();

        [JsonIgnore]
        public TaskSettingsTools ToolsSettingsReference
        {
            get
            {
                if (UseDefaultToolsSettings)
                {
                    return ApplicationState.DefaultTaskSettings.ToolsSettings;
                }

                return TaskSettingsReference.ToolsSettings;
            }
        }

        public bool UseDefaultAdvancedSettings = true;
        public TaskSettingsAdvanced AdvancedSettings = new TaskSettingsAdvanced();


        public override string ToString()
        {
            return !string.IsNullOrEmpty(Description) ? Description : Job.GetLocalizedDescription();
        }

        public bool IsUsingDefaultSettings
        {
            get
            {
                return UseDefaultAfterCaptureJob && !OverrideScreenshotsFolder && UseDefaultGeneralSettings &&
                    UseDefaultImageSettings && UseDefaultCaptureSettings && UseDefaultActions &&
                    UseDefaultToolsSettings && UseDefaultAdvancedSettings;
            }
        }

        public static TaskSettings GetDefaultTaskSettings()
        {
            TaskSettings taskSettings = new TaskSettings();
            taskSettings.SetDefaultSettings();
            taskSettings.TaskSettingsReference = ApplicationState.DefaultTaskSettings;
            return taskSettings;
        }

        public static TaskSettings GetSafeTaskSettings(TaskSettings taskSettings)
        {
            TaskSettings safeTaskSettings;

            if (taskSettings.IsUsingDefaultSettings && ApplicationState.DefaultTaskSettings != null)
            {
                safeTaskSettings = ApplicationState.DefaultTaskSettings.Copy();
                safeTaskSettings.Description = taskSettings.Description;
                safeTaskSettings.Job = taskSettings.Job;
            }
            else
            {
                safeTaskSettings = taskSettings.Copy();
                safeTaskSettings.SetDefaultSettings();
            }

            safeTaskSettings.TaskSettingsReference = taskSettings;
            return safeTaskSettings;
        }

        public void SetDefaultSettings()
        {
            if (ApplicationState.DefaultTaskSettings != null)
            {
                TaskSettings defaultTaskSettings = ApplicationState.DefaultTaskSettings.Copy();

                if (UseDefaultAfterCaptureJob)
                {
                    AfterCaptureJob = defaultTaskSettings.AfterCaptureJob;
                }


                if (UseDefaultGeneralSettings)
                {
                    GeneralSettings = defaultTaskSettings.GeneralSettings;
                }

                if (UseDefaultImageSettings)
                {
                    ImageSettings = defaultTaskSettings.ImageSettings;
                }

                if (UseDefaultCaptureSettings)
                {
                    CaptureSettings = defaultTaskSettings.CaptureSettings;
                }


                if (UseDefaultActions)
                {
                    ExternalPrograms = defaultTaskSettings.ExternalPrograms;
                }

                if (UseDefaultToolsSettings)
                {
                    ToolsSettings = defaultTaskSettings.ToolsSettings;
                }

                if (UseDefaultAdvancedSettings)
                {
                    AdvancedSettings = defaultTaskSettings.AdvancedSettings;
                }
            }
        }

        public void Cleanup()
        {
            if (UseDefaultGeneralSettings)
            {
                GeneralSettings = null;
            }

            if (UseDefaultImageSettings)
            {
                ImageSettings = null;
            }

            if (UseDefaultCaptureSettings)
            {
                CaptureSettings = null;
            }


            if (UseDefaultActions)
            {
                ExternalPrograms = null;
            }

            if (UseDefaultToolsSettings)
            {
                ToolsSettings = null;
            }

            if (UseDefaultAdvancedSettings)
            {
                AdvancedSettings = null;
            }
        }

    }

    [JsonConverter(typeof(LocalSettingsConverter))]
    public class TaskSettingsGeneral
    {
        #region General / Notifications

        public bool PlaySoundAfterCapture = true;
        public bool PlaySoundAfterTaskCompleted = true;
        public bool PlaySoundAfterAction = true;
        public bool ShowToastNotificationAfterTaskCompleted = true;
        public float ToastWindowDuration = 3f;
        public float ToastWindowFadeDuration = 1f;
        public ImageContentAlignment ToastWindowPlacement = ImageContentAlignment.BottomRight;
        public Size ToastWindowSize = new Size(400, 300);
        public ToastClickAction ToastWindowLeftClickAction = ToastClickAction.OpenUrl;
        public ToastClickAction ToastWindowRightClickAction = ToastClickAction.CloseNotification;
        public ToastClickAction ToastWindowMiddleClickAction = ToastClickAction.AnnotateImage;
        public int ToastWindowButtonSize = 40;
        public List<NotificationActionButton> ToastWindowButtons = NotificationActionButton.CreateDefaultButtons();
        public bool ToastWindowAutoHide = true;
        public bool DisableNotificationsOnFullscreen = false;
        public bool UseCustomCaptureSound = false;
        public string CustomCaptureSoundPath = "";
        public bool UseCustomTaskCompletedSound = false;
        public string CustomTaskCompletedSoundPath = "";
        public bool UseCustomActionCompletedSound = false;
        public string CustomActionCompletedSoundPath = "";
        public bool UseCustomErrorSound = false;
        public string CustomErrorSoundPath = "";

        #endregion
    }

    public class TaskSettingsImage
    {
        #region Image / General

        public EImageFormat ImageFormat = EImageFormat.PNG;
        public bool UseCustomTimeZone = false;
        public TimeZoneInfo CustomTimeZone = TimeZoneInfo.Utc;
        public string NameFormatPattern = "%ra{10}";
        public string NameFormatPatternActiveWindow = "%pn_%ra{10}";
        public PNGBitDepth ImagePNGBitDepth = PNGBitDepth.Default;
        public BMPBitDepth ImageBMPBitDepth = BMPBitDepth.Bit24;
        public int ImagePNGCompressionLevel = 1;
        public SKPngEncoderFilterFlags ImagePNGFilter = SKPngEncoderFilterFlags.AllFilters;
        public int ImageJPEGQuality = 90;
        public SKJpegEncoderDownsample ImageJPEGSubsampling = SKJpegEncoderDownsample.Downsample420;
        public GIFQuality ImageGIFQuality = GIFQuality.Adaptive;
        public bool ImageAutoUseJPEG = true;
        public int ImageAutoUseJPEGSize = 2048;
        public bool ImageAutoJPEGQuality = false;
        public FileExistAction FileExistAction = FileExistAction.Ask;

        #endregion Image / General

        #region Image / Effects

        public List<ImageEffectPreset> ImageEffectPresets = new List<ImageEffectPreset>() { ImageEffectPreset.GetDefaultPreset() };
        public int SelectedImageEffectPreset = 0;

        public bool ShowImageEffectsWindowAfterCapture = false;
        public bool ImageEffectOnlyRegionCapture = false;
        public bool UseRandomImageEffect = false;

        #endregion Image / Effects

        #region Image / Thumbnail

        public int ThumbnailWidth = 200;
        public int ThumbnailHeight = 0;
        public string ThumbnailName = "-thumbnail";
        public bool ThumbnailCheckSize = false;

        #endregion Image / Thumbnail
    }

    public class TaskSettingsCapture
    {
        #region Capture / General

        public bool ShowCursor = true;
        public decimal ScreenshotDelay = 0;
        public bool CaptureTransparent = false;
        public bool CaptureShadow = true;
        public int CaptureShadowOffset = 100;
        public bool CaptureClientArea = false;
        public bool CaptureAutoHideTaskbar = false;
        public bool CaptureAutoHideDesktopIcons = false;
        public bool HDRScreenshotColorCorrection = false;
        public Rectangle CaptureCustomRegion = new Rectangle(0, 0, 0, 0);
        public string CaptureCustomWindow = "";

        #endregion Capture / General

        #region Capture / Region capture

        public RegionCaptureOptions RegionCaptureOptions = new RegionCaptureOptions();

        #endregion Capture / Region capture

        #region Capture / Screen recorder

        public FFmpegOptions FFmpegOptions = new FFmpegOptions();
        public bool ScreenRecordUseNative = true;
        public bool ScreenRecordShowBar = true;
        public bool ScreenRecordSystemAudio = true;
        public string ScreenRecordSystemAudioDeviceId = "";
        public float ScreenRecordSystemAudioGain = 1f;
        public bool ScreenRecordMicrophone = false;
        public string ScreenRecordMicrophoneDeviceId = "";
        public float ScreenRecordMicrophoneGain = 1f;
        public bool ScreenRecordCamera = false;
        public string ScreenRecordCameraDeviceId = "";
        public ShareX.ScreenRecordingLib.CameraCaptureResolution ScreenRecordCameraResolution = ShareX.ScreenRecordingLib.CameraCaptureResolution.Size1280x720;
        public int ScreenRecordCameraFPS = 30;
        public ShareX.ScreenRecordingLib.CameraOverlayPosition ScreenRecordCameraPosition = ShareX.ScreenRecordingLib.CameraOverlayPosition.BottomRight;
        public ShareX.ScreenRecordingLib.CameraOverlayShape ScreenRecordCameraShape = ShareX.ScreenRecordingLib.CameraOverlayShape.Rectangle;
        public int ScreenRecordCameraWidthPercent = 20;
        public int ScreenRecordCameraMargin = 20;
        public bool ScreenRecordRequireHardwareEncoder = true;
        public int ScreenRecordVideoBitrate = 10000; // kbps
        public int ScreenRecordFPS = 30;
        public int GIFFPS = 15;
        public bool ScreenRecordShowCursor = true;
        public bool ScreenRecordMouseHighlighter = false;
        public bool ScreenRecordShowTimer = true;
        public bool ScreenRecordShowButtonLabels = false;
        public bool ScreenRecordStartDelayEnabled = false;
        public float ScreenRecordStartDelay = 3f;
        public bool ScreenRecordFixedDuration = false;
        public float ScreenRecordDuration = 3f;
        public bool ScreenRecordTwoPassEncoding = false;
        public bool ScreenRecordAskConfirmationOnAbort = false;

        #endregion Capture / Screen recorder

        #region Capture / Scrolling capture

        public ScrollingCaptureOptions ScrollingCaptureOptions = new ScrollingCaptureOptions();

        #endregion Capture / Scrolling capture

        #region Capture / OCR

        public OCROptions OCROptions = new OCROptions();

        #endregion Capture / OCR
    }


    public class TaskSettingsTools
    {
        public ColorPickerOptions ColorPickerOptions = new ColorPickerOptions();
        public ScreenColorPickerOptions ScreenColorPickerOptions = new ScreenColorPickerOptions();
        public PinToScreenOptions PinToScreenOptions = new PinToScreenOptions();
        public MouseHighlighterOptions MouseHighlighterOptions = new MouseHighlighterOptions();
        public IndexerSettings IndexerSettings = new IndexerSettings();
        public ImageCombinerOptions ImageCombinerOptions = new ImageCombinerOptions();
        public VideoConverterOptions VideoConverterOptions = new VideoConverterOptions();
        public VideoThumbnailOptions VideoThumbnailOptions = new VideoThumbnailOptions();
        public BorderlessWindowSettings BorderlessWindowSettings = new BorderlessWindowSettings();
        public ImageEditorOptions ImageEditorOptions = new ImageEditorOptions();
        public BackgroundRemoverOptions BackgroundRemoverOptions = new BackgroundRemoverOptions();
    }

    public class TaskSettingsAdvanced
    {

        [Category("General"), DefaultValue(true), Description("Save locally generated text as a file.")]
        public bool TextTaskSaveAsFile { get; set; }


        [Category("Capture"), DefaultValue(false), Description("Disable annotation support in region capture.")]
        public bool RegionCaptureDisableAnnotation { get; set; }

        [Category("Files"), Description("File extensions treated as images when running local tasks.")]
        public List<string> ImageExtensions { get; set; }

        [Category("Files"), Description("File extensions treated as text when running local tasks.")]
        public List<string> TextExtensions { get; set; }


        [Category("Text"), DefaultValue("txt"), Description("File extension when saving text to the local hard disk.")]
        public string TextFileExtension { get; set; }

        [Category("Text"), DefaultValue("text"), Description("Text format e.g. csharp, cpp, etc.")]
        public string TextFormat { get; set; }

        [Category("Text"), DefaultValue(""), Description("Custom text input. Use %input for text input. Example you can create web page with your text in it.")]
        public string TextCustom { get; set; }

        [Category("Text"), DefaultValue(true), Description("HTML encode custom text input.")]
        public bool TextCustomEncodeInput { get; set; }


        [Category("Name pattern"), DefaultValue(100), Description("Maximum name pattern length for file name.")]
        public int NamePatternMaxLength { get; set; }

        [Category("Name pattern"), DefaultValue(50), Description("Maximum name pattern title (%t) length for file name.")]
        public int NamePatternMaxTitleLength { get; set; }

        public TaskSettingsAdvanced()
        {
            this.ApplyDefaultPropertyValues();
            ImageExtensions = FileHelpers.ImageFileExtensions.ToList();
            TextExtensions = FileHelpers.TextFileExtensions.ToList();
        }
    }
}

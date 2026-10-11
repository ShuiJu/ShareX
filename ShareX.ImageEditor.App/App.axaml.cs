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

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ShareX.AvaloniaUI.Theming;
using Newtonsoft.Json;
using ShareX.ImageEditor.Integration;
using ShareX.ImageEditor.Core.OCR;
using ShareX.ImageEditor.Presentation.ViewModels;
using ShareX.ImageEditor.Presentation.Views;
using SkiaSharp;
using System;
using System.IO;

namespace ShareX.ImageEditor.App
{
    public partial class App : Application
    {
        private static readonly string[] ImageExtensions = [".png", ".jpg", ".jpeg", ".gif", ".bmp", ".webp", ".ico"];
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ShareX", "ImageEditor", "settings.json");

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            ThemeManager.Configure(new ApplicationThemeOptions());
#if WINDOWS
            EditorServices.BuiltInOcr = new WindowsOcrService((image, cancellationToken) =>
                ShareX.Tools.OCRHelper.OCR(image, cancellationToken: cancellationToken));
#endif

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                ImageEditorOptions options = LoadOptions();

#if DEBUG
                options.ShowExitConfirmation = false;
#endif

                EditorWindow window = new EditorWindow(options);
                desktop.MainWindow = window;
                window.Closed += (_, _) => SaveOptions(options);

                if (window.DataContext is MainViewModel vm)
                {
                    vm.ShowFileMenu = true;
                    vm.ShowTaskButtons = false;
                    vm.UseContinueWorkflow = false;
                    vm.ShowBottomToolbar = true;
                    vm.ShowStartScreen = true;

                    string? imagePath = GetImagePathFromArgs(desktop.Args);

#if DEBUG
                    vm.ShowStartScreen = false;

                    if (!vm.ShowStartScreen && string.IsNullOrEmpty(imagePath))
                    {
                        SKBitmap debugBitmap = new SKBitmap(800, 600);
                        debugBitmap.Erase(new SKColor(35, 35, 35));
                        window.LoadImage(debugBitmap);
                    }
#endif

                    if (!string.IsNullOrEmpty(imagePath))
                    {
                        window.LoadImage(imagePath);
                    }
                }
            }

            base.OnFrameworkInitializationCompleted();
        }

        private static ImageEditorOptions LoadOptions()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    return JsonConvert.DeserializeObject<ImageEditorOptions>(File.ReadAllText(SettingsPath))
                        ?? new ImageEditorOptions();
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
            {
                EditorServices.ReportWarning(nameof(App), "Failed to load editor settings.", ex);
            }

            return new ImageEditorOptions();
        }

        private static void SaveOptions(ImageEditorOptions options)
        {
            string temporaryPath = SettingsPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
                File.WriteAllText(temporaryPath, JsonConvert.SerializeObject(options, Formatting.Indented));
                File.Move(temporaryPath, SettingsPath, overwrite: true);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
            {
                EditorServices.ReportWarning(nameof(App), "Failed to save editor settings.", ex);
            }
            finally
            {
                try { File.Delete(temporaryPath); }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    EditorServices.ReportWarning(nameof(App), "Failed to remove temporary editor settings.", ex);
                }
            }
        }

        private string? GetImagePathFromArgs(string[]? args)
        {
            if (args == null || args.Length == 0)
            {
                return null;
            }

            string filePath = args[0];

            if (File.Exists(filePath))
            {
                string extension = Path.GetExtension(filePath);

                if (Array.Exists(ImageExtensions, ext => ext.Equals(extension, StringComparison.OrdinalIgnoreCase)))
                {
                    return filePath;
                }
            }

            return null;
        }
    }
}

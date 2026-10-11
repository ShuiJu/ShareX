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

using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ShareX.ImageEditor.Core.OCR;
using ShareX.ImageEditor.Localization;
using SkiaSharp;
using System.Text;

namespace ShareX.ImageEditor.Presentation.Views
{
    public partial class OcrWindow : Window
    {
        private readonly IOcrService _service;
        private readonly bool _useGpu;
        private readonly CancellationTokenSource _cancellation = new();
        private readonly TaskCompletionSource _firstFrame = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TextBlock _statusText;
        private readonly ProgressBar _busyProgress;
        private readonly TextBox _resultText;
        private readonly Button _saveButton;
        private readonly Button _copyButton;
        private SKBitmap? _image;
        private Task? _recognitionTask;
        private string? _text;
        private bool _closingRequested;
        private bool _allowClose;
        private bool _closed;
        private bool _actionRunning;
        private bool _recognitionFinished;

        /// <summary>Takes ownership of the snapshot until recognition and native cleanup finish.</summary>
        public OcrWindow(SKBitmap image, bool useGpu, IOcrService service)
        {
            ArgumentNullException.ThrowIfNull(image);
            ArgumentNullException.ThrowIfNull(service);

            AvaloniaXamlLoader.Load(this);
            _statusText = this.FindControl<TextBlock>("StatusText")!;
            _busyProgress = this.FindControl<ProgressBar>("BusyProgress")!;
            _resultText = this.FindControl<TextBox>("ResultText")!;
            _saveButton = this.FindControl<Button>("SaveButton")!;
            _copyButton = this.FindControl<Button>("CopyButton")!;
            _service = service;
            _useGpu = useGpu;
            _image = image;
        }

        /// <summary>Completes after recognition, backend cleanup, and disposal of the snapshot.</summary>
        public Task RecognitionTask => _recognitionTask ?? Task.CompletedTask;

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            if (_recognitionTask != null || _closingRequested) return;

            RequestAnimationFrame(_ => _firstFrame.TrySetResult());
            _recognitionTask = RecognizeAsync();
        }

        private async Task RecognizeAsync()
        {
            try
            {
                // The frame callback runs before painting. Background dispatch lets that frame
                // finish before any encoding, model preparation, or service work can begin.
                await _firstFrame.Task.WaitAsync(_cancellation.Token);
                await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
                _cancellation.Token.ThrowIfCancellationRequested();

                var progress = new Progress<OcrProgress>(ShowProgress);
                string text = await Task.Run(() => _service.RecognizeAsync(
                    _image!, _useGpu, progress, _cancellation.Token));

                // The service does not return until its native process has exited.
                if (_closingRequested || _closed) return;
                _text = text;
                _resultText.Text = text;
                _resultText.IsVisible = true;
                _statusText.Text = string.Empty;
                _statusText.IsVisible = false;
                UpdateActionButtons();
                _resultText.Focus();
            }
            catch (OperationCanceledException) when (_cancellation.IsCancellationRequested)
            {
                SetStatus(Strings.OcrWindow_Cancelled);
            }
            catch (Exception ex)
            {
                SetStatus($"{Strings.OcrWindow_Failed}{Environment.NewLine}{ex.Message}");
            }
            finally
            {
                _recognitionFinished = true;
                DisposeImage();
                if (!_closed) _busyProgress.IsVisible = false;
            }
        }

        private void ShowProgress(OcrProgress progress)
        {
            if (_closed || _closingRequested || _recognitionFinished) return;

            string status = progress.Stage == OcrStage.Recognizing
                ? Strings.OcrWindow_Recognizing
                : Strings.OcrWindow_Loading;
            SetStatus(string.IsNullOrWhiteSpace(progress.Detail)
                ? status
                : $"{status}{Environment.NewLine}{progress.Detail}");
            _busyProgress.IsIndeterminate = !progress.Fraction.HasValue;
            _busyProgress.Value = progress.Fraction is double fraction
                ? Math.Clamp(fraction, 0, 1) * 100
                : 0;
        }

        protected override async void OnClosing(WindowClosingEventArgs e)
        {
            base.OnClosing(e);
            if (_allowClose) return;

            e.Cancel = true;
            if (_closingRequested) return;
            _closingRequested = true;
            UpdateActionButtons();

            try
            {
                try
                {
                    _cancellation.Cancel();
                }
                catch (AggregateException ex)
                {
                    SetStatus($"{Strings.OcrWindow_Failed}{Environment.NewLine}{ex.Message}");
                }

                if (_recognitionTask != null) await _recognitionTask;
            }
            catch (Exception ex)
            {
                SetStatus($"{Strings.OcrWindow_Failed}{Environment.NewLine}{ex.Message}");
            }
            finally
            {
                DisposeImage();
                _allowClose = true;
                Dispatcher.UIThread.Post(Close);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _closed = true;
            DisposeImage();
            _cancellation.Dispose();
            base.OnClosed(e);
        }

        private void DisposeImage()
        {
            SKBitmap? image = _image;
            _image = null;
            image?.Dispose();
        }

        private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();

        private async void OnCopyClick(object? sender, RoutedEventArgs e)
        {
            if (_text == null || _actionRunning || _closingRequested) return;
            _actionRunning = true;
            UpdateActionButtons();
            try
            {
                if (Clipboard == null) throw new InvalidOperationException(Strings.OcrWindow_ActionFailed);
                await Clipboard.SetTextAsync(_text);
            }
            catch (Exception ex)
            {
                SetStatus($"{Strings.OcrWindow_ActionFailed}{Environment.NewLine}{ex.Message}");
            }
            finally
            {
                _actionRunning = false;
                UpdateActionButtons();
            }
        }

        private async void OnSaveClick(object? sender, RoutedEventArgs e)
        {
            if (_text == null || _actionRunning || _closingRequested) return;
            _actionRunning = true;
            UpdateActionButtons();
            string text = _text;
            try
            {
                if (!StorageProvider.CanSave) throw new InvalidOperationException(Strings.OcrWindow_ActionFailed);
                using IStorageFile? file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = Strings.OcrWindow_Save,
                    SuggestedFileName = "ocr.txt",
                    DefaultExtension = "txt",
                    ShowOverwritePrompt = true,
                    FileTypeChoices =
                    [
                        new FilePickerFileType(Strings.OcrWindow_TextFile)
                        {
                            Patterns = ["*.txt"],
                            MimeTypes = ["text/plain"]
                        }
                    ]
                });

                if (file == null || _closed || _closingRequested) return;
                await using Stream stream = await file.OpenWriteAsync();
                if (stream.CanSeek) stream.SetLength(0);
                await using var writer = new StreamWriter(stream, new UTF8Encoding(false, true));
                await writer.WriteAsync(text);
            }
            catch (Exception ex)
            {
                SetStatus($"{Strings.OcrWindow_ActionFailed}{Environment.NewLine}{ex.Message}");
            }
            finally
            {
                _actionRunning = false;
                UpdateActionButtons();
            }
        }

        private void SetStatus(string text)
        {
            if (_closed) return;
            _statusText.Text = text;
            _statusText.IsVisible = true;
        }

        private void UpdateActionButtons()
        {
            if (_closed) return;
            bool enabled = _text != null && !_actionRunning && !_closingRequested;
            _saveButton.IsEnabled = enabled;
            _copyButton.IsEnabled = enabled;
        }
    }
}

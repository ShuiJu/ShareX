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
using ShareX.ImageEditor.Core.OCR;
using ShareX.ImageEditor.Integration;
using ShareX.ImageEditor.Presentation.ViewModels;
using SkiaSharp;

namespace ShareX.ImageEditor.Presentation.Views;

public partial class EditorView
{
    private OcrWindow? _ocrWindow;

    private async Task OnOcrRequested()
    {
        if (DataContext is not MainViewModel vm || !vm.HasPreviewImage ||
            TopLevel.GetTopLevel(this) is not Window owner)
        {
            return;
        }

        if (_ocrWindow != null)
        {
            _ocrWindow.Activate();
            return;
        }

        SKBitmap? snapshot = GetSnapshot();
        if (snapshot == null) return;

        try
        {
            var service = vm.IsXiaomiOcr
                ? EditorServices.XiaomiOcr
                : EditorServices.BuiltInOcr;
            _ocrWindow = new OcrWindow(snapshot, vm.OcrUseGpu, service);
            snapshot = null; // Ownership passes to the OCR window.
            await _ocrWindow.ShowDialog(owner);
        }
        catch (Exception ex)
        {
            EditorServices.ReportError(nameof(EditorView), "Failed to open OCR window.", ex);
        }
        finally
        {
            snapshot?.Dispose();
            _ocrWindow = null;
        }
    }
}

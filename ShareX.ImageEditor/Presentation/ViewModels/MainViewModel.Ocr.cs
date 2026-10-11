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

using CommunityToolkit.Mvvm.Input;
using ShareX.ImageEditor.Core.OCR;

namespace ShareX.ImageEditor.Presentation.ViewModels;

public partial class MainViewModel
{
    public event Func<Task>? OcrRequested;

    public int OcrEngineIndex
    {
        get => (int)Options.OcrEngine;
        set
        {
            if (value is < 0 or > 1 || Options.OcrEngine == (OcrEngineKind)value) return;
            Options.OcrEngine = (OcrEngineKind)value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsXiaomiOcr));
        }
    }

    public bool IsXiaomiOcr => Options.OcrEngine == OcrEngineKind.Xiaomi;

    public bool OcrUseGpu
    {
        get => Options.OcrUseGpu;
        set
        {
            if (Options.OcrUseGpu == value) return;
            Options.OcrUseGpu = value;
            OnPropertyChanged();
        }
    }

    [RelayCommand(CanExecute = nameof(HasPreviewImage))]
    private async Task RecognizeTextAsync()
    {
        if (!IsModalOpen && OcrRequested is { } handler)
        {
            await handler();
        }
    }
}

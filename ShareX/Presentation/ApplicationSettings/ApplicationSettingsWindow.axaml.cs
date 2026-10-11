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

#nullable enable

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ShareX.AvaloniaUI.Theming;
using ShareX.HelpersLib;
using ShareX.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ShareX;

public partial class ApplicationSettingsWindow : Window
{
    private ApplicationSettingsViewModel ViewModel => (ApplicationSettingsViewModel)DataContext!;
    private ContextMenu? _activeCodeMenu;


    public ApplicationSettingsWindow()
    {
        InitializeComponent();
        RequestedThemeVariant = ThemeManager.GetCurrentTheme();
        DataContext = new ApplicationSettingsViewModel();
        AttachFilenamePatternMenu(SaveImageSubFolderPatternTextBox,
            CodeMenuEntryFilename.t, CodeMenuEntryFilename.pn, CodeMenuEntryFilename.i,
            CodeMenuEntryFilename.width, CodeMenuEntryFilename.height, CodeMenuEntryFilename.n);
        AttachFilenamePatternMenu(SaveImageSubFolderPatternWindowTextBox,
            CodeMenuEntryFilename.i, CodeMenuEntryFilename.n);
        Opened += (_, _) => Activate();
        ThemeManager.ThemeChanged += OnThemeChanged;
        Closed += OnClosed;
    }

    private void OnThemeChanged(object? sender, Avalonia.Styling.ThemeVariant theme) =>
        Dispatcher.UIThread.Post(() =>
        {
            RequestedThemeVariant = theme;
            ViewModel.RefreshThemeSettings();
        });

    private void OnClosed(object? sender, EventArgs e)
    {
        ThemeManager.ThemeChanged -= OnThemeChanged;
        ViewModel.Dispose();
    }

    private void OnRestartClick(object? sender, RoutedEventArgs e) => ViewModel.Restart();
    private void OnEditQuickTaskMenuClick(object? sender, RoutedEventArgs e) => ViewModel.EditQuickTaskMenu();
    private async void OnCheckDevBuildClick(object? sender, RoutedEventArgs e) => await ViewModel.CheckDevBuildAsync();
    private void OnOpenPersonalFolderClick(object? sender, RoutedEventArgs e) => ViewModel.OpenPersonalFolder();
    private void OnOpenScreenshotsFolderClick(object? sender, RoutedEventArgs e) => ViewModel.OpenScreenshotsFolder();
    private void OnResetThumbnailSizeClick(object? sender, RoutedEventArgs e) => ViewModel.ResetThumbnailSize();
    private void OnImagePrintSettingsClick(object? sender, RoutedEventArgs e) => ViewModel.ShowImagePrintSettings(this);
    private async void OnResetSettingsClick(object? sender, RoutedEventArgs e) => await ViewModel.ResetAsync();

    private async void OnBrowsePersonalFolderClick(object? sender, RoutedEventArgs e)
    {
        string? path = await PickFolderAsync(Strings.ApplicationSettingsWindow_ChooseShareXPersonalFolderPath);
        if (!string.IsNullOrEmpty(path))
        {
            ViewModel.PersonalFolderPath = path;
        }
    }

    private async void OnBrowseScreenshotsFolderClick(object? sender, RoutedEventArgs e)
    {
        string? path = await PickFolderAsync(Strings.ApplicationSettingsWindow_ChooseScreenshotsFolderPath);
        if (!string.IsNullOrEmpty(path))
        {
            ViewModel.CustomScreenshotsPath = path;
        }
    }

    private async System.Threading.Tasks.Task<string?> PickFolderAsync(string title)
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = title,
            AllowMultiple = false
        });
        return folders.FirstOrDefault()?.TryGetLocalPath();
    }

    private async void OnExportClick(object? sender, RoutedEventArgs e)
    {
        string machineName = FileHelpers.SanitizeFileName(Environment.MachineName.ToLowerInvariant());
        IStorageFile? file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = Strings.ApplicationSettingsWindow_ExportShareXBackup,
            SuggestedFileName = $"ShareX-{Helpers.GetApplicationVersion()}-{machineName}-backup.sxb",
            DefaultExtension = "sxb",
            FileTypeChoices =
            [
                new FilePickerFileType(Strings.ApplicationSettingsWindow_ShareXBackup) { Patterns = ["*.sxb"] },
                FilePickerFileTypes.All
            ]
        });

        string? path = file?.TryGetLocalPath();
        if (!string.IsNullOrEmpty(path))
        {
            await ViewModel.ExportAsync(path);
        }
    }

    private async void OnImportClick(object? sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = Strings.ApplicationSettingsWindow_ImportShareXBackup,
            AllowMultiple = false,
            FileTypeFilter = [new FilePickerFileType(Strings.ApplicationSettingsWindow_ShareXBackup) { Patterns = ["*.sxb"] }]
        });

        string? path = files.FirstOrDefault()?.TryGetLocalPath();
        if (!string.IsNullOrEmpty(path) && File.Exists(path))
        {
            await ViewModel.ImportAsync(path);
        }
    }


    private void AttachFilenamePatternMenu(TextBox textBox, params CodeMenuEntryFilename[] ignoredEntries)
    {
        HashSet<CodeMenuEntryFilename> ignored = ignoredEntries.ToHashSet();
        IEnumerable<CodeMenuEntryFilename> entries = typeof(CodeMenuEntryFilename)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.FieldType == typeof(CodeMenuEntryFilename))
            .Select(field => field.GetValue(null))
            .OfType<CodeMenuEntryFilename>()
            .Where(entry => !ignored.Contains(entry));

        MenuItem CreateItem(CodeMenuEntryFilename entry)
        {
            string pattern = entry.ToPrefixString();
            MenuItem item = new() { Header = $"{pattern} - {entry.Description}", Focusable = false };
            item.Click += (_, _) => InsertText(textBox, pattern);
            return item;
        }

        List<MenuItem> rootItems = [];
        foreach (IGrouping<string?, CodeMenuEntryFilename> group in entries.GroupBy(entry => entry.Category))
        {
            List<MenuItem> items = group.Select(CreateItem).ToList();
            if (string.IsNullOrWhiteSpace(group.Key))
            {
                rootItems.AddRange(items);
            }
            else
            {
                rootItems.Add(new MenuItem { Header = group.Key, ItemsSource = items, Focusable = false });
            }
        }

        ContextMenu menu = new()
        {
            Focusable = false,
            Placement = PlacementMode.RightEdgeAlignedTop,
            PlacementTarget = textBox,
            ItemsSource = rootItems
        };
        textBox.ContextMenu = menu;

        void OpenMenu()
        {
            if (_activeCodeMenu != null && !ReferenceEquals(_activeCodeMenu, menu))
            {
                _activeCodeMenu.Close();
            }

            if (!menu.IsOpen)
            {
                _activeCodeMenu = menu;
                menu.Open(textBox);
            }
        }

        menu.Closed += (_, _) =>
        {
            if (ReferenceEquals(_activeCodeMenu, menu))
            {
                _activeCodeMenu = null;
            }
        };
        textBox.GotFocus += (_, _) => OpenMenu();
        textBox.PointerReleased += (_, _) => OpenMenu();
    }

    private static void InsertText(TextBox textBox, string textToInsert)
    {
        string text = textBox.Text ?? string.Empty;
        int start = Math.Clamp(Math.Min(textBox.SelectionStart, textBox.SelectionEnd), 0, text.Length);
        textBox.Text = text.Insert(start, textToInsert);
        textBox.CaretIndex = start + textToInsert.Length;
        textBox.Focus();
    }

}

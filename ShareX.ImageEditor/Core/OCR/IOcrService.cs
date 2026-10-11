#region License Information (GPL v3)

/*
    ShareX - A program that allows you to take screenshots and share any file type
    Copyright (c) 2007-2026 ShareX Team

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.
*/

#endregion License Information (GPL v3)

using SkiaSharp;

namespace ShareX.ImageEditor.Core.OCR;

public enum OcrEngineKind
{
    ShareX = 0,
    Xiaomi = 1
}

public enum OcrStage
{
    LoadingModel,
    Recognizing
}

public sealed record OcrProgress(OcrStage Stage, string? Detail = null, double? Fraction = null);

public interface IOcrService
{
    Task<string> RecognizeAsync(SKBitmap image, bool useGpu, IProgress<OcrProgress>? progress = null,
        CancellationToken cancellationToken = default);
}

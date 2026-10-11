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

using System.Runtime.InteropServices;

namespace ShareX.ImageEditor.Core.OCR;

internal static class OcrCudaDriver
{
    internal static bool IsAvailable()
    {
        if (!OperatingSystem.IsWindows() || RuntimeInformation.ProcessArchitecture != Architecture.X64 ||
            !NativeLibrary.TryLoad("nvcuda.dll", typeof(OcrCudaDriver).Assembly, DllImportSearchPath.System32, out nint library))
        {
            return false;
        }
        try
        {
            if (!NativeLibrary.TryGetExport(library, "cuDriverGetVersion", out nint versionAddress) ||
                !NativeLibrary.TryGetExport(library, "cuInit", out nint initializeAddress))
            {
                return false;
            }
            var getVersion = Marshal.GetDelegateForFunctionPointer<GetDriverVersion>(versionAddress);
            var initialize = Marshal.GetDelegateForFunctionPointer<InitializeDriver>(initializeAddress);
            return getVersion(out int version) == 0 && version >= 12040 && initialize(0) == 0;
        }
        finally
        {
            NativeLibrary.Free(library);
        }
    }

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int GetDriverVersion(out int version);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int InitializeDriver(uint flags);
}

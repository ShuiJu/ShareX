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

using System.Diagnostics;
using System.Text;

namespace ShareX.ImageEditor.Core.OCR;

internal sealed class OcrProcess : IAsyncDisposable
{
    private const int LogCapacity = 16384;
    private readonly Process process;
    private readonly StringBuilder log = new();
    private readonly object logLock = new();
    private readonly Task stdout;
    private readonly Task stderr;

    private OcrProcess(Process process)
    {
        this.process = process;
        stdout = DrainAsync(process.StandardOutput);
        stderr = DrainAsync(process.StandardError);
    }

    internal bool HasExited => process.HasExited;
    internal int ExitCode => process.ExitCode;
    internal int Id => process.Id;
    internal string Log
    {
        get
        {
            lock (logLock)
            {
                return log.ToString();
            }
        }
    }

    internal static OcrProcess Start(string executable, IEnumerable<string> arguments)
    {
        var start = new ProcessStartInfo
        {
            FileName = executable,
            WorkingDirectory = Path.GetDirectoryName(executable)!,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        foreach (string argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }
        // Inherited llama.cpp options must not override the editor's device and ownership choices.
        foreach (string key in start.Environment.Keys.Where(x => x.StartsWith("LLAMA_ARG_", StringComparison.Ordinal) ||
            x.StartsWith("MTMD_", StringComparison.Ordinal)).ToArray())
        {
            start.Environment.Remove(key);
        }
        if (OperatingSystem.IsLinux())
        {
            start.Environment.TryGetValue("LD_LIBRARY_PATH", out string? previous);
            start.Environment["LD_LIBRARY_PATH"] = start.WorkingDirectory + (string.IsNullOrEmpty(previous) ? "" : ":" + previous);
        }
        var process = new Process { StartInfo = start };
        try
        {
            if (!process.Start())
            {
                throw new InvalidOperationException("The OCR runtime could not be started.");
            }
            return new OcrProcess(process);
        }
        catch
        {
            try
            {
                if (process.Id != 0 && !process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                    process.WaitForExit();
                }
            }
            catch (InvalidOperationException) { }
            finally
            {
                process.Dispose();
            }
            throw;
        }
    }

    private async Task DrainAsync(StreamReader reader)
    {
        char[] buffer = new char[2048];
        int count;
        while ((count = await reader.ReadAsync(buffer).ConfigureAwait(false)) != 0)
        {
            lock (logLock)
            {
                int overflow = log.Length + count - LogCapacity;
                if (overflow > 0)
                {
                    log.Remove(0, overflow);
                }
                log.Append(buffer, 0, count);
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (!process.HasExited)
            {
                try
                {
                    process.Kill(entireProcessTree: true);
                }
                catch (InvalidOperationException) when (process.HasExited) { }
            }
            // Cleanup deliberately ignores cancellation: callers cannot present a result while native memory survives.
            await process.WaitForExitAsync(CancellationToken.None).ConfigureAwait(false);
            await Task.WhenAll(stdout, stderr).ConfigureAwait(false);
        }
        finally
        {
            process.Dispose();
        }
    }
}

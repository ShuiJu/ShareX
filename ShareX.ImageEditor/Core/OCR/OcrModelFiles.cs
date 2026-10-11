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

using ShareX.ImageEditor.Localization;
using System.Formats.Tar;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace ShareX.ImageEditor.Core.OCR;

internal sealed class OcrModelFiles
{
    internal const string RuntimeVersion = "b11557";
    private const string ModelRevision = "0f0e77db7c97c409bb87347e6c2f151829651947";
    private const string ModelBaseUrl = "https://huggingface.co/prithivMLmods/Xiaomi-OCR-0-GGUF/resolve/" + ModelRevision + "/";
    private static readonly Artifact LanguageModel = new("Xiaomi-OCR-0.Q6_K.gguf", 629744384,
        "6da9896bd738852ec01d463612e463656f43b68e6cfbe09d3b0a3ed6f4ad9fd3");
    private static readonly Artifact Projector = new("Xiaomi-OCR-0.mmproj-bf16.gguf", 207346624,
        "634d6bc95781ebf4a3f06570a59859fa5082d488db540fd474bc6b4061aec90e");
    private static readonly Artifact CudaDependencies = new("cudart-llama-bin-win-cuda-12.4-x64.zip", 391443627,
        "8c79a9b226de4b3cacfd1f83d24f962d0773be79f1e7b75c6af4ded7e32ae1d6");
    private readonly string directory;
    private readonly IProgress<OcrProgress>? progress;
    private readonly CancellationToken cancellationToken;
    private readonly Dictionary<string, VerifiedFile> verified = new(StringComparer.Ordinal);
    private string manifestPath => Path.Combine(directory, "verified-files.json");

    internal OcrModelFiles(string directory, IProgress<OcrProgress>? progress, CancellationToken cancellationToken)
    {
        this.directory = Path.GetFullPath(directory);
        this.progress = progress;
        this.cancellationToken = cancellationToken;
    }

    internal async Task<InstalledFiles> PrepareAsync(bool useGpu)
    {
        Artifact runtime = GetRuntimeArtifact(useGpu);
        Directory.CreateDirectory(directory);
        // A file lock also serializes installation across independent editor processes.
        using FileStream setupLock = await AcquireLockAsync().ConfigureAwait(false);
        await ReadManifestAsync().ConfigureAwait(false);
        using var handler = new SocketsHttpHandler { ConnectTimeout = TimeSpan.FromSeconds(30) };
        using var client = new HttpClient(handler) { Timeout = Timeout.InfiniteTimeSpan };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("ShareX-ImageEditor-OCR/1.0");
        string model = await EnsureArtifactAsync(client, LanguageModel, ModelBaseUrl).ConfigureAwait(false);
        string projector = await EnsureArtifactAsync(client, Projector, ModelBaseUrl).ConfigureAwait(false);
        string archive = await EnsureArtifactAsync(client, runtime,
            $"https://github.com/ggml-org/llama.cpp/releases/download/{RuntimeVersion}/",
            runtime.Name == "llama-b11557-bin-win-vulkan-x64.zip" ? "runtime.zip" : null).ConfigureAwait(false);
        string? dependencies = runtime.Name == "llama-b11557-bin-win-cuda-12.4-x64.zip"
            ? await EnsureArtifactAsync(client, CudaDependencies,
                $"https://github.com/ggml-org/llama.cpp/releases/download/{RuntimeVersion}/").ConfigureAwait(false)
            : null;
        string runtimeDirectory = Path.Combine(directory, "runtime", runtime.Name.Replace(".tar.gz", "").Replace(".zip", ""));
        string server = await EnsureRuntimeAsync(archive, runtimeDirectory, runtime, dependencies).ConfigureAwait(false);
        await WriteManifestAsync().ConfigureAwait(false);
        return new InstalledFiles(model, projector, server);
    }

    private async Task<FileStream> AcquireLockAsync()
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                return new FileStream(Path.Combine(directory, "setup.lock"), FileMode.OpenOrCreate,
                    FileAccess.ReadWrite, FileShare.None, 1, FileOptions.None);
            }
            catch (IOException ex) when ((ex.HResult & 0xffff) is 32 or 33)
            {
                await Task.Delay(150, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private async Task ReadManifestAsync()
    {
        try
        {
            await using var stream = File.OpenRead(manifestPath);
            var manifest = await JsonSerializer.DeserializeAsync<Dictionary<string, VerifiedFile>>(stream,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            if (manifest != null)
            {
                foreach (var entry in manifest)
                {
                    verified[entry.Key] = entry.Value;
                }
            }
        }
        catch (FileNotFoundException) { }
        catch (JsonException) { }
    }

    private async Task WriteManifestAsync()
    {
        string temporary = manifestPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            await using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                4096, FileOptions.Asynchronous))
            {
                await JsonSerializer.SerializeAsync(stream, verified, cancellationToken: cancellationToken).ConfigureAwait(false);
                await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
            cancellationToken.ThrowIfCancellationRequested();
            File.Move(temporary, manifestPath, true);
        }
        finally
        {
            File.Delete(temporary);
        }
    }

    private bool IsVerified(string path, string identity)
    {
        var file = new FileInfo(path);
        return file.Exists && verified.TryGetValue(Path.GetRelativePath(directory, path), out VerifiedFile? stamp) &&
            stamp.Identity == identity && stamp.Length == file.Length && stamp.LastWriteTicks == file.LastWriteTimeUtc.Ticks;
    }

    private void Remember(string path, string identity)
    {
        var file = new FileInfo(path);
        verified[Path.GetRelativePath(directory, path)] = new VerifiedFile(identity, file.Length, file.LastWriteTimeUtc.Ticks);
    }

    private async Task<string> EnsureArtifactAsync(HttpClient client, Artifact artifact, string baseUrl, string? existingName = null)
    {
        string path = Path.Combine(directory, artifact.Name);
        if (!File.Exists(path) && existingName != null && File.Exists(Path.Combine(directory, existingName)))
        {
            path = Path.Combine(directory, existingName);
        }
        if (IsVerified(path, artifact.Sha256))
        {
            return path;
        }
        if (File.Exists(path) && new FileInfo(path).Length == artifact.Length)
        {
            progress?.Report(new OcrProgress(OcrStage.LoadingModel, Strings.OcrWindow_Preparing));
            await using var existing = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
                1024 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);
            byte[] hash = await SHA256.HashDataAsync(existing, cancellationToken).ConfigureAwait(false);
            if (Convert.ToHexString(hash).Equals(artifact.Sha256, StringComparison.OrdinalIgnoreCase))
            {
                Remember(path, artifact.Sha256);
                await WriteManifestAsync().ConfigureAwait(false);
                return path;
            }
        }

        progress?.Report(new OcrProgress(OcrStage.LoadingModel, Strings.OcrWindow_Download, 0));
        string temporary = path + "." + Guid.NewGuid().ToString("N") + ".download";
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromMinutes(30));
        try
        {
            using HttpResponseMessage response = await client.GetAsync(baseUrl + artifact.Name,
                HttpCompletionOption.ResponseHeadersRead, timeout.Token).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            await using Stream source = await response.Content.ReadAsStreamAsync(timeout.Token).ConfigureAwait(false);
            using IncrementalHash hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            await using (var destination = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                1024 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                byte[] buffer = new byte[1024 * 1024];
                long received = 0;
                long lastReported = 0;
                int count;
                while ((count = await source.ReadAsync(buffer, timeout.Token).ConfigureAwait(false)) != 0)
                {
                    received += count;
                    if (received > artifact.Length)
                    {
                        throw new InvalidDataException($"Unexpected size for {artifact.Name}.");
                    }
                    hash.AppendData(buffer, 0, count);
                    await destination.WriteAsync(buffer.AsMemory(0, count), timeout.Token).ConfigureAwait(false);
                    if (received - lastReported >= 4 * 1024 * 1024)
                    {
                        progress?.Report(new OcrProgress(OcrStage.LoadingModel, Strings.OcrWindow_Download,
                            (double)received / artifact.Length));
                        lastReported = received;
                    }
                }
                if (received != artifact.Length ||
                    !Convert.ToHexString(hash.GetHashAndReset()).Equals(artifact.Sha256, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException($"Integrity verification failed for {artifact.Name}.");
                }
                await destination.FlushAsync(timeout.Token).ConfigureAwait(false);
            }
            cancellationToken.ThrowIfCancellationRequested();
            File.Move(temporary, path, true);
            Remember(path, artifact.Sha256);
            await WriteManifestAsync().ConfigureAwait(false);
            return path;
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException($"Downloading {artifact.Name} timed out.", ex);
        }
        finally
        {
            File.Delete(temporary);
        }
    }

    private async Task<string> EnsureRuntimeAsync(string archive, string runtimeDirectory, Artifact runtime, string? dependencies)
    {
        string executableName = OperatingSystem.IsWindows() ? "llama-server.exe" : "llama-server";
        string identity = runtime.Sha256 + (dependencies == null ? "" : "+" + CudaDependencies.Sha256);
        if (Directory.Exists(runtimeDirectory))
        {
            string? existing = Directory.EnumerateFiles(runtimeDirectory, executableName, SearchOption.AllDirectories).FirstOrDefault();
            string prefix = Path.GetRelativePath(directory, runtimeDirectory) + Path.DirectorySeparatorChar;
            var entries = verified.Where(x => x.Key.StartsWith(prefix, StringComparison.Ordinal)).ToArray();
            if (existing != null && entries.Length > 0 && entries.All(x => IsVerified(Path.Combine(directory, x.Key), identity)))
            {
                return existing;
            }
        }

        progress?.Report(new OcrProgress(OcrStage.LoadingModel, Strings.OcrWindow_Preparing));
        string temporary = runtimeDirectory + "." + Guid.NewGuid().ToString("N") + ".extract";
        Directory.CreateDirectory(temporary);
        try
        {
            await ExtractRuntimeAsync(archive, temporary).ConfigureAwait(false);
            if (dependencies != null)
            {
                await ExtractRuntimeAsync(dependencies, temporary).ConfigureAwait(false);
            }
            string? server = Directory.EnumerateFiles(temporary, executableName, SearchOption.AllDirectories).FirstOrDefault();
            if (server == null)
            {
                throw new InvalidDataException("The pinned OCR runtime does not contain llama-server.");
            }
            if (!OperatingSystem.IsWindows())
            {
                File.SetUnixFileMode(server, File.GetUnixFileMode(server) | UnixFileMode.UserExecute);
            }
            cancellationToken.ThrowIfCancellationRequested();
            if (Directory.Exists(runtimeDirectory))
            {
                Directory.Delete(runtimeDirectory, true);
            }
            Directory.CreateDirectory(Path.GetDirectoryName(runtimeDirectory)!);
            Directory.Move(temporary, runtimeDirectory);
            foreach (string path in Directory.EnumerateFiles(runtimeDirectory, "*", SearchOption.AllDirectories))
            {
                Remember(path, identity);
            }
            await WriteManifestAsync().ConfigureAwait(false);
            return Path.Combine(runtimeDirectory, Path.GetRelativePath(temporary, server));
        }
        finally
        {
            if (Directory.Exists(temporary))
            {
                Directory.Delete(temporary, true);
            }
        }
    }

    private async Task ExtractRuntimeAsync(string archive, string destinationDirectory)
    {
        if (archive.EndsWith(".zip", StringComparison.Ordinal))
        {
            using ZipArchive zip = ZipFile.OpenRead(archive);
            foreach (ZipArchiveEntry entry in zip.Entries)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string target = Path.GetFullPath(Path.Combine(destinationDirectory, entry.FullName));
                if (!target.StartsWith(destinationDirectory + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException("Runtime archive contains an invalid path.");
                }
                if (string.IsNullOrEmpty(entry.Name))
                {
                    Directory.CreateDirectory(target);
                    continue;
                }
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                await using Stream source = entry.Open();
                await using var destination = new FileStream(target, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                    81920, FileOptions.Asynchronous);
                await source.CopyToAsync(destination, cancellationToken).ConfigureAwait(false);
            }
        }
        else
        {
            await using var file = File.OpenRead(archive);
            await using var gzip = new GZipStream(file, CompressionMode.Decompress);
            await TarFile.ExtractToDirectoryAsync(gzip, destinationDirectory, false, cancellationToken).ConfigureAwait(false);
        }
    }

    private static Artifact GetRuntimeArtifact(bool useGpu)
    {
        Architecture architecture = RuntimeInformation.ProcessArchitecture;
        if (OperatingSystem.IsWindows())
        {
            if (!useGpu)
            {
                return architecture switch
                {
                    Architecture.X64 => new("llama-b11557-bin-win-cpu-x64.zip", 19501541,
                        "bcfca9c8198899d4268e4e6b586113905280dec372a82c7106ebe78bc0eacb5b"),
                    Architecture.Arm64 => new("llama-b11557-bin-win-cpu-arm64.zip", 12371561,
                        "1cf166deac86daf034d740768837a0ccaf3a772273a4e1eb0ff5aebe12e99c43"),
                    _ => throw new PlatformNotSupportedException("OCR requires a 64-bit Windows runtime.")
                };
            }
            if (architecture == Architecture.X64 && OcrCudaDriver.IsAvailable())
            {
                // The larger precompiled CUDA distribution avoids repeated Vulkan kernel initialization.
                return new Artifact("llama-b11557-bin-win-cuda-12.4-x64.zip", 265207448,
                    "1108e4e77a62009365caee1208c33c1d76859b1e45c02e21f63b54090388cc29");
            }
            return architecture switch
            {
                Architecture.X64 => new("llama-b11557-bin-win-vulkan-x64.zip", 32084441,
                    "6db23c5cf69e51f3dc2bf7f68277f4a791217bab853bd5b93afac1898d450b83"),
                Architecture.Arm64 => new("llama-b11557-bin-win-vulkan-arm64.zip", 24684191,
                    "3bb6e931ed228da47d4972607e622207d6884ec9ef2b4227a36799e15a573cd3"),
                _ => throw new PlatformNotSupportedException("OCR requires a 64-bit Windows runtime.")
            };
        }
        if (OperatingSystem.IsLinux())
        {
            if (!useGpu)
            {
                return architecture switch
                {
                    Architecture.X64 => new("llama-b11557-bin-ubuntu-x64.tar.gz", 17717755,
                        "421fef77e12d5a97202ed838d5f6a509c76737dcd8fa7a5afed7688d05a699c0"),
                    Architecture.Arm64 => new("llama-b11557-bin-ubuntu-arm64.tar.gz", 13834442,
                        "8b1bb544b523a9774807f83bf8ba580d375d71600644cd03a8e44a733f6865bd"),
                    _ => throw new PlatformNotSupportedException("OCR requires a 64-bit Linux runtime.")
                };
            }
            return architecture switch
            {
                Architecture.X64 => new("llama-b11557-bin-ubuntu-vulkan-x64.tar.gz", 30279277,
                    "c05ccb11f6de7c907c9ca861a0bb921bc3a52a94912cd0f1d47d53cea0c3dc08"),
                Architecture.Arm64 => new("llama-b11557-bin-ubuntu-vulkan-arm64.tar.gz", 24056295,
                    "7c9a2e6de70f005c597de301a53874b2413b3f0b7a25183f6737130248d6786b"),
                _ => throw new PlatformNotSupportedException("OCR requires a 64-bit Linux runtime.")
            };
        }
        if (OperatingSystem.IsMacOS())
        {
            return architecture switch
            {
                Architecture.X64 => new("llama-b11557-bin-macos-x64.tar.gz", 11597244,
                    "c6cb1d85c6528bf02202d59ff67b4aa16feec8f442c41dce91dbdbe6a3d6ec93"),
                Architecture.Arm64 => new("llama-b11557-bin-macos-arm64.tar.gz", 12105818,
                    "83cd5827293f7fa33403627436da2606413e48978bcedfe7aa3e587f62742f5c"),
                _ => throw new PlatformNotSupportedException("OCR requires a 64-bit macOS runtime.")
            };
        }
        throw new PlatformNotSupportedException("OCR is supported on Windows, Linux and macOS.");
    }

    private sealed record Artifact(string Name, long Length, string Sha256);
    private sealed record VerifiedFile(string Identity, long Length, long LastWriteTicks);
    internal sealed record InstalledFiles(string Model, string Projector, string Server);
}

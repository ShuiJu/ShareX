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

using ShareX.ImageEditor.Integration;
using SkiaSharp;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text.Json;

namespace ShareX.ImageEditor.Core.OCR;

/// <summary>
/// Runs the pinned Xiaomi OCR model in an owned, short-lived llama.cpp process.
/// Downloaded files remain on disk; native model memory is released before any call completes.
/// </summary>
public sealed class XiaomiOcrService : IOcrService
{
    private readonly string? modelDirectory;

    public XiaomiOcrService(string? modelDirectory = null)
    {
        this.modelDirectory = modelDirectory;
    }

    public Task<string> RecognizeAsync(SKBitmap image, bool useGpu, IProgress<OcrProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(image);
        // File verification, native startup and Skia encoding must never run on the editor's UI thread.
        return Task.Run(() => RecognizeCoreAsync(image, useGpu, progress, cancellationToken), cancellationToken);
    }

    private async Task<string> RecognizeCoreAsync(SKBitmap image, bool useGpu, IProgress<OcrProgress>? progress,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        progress?.Report(new OcrProgress(OcrStage.LoadingModel));
        string imageUrl = EncodeImage(image, cancellationToken);
        var files = new OcrModelFiles(modelDirectory ?? EditorServices.OcrModelDirectory, progress, cancellationToken);
        OcrModelFiles.InstalledFiles installed = await files.PrepareAsync(useGpu).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        progress?.Report(new OcrProgress(OcrStage.LoadingModel));

        int port = GetLoopbackPort();
        string apiKey = Guid.NewGuid().ToString("N");
        var arguments = new List<string>
        {
            "--model", installed.Model,
            "--mmproj", installed.Projector,
            "--host", "127.0.0.1",
            "--port", port.ToString(System.Globalization.CultureInfo.InvariantCulture),
            "--api-key", apiKey,
            "--ctx-size", "8192",
            "--parallel", "1",
            "--image-min-tokens", "64",
            "--image-max-tokens", "16384",
            "--reasoning", "off",
            "--chat-template-kwargs", "{\"enable_thinking\":false}",
            "--no-context-shift",
            "--no-warmup",
            "--fit", "off",
            "--gpu-layers", useGpu ? "all" : "0"
        };
        if (!useGpu)
        {
            arguments.AddRange(["--device", "none", "--no-mmproj-offload", "--no-op-offload"]);
        }

        await using OcrProcess native = OcrProcess.Start(installed.Server, arguments);
        EditorServices.ReportInformation(nameof(XiaomiOcrService), $"Started owned OCR process {native.Id}.");
        using var handler = new SocketsHttpHandler
        {
            UseProxy = false,
            ConnectTimeout = TimeSpan.FromSeconds(2)
        };
        using var client = new HttpClient(handler)
        {
            BaseAddress = new Uri($"http://127.0.0.1:{port}/"),
            Timeout = Timeout.InfiniteTimeSpan
        };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        try
        {
            await WaitForHealthAsync(client, native, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(new OcrProgress(OcrStage.Recognizing));
            using var requestTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            requestTimeout.CancelAfter(TimeSpan.FromMinutes(10));
            try
            {
                var request = new
                {
                    messages = new[]
                    {
                        new
                        {
                            role = "user",
                            content = new object[]
                            {
                                new { type = "image_url", image_url = new { url = imageUrl } },
                                new { type = "text", text = "Extract the text in the image." }
                            }
                        }
                    },
                    temperature = 0,
                    repeat_penalty = 1,
                    presence_penalty = 0,
                    max_tokens = 8192,
                    stream = false
                };
                using HttpResponseMessage response = await client.PostAsJsonAsync("v1/chat/completions", request,
                    requestTimeout.Token).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    string error = await ReadBoundedErrorAsync(response, requestTimeout.Token).ConfigureAwait(false);
                    throw new InvalidOperationException($"OCR request failed ({(int)response.StatusCode}): {error}");
                }
                await using Stream content = await response.Content.ReadAsStreamAsync(requestTimeout.Token).ConfigureAwait(false);
                using JsonDocument document = await JsonDocument.ParseAsync(content,
                    cancellationToken: requestTimeout.Token).ConfigureAwait(false);
                JsonElement choice = document.RootElement.GetProperty("choices")[0];
                string? finishReason = choice.GetProperty("finish_reason").GetString();
                if (finishReason == "length")
                {
                    throw new InvalidOperationException("The image contains more text than the OCR context can return. Recognize smaller image sections.");
                }
                if (finishReason != "stop")
                {
                    throw new InvalidOperationException($"OCR did not complete normally ({finishReason ?? "missing finish reason"}).");
                }
                string? result = choice.GetProperty("message").GetProperty("content").GetString();
                cancellationToken.ThrowIfCancellationRequested();
                return result?.Trim() ?? throw new InvalidDataException("The OCR runtime returned no text response.");
            }
            catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException("Text recognition timed out.", ex);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            string log = native.Log;
            EditorServices.ReportError(nameof(XiaomiOcrService), log, ex);
            throw new InvalidOperationException($"{ex.Message}{Environment.NewLine}{log}", ex);
        }
        // The await-using scope kills the entire owned process tree and awaits its exit before returning or throwing.
    }

    private static string EncodeImage(SKBitmap image, CancellationToken cancellationToken)
    {
        if (image.Width < 1 || image.Height < 1)
        {
            throw new ArgumentException("The image is empty.", nameof(image));
        }
        cancellationToken.ThrowIfCancellationRequested();
        double scale = Math.Min(1, 2048d / Math.Max(image.Width, image.Height));
        int width = Math.Max(1, (int)Math.Round(image.Width * scale));
        int height = Math.Max(1, (int)Math.Round(image.Height * scale));
        using var flattened = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Opaque);
        using SKImage sourceImage = SKImage.FromBitmap(image);
        using (var canvas = new SKCanvas(flattened))
        using (var paint = new SKPaint { IsAntialias = true })
        {
            canvas.Clear(SKColors.White);
            canvas.DrawImage(sourceImage, new SKRect(0, 0, width, height), new SKSamplingOptions(SKCubicResampler.Mitchell), paint);
        }
        cancellationToken.ThrowIfCancellationRequested();
        using SKImage encodedImage = SKImage.FromBitmap(flattened);
        using SKData data = encodedImage.Encode(SKEncodedImageFormat.Png, 100) ??
            throw new InvalidOperationException("The image could not be encoded for OCR.");
        cancellationToken.ThrowIfCancellationRequested();
        return "data:image/png;base64," + Convert.ToBase64String(data.AsSpan());
    }

    private static int GetLoopbackPort()
    {
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        return ((IPEndPoint)socket.LocalEndPoint!).Port;
    }

    private static async Task WaitForHealthAsync(HttpClient client, OcrProcess native, CancellationToken cancellationToken)
    {
        using var startup = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        startup.CancelAfter(TimeSpan.FromMinutes(3));
        try
        {
            while (true)
            {
                startup.Token.ThrowIfCancellationRequested();
                if (native.HasExited)
                {
                    throw new InvalidOperationException($"The OCR runtime exited while loading (exit code {native.ExitCode}).");
                }
                using var probe = CancellationTokenSource.CreateLinkedTokenSource(startup.Token);
                probe.CancelAfter(TimeSpan.FromSeconds(2));
                try
                {
                    using HttpResponseMessage response = await client.GetAsync("health", probe.Token).ConfigureAwait(false);
                    if (response.IsSuccessStatusCode)
                    {
                        if (native.HasExited)
                        {
                            throw new InvalidOperationException("The OCR runtime exited while loading.");
                        }
                        return;
                    }
                    if (response.StatusCode != HttpStatusCode.ServiceUnavailable)
                    {
                        throw new InvalidOperationException($"The OCR health check failed ({(int)response.StatusCode}).");
                    }
                }
                catch (HttpRequestException) { }
                catch (OperationCanceledException) when (!startup.IsCancellationRequested) { }
                await Task.Delay(100, startup.Token).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException("Loading the OCR model timed out.", ex);
        }
    }

    private static async Task<string> ReadBoundedErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var reader = new StreamReader(stream);
        char[] buffer = new char[4096];
        int count = await reader.ReadAsync(buffer.AsMemory(), cancellationToken).ConfigureAwait(false);
        return new string(buffer, 0, count);
    }
}

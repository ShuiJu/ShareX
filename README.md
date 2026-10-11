<p align="center"><a href="https://getsharex.com"><img src="https://getsharex.com/img/ShareX_Banner.png" alt="ShareX Banner"/></a></p>
<h3 align="center">Screen capture, image editing and productivity tool</h3>
<br>
<div align="center">
  <a href="https://github.com/ShareX/ShareX/actions/workflows/build.yml"><img src="https://img.shields.io/github/actions/workflow/status/ShareX/ShareX/build.yml?branch=develop&label=Build&cacheSeconds=3600" alt="GitHub Workflow Status"/></a>
  <a href="./LICENSE.txt"><img src="https://img.shields.io/github/license/ShareX/ShareX?label=License&color=brightgreen&cacheSeconds=3600" alt="License"/></a>
  <a href="https://github.com/ShareX/ShareX/releases/latest"><img src="https://img.shields.io/github/v/release/ShareX/ShareX?label=Release&color=brightgreen&cacheSeconds=3600" alt="Release"/></a>
  <a href="https://getsharex.com/downloads"><img src="https://img.shields.io/github/downloads/ShareX/ShareX/total?label=Downloads&cacheSeconds=3600" alt="Downloads"/></a>
  <a href="https://discord.gg/ShareX"><img src="https://img.shields.io/discord/194170124859736065?label=Discord&cacheSeconds=3600" alt="Discord"/></a>
  <a href="https://twitter.com/intent/follow?screen_name=ShareX"><img src="https://img.shields.io/twitter/follow/ShareX?cacheSeconds=3600" alt="Twitter"/></a>
</div>
<br>
<p align="center"><a href="https://getsharex.com"><img src="https://getsharex.com/img/ShareX_Screenshot.png" alt="ShareX Screenshot"/></a></p>
<p align="center">For further information please check our <a href="https://getsharex.com">website</a></p>

# ShareX - Free Screen Capture, Image Editing and Productivity Tool

ShareX is a free and open source screenshot tool, screen recorder and productivity application for Windows. This build focuses on local capture, screenshot editing, OCR, file saving, clipboard operations and desktop utilities. Upload destinations, upload tasks, custom uploaders and browser upload integration have been removed.

With ShareX, you can capture any area of your screen, record video or GIFs, annotate screenshots, extract text with OCR, scan QR codes, pick colors and run custom workflows from hotkeys. Capture a screenshot, edit it, save it, copy it or pass it through your own local task chain.

## Why ShareX?

ShareX combines screen capture, screen recording, image editing and local automation. It is completely free, open source and has no advertisements. No account is required to use ShareX.

ShareX is useful for developers, designers, support teams, content creators, technical writers, QA testers and power users who create screenshots, record short clips or document workflows. It supports configurable capture methods, after-capture tasks and hotkeys.

## Screenshot and Screen Recording Features

ShareX supports many ways to capture your screen:

* Fullscreen capture
* Active window capture
* Active monitor capture
* Region capture
* Scrolling screenshot capture
* Last region capture
* Custom region capture
* Screen recording
* GIF screen recording
* Auto capture

After capturing a screenshot or recording, ShareX can automatically copy the result to the clipboard, save it to a file, open it in the image editor, print it, show it in Windows Explorer, run an action, scan a QR code or recognize text with OCR.

## Region Capture and Annotation

ShareX region capture includes tools for selecting exactly what you want to capture and marking it before saving or copying. You can draw rectangles, ellipses, freehand lines, arrows, text, speech balloons, step numbers, highlights, blur effects, pixelation, magnification and spotlight effects.

These annotation tools help create clear screenshots for bug reports, documentation, tutorials, support replies, pull requests and release notes. Sensitive information can be hidden with blur, pixelate or smart eraser tools before a screenshot is shared.

## Built-in Image Editor

The ShareX image editor lets you crop, annotate, redact, highlight and prepare screenshots after capture. It includes common editing tools such as shapes, arrows, text, freehand drawing, image insertion, cursor insertion, blur, pixelate, magnify, spotlight, crop, cut out, background editing and image effects.

Because the editor is part of the capture workflow, you can take a screenshot, mark the important area, hide private details and then copy or save the edited image without switching between separate apps.

The editor's **Text recognition** toolbar button uses the existing **ShareX OCR** (Windows OCR) by default. Select **Xiaomi OCR** under **Task settings → Tools → Image editor → OCR engine** or in the editor's settings sidebar to use [Xiaomi-OCR-0](https://huggingface.co/SeerRay-Lab/Xiaomi-OCR-0) locally with the [Q6_K GGUF model and BF16 vision projector](https://huggingface.co/prithivMLmods/Xiaomi-OCR-0-GGUF). Both engines use the same selectable result window with **Save to file**, **Copy to clipboard** and **Close** buttons. Selected text supports the usual copy shortcut. The ShareX host retains its existing OCR language, scale and single-line options; Windows OCR requires an installed recognition language. On other platforms, choose Xiaomi OCR.

For Xiaomi OCR, the window shows model loading and recognition progress. Each operation exits its inference process before displaying the result, releasing the model's RAM and GPU memory; closing during recognition cancels the operation and waits for that cleanup. Graphics acceleration is enabled by default and its setting is available when Xiaomi OCR is selected. Disable **Use graphics acceleration for OCR** to use the CPU. First use downloads approximately 837 MB of model files plus the matching llama.cpp runtime. Downloads are verified and prepared files are kept on disk, so later operations avoid downloading, unpacking and hashing the entire model again. Compatible NVIDIA GPUs on Windows x64 use the larger precompiled CUDA 12.4 runtime to reduce repeated startup overhead; its two downloads total approximately 657 MB. Other supported GPU configurations use Vulkan or Metal. Recognition requires no network connection once the files are prepared.

ShareX stores these files under `Models/XiaomiOCR` in its personal folder. The standalone editor uses `%LOCALAPPDATA%/ShareX/ImageEditor/Models/XiaomiOCR` on Windows and persists its editor settings in `%LOCALAPPDATA%/ShareX/ImageEditor/settings.json`.

## Productivity Tools

ShareX includes many utilities that support everyday desktop work:

### Screen & color

* [Color picker](https://getsharex.com/docs/color-picker)
* [Mouse highlighter](https://getsharex.com/docs/mouse-highlighter)
* [Pin to screen](https://getsharex.com/docs/pin-to-screen)
* [Ruler](https://getsharex.com/docs/ruler)
* [Screen color picker](https://getsharex.com/docs/screen-color-picker)

### Images

* [Background remover](https://getsharex.com/docs/background-remover)
* [Icon converter](https://getsharex.com/docs/icon-converter)
* [Image beautifier](https://getsharex.com/docs/image-beautifier)
* [Image combiner](https://getsharex.com/docs/image-combiner)
* [Image comparer](https://getsharex.com/docs/image-comparer)
* [Image converter](https://getsharex.com/docs/image-converter)
* [Image editor](https://getsharex.com/docs/image-editor)
* [Image effects](https://getsharex.com/docs/image-effects)
* [Image resizer](https://getsharex.com/docs/image-resizer)
* [Image splitter](https://getsharex.com/docs/image-splitter)
* [Image thumbnailer](https://getsharex.com/docs/image-thumbnailer)
* [Image viewer](https://getsharex.com/docs/image-viewer)
* [Image watermark](https://getsharex.com/docs/image-watermark)

### Video

* [Animated GIF maker](https://getsharex.com/docs/animated-gif-maker)
* [Animated GIF trimmer](https://getsharex.com/docs/animated-gif-trimmer)
* [Video converter](https://getsharex.com/docs/video-converter)
* [Video thumbnailer](https://getsharex.com/docs/video-thumbnailer)
* [Video trimmer](https://getsharex.com/docs/video-trimmer)

### Data & text

* [Hash checker](https://getsharex.com/docs/hash-checker)
* [Index folder](https://getsharex.com/docs/index-folder)
* [Metadata](https://getsharex.com/docs/metadata)
* [OCR](https://getsharex.com/docs/ocr)
* [QR code](https://getsharex.com/docs/qr-code)

### System

* [Borderless window](https://getsharex.com/docs/borderless-window)
* [Clipboard viewer](https://getsharex.com/docs/clipboard-viewer)
* [Inspect window](https://getsharex.com/docs/inspect-window)
* [Monitor test](https://getsharex.com/docs/monitor-test)
* [Network monitor](https://getsharex.com/docs/network-monitor)

These tools make ShareX useful beyond screenshots. It can help inspect images, prepare assets, extract information, verify files and speed up repetitive tasks.

## Custom Workflows and Hotkeys

ShareX is built around configurable workflows. Assign hotkeys to capture methods, choose what happens after capture and create actions that run external tools or scripts. A workflow can capture a region, annotate the image, save it locally and copy the image or file path to the clipboard. Older configurations retain their local capture tasks and filename patterns when obsolete upload settings are discarded.

Simple tasks can stay simple, while advanced users can automate detailed screenshot and screen recording workflows.

## Download ShareX

ShareX is available from the official website, GitHub releases, Microsoft Store and Steam. You can install the regular setup version, use a portable version or try development builds if you want the newest changes before a stable release.

For the safest download options, use the official links below.

## Links
* Official website: https://getsharex.com
* Downloads: https://getsharex.com/downloads
* GitHub: https://github.com/ShareX/ShareX
* Changelog: https://getsharex.com/changelog
* Screenshots: https://getsharex.com/screenshots
* Privacy policy: https://getsharex.com/privacy-policy
* Donate: https://getsharex.com/donate
* X: https://x.com/ShareX
* Discord: https://discord.gg/ShareX
* Reddit: https://www.reddit.com/r/sharex
* Steam page: https://store.steampowered.com/app/400040/ShareX/
* Microsoft Store page: https://apps.microsoft.com/detail/9nblggh4z1sp
* ShareX related projects on GitHub: https://github.com/topics/sharex

## Documents
* Image effects: https://getsharex.com/image-effects
* Actions: https://getsharex.com/actions
* Dev builds: https://getsharex.com/docs/dev-builds
* Keybinds: https://getsharex.com/docs/keybinds
* Region capture: https://getsharex.com/docs/region-capture
* Image editor: https://getsharex.com/docs/image-editor
* Background remover: https://getsharex.com/docs/background-remover
* Pin to screen: https://getsharex.com/docs/pin-to-screen
* Scrolling screenshot: https://getsharex.com/docs/scrolling-screenshot
* Command line arguments: https://getsharex.com/docs/command-line-arguments
* Translation: https://getsharex.com/docs/translation
* OCR: https://getsharex.com/docs/ocr
* Brand assets: https://getsharex.com/brand-assets

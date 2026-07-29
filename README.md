<div align="center">

<img src="PhotoOrganizer.App/Assets/logo.png" width="96" alt="PhotoOrganizer logo">

# PhotoOrganizer

**Organize your photos and videos by the date they were taken — safely.**

Sorts your media into clean `YYYY/MM/DD` folders. Runs on **Windows, macOS and Linux**.
**100% offline** — your files never leave your computer. Free & open source.

[![License](https://img.shields.io/github/license/kloss11/photo-organizer)](LICENSE)
[![Latest release](https://img.shields.io/github/v/release/kloss11/photo-organizer)](https://github.com/kloss11/photo-organizer/releases/latest)
[![Build](https://img.shields.io/github/actions/workflow/status/kloss11/photo-organizer/ci.yml?label=build)](https://github.com/kloss11/photo-organizer/actions)
![Platforms](https://img.shields.io/badge/platform-Windows%20%7C%20macOS%20%7C%20Linux-blue)

🌐 **[Website &amp; downloads →](https://kloss11.github.io/photo-organizer/)**

**English** · [Polski](README.pl.md)

</div>

<!-- TODO: add a demo GIF here (e.g. docs/demo.gif) showing Preview → Apply → Undo -->

---

## Why PhotoOrganizer?

Thousands of photos and videos scattered across folders, camera dumps and phone backups — with dates buried in metadata. PhotoOrganizer reads the **real capture date** of each file and moves it into a tidy, predictable folder structure.

- 🛡️ **Safe by design** — always **preview** the full plan first (nothing is moved), then **apply**, and **undo** the last run at any time.
- 🔒 **Private** — everything runs locally. No cloud, no upload, no account.
- 🖥️ **Cross-platform** — one app for Windows, macOS and Linux.
- 🌍 **6 languages** — Polski, English, Deutsch, Русский, Español, Français.
- 🎯 **Smart date detection** — EXIF, video metadata, file dates, and even dates embedded in file names.

---

## Download

Grab the latest ready-to-run version — **no installation, no .NET required** (self-contained single file):

| Platform | Download |
|---|---|
| 🪟 **Windows** (x64) | [PhotoOrganizer-windows-x64.zip](https://github.com/kloss11/photo-organizer/releases/latest/download/PhotoOrganizer-windows-x64.zip) |
| 🐧 **Linux** (x64) | [PhotoOrganizer-linux-x64.zip](https://github.com/kloss11/photo-organizer/releases/latest/download/PhotoOrganizer-linux-x64.zip) |
| 🍎 **macOS** (Apple Silicon) | [PhotoOrganizer-macos-arm64.zip](https://github.com/kloss11/photo-organizer/releases/latest/download/PhotoOrganizer-macos-arm64.zip) |
| 🍎 **macOS** (Intel) | [PhotoOrganizer-macos-x64.zip](https://github.com/kloss11/photo-organizer/releases/latest/download/PhotoOrganizer-macos-x64.zip) |

All releases: **[github.com/kloss11/photo-organizer/releases](https://github.com/kloss11/photo-organizer/releases)**

**First run:**
- **Windows** — unzip and double-click `PhotoOrganizer.App.exe`. The app is not code-signed yet, so Windows SmartScreen may warn about an "unknown publisher" — click **More info → Run anyway**.
- **macOS** — unzip, then right-click the app → **Open** the first time to bypass Gatekeeper (unsigned build).
- **Linux** — unzip, then make it executable: `chmod +x PhotoOrganizer.App` and run it.

---

## How to use

1. **Launch the app.** Pick your interface **language** in the top-right corner.
2. **Choose a working folder** in one of three ways:
   - the **"Choose folder…"** button,
   - **drag & drop** a folder onto the window,
   - a **gesture** (Windows): **hold `Esc` and left-click** inside a File Explorer window to grab the folder currently open there. *(macOS: same, after granting permissions; Linux: no gesture.)*
3. **Set your options:**

   | Option | Values | Default | Effect |
   |---|---|---|---|
   | **Granularity** | Year / Year+month / Year+month+day | Year+month | Folder depth: `2024`, `2024/03`, `2024/03/15` |
   | **Name collisions** | Skip / Overwrite | Skip | What to do when a file of the same name already exists at the target |
   | **Scan scope** | Recursive / Top level only | Recursive | Whether to descend into subfolders |
   | **Undated files** | Move to "Undated" / Skip | Move to "Undated" | What happens to files with no determinable date |
   | **Zero-pad (03)** | on / off | on | `03` instead of `3` for month/day (year is always 4 digits) |

4. **Click "Preview (dry-run)".** Nothing is moved — you get a plan table with columns **File · Date · Source · Action · Target folder**. The *Source* column shows where the date came from. A summary counts: *To move, Overwrites, Already in place, Collisions, Undated, Online-only.*
5. **Review the plan**, then click **"Apply"** to actually move the files.
6. **"Undo last operation"** restores files to their previous locations (including recovering overwritten files).

### Safety behaviors

- **Preview (dry-run)** changes nothing — you always see the plan before anything happens.
- **Undo:** the operation log is stored in a `.photoorganizer` folder inside the working area; that folder is **never scanned** or moved.
- **Collision → Skip:** the source file is left untouched. **Collision → Overwrite:** undo restores the previous content of the target file.
- **Byte-identical duplicates** are skipped (no meaningful move).
- **Online-only files** (OneDrive/cloud placeholders) are skipped by default — the app won't force them to download.
- **Symbolic links** (files and folders) are skipped — protects against loops and escaping the working area.
- **Sidecar files** are kept together with their main file and inherit its date.
- Folders without permission, or that vanish mid-scan, are skipped rather than aborting the whole run.

---

## Supported formats

**Photos:** `jpg`, `jpeg`, `png`, `tif`, `tiff`, `heic`, `heif`, `cr2`, `nef`, `arw`, `dng`

**Videos:** `mp4`, `mov`, `m4v`, `avi`, `mts`, `m2ts`, `3gp`

**Sidecar files** — grouped with the main file: `xmp`, `aae`, `thm`

### How the capture date is determined

The app tries these sources in order (first hit wins):

1. **EXIF – DateTimeOriginal** (original photo date),
2. **EXIF – DateTimeDigitized** (digitized date),
3. **QuickTime "Created"** (for mp4/mov video),
4. **file last-write date**,
5. **file creation date**,
6. **date from the file name** — patterns like `IMG_20230415_123456`, `VID-20230415-WA0012`, `2023-04-15 holiday`, `Screenshot_2023-04-15` (year-month-day order only),
7. if nothing works → the file is treated as **"undated"** (per the *Undated files* option).

**Plausibility window:** only dates between **1950-01-01 and today (+1 day margin)** are accepted. Out-of-range dates (e.g. the QuickTime **1904-01-01** epoch from a zeroed video "creation time", the FILETIME 1601 epoch, or future dates) are rejected and the chain falls through to the next source — so you never get folders like `1904/01`.

> Metadata reading never aborts the run — corrupt or unusual metadata simply degrades to the next step in the chain.

---

## Build from source

Requires the **.NET 10 SDK**. You can cross-build every platform from any OS:

```bash
for rid in win-x64 linux-x64 osx-x64 osx-arm64; do
  dotnet publish PhotoOrganizer.App/PhotoOrganizer.App.csproj -c Release -r $rid \
    --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true \
    -o "bin/publish/$rid"
done
```

Platform-specific packaging (Windows `.exe`, macOS `.app` bundle, Linux AppImage) lives in [`packaging/`](packaging/README.md). Run the tests with `dotnet test`.

The project follows a Clean Architecture layout (Domain / Application / Infrastructure / Presentation + per-platform adapters) and is fully covered by unit, integration, architecture and UI tests.

---

## Support

PhotoOrganizer is **free and open source (MIT)**. If it saved you time and you'd like to support further development:

<!-- TODO: replace with your Buy Me a Coffee link once the account is ready -->
☕ **Buy Me a Coffee** — _link coming soon_

You can also help by ⭐ starring the repo and reporting issues.

---

## License

Released under the [MIT License](LICENSE) — © 2026 Marcin Nadolny.

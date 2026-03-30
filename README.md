# ⭐ Windows On Steroids

**Windows 11 optimizer** — 30+ safe, one-click registry tweaks for privacy, performance, gaming, Copilot, and daily usability.

Built as a clean WPF app (v1.0) — WinUI 3 version coming soon.

## Features
- Runs as Administrator (required for HKLM tweaks)
- Dark/Light mode auto-detection
- Toggle-based UI with live registry sync
- Bulk Apply / Restore Defaults
- State saved to `%AppData%`

## Screenshots
(Add 2–3 screenshots here later)

## Download
→ [Latest Release](https://github.com/YOURUSERNAME/WindowsOnSteroids/releases)

## Build from source
```bash
dotnet build -c Release
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true

# TorchKeeper Release Guide

## Prerequisites

### Apple Developer Account
- Sign up at https://developer.apple.com ($99/year)
- Covers Mac, iPhone, and iPad distribution
- Required for: notarized Mac builds, App Store submission, TestFlight beta testing

---

## Mac (Mac Catalyst)

### Build
**Must be run in Terminal.app** (not Claude Code — `actool` needs temp directory access that Claude Code's sandbox blocks).

```bash
cd ~/projects/torchkeeper
dotnet publish TorchKeeper/TorchKeeper.csproj \
  -f net10.0-maccatalyst \
  -c Release \
  -p:CreatePackage=false
```

Output: `TorchKeeper/bin/Release/net10.0-maccatalyst/TorchKeeper.app` (~98MB universal binary, arm64 + x64)

> There are also architecture-specific builds at `maccatalyst-arm64/` and `maccatalyst-x64/` — use the root one for distribution as it runs on both Intel and Apple Silicon Macs.

#### Known issues
- **Xcode version mismatch**: `ValidateXcodeVersion` is set to `false` in the csproj to bypass the check when the installed .NET MacCatalyst SDK lags behind the installed Xcode version. This is safe as long as the SDK/Xcode versions are close.
- **SDK updates**: The .NET SDK at `/usr/local/share/dotnet` is installed by Microsoft's own installer (not Homebrew). To update it, download the latest `.pkg` from https://dotnet.microsoft.com/download/dotnet/10.0, then run `sudo dotnet workload update` in Terminal.

### Package for GitHub release
```bash
cd ~/projects/torchkeeper/TorchKeeper/bin/Release/net10.0-maccatalyst/
zip -r TorchKeeper.zip TorchKeeper.app
```

### Sign and notarize (requires Apple Developer account)
```bash
# Sign
codesign --deep --force --verify --verbose \
  --sign "Developer ID Application: Your Name (TEAMID)" \
  TorchKeeper.app

# Notarize
xcrun notarytool submit TorchKeeper.zip \
  --apple-id your@email.com \
  --team-id TEAMID \
  --password APP_SPECIFIC_PASSWORD \
  --wait

# Staple ticket to app
xcrun stapler staple TorchKeeper.app

# Re-zip after stapling
zip -r TorchKeeper.zip TorchKeeper.app
```

### Without Developer account (workaround for users)
Users will see a Gatekeeper warning. They can bypass it by right-clicking → Open, or running:
```bash
xattr -cr TorchKeeper.app
```

---

## iOS (iPhone/iPad)

### Build
```bash
dotnet publish TorchKeeper/TorchKeeper.csproj \
  -f net10.0-ios \
  -c Release
```

### Distribution
- Submit via App Store Connect (https://appstoreconnect.apple.com)
- Beta testing via TestFlight
- Review process: ~1-3 days
- Need: app metadata, screenshots, description, privacy policy

---

## TODO
- [x] Sign up for Apple Developer account
- [ ] Obtain Developer ID certificate (for Mac)
- [ ] Set up code signing in project / CI
- [ ] Create GitHub Actions workflow to auto-build on `v*` tags
- [ ] Test iOS build on real device
- [ ] Prepare App Store metadata (screenshots, description, etc.)

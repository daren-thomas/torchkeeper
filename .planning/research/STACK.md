# Technology Stack

**Project:** Shadowdark Character Sheet (.NET MAUI)
**Researched:** 2026-03-08
**Confidence note:** All external tools (WebSearch, WebFetch, Bash) were unavailable during this research session. Every version number below comes from training data with cutoff August 2025 and MUST be verified against NuGet before use. Confidence levels reflect this constraint.

---

## Recommended Stack

### Core Platform

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| .NET | 9.0 | Runtime | LTS release as of Nov 2024; MAUI 9 ships on it. .NET 10 preview exists but not production-ready as of Aug 2025. Verify if .NET 10 LTS has shipped by now. |
| .NET MAUI | 9.x (ships with .NET 9) | Cross-platform UI framework | Required per PROJECT.md constraints. Targets Windows, macOS (Mac Catalyst), iOS, Android from a single codebase. |
| C# | 13 | Language | Ships with .NET 9. Required for MAUI. |

**Confidence:** MEDIUM — .NET 9 was current as of training cutoff. Verify whether .NET 10 has shipped as LTS (due Nov 2025) and whether upgrading is warranted.

---

### MVVM Framework

**Recommendation: CommunityToolkit.Mvvm (formerly MVVM Toolkit)**

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| CommunityToolkit.Mvvm | ~8.3.x | MVVM infrastructure | Source-generator-based ObservableObject, RelayCommand, ObservableProperty. Zero-overhead at runtime vs reflection-based alternatives. First-party Microsoft community project. Works identically on all MAUI targets. |

**Why not alternatives:**
- **Prism.Maui**: Heavy DI container, page navigation framework built in. Overkill for a single-screen data entry app; adds complexity without benefit when you have only one or two "pages."
- **ReactiveUI**: Rx-based reactive programming model. Powerful but steep learning curve. Wrong fit for a straightforward form-entry/CRUD app where standard two-way binding suffices.
- **Plain MVVM (no toolkit)**: Viable but requires manually writing INotifyPropertyChanged boilerplate. The toolkit's source generators eliminate this entirely.

**Confidence:** HIGH — CommunityToolkit.Mvvm is the de-facto standard for .NET MAUI MVVM as of mid-2025. The choice is well-established and unlikely to have changed.

---

### UI Component Library

**Recommendation: CommunityToolkit.Maui**

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| CommunityToolkit.Maui | ~9.x | Extended controls and behaviors | Provides Popup, Toast, Snackbar, MediaElement, numeric entry helpers, and platform behaviors. Maintained by Microsoft community team alongside MAUI itself. No 3rd-party license risk. |
| CommunityToolkit.Maui.Core | same | Core controls subset | Included with CommunityToolkit.Maui; exposes Popup API used for drill-down bonus breakdown panels. |

**Why not 3rd-party control suites:**
- **Syncfusion MAUI**: Extensive controls (charts, grids, etc.) but requires a commercial license for production use. Overkill for a character sheet with no complex data visualizations.
- **DevExpress MAUI**: Same concern — commercial licensing, heavier than needed.
- **Telerik UI for MAUI**: Licensed, adds cost and dependency risk.

For this app, the stat bonus breakdown (the most "complex" UI requirement) can be built with a standard CollectionView + popups from CommunityToolkit.Maui.Core. No third-party control suite is needed.

**Confidence:** MEDIUM — CommunityToolkit.Maui is the right call; exact version requires NuGet verification.

---

### File Persistence

**Recommendation: System.Text.Json (built-in) + .NET file I/O**

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| System.Text.Json | Built into .NET 9 | Serialization to/from JSON | AOT-safe, zero extra dependency, fast. The native save format is app-defined (per PROJECT.md); JSON is the obvious choice — human-readable, diffs well, easy to hand-edit if needed. |
| System.IO (FileStream, StreamReader/Writer) | Built into .NET 9 | File read/write | Use FilePicker (MAUI built-in) to let user pick save location. Use FileSystem.AppDataDirectory for default save path. |

**Native file format design (recommendation):**
- Use `.sdchar` extension for the app's native save file
- Serialize the character model to UTF-8 JSON inside
- Keep format flat and human-readable rather than compressed binary — the character data is tiny (<50KB); no performance gain from binary formats
- Version the format with a `"formatVersion": 1` field for future migration support

**Why not SQLite:**
- SQLite (via sqlite-net or EF Core) makes sense for multi-record apps (e.g., a character roster). For a single-character-file model where file-picker-based open/save is the UX, SQLite is the wrong abstraction. A single JSON file maps naturally to a single character document.
- SQLite complicates "Open file from Files app" UX on iOS/macOS (you'd be picking a .db file, which users can't inspect or back up easily).

**Why not Newtonsoft.Json:**
- System.Text.Json is built-in, AOT-trimming-safe, and has no extra package weight. Newtonsoft.Json adds ~1.7MB and is not AOT-safe without workarounds. Avoid on MAUI where AOT is used for iOS/macOS deployment.

**Confidence:** HIGH — This is a well-established pattern. System.Text.Json + file I/O is the correct approach for a document-centric app on MAUI.

---

### Shadowdarklings Import

**Recommendation: System.Text.Json with a dedicated ImportModel**

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| System.Text.Json | Built into .NET 9 | Parse Shadowdarklings JSON export | Use a separate `ShadowdarklingsImportModel` with `[JsonPropertyName]` attributes matching the external format. Never reuse the internal character model for import — keep them decoupled so external format changes don't break the app. |

**Confidence:** HIGH — Standard deserialization approach; no external package needed.

---

### Markdown Export

**Recommendation: Markdig (for validation/rendering) or plain StringBuilder (for generation)**

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| Pure StringBuilder template | N/A — no package | Generate Markdown export | For this app, Markdown export is one-way output (character sheet → .md file). A StringBuilder template building the Markdown string is simpler, zero-dependency, and fully controllable. Markdig is a Markdown *parser/renderer*, not a generator — you don't need it for output. |

**Why not Markdig:**
- Markdig parses Markdown to HTML. It does not help with generating Markdown from data. For export, a well-structured string template (using C# interpolated strings or a simple template method) is the right tool.
- If a preview-in-app feature is later desired (render the Markdown in a WebView), Markdig becomes relevant then.

**Why not a template engine (Scriban, Handlebars.Net, etc.):**
- The export format is a single character sheet, not a multi-document reporting problem. A `MarkdownExporter` class with a `Export(Character character) → string` method is 50 lines of clear code. No template engine overhead justified.

**Markdown sharing:**
- On iOS/Android, use `Share.RequestAsync()` from MAUI Essentials (built-in to MAUI) to share the generated Markdown string as a `.md` file.
- On Windows/macOS, write to a file via `FileSaver` from CommunityToolkit.Maui.

**Confidence:** HIGH — This is the correct architecture. No external package needed for Markdown generation.

---

### Dependency Injection

**Recommendation: Microsoft.Extensions.DependencyInjection (built-in to MAUI)**

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| Microsoft.Extensions.DependencyInjection | Built into .NET MAUI | DI container | MAUI's MauiProgram.cs already scaffolds this. Register ViewModels, services (CharacterService, MarkdownExporter, ImportService) in `CreateMauiApp()`. No external DI container needed. |

**Confidence:** HIGH — Built-in; nothing to install.

---

### Testing

**Recommendation: xUnit + bUnit is NOT the right model for MAUI. Use xUnit with ViewModel unit tests only.**

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| xUnit | ~2.9.x | Unit test runner | Industry standard for .NET. Tests run on the host machine (not device), so all ViewModel logic, the import parser, the slot calculator, and the Markdown exporter are fully testable without a device. |
| Microsoft.Maui.Controls.Testing (or xUnit only) | — | UI-layer testing | MAUI UI testing is painful: device-required UITest (Appium-based) is slow and fragile. For this app, keep UI thin (no logic in code-behind), test everything at the ViewModel/service layer via xUnit. |

**Testing strategy:**
1. **ViewModel tests**: Test `CharacterViewModel` slot calculations, stat modifier math, bonus toggle logic — pure C#, no MAUI dependency.
2. **Import tests**: Test `ShadowdarklingsImportService` against the sample `Brim.json` — JSON deserialization, no UI needed.
3. **Export tests**: Test `MarkdownExporter.Export()` produces expected output for known character state.
4. **Skip UI tests**: Don't invest in Appium/UITest for this scope. The logic is in the ViewModel; the UI is declarative XAML binding.

**Why not NUnit or MSTest:**
- xUnit is the modern standard for .NET. CommunityToolkit.Mvvm source generators work identically under xUnit. No reason to use the others.

**Confidence:** MEDIUM — xUnit version requires NuGet verification; the strategy (test ViewModels, skip device UI tests) is HIGH confidence and the correct tradeoff for this project scope.

---

### Platform File Picker / Save Integration

**Recommendation: MAUI built-ins (FilePicker, FileSaver via CommunityToolkit.Maui)**

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| FilePicker (Microsoft.Maui.Storage) | Built into MAUI | Open .sdchar files | Cross-platform file open dialog. Filter by `.sdchar` extension. |
| FileSaver (CommunityToolkit.Maui) | ~9.x | Save .sdchar files | CommunityToolkit.Maui adds `FileSaver` which provides a cross-platform save-as dialog. The built-in MAUI has no `FileSaver` — this is why CommunityToolkit.Maui is needed. |
| FileSystem.AppDataDirectory | Built into MAUI | Default auto-save path | Use for auto-save between explicit user saves. Platform-appropriate sandboxed location. |

**Confidence:** HIGH — FilePicker is well-established in MAUI; FileSaver from CommunityToolkit.Maui is the documented solution for save-as dialogs.

---

## Complete NuGet Package List

```xml
<!-- In the MAUI project .csproj -->

<!-- MVVM -->
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.3.*" />

<!-- UI Controls and FileSaver -->
<PackageReference Include="CommunityToolkit.Maui" Version="9.*" />

<!-- No other runtime packages needed -->
<!-- System.Text.Json is in-box with .NET 9 -->
<!-- FilePicker is in-box with MAUI -->
<!-- DI is in-box with MAUI -->
<!-- Share/Essentials is in-box with MAUI -->
```

```xml
<!-- In the test project .csproj (plain .NET class library) -->
<PackageReference Include="xunit" Version="2.9.*" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.8.*" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
```

**IMPORTANT:** All versions above are from training data (cutoff Aug 2025). Before creating the project, run:
```
dotnet add package CommunityToolkit.Mvvm
dotnet add package CommunityToolkit.Maui
```
...and let NuGet resolve current stable versions. Then pin those versions in the .csproj.

---

## Alternatives Considered

| Category | Recommended | Alternative | Why Not |
|----------|-------------|-------------|---------|
| MVVM framework | CommunityToolkit.Mvvm | Prism.Maui | Prism adds navigation framework + heavy DI; overkill for this scope |
| MVVM framework | CommunityToolkit.Mvvm | ReactiveUI | Rx model is wrong fit for form-entry CRUD; steep learning curve |
| Persistence | System.Text.Json + flat file | SQLite (EF Core / sqlite-net) | SQLite is wrong UX model for single-document file-picker pattern |
| Persistence | System.Text.Json | Newtonsoft.Json | Not AOT-safe; unnecessary weight; System.Text.Json is in-box |
| Markdown generation | StringBuilder template | Markdig | Markdig parses Markdown, doesn't generate it |
| Markdown generation | StringBuilder template | Scriban/Handlebars | Template engines are overkill for a single-document export |
| UI controls | CommunityToolkit.Maui | Syncfusion / DevExpress / Telerik | Commercial licenses required; features far exceed what's needed |
| Testing | xUnit (ViewModel only) | Appium UITest | Device-required, slow, fragile; avoid for this scope |

---

## Project Structure Recommendation

```
ShadowdarkSheet/
├── ShadowdarkSheet/              # MAUI app project
│   ├── MauiProgram.cs            # DI setup
│   ├── Models/
│   │   ├── Character.cs          # Core domain model
│   │   ├── GearItem.cs
│   │   ├── StatBonus.cs
│   │   └── Import/
│   │       └── ShadowdarklingsModel.cs  # Import-only model, never reused
│   ├── ViewModels/
│   │   ├── CharacterViewModel.cs
│   │   └── InventoryViewModel.cs
│   ├── Views/
│   │   ├── CharacterPage.xaml
│   │   └── InventoryPage.xaml
│   ├── Services/
│   │   ├── CharacterFileService.cs    # JSON serialization + FilePicker
│   │   ├── ImportService.cs           # Shadowdarklings import
│   │   └── MarkdownExporter.cs        # StringBuilder-based export
│   └── Resources/
├── ShadowdarkSheet.Tests/        # Plain .NET class library (xUnit)
│   ├── ViewModels/
│   ├── Services/
│   └── Import/
└── examples/
    └── Brim.json
```

---

## Sources

**Note:** All external research tools were unavailable during this session. The following are authoritative sources to verify version numbers before project creation:

- NuGet: https://www.nuget.org/packages/CommunityToolkit.Mvvm — verify current stable version
- NuGet: https://www.nuget.org/packages/CommunityToolkit.Maui — verify current stable version
- Official MAUI docs: https://learn.microsoft.com/en-us/dotnet/maui/
- CommunityToolkit.Maui docs: https://learn.microsoft.com/en-us/dotnet/communitytoolkit/maui/
- CommunityToolkit.Mvvm docs: https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/
- .NET 9 release notes: https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9/overview
- FileSaver API: https://learn.microsoft.com/en-us/dotnet/communitytoolkit/maui/essentials/file-saver

**Training data cutoff:** August 2025. .NET 10 may have shipped as LTS by March 2026 — verify and consider upgrading if so.

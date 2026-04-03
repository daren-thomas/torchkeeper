# Phase 7: MAUI Layer IsFreeCarry Fix - Research

**Researched:** 2026-03-29
**Domain:** .NET MAUI C# — MAUI-local model/DTO/service sync with Core library
**Confidence:** HIGH

## Summary

Phase 7 is a surgical four-file patch to the MAUI-local model layer. The gap was precisely documented by the v1.2 milestone audit (MILESTONE-AUDIT.md): the Core library (`TorchKeeper.Core`) was fully updated with `IsFreeCarry` during Phase 6, but the MAUI project (`TorchKeeper`) maintains parallel copies of four files under the same namespace (`TorchKeeper.Models`, `TorchKeeper.DTOs`, `TorchKeeper.Services`) that were never updated. Because the local definition wins the CS0436 ambiguity, MAUI compiles against the local `GearItem` which lacks `IsFreeCarry`, likely producing CS0117 compile errors on `GearItemViewModel`'s accesses. At runtime (if it even compiles), `MapToDto` and `MapFromDto` silently drop the property from JSON, destroying manually-flagged free-carry status on every save/load cycle.

The fix is a direct port of the four lines added to Core in Phase 6: one property per model file, one property per DTO record, and two assignments per mapping direction in the service. The `GearItemViewModel` that accesses `g.IsFreeCarry` already has the correct logic and needs no changes — it just needs the local model to expose the property. `MauiCharacterFileService` inherits `MapToDto`/`MapFromDto` from the local `CharacterFileService` and needs no changes once the service is fixed.

Because `TorchKeeper.Tests` references only `TorchKeeper.Core` (verified in the `.csproj`), there is no existing test that exercises the MAUI-local service path. The phase needs a new integration test that exercises the MAUI-local `CharacterFileService` (not the Core one) to verify the round-trip, or alternatively a build-verification step confirming the MAUI project compiles without CS0117 errors. The nyquist_validation config flag is `true`, so a VALIDATION.md is required.

**Primary recommendation:** Four targeted property additions and two mapping assignments across four files. Add one new xUnit test exercising the MAUI-local round-trip. Verify build with `dotnet build` targeting net10.0-maccatalyst.

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| GEAR-01 | `IsFreeCarry` throughout model/DTO/service/VM/UI layers; auto-detect; slot exclusion; sub-collections. The MAUI-local layer must be updated to achieve parity with the Core layer already implemented in Phase 6. | Audit documents exact four files and exact properties/assignments needed. Core implementation provides the exact code pattern to copy. |
</phase_requirements>

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| .NET MAUI | 10.0.41 | Cross-platform UI framework | Existing project framework |
| CommunityToolkit.Mvvm | 8.4.0 | ObservableObject / source generators | Already used throughout ViewModels |
| xUnit | 2.9.3 | Test framework | Already used in TorchKeeper.Tests |
| System.Text.Json | Built-in | JSON serialization for .sdchar files | Already in use via JsonSerializer |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| CommunityToolkit.Maui | 14.0.1 | IFileSaver interface for MAUI save dialog | Already wired into CharacterFileService |

**No new packages required.** All dependencies are present in the existing project.

### Build Commands
```bash
# Build MAUI project (verifies no CS0117 errors)
dotnet build TorchKeeper/TorchKeeper.csproj -f net10.0-maccatalyst

# Build and run tests (Core layer only — MAUI layer untestable without platform)
dotnet test TorchKeeper.Tests/TorchKeeper.Tests.csproj
```

**Version note:** All package versions verified from existing `.csproj` files in the project. No version changes required.

## Architecture Patterns

### The Dual-Layer Architecture (Critical Context)

This project has a structural pattern where the MAUI project (`TorchKeeper/`) contains parallel copies of models, DTOs, and services under the same C# namespaces as `TorchKeeper.Core/`. The MAUI project also has a `<ProjectReference>` to Core, but because both assemblies define types in the same namespace, the local definition wins via CS0436 warning (type from referenced assembly is shadowed by local definition).

This means:
- `TorchKeeper/ViewModels/GearItemViewModel.cs` uses `using TorchKeeper.Models` — this resolves to the **MAUI-local** `GearItem`, not the Core one
- `TorchKeeper/Services/CharacterFileService.cs` maps MAUI-local models to MAUI-local DTOs
- `MauiCharacterFileService` inherits from MAUI-local `CharacterFileService`, not Core's

**Consequence for this phase:** Updating Core alone (as Phase 6 did) is insufficient. The MAUI-local files must be updated independently.

### Recommended Project Structure (Scope)

```
TorchKeeper/
├── Models/
│   ├── GearItem.cs          ← ADD: public bool IsFreeCarry { get; set; }
│   └── MagicItem.cs         ← ADD: public bool IsFreeCarry { get; set; }
├── DTOs/
│   └── CharacterSaveData.cs ← ADD: IsFreeCarry to GearItemData + MagicItemData
└── Services/
    └── CharacterFileService.cs ← ADD: IsFreeCarry = g.IsFreeCarry in MapToDto + MapFromDto

TorchKeeper.Tests/
└── Services/
    └── CharacterFileServiceTests.cs ← ADD: GEAR-01 MAUI round-trip test (new test)
```

### Pattern 1: Property Parity with Core

Each MAUI-local file should match the Core counterpart exactly for the added property. The exact patterns from Core:

**GearItem / MagicItem (Source: TorchKeeper.Core/Models/)**
```csharp
// Core pattern — copy verbatim to MAUI-local files
public bool IsFreeCarry { get; set; }         // true = excluded from GearSlotsUsed (D-05)
```

**CharacterSaveData DTOs (Source: TorchKeeper.Core/DTOs/CharacterSaveData.cs line 65, 73)**
```csharp
// In GearItemData:
public bool IsFreeCarry { get; init; }        // added for GEAR-01 persistence

// In MagicItemData:
public bool IsFreeCarry { get; init; }        // added for GEAR-01 persistence
```

Note: DTOs use `init` not `set` — consistent with all other DTO properties in the file.

**CharacterFileService MapToDto (Source: TorchKeeper.Core/Services/CharacterFileService.cs lines 81, 90)**
```csharp
// In Gear select:
new GearItemData
{
    Name = g.Name,
    Slots = g.Slots,
    ItemType = g.ItemType,
    Note = g.Note,
    IsFreeCarry = g.IsFreeCarry,   // added
}

// In MagicItems select:
new MagicItemData
{
    Name = m.Name,
    Slots = m.Slots,
    Note = m.Note,
    IsFreeCarry = m.IsFreeCarry,   // added
}
```

**CharacterFileService MapFromDto (Source: TorchKeeper.Core/Services/CharacterFileService.cs lines 139, 148)**
```csharp
// In Gear select:
new GearItem
{
    Name = g.Name,
    Slots = g.Slots,
    ItemType = g.ItemType,
    Note = g.Note,
    IsFreeCarry = g.IsFreeCarry,   // added
}

// In MagicItems select:
new MagicItem
{
    Name = m.Name,
    Slots = m.Slots,
    Note = m.Note,
    IsFreeCarry = m.IsFreeCarry,   // added
}
```

### Pattern 2: JSON Serialization Behavior (Critical Gotcha)

The MAUI `CharacterFileService` uses `DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault` in `SaveOptions`. This means `IsFreeCarry = false` (the default for `bool`) will be **omitted from the JSON output**. This is correct behavior: items without explicit free-carry status don't need the field serialized.

On `LoadOptions`, `PropertyNameCaseInsensitive = true` handles deserialization. When `IsFreeCarry` is absent in JSON (old saves), `System.Text.Json` defaults `bool` to `false`, which is correct — old saves didn't have manually-flagged items, and the `KnownFreeCarryNames` auto-detect fallback in `GearItemViewModel` covers the backpack/coins/thieves tools cases.

**Backward compatibility:** Adding `IsFreeCarry` to the MAUI-local DTO is fully backward-compatible. Old `.sdchar` files without the field will deserialize with `IsFreeCarry = false`, and `GearItemViewModel`'s constructor auto-detects known names.

### Pattern 3: Test Pattern for MAUI-Local Service

The existing test `RoundTrip_GearItem_IsFreeCarry_Persists` (CharacterFileServiceTests.cs line 122-144) tests the **Core** `CharacterFileService`. A new test should exercise the MAUI-local `CharacterFileService` using the same `NullFileSaver` stub pattern. However, since `TorchKeeper.Tests.csproj` only references `TorchKeeper.Core`, adding a test for the MAUI-local service requires either:

1. Adding a `<ProjectReference>` to `TorchKeeper` in `TorchKeeper.Tests.csproj` — **problematic** because MAUI project targets platform frameworks (net10.0-ios, net10.0-maccatalyst) which aren't compatible with the test project's `net10.0` target.
2. Verifying correctness through build verification (`dotnet build` the MAUI project, confirming no CS0117 or CS0200 errors) rather than a unit test.

**Conclusion:** The MAUI-layer round-trip cannot be unit-tested from `TorchKeeper.Tests` because the MAUI project doesn't target `net10.0` (only platform targets). The phase success criteria are validated by (a) MAUI build with zero C# errors, and (b) manual smoke test or build-time verification. The Nyquist validation should document this constraint.

### Anti-Patterns to Avoid

- **Only updating Core:** Phase 6 made this mistake. Both layers must be updated when models change.
- **Using `set` on DTO properties:** All DTO properties use `init` — the new `IsFreeCarry` on DTOs must also use `init`.
- **Adding `[JsonIgnore]` to the model property:** `IsFreeCarry` must serialize to JSON for persistence. The `WhenWritingDefault` option already suppresses `false` values.
- **Changing `GearItemViewModel`:** It already has correct logic (`isFreeCarry = g.IsFreeCarry || IsKnownFreeCarry(g.Name)`). Do not modify it.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| JSON serialization | Custom serializer | System.Text.Json (already wired) | Already handles `init`-only records, WhenWritingDefault |
| Backward compat for old saves | Migration code | Default value `false` + auto-detect fallback | `GearItemViewModel` constructor already handles this |
| MAUI platform testing | Platform test harness | Build verification + manual smoke test | MAUI targets are not testable from net10.0 test project |

**Key insight:** This phase is a sync operation, not a feature build. The pattern is already defined in Core; this phase ports it to the MAUI-local layer.

## Runtime State Inventory

> Phase is a code-only change — no data migration needed.

| Category | Items Found | Action Required |
|----------|-------------|------------------|
| Stored data | `.sdchar` JSON files saved by users | None — backward compatible (missing field deserializes as `false`) |
| Live service config | None | None |
| OS-registered state | None | None |
| Secrets/env vars | None | None |
| Build artifacts | `TorchKeeper/bin/` and `obj/` contain stale compiled output | `dotnet build` will update; no manual action |

**Old `.sdchar` files:** Backward compatible. Existing saves without `IsFreeCarry` in JSON load with `IsFreeCarry = false`, and `GearItemViewModel` auto-detects `KnownFreeCarryNames` (Backpack, Bag of Coins, Thieves Tools) via the constructor fallback. No migration needed.

## Common Pitfalls

### Pitfall 1: CS0436 Namespace Ambiguity Confusion

**What goes wrong:** Developer assumes `using TorchKeeper.Models` in the MAUI project resolves to Core's `GearItem`. It does not. The local assembly's type wins. The compiler emits a CS0436 warning but uses the local definition.

**Why it happens:** Both Core and MAUI-local define `TorchKeeper.Models.GearItem`. The local project's definition always shadows the referenced assembly.

**How to avoid:** Always check whether a file being edited is in `TorchKeeper/` (MAUI-local) or `TorchKeeper.Core/` (Core). They are separate files that must be kept in sync manually.

**Warning signs:** CS0117 error ("TorchKeeper.Models.GearItem does not contain a definition for 'IsFreeCarry'") means the local model is outdated.

### Pitfall 2: `init` vs `set` on DTOs

**What goes wrong:** Adding `public bool IsFreeCarry { get; set; }` to a DTO that uses `init` everywhere.

**Why it happens:** Model files use `set`; DTO files use `init`. It's easy to copy the wrong pattern.

**How to avoid:** Check the surrounding properties in the DTO file before adding — all existing properties in `GearItemData` and `MagicItemData` use `init`.

**Warning signs:** The `init` pattern is enforced by convention, not by compiler error — it will compile either way, but deviates from the established DTO pattern.

### Pitfall 3: MAUI Build Failure Masking CS0117

**What goes wrong:** The MAUI build is attempted via full MAUI toolchain (Xcode actool/xcrun). If the Xcode build stage fails first, C# compilation errors may not surface in the output.

**Why it happens:** The MAUI build pipeline on macOS runs asset compilation (actool) before or alongside C# compilation. toolchain errors can mask downstream errors.

**How to avoid:** Use `dotnet build TorchKeeper/TorchKeeper.csproj -f net10.0-maccatalyst 2>&1 | grep -E "error CS|warning CS"` to isolate C# errors from Xcode errors.

**Warning signs:** Build output shows `actool` or `xcrun` errors but no C# error lines — does not mean C# is clean.

### Pitfall 4: Forgetting Both Mapping Directions

**What goes wrong:** Adding `IsFreeCarry` to `MapToDto` but forgetting `MapFromDto` (or vice versa).

**Why it happens:** The two methods are far apart in the file (lines 69-122 vs 124-176 in the MAUI-local service).

**How to avoid:** Search for both occurrences: `MapToDto` writes `GearItemData`/`MagicItemData`, `MapFromDto` writes `GearItem`/`MagicItem`. Both must be updated.

**Warning signs:** `IsFreeCarry` appears in JSON on save but is `false` after reload — MapFromDto was missed.

## Code Examples

### Full Diff: MAUI-local GearItem.cs
```csharp
// Source: TorchKeeper.Core/Models/GearItem.cs (line 9) — copy pattern
// Add to TorchKeeper/Models/GearItem.cs after line 8 (Note property)
public bool IsFreeCarry { get; set; }         // true = excluded from GearSlotsUsed (D-05)
```

### Full Diff: MAUI-local MagicItem.cs
```csharp
// Source: TorchKeeper.Core/Models/MagicItem.cs (line 8) — copy pattern
// Add to TorchKeeper/Models/MagicItem.cs after line 7 (Note property)
public bool IsFreeCarry { get; set; }         // true = excluded from GearSlotsUsed (D-05)
```

### Full Diff: MAUI-local CharacterSaveData.cs
```csharp
// In GearItemData (after Note property, line 62):
public bool IsFreeCarry { get; init; }        // added for GEAR-01 persistence

// In MagicItemData (after Note property, line 69):
public bool IsFreeCarry { get; init; }        // added for GEAR-01 persistence
```

### Build Verification Command
```bash
dotnet build /Users/daren/projects/sd-character-sheet/TorchKeeper/TorchKeeper.csproj \
  -f net10.0-maccatalyst 2>&1 | grep -E "^.*error CS|^Build succeeded|^Build FAILED"
```

Expected success output: `Build succeeded.` with zero `error CS` lines.

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Core-only update (Phase 6) | Core + MAUI-local sync (Phase 7) | Phase 7 | Closes GEAR-01 gap; IsFreeCarry round-trips in running app |
| CS0436 silent masking | Explicit parallel file maintenance | Architectural pattern | Both layers must be updated when models change |

**Deprecated/outdated:**
- MAUI-local `GearItem`, `MagicItem`, `GearItemData`, `MagicItemData` without `IsFreeCarry`: replaced by this phase

## Open Questions

1. **MAUI-local `Character.cs` has drifted from Core**
   - What we know: MAUI-local `Character.cs` is missing `MaxXP` and `Talents` properties that Core's `Character.cs` has. The MAUI-local `CharacterFileService.MapToDto` uses `character.Talents` but the local `Character.cs` doesn't declare it — this should also produce CS0117.
   - What's unclear: Whether this causes actual compile errors (the GearItemViewModel might resolve to Core's `Character` since there's no MAUI-local ViewModel namespace clash), or whether existing builds have been masking this.
   - Recommendation: Scope Phase 7 strictly to GEAR-01 (IsFreeCarry). Document the `Character.cs` divergence as a separate tech debt item. Do not expand scope without explicit decision.

2. **Test coverage for MAUI-local service path**
   - What we know: `TorchKeeper.Tests` targets `net10.0` and references only Core. MAUI targets are platform-specific (`net10.0-maccatalyst`, `net10.0-ios`) and cannot be referenced from a standard xUnit project.
   - What's unclear: Whether adding a `net10.0` fallback target to the MAUI project would allow importing without Xcode.
   - Recommendation: Accept build verification as the test signal for this phase. Document in VALIDATION.md that MAUI-layer round-trip is verified by build (CS0117 absence) and manual smoke test, not automated unit test.

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|------------|-----------|---------|----------|
| .NET SDK | Build/test | ✓ | 10.0.201 | — |
| dotnet build | C# compile verification | ✓ | 10.0.201 | — |
| Xcode / actool | Full MAUI app bundle (not needed for this phase) | Unknown | — | Use `-f net10.0-maccatalyst` for C# check only |
| dotnet test | Unit test runner | ✓ (build) | 10.0.201 | Note: socket error in sandbox; run outside sandbox |

**Missing dependencies with no fallback:** None — the phase only requires `dotnet build` and optional `dotnet test`.

**Note on test runner:** `dotnet test` fails with `SocketException (13): Permission denied` in the sandbox environment. Tests must be run outside the sandbox (normal terminal). The build step has no such restriction.

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | xUnit 2.9.3 |
| Config file | `TorchKeeper.Tests/TorchKeeper.Tests.csproj` |
| Quick run command | `dotnet test TorchKeeper.Tests/TorchKeeper.Tests.csproj` |
| Full suite command | `dotnet test TorchKeeper.Tests/TorchKeeper.Tests.csproj` |

### Phase Requirements → Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| GEAR-01 | IsFreeCarry persists through MAUI-local MapToDto/MapFromDto | build | `dotnet build TorchKeeper/TorchKeeper.csproj -f net10.0-maccatalyst` — CS0117 absence confirms property exists | ✅ (build, not unit test) |
| GEAR-01 | IsFreeCarry round-trips through Core service JSON | unit | `dotnet test TorchKeeper.Tests/TorchKeeper.Tests.csproj` | ✅ `CharacterFileServiceTests.cs:122` |
| GEAR-01 | MAUI-local build compiles without CS0117 | build | `dotnet build TorchKeeper/TorchKeeper.csproj -f net10.0-maccatalyst 2>&1 \| grep "error CS"` | ✅ (build gate) |

### Sampling Rate
- **Per task commit:** `dotnet build TorchKeeper/TorchKeeper.csproj -f net10.0-maccatalyst`
- **Per wave merge:** `dotnet test TorchKeeper.Tests/TorchKeeper.Tests.csproj`
- **Phase gate:** Both build (zero CS errors) and test suite green before `/gsd:verify-work`

### Wave 0 Gaps
- None — existing test infrastructure covers all automated assertions for Core layer. MAUI-layer validation is build-based (no xUnit gap to fill given platform target incompatibility).

## Sources

### Primary (HIGH confidence)
- Direct file inspection: `TorchKeeper/Models/GearItem.cs` — confirmed missing `IsFreeCarry`
- Direct file inspection: `TorchKeeper/Models/MagicItem.cs` — confirmed missing `IsFreeCarry`
- Direct file inspection: `TorchKeeper/DTOs/CharacterSaveData.cs` — confirmed `GearItemData` and `MagicItemData` missing `IsFreeCarry`
- Direct file inspection: `TorchKeeper/Services/CharacterFileService.cs` — confirmed `MapToDto`/`MapFromDto` missing `IsFreeCarry` assignments
- Direct file inspection: `TorchKeeper.Core/**` counterpart files — confirmed correct implementation pattern
- Direct file inspection: `TorchKeeper.Tests/TorchKeeper.Tests.csproj` — confirmed Core-only reference
- `.planning/v1.2-MILESTONE-AUDIT.md` — authoritative gap documentation with exact file/line references
- `TorchKeeper/TorchKeeper.csproj` — confirmed platform targets prevent net10.0 test inclusion

### Secondary (MEDIUM confidence)
- `TorchKeeper/Services/MauiCharacterFileService.cs` — confirms inheritance chain; MauiCharacterFileService inherits from MAUI-local CharacterFileService and needs no changes after parent is fixed

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — all packages already in project; no new dependencies
- Architecture: HIGH — both Core and MAUI-local files directly inspected; gap precisely documented
- Pitfalls: HIGH — CS0436 behavior and init/set patterns verified from actual code
- Test limitations: HIGH — platform target incompatibility confirmed from `.csproj`

**Research date:** 2026-03-29
**Valid until:** 2026-04-29 (stable codebase; no external API dependencies)

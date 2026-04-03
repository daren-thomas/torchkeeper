# Phase 5: Talents Editor - Research

**Researched:** 2026-03-22
**Domain:** .NET MAUI — XAML UI, MVVM data binding, save/load round-trip, Markdown export
**Confidence:** HIGH

## Summary

Phase 5 was implemented inline on 2026-03-21 (commit 4239483) before formal planning began. The
full-stack implementation is already shipped: `Talents` and `SpellsKnown` free-text fields are
wired through the domain model (`Character`), the save DTO (`CharacterSaveData`), the file service
(`CharacterFileService` save and load paths), the export data object (`CharacterExportData`), the
Markdown builder (`MarkdownBuilder`), and the ViewModel (`CharacterViewModel`). The Notes tab UI
now shows three labeled sections — Talents, Spells, Notes — each in a `Frame`-wrapped `Editor`
inside a `ScrollView`.

However, the `Talents` field was added without updating the existing automated tests. The
`CharacterFileServiceTests` round-trip fixture includes `SpellsKnown` but has no assertion for
`Talents`. The `MarkdownBuilderTests` `MinimalData` helper does not accept a `talents` parameter,
and there are no tests for the Talents section appearing or being omitted in Markdown output. These
are real gaps: if the Talents save/load path regressed, no test would catch it.

The planning work for Phase 5 therefore has one job: close the test coverage gaps and record the
phase as complete. No new feature implementation is needed.

**Primary recommendation:** Write two targeted xUnit tests (round-trip coverage for Talents in
`CharacterFileServiceTests` and Talents section rendering in `MarkdownBuilderTests`), run the full
test suite, and mark the phase done.

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| TLNT-01 | User can view and edit a Talents/Spells free-text area in the Notes tab, above the Notes editor | Implementation complete in commit 4239483. UI shows Talents then Spells then Notes on NotesPage.xaml. Full data stack wired. Test coverage for Talents field specifically is the remaining gap. |
</phase_requirements>

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| .NET MAUI | 10 | Cross-platform UI framework | Project's chosen stack |
| CommunityToolkit.Mvvm | (project version) | Observable properties, RelayCommand | Already in use throughout project |
| xUnit | (project version) | Unit test framework | Already in use (27+ tests) |
| System.Text.Json | (built-in) | JSON serialization for .sdchar | Already in use |

No new libraries are needed for this phase. All stack components are already installed.

### Alternatives Considered
None — the implementation is complete. Stack decisions are locked.

## Architecture Patterns

### Implementation Already in Place

The inline implementation followed the established full-stack pattern:

```
Domain model layer:   TorchKeeper.Core/Models/Character.cs
                      → public string Talents { get; set; } = "";

Save DTO:             TorchKeeper.Core/DTOs/CharacterSaveData.cs
                      → public string Talents { get; init; } = "";

File service (save):  CharacterFileService.MapToDto()
                      → Talents = character.Talents,

File service (load):  CharacterFileService.LoadFromStreamAsync()
                      → Talents = dto.Talents,

Export data:          TorchKeeper.Core/Export/CharacterExportData.cs
                      → public string Talents { get; init; } = "";

Markdown builder:     MarkdownBuilder.BuildMarkdown()
                      → if (!string.IsNullOrWhiteSpace(data.Talents)) { ... }

ViewModel:            CharacterViewModel.BuildCharacterFromViewModel()
                      → Talents = Talents,
                      CharacterViewModel.LoadCharacter()
                      → talents = character.Talents;

UI:                   TorchKeeper/Views/NotesPage.xaml
                      → ScrollView > VerticalStackLayout:
                         Label "Talents" + Frame > Editor (Talents binding)
                         Label "Spells"  + Frame > Editor (SpellsKnown binding)
                         Label "Notes"   + Frame > Editor (Notes binding)
```

### Pattern: AutoSize Editor in Scrollable Container
**What:** Each `Editor` uses `AutoSize="TextChanges"` with a `MinimumHeightRequest`, inside a
`Frame` inside a `VerticalStackLayout` inside a `ScrollView`. This makes each text area grow with
content while the page itself scrolls.

**When to use:** When multiple text areas must be independently editable and the total height may
exceed the screen. This is exactly the TLNT-01 success criterion ("independently editable and
scrollable").

**Important:** The outer `ScrollView` provides page-level scrolling. The `Editor` itself does NOT
need to scroll independently — `AutoSize="TextChanges"` expands the editor instead. This avoids
nested scroll conflict.

### Anti-Patterns to Avoid
- **Nested ScrollViews:** Do not put a scrollable `Editor` inside a `ScrollView`. Use
  `AutoSize="TextChanges"` instead — the editor grows and the outer `ScrollView` scrolls.
- **Grid with RowDefinitions for text areas:** Makes height management complex. `VerticalStackLayout`
  with `AutoSize` editors is simpler and correct for variable-height content.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Observable `Talents` property | Manual `INotifyPropertyChanged` | `[ObservableProperty]` from CommunityToolkit.Mvvm | Project already uses this pattern everywhere |
| Save/load JSON | Custom serializer | `System.Text.Json` + `CharacterSaveData` DTO | Already established; round-trip verified |

## Runtime State Inventory

> This is not a rename/refactor/migration phase. No runtime state inventory needed.

None — implementation adds a new field. No existing stored data, OS-registered state, secrets,
or build artifacts reference anything being renamed.

## Common Pitfalls

### Pitfall 1: Talents Field Missing from Test Fixture
**What goes wrong:** The existing `CharacterFileServiceTests.RoundTrip_SaveLoad_NoDataLoss` test
does not set or assert `Talents`. A bug in the Talents save or load path would go undetected.

**Why it happens:** The field was added inline without updating the existing round-trip test.

**How to avoid:** Add `Talents = "Backstab +1"` to the test character and add
`Assert.Equal("Backstab +1", loaded.Talents)` to the assertions.

**Warning signs:** Test passes but Talents is always empty after load — regression goes unnoticed
at the table.

### Pitfall 2: Talents Missing from MarkdownBuilder Test Coverage
**What goes wrong:** `MarkdownBuilderTests.MinimalData()` helper has no `talents` parameter.
The Talents section in `MarkdownBuilder.BuildMarkdown()` is exercised only indirectly (if at all).

**Why it happens:** Same as above — inline implementation predated test updates.

**How to avoid:** Add a `talents` parameter to `MinimalData()` and write two tests:
- `BuildMarkdown_TalentsSection_AppearsWhenTalentsNonEmpty`
- `BuildMarkdown_TalentsSection_OmittedWhenTalentsEmpty`

These mirror the existing `SpellsKnown` tests (Tests 12 and 13, lines 279–304 of
`MarkdownBuilderTests.cs`).

### Pitfall 3: AutoSize + ScrollView Scroll Conflict on iOS
**What goes wrong:** On iOS, a non-autosizing `Editor` inside a `ScrollView` can create a
nested scroll conflict where the inner editor captures all scroll events.

**Why it happens:** MAUI `Editor` with fixed height inside `ScrollView` triggers platform
gesture recognizer conflicts.

**How to avoid:** The current implementation correctly uses `AutoSize="TextChanges"` — editors
grow instead of scroll. Do not change this to a fixed-height editor.

## Code Examples

### Verified: NotesPage.xaml (current state, commit 4239483)
```xml
<!-- Source: TorchKeeper/Views/NotesPage.xaml -->
<ScrollView>
    <VerticalStackLayout Padding="16" Spacing="16">
        <Label Text="Talents" FontAttributes="Bold" FontSize="16" />
        <Frame Padding="8">
            <Editor Text="{Binding Talents, Mode=TwoWay}"
                    Placeholder="Talents gained at each level..."
                    MinimumHeightRequest="80"
                    AutoSize="TextChanges" />
        </Frame>
        <Label Text="Spells" FontAttributes="Bold" FontSize="16" />
        <Frame Padding="8">
            <Editor Text="{Binding SpellsKnown, Mode=TwoWay}"
                    Placeholder="Spells known..."
                    MinimumHeightRequest="80"
                    AutoSize="TextChanges" />
        </Frame>
        <Label Text="Notes" FontAttributes="Bold" FontSize="16" />
        <Frame Padding="8">
            <Editor Text="{Binding Notes, Mode=TwoWay}"
                    Placeholder="Your notes here..."
                    MinimumHeightRequest="120"
                    AutoSize="TextChanges" />
        </Frame>
    </VerticalStackLayout>
</ScrollView>
```

### Pattern to Replicate: SpellsKnown Tests (existing, verified)
```csharp
// Source: TorchKeeper.Tests/Export/MarkdownBuilderTests.cs lines 279-304
// Tests 12 and 13 — mirror these exactly for Talents:

[Fact]
public void BuildMarkdown_TalentsSection_AppearsWhenTalentsNonEmpty()
{
    var data = MinimalData(talents: "Backstab +1, Tough as Nails");
    var md = MarkdownBuilder.BuildMarkdown(data);
    Assert.Contains("## Talents", md);
    Assert.Contains("Backstab +1", md);
}

[Fact]
public void BuildMarkdown_TalentsSection_OmittedWhenTalentsEmpty()
{
    var data = MinimalData(talents: "");
    var md = MarkdownBuilder.BuildMarkdown(data);
    Assert.DoesNotContain("## Talents", md);
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Single Notes Editor filling the tab | Three labeled sections (Talents, Spells, Notes) in ScrollView | Commit 4239483 (2026-03-21) | TLNT-01 success criteria met |
| No Talents field in model | `Character.Talents` string property | Commit 4239483 | Full stack wired |

## Open Questions

1. **Should the Talents round-trip test be a separate test or expand the existing fixture?**
   - What we know: The existing `RoundTrip_SaveLoad_NoDataLoss` is a comprehensive fixture test;
     adding one field + one assertion is a minimal change.
   - What's unclear: Whether project convention prefers expansion vs. a new focused test.
   - Recommendation: Expand the existing fixture — it is explicitly a "no data loss" test, and
     Talents is a data field that should be part of that guarantee.

2. **Is human verification of the UI required for phase completion?**
   - What we know: Phase 4 included a dedicated human-verification plan (04-02). Phase 5 was
     implemented inline and visually reviewed by the author (STATE.md: "Committed 4239483").
   - What's unclear: Whether the project's verification bar requires a formal checklist or a
     simple "ran the app" confirmation.
   - Recommendation: Given coarse granularity mode, a brief manual smoke-test step in the plan
     is sufficient — not a dedicated verification plan.

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | xUnit (version as in TorchKeeper.Tests.csproj) |
| Config file | TorchKeeper.Tests/TorchKeeper.Tests.csproj |
| Quick run command | `dotnet test TorchKeeper.Tests/ --filter "Category=Unit" -q` |
| Full suite command | `dotnet test TorchKeeper.Tests/ -q` |

### Phase Requirements to Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|--------------|
| TLNT-01 | Talents text survives save/load round-trip | unit | `dotnet test TorchKeeper.Tests/ --filter "DisplayName~RoundTrip" -q` | Exists but needs update |
| TLNT-01 | Talents section appears in Markdown when non-empty | unit | `dotnet test TorchKeeper.Tests/ --filter "DisplayName~Talents" -q` | ❌ Wave 0 |
| TLNT-01 | Talents section omitted from Markdown when empty | unit | `dotnet test TorchKeeper.Tests/ --filter "DisplayName~Talents" -q` | ❌ Wave 0 |
| TLNT-01 | UI shows Talents above Spells above Notes (visual) | manual | Run app, navigate to Notes tab | N/A — manual only |

### Sampling Rate
- **Per task commit:** `dotnet test TorchKeeper.Tests/ -q`
- **Per wave merge:** `dotnet test TorchKeeper.Tests/ -q`
- **Phase gate:** Full suite green before marking phase complete

### Wave 0 Gaps
- [ ] `TorchKeeper.Tests/Export/MarkdownBuilderTests.cs` — add `talents` parameter to `MinimalData()` + two Talents section tests (mirrors existing Tests 12 and 13 for SpellsKnown)
- [ ] `TorchKeeper.Tests/Services/CharacterFileServiceTests.cs` — add `Talents = "Backstab +1"` to fixture + `Assert.Equal("Backstab +1", loaded.Talents)` assertion

## Sources

### Primary (HIGH confidence)
- Direct code inspection of commit 4239483 — verified all 8 changed files
- `TorchKeeper/Views/NotesPage.xaml` — current UI state confirmed
- `TorchKeeper.Core/Models/Character.cs` — Talents and SpellsKnown properties confirmed
- `TorchKeeper.Core/DTOs/CharacterSaveData.cs` — DTO fields confirmed
- `TorchKeeper.Core/Services/CharacterFileService.cs` — save/load wiring confirmed
- `TorchKeeper.Core/Export/MarkdownBuilder.cs` — Talents section rendering confirmed
- `TorchKeeper/ViewModels/CharacterViewModel.cs` — ViewModel property and build/load confirmed
- `TorchKeeper.Tests/Services/CharacterFileServiceTests.cs` — gap confirmed (no Talents assertion)
- `TorchKeeper.Tests/Export/MarkdownBuilderTests.cs` — gap confirmed (no Talents parameter or tests)
- `.planning/STATE.md` — inline implementation and pending todo documented

### Secondary (MEDIUM confidence)
- MAUI `AutoSize="TextChanges"` + `ScrollView` scroll-conflict avoidance: project's existing
  pattern works correctly; behavior confirmed by prior phase implementation

### Tertiary (LOW confidence)
- None

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — same stack throughout project, no new dependencies
- Architecture: HIGH — implementation is already shipped and verified against success criteria
- Pitfalls: HIGH — gaps identified by direct code inspection, not inference
- Test gaps: HIGH — confirmed by grepping test files for "Talents"

**Research date:** 2026-03-22
**Valid until:** 2026-04-22 (stable domain, no external dependencies)

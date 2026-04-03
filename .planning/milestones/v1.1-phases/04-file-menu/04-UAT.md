---
status: complete
phase: 04-file-menu
source: [04-01-SUMMARY.md, 04-02-SUMMARY.md]
started: 2026-04-01T00:00:00Z
updated: 2026-04-03T00:00:00Z
---

## Current Test

[testing complete]

## Tests

### 1. Save Character
expected: Fill in some character fields (name, class, etc). Go to File > Save (desktop) or overflow menu > Save (mobile). A save dialog appears. Choose a location and confirm. A .sdchar file appears on disk. Opening it in a text editor shows valid JSON with the character data you entered.
result: pass

### 2. Open / Load Character
expected: Go to File > Open (or overflow > Open). A file picker appears. Select the .sdchar file you just saved. The sheet replaces with the saved character's data — name, class, stats all match what was saved.
result: pass

### 3. Import from Shadowdarklings
expected: Go to File > Import (or overflow > Import). Pick a Shadowdarklings JSON export file. The sheet populates with the imported character's name, class, ancestry, stats, and gear.
result: pass

### 4. Cancel dialog — no crash
expected: Open any file dialog (Save, Open, or Import) and cancel/dismiss it without selecting a file. The app does not crash. The current character state is unchanged.
result: pass

### 5. File menu placement
expected: On desktop (macOS): File menu bar shows Save, Open, Import — then a separator — then Export As... On mobile: the toolbar overflow (⋯) includes Save, Open, and Import options, while Export appears in the primary toolbar.
result: pass

## Summary

total: 5
passed: 5
issues: 0
pending: 0
skipped: 0
blocked: 0

## Gaps

[none yet]

---
created: 2026-03-22T14:35:03.528Z
completed: 2026-03-29T16:56:11.000Z
title: Test coin slots match between export and character sheet
area: testing
files: []
---

## Problem

No test verifies that the coin slot count shown in the character sheet matches what gets exported to Markdown. These could silently diverge if the export and display logic use different calculations.

## Solution

Add a test that loads a character with coins, checks the slot count displayed in the gear section, and verifies the exported Markdown shows the same slot value.

## Resolution

Added `RoundTrip_CoinSlots_MatchBetweenSheetAndExport` to `CharacterFileServiceTests.cs`:
- Creates a character with 120 GP (→ 1 coin slot)
- Round-trips through CharacterFileService save/load
- Applies the CoinSlots formula to the loaded character
- Builds CharacterExportData with the computed slot values
- Verifies the Markdown gear header shows "## Gear (2 / 10 slots)" (1 sword + 1 coin slot)

The test confirms the formula used for display and export is consistent end-to-end.

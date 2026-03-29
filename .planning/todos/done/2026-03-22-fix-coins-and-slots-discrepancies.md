---
created: 2026-03-22T14:35:03.528Z
completed: 2026-03-29T16:56:11.000Z
title: Fix coins and slots discrepancies
area: ui
files: []
---

## Problem

There are discrepancies in how coins and slots are handled/displayed in the character sheet. The exact nature needs investigation — likely mismatches between display, storage, or calculation of coin weight slots vs actual gear slots.

## Solution

TBD — investigate discrepancies between coin display and slot counting, align with Shadowdark rules.

## Resolution

Root cause: Shadowdarklings JSON includes a phantom "Coins" gear item (`gearId: "coins"`, `slots: 1`) to represent coin weight. Our app independently computes `CoinSlots` from the GP/SP/CP currency fields. Importing the "Coins" gear item was double-counting coin weight.

**Fix applied in Core and MAUI `ShadowdarklingsImportService`:** gear items with `gearId == "coins"` are now skipped during import. Coin weight is always computed from GP/SP/CP using our formula (first 100 coins free, each additional 100 = 1 slot).

Verified against Brim.json: our computed gear slots used (8) now matches Shadowdarklings' `gearSlotsUsed: 8`.

Tests added: `Import_CoinsGearItem_IsSkipped`.

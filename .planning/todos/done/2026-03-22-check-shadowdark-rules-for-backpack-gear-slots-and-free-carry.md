---
created: 2026-03-22T14:35:03.528Z
completed: 2026-03-29T16:56:11.000Z
title: Check Shadowdark rules for backpack gear slots and free-carry items
area: general
files:
  - /Users/daren/Downloads/Shadowdark_RPG_-_V4-9.pdf
  - /Users/daren/Downloads/Brim Qu'arkh.pdf
---

## Problem

Unclear whether the Backpack counts as a gear slot or not. The Shadowdarklings character sheet PDF (Brim Qu'arkh.pdf) appears to have a "free to carry" category that includes: Backpack, Thieves Tools, and Bag of Coins. Need to verify against the official Shadowdark RPG rules (V4-9.pdf) whether these items should be treated as free/slotless.

## Solution

1. Read the gear/encumbrance section of Shadowdark_RPG_-_V4-9.pdf to find the official ruling on backpack slots.
2. Compare with the Shadowdarklings format — confirm the "free to carry" category and which items qualify.
3. Update the app's slot calculation logic and display accordingly.

## Resolution

Confirmed via Shadowdarklings JSON export format: items with `slots: 0` are free-carry items (Backpack across all 6 example characters has `slots: 0`).

**Fix applied in Core and MAUI `ShadowdarklingsImportService`:** items with `g.Slots == 0` are imported with `IsFreeCarry = true`, placing them in the Free Carry section and excluding them from GearSlotsUsed.

Tests added: `Import_GearItemWithSlots0_SetsFreeCarry`.

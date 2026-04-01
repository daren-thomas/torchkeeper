---
created: 2026-04-01T07:33:24.239Z
title: Fix coin slot calculation for mixed denominations
area: general
files: []
---

## Problem

Coin slot calculation is wrong when using mixed coin denominations. The slot only increments when the total coin count reaches >= 200, but it should convert all denominations to a common unit first, then compute slots.

**Repro:** Set coins to 1 gp and 100 sp.
- **Expected:** 1 coin slot used (for the excess coins). Coin entry should appear in gear list as "Coins".
- **Found:** 0 coin slots used. The slot only jumps when the raw total count of all coins >= 200, ignoring denomination conversion (e.g., 1 gp = 10 sp = 100 cp).

## Solution

Convert all coin denominations to a base unit (e.g., copper pieces) before calculating total coin value, then divide by the per-slot threshold (200 cp = 1 slot). Also ensure a "Coins" entry appears in the gear list whenever any coins are carried.

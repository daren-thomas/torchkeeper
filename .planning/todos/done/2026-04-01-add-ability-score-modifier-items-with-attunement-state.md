---
created: 2026-04-01T07:33:24.239Z
title: Add ability score modifier items with attunement state
area: ui
files: []
---

## Problem

The ability score drill-down list has no way to add custom modifier sources (e.g. magic items, racial traits, temporary effects). Users need to track *why* a score has a particular modifier, and whether that modifier is currently active.

## Solution

Add an "add item" action to the ability score drill-down panel. Each item should have:
- A numeric modifier value (e.g. `+1`, `-2`)
- A description/label (e.g. "Dwarven Belt of Strength")
- A checkbox state to express the item's current status — options like "Attuned", "Wearing", "Currently in effect"
- A remove/delete option

Only modifiers whose checkbox state is active should contribute to the final ability score total.

---
created: 2026-03-22T00:00:00.000Z
title: Check in with Talents and Shadowdarklings export format — should spells appear in Talents area?
area: general
files: []
---

## Problem

The Talents/Spells area on the Notes tab is a free-text field. When importing from Shadowdarklings JSON, spells (SpellsKnown) may populate in other locations on the sheet already. It's unclear whether the import pipeline should also populate the Talents/Spells free-text area from the Shadowdarklings export, and if so what data should go there.

## Questions to answer

1. Does the Shadowdarklings JSON export contain a spells/talents list? If so, where (SpellsKnown, talents table, etc.)?
2. What does the current import do with that data — does it silently drop it or map it somewhere else?
3. Should the import populate the Talents/Spells free-text area as a formatted string? If so, what format?
4. Is the free-text area intended to be import-populated or always manually entered?

## Solution

1. Review `examples/Brim.json` and ShadowdarklingsImportService to see what spell/talent data exists and where it goes today.
2. Decide: import-populate the Talents field from Shadowdarklings spells/talents, or leave as manual-only.
3. If import-populate: update ShadowdarklingsImportService and CharacterSaveData mapping; add test coverage.

---
created: 2026-03-22T14:35:03.528Z
title: Test coin slots match between export and character sheet
area: testing
files: []
---

## Problem

No test verifies that the coin slot count shown in the character sheet matches what gets exported to Markdown. These could silently diverge if the export and display logic use different calculations.

## Solution

Add a test that loads a character with coins, checks the slot count displayed in the gear section, and verifies the exported Markdown shows the same slot value.

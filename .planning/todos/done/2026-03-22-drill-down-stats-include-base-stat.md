---
created: 2026-03-22T14:35:03.528Z
completed: 2026-03-29T16:56:11.000Z
title: Drill down for stats — include base stat in drill down
area: ui
files: []
---

## Problem

The stat drill-down view (tapping a stat to see bonus breakdown) does not currently show the base stat value — only the modifier/bonus. Users need to see the base stat (e.g. STR 14) as well as the derived modifier (+2).

## Solution

Update the stat drill-down popup/view to include the raw base stat value alongside the modifier and any bonus sources.

## Resolution

Already implemented in SheetPage.xaml (line 193):
```xml
<Label Text="{Binding BaseStat, StringFormat='Base: {0}'}" FontSize="13" TextColor="Gray" />
```
The expanded stat row shows "Base: 14" above the bonus sources list.

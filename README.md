# Strategy
From a Udemy course: [The Ultimate Guide to Making a 2D Strategy Game in Unity](https://www.udemy.com/course/the-ultimate-guide-to-making-a-2d-strategy-game-in-unity/)

This project has a *lot* of sprite assets, because when I started this, I wasn't sure which ones I would be using.

Features I've added/revised:
- Vehicle combat theme in place of tutorial's fantasy/magic theme.
- 8 unit types with diverse stats and abilities.
- Place-then-buy mechanism (store dialog) for adding units, in place of the tutorial's buy-then-place mechanism (barracks menu).
- Additional input support:
  - WASD/Arrow keys (or Mouse) to move selection indicator.
  - Tab/Shift-Tab or MouseWheel to select next/previous unit.
  - Space to select, Enter to end turn.
- A roads system that allows for bonus movement.
- Scattered rubble, and units (e.g. Trucks) that can collect it.
- Revised attack and damage system:
  - Damage split into melee and ranged types.
  - Ranged counter-attacks do less damage if out of range.

Features I plan to implement:
- [ ] Let Dozers run into obstacles to convert them into rubble.
- [ ] Let Dozers create dirt roads on bare terrain.

Ideas for future improvements:
- [ ] Use Unity's built-in Tilemap System and Rule Tiles.
- [ ] Generate initial roads and terrain procedurally?

Graphics: [KenneyNL](https://www.kenney.nl/assets)


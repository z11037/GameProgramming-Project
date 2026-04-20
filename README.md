# GameProgramming-Project
A simplified 2D grid-based word puzzle game, inspired by the classic game "Baba Is You". This project is a vertical slice for a game development course, focusing on core mechanics with minimal complexity.

## Game Overview
This is a minimalist word rule puzzle game where players manipulate word blocks to form sentences, which change the game's collision, movement, and victory/defeat rules. The game features a single built-in level and a simple custom level editor for extended play.

## Core Rules & Vocabulary
### Nouns (Game Objects)
- Baba: The main character/player avatar
- Wall: Obstacle object
- Flag: Goal object

### Connectors & Attributes
- Is: Connector word to form rules (format: "XX Is XX")
- You: Defines the controllable player object
- Win: Defines the object that triggers victory when touched
- Stop: Defines objects that block movement (impassable)
- Push: Defines objects that can be pushed by the player
- Defeat: Defines objects that trigger a level reset when touched

## Gameplay
1. Use keyboard arrow keys to move and push word blocks.
2. Form valid sentences (e.g., "Baba Is You", "Flag Is Win", "Wall Is Stop") to change the game rules.
3. Complete the level by touching the object marked as "Win".

## Project Scope
- 1 built-in test level (8x8 grid)
- Simple level editor (supports placing/deleting blocks, saving/loading custom levels)
- Developed with Unity 2D Core (no complex physics or animations)

## Development Plan
This project will be iteratively developed and updated on GitHub, with regular commits to show the development process.

## Assets & Copyright
All sprites used are simple, self-made pixel art (64x64px) for educational purposes. No copyrighted materials are used.

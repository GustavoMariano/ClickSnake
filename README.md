# ClickSnake

![Unity](https://img.shields.io/badge/Unity-6.5.10f1-000000?logo=unity&logoColor=white)
![C%23](https://img.shields.io/badge/C%23-Unity-512BD4?logo=csharp&logoColor=white)
![Status](https://img.shields.io/badge/status-initial%20scope%20complete-success)
![AI Assisted](https://img.shields.io/badge/development-AI%20assisted-blueviolet)

**ClickSnake** is a small 2D Unity game inspired by the classic Snake formula, with one major twist:

> **The snake does not move automatically.**

Instead, the player moves exactly one grid cell at a time using **WASD**.

This changes the traditional reflex-based Snake gameplay into a more deliberate positioning game where every move matters. The challenge comes from growing the snake, managing increasingly limited space, reacting to progressively shorter food timers, and avoiding situations where no valid move remains.

ClickSnake was created as a hands-on introduction to game development with **Unity and C#**, with a deliberately small scope that could be completed quickly and used to explore core game-development concepts.

---

## Gameplay

The objective is simple:

- Move around the board.
- Collect food.
- Increase your score.
- Grow the snake.
- Avoid trapping yourself.
- React before food expires.
- Take advantage of rare special food.
- Avoid bad food when it appears.
- Survive for as long as possible.

Unlike classic Snake, pressing a direction does not start continuous movement.

Each valid key press moves the snake by exactly **one cell**.

---

## Controls

| Key | Action |
| --- | --- |
| **W** | Move up |
| **A** | Move left |
| **S** | Move down |
| **D** | Move right |
| **Retry** | Restart the game after Game Over |

Invalid moves are ignored.

The snake cannot:

- leave the board;
- move directly into its own body.

---

## Core Mechanics

### Grid-Based Movement

The game uses a fixed **15 x 9 grid**.

Current logical limits:

```text
X: -7 to 7
Y: -4 to 4
```

The snake always occupies integer grid coordinates.

This keeps movement deterministic and makes it possible to handle:

- body movement;
- collisions;
- food spawning;
- Game Over detection;

without relying on Unity physics.

---

## Snake Movement

The snake moves only when the player presses a movement key.

Example:

```text
Initial:

BODY HEAD


Press D:

     BODY HEAD


Press W:

          HEAD
          BODY
```

Each body segment takes the previous position of the segment in front of it.

---

## Snake Growth

The snake starts with:

```text
Head + 1 body segment
```

Collecting good food increases the snake's length.

Growth uses a **pending growth system** instead of spawning multiple segments immediately in the same position.

This allows the game to safely support arbitrary growth values such as:

```csharp
Grow(1);
Grow(2);
```

without overlapping body segments.

Currently, both normal and special food increase the snake by **1 segment**.

---

## Collision Rules

The snake cannot move:

- outside the board;
- into a cell currently occupied by its own body.

Trying an invalid direction simply does nothing.

For example:

```text
BODY HEAD
```

Pressing `A` would attempt to move the head into the body, so the move is ignored.

---

## Game Over

Game Over does **not** happen simply because the player presses an invalid direction.

Instead, the game checks whether the snake still has at least one valid move.

After every valid turn, the game evaluates:

```text
Up
Down
Left
Right
```

If all four neighboring cells are blocked by either:

- the board boundary;
- the snake's body;

the game ends.

The Game Over screen displays:

```text
GAME OVER

SCORE: X

[ RETRY ]
```

The Retry button reloads the current scene and starts a completely new run.

---

# Food System

ClickSnake currently contains three gameplay food states:

- Normal Food
- Special Food
- Bad Food

---

## Normal Food

Normal food is the standard progression item.

| Property | Value |
| --- | ---: |
| Spawn chance | **95%** |
| Score | **+1** |
| Snake growth | **+1** |

After being collected, the food respawns at another valid position.

---

## Special Food

Special food has a **5% chance** of replacing the normal food during a spawn.

It uses the same food GameObject, but changes its visual color to yellow.

| Property | Value |
| --- | ---: |
| Spawn chance | **5%** |
| Score | **+2** |
| Snake growth | **+1** |

This gives the player a scoring bonus without making the snake grow faster than usual.

That distinction is intentional:

> The score represents points, while snake length represents difficulty.

---

## Bad Food

Bad Food introduces an additional risk later in the run.

It does not appear at the beginning of the game.

### Unlock Condition

Bad Food becomes eligible after:

```text
8 good foods collected
```

Good foods include both:

- normal food;
- special yellow food.

The game tracks this separately from score.

This is important because:

```text
Normal Food  = +1 score
Special Food = +2 score
Bad Food     = -2 score
```

Therefore, score cannot be used as a reliable count of how many good foods were collected.

### Spawn Chance

After the player has collected at least 8 good foods:

```text
Every successful good-food collection
          |
          v
50% chance
          |
          v
Bad Food appears
```

If the 50% roll fails, Bad Food remains hidden for that cycle.

Bad Food does not remain permanently active.

### Bad Food Penalty

Collecting Bad Food:

```text
Score: -2
Snake growth: 0
```

The snake does **not** become smaller.

This is intentional.

Reducing the snake's size after a mistake would make the game easier, which would partially reward the player for collecting the wrong item.

Instead, Bad Food:

- reduces score;
- keeps the current snake length;
- preserves the existing difficulty.

The score can never go below:

```text
0
```

### After Collecting Bad Food

When Bad Food is collected:

- score is reduced by 2;
- snake size does not change;
- good-food collection count does not change;
- good food respawns;
- Bad Food disappears.

Bad Food can only return after a future successful good-food collection passes the 50% spawn roll.

---

# Progressive Food Timer

Good Food has an invisible countdown timer.

The player does not see the timer directly.

The initial food time limit is:

```text
20 seconds
```

Every successfully collected good food reduces the time available for the next food by:

```text
0.5 seconds
```

The timer never becomes shorter than:

```text
3 seconds
```

## Timer Progression

Example:

```text
20.0s
19.5s
19.0s
18.5s
18.0s
...
3.0s
```

The time limit decreases only when the player successfully collects good food.

## Food Timeout

If the timer reaches zero before the player collects the food:

- score does not change;
- snake length does not change;
- good-food collection count does not change;
- the current time limit does not decrease;
- food moves to another valid position;
- food type is randomized again.

For example:

```text
Current limit: 12.5 seconds

Food expires
|
v
Food respawns
|
v
Next timer: still 12.5 seconds
```

The new food can reroll between:

```text
95% Normal
5% Special
```

A timeout does not reroll Bad Food.

---

# Safe Spawn System

Food placement respects all occupied cells.

Good Food cannot spawn on:

- the snake head;
- any snake body segment;
- active Bad Food.

Bad Food cannot spawn on:

- the snake head;
- any snake body segment;
- Good Food.

Therefore:

```text
GoodFoodPosition != BadFoodPosition
```

at all times.

Instead of repeatedly generating random coordinates until one works, the game first calculates the available cells and then chooses one randomly.

This avoids potentially inefficient retry loops as the board becomes crowded.

---

# Scoring

Score represents **points**, not the number of food items collected.

Current scoring rules:

| Item | Score | Growth |
| --- | ---: | ---: |
| Normal Food | **+1** | **+1** |
| Special Food | **+2** | **+1** |
| Bad Food | **-2** | **0** |

Score is always clamped to a minimum of:

```text
0
```

---

# Difficulty Progression

Difficulty increases naturally throughout a run.

### 1. Snake Growth

Every good food increases snake length.

A longer snake means:

- less free board space;
- more opportunities to block yourself;
- more difficult navigation.

### 2. Progressive Timer

The player starts with 20 seconds per food.

Eventually this reaches only 3 seconds.

This gradually changes the game from:

```text
careful planning
```

into:

```text
fast decision-making
```

### 3. Bad Food

After eight good foods, an additional hazard can appear.

The player must then consider both:

- where to go;
- what to avoid.

### 4. Self-Trapping

The final challenge is not hitting something.

It is avoiding a board state where:

```text
Up    = blocked
Down  = blocked
Left  = blocked
Right = blocked
```

Once that happens:

```text
GAME OVER
```

---

# Tech Stack

ClickSnake currently uses:

- **Unity 6.5.10f1**
- **C#**
- **Unity Input System**
- **TextMeshPro**
- **Visual Studio 2026**
- **Git**
- **GitHub**

---

# Architecture Overview

The project intentionally keeps the architecture relatively small.

It is a learning project, so the goal is to keep responsibilities understandable without introducing unnecessary enterprise-style abstractions.

## SnakeHeadController

`Assets/Scripts/Player/SnakeHeadController.cs`

The main gameplay coordinator.

Responsibilities currently include:

- reading WASD input;
- grid movement;
- movement validation;
- board boundaries;
- self-collision prevention;
- body movement;
- pending growth;
- food collection;
- bad-food collection;
- food timer;
- food respawning;
- Bad Food spawn rolls;
- occupied-position calculation;
- checking for available moves;
- triggering Game Over.

## GameManager

`Assets/Scripts/Game/GameManager.cs`

Controls run-level state.

Responsibilities include:

- current score;
- number of good foods collected;
- Game Over state;
- score UI;
- final score UI;
- score penalties;
- preventing negative scores;
- Retry behavior.

## FoodController

`Assets/Scripts/Food/FoodController.cs`

Controls Good Food.

Responsibilities include:

- current grid position;
- valid random placement;
- food type;
- score value;
- growth value;
- normal/special randomization;
- visual color switching.

Food types:

```csharp
FoodType.Normal
FoodType.Special
```

## BadFoodController

`Assets/Scripts/Food/BadFoodController.cs`

Represents Bad Food.

Responsibilities include:

- exposing its grid position;
- moving to a valid random position.

Game rules such as score penalties remain outside this component.

## GridPositionUtility

`Assets/Scripts/Food/GridPositionUtility.cs`

Shared utility for random grid placement.

It:

1. scans the board;
2. removes occupied positions;
3. builds a collection of available cells;
4. randomly chooses one valid position.

This logic is shared by Good Food and Bad Food.

---

# Project Structure

```text
ClickSnake/
|
├── Assets/
│   |
│   ├── Prefabs/
│   │   └── SnakeBody.prefab
│   |
│   ├── Scenes/
│   │   └── Game.unity
│   |
│   ├── Scripts/
│   │   |
│   │   ├── Food/
│   │   │   ├── BadFoodController.cs
│   │   │   ├── FoodController.cs
│   │   │   └── GridPositionUtility.cs
│   │   |
│   │   ├── Game/
│   │   │   └── GameManager.cs
│   │   |
│   │   └── Player/
│   │       └── SnakeHeadController.cs
│   |
│   ├── Settings/
│   └── TextMesh Pro/
|
├── ProjectSettings/
├── .gitattributes
├── .gitignore
├── .vsconfig
└── ClickSnake.slnx
```

---

# Running the Project

## Requirements

The project is currently developed with:

```text
Unity 6.5.10f1
```

Using the same Unity version is recommended.

For C# development, Visual Studio can be configured with the Unity / managed game development workload.

## Clone

```bash
git clone https://github.com/GustavoMariano/ClickSnake.git
cd ClickSnake
```

Then:

1. Open **Unity Hub**.
2. Select **Add project from disk**.
3. Select the `ClickSnake` folder.
4. Open the project using Unity 6.5.
5. Open:

```text
Assets/Scenes/Game.unity
```

6. Press **Play**.

---

# Project Status

**ClickSnake is considered complete for its original learning scope.**

The main gameplay loop and the mechanics planned for the initial version are implemented and playable.

The project may receive additional improvements in the future, but there is **no fixed roadmap or maintenance schedule**.

## Implemented

- [x] Grid-based manual movement
- [x] WASD input
- [x] Board boundaries
- [x] Body following
- [x] Snake growth
- [x] Pending growth system
- [x] Self-collision prevention
- [x] Safe random food spawning
- [x] Score system
- [x] Game Over detection
- [x] Final score display
- [x] Retry
- [x] Normal food
- [x] Special yellow food
- [x] Progressive food timer
- [x] Bad Food
- [x] Random Bad Food spawning
- [x] Progressive difficulty

## Possible Future Ideas

If I decide to revisit the project, some possible improvements include:

- custom snake and food artwork;
- improved board visuals;
- sound effects;
- food collection animations;
- score feedback effects;
- particles;
- UI improvements;
- further gameplay balancing;
- a packaged playable build.

These are ideas rather than commitments. The original project scope is already considered complete.

---

# AI-Assisted Development

ClickSnake is an intentionally small learning project developed using an **AI-assisted workflow**.

The game concept, gameplay rules, balancing decisions, Unity scene setup, manual testing, and project direction are driven by **Gustavo Mariano**.

Parts of the C# implementation, refactoring, development guidance, and documentation were produced with assistance from:

- **OpenAI Codex**
- **ChatGPT**

AI-generated changes were reviewed, integrated, tested, and adjusted during development.

The repository commit history explicitly identifies Codex-assisted implementation commits.

This project is also an experiment in understanding how AI coding tools can fit into a real iterative development workflow without replacing manual design, testing, and technical decision-making.

---

# Learning Goals

ClickSnake began as a small project to explore whether game development with C# and Unity would be enjoyable.

Despite its small scope, the project covers several fundamental game-development concepts:

- Unity scenes;
- GameObjects;
- MonoBehaviours;
- Components;
- serialized Inspector references;
- prefabs;
- runtime instantiation;
- game loops;
- frame updates;
- player input;
- grid-based movement;
- deterministic collision logic;
- body-following algorithms;
- state management;
- procedural/random spawning;
- UI with TextMeshPro;
- scoring;
- progressive difficulty;
- Game Over conditions;
- scene reloading;
- source control for Unity;
- iterative game design.

---

# Author

**Gustavo Mariano**

GitHub: [@GustavoMariano](https://github.com/GustavoMariano)

---

> ClickSnake started as a small experiment built around one question:
>
> **"Do I enjoy making games in C#?"**

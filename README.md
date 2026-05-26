# 🏰 Tower Defense — Unity Project

**👥Authors:** [Oliinyk Andrii](https://github.com/oliinykam) • [Nadiia Stelmakh](https://github.com/StelmakhNadiia) 

> Academic project. A 2D isometric Tower Defense game built with Unity, targeting WebGL.

🎮 **[Play the WebGL build](https://oliinikam.itch.io/unityproject)**

---

## About

A fully featured Tower Defense game with a round-based structure and two player roles: the **Defender** (builds towers) and the **Attacker** (sends enemy waves). Supports three game modes: PvE, Endless, and PvP Hot-Seat.

Built with an emphasis on clean architecture and WebGL performance:

- **Data-Driven Design** via ScriptableObjects (`TowerData`, `EnemyData`) — balance tuning without touching code
- **Object Pooling** for enemies, projectiles, and audio sources — critical for stable WebGL performance
- **State Machine** via C# events — decoupled game state management
- **Isometric Y-sorting** — automatic sprite depth sorting using a custom camera axis

---

## Requirements

| Requirement | Version |
| :--- | :--- |
| **Unity Editor** | **Unity 6.3 LTS** `6000.3.11f1` |
| **Unity Hub** | 3.0 or newer |
| **OS** | Windows 10/11, macOS 12+, Ubuntu 20.04+ |

> ⚠️ **Important:** The project was developed and tested on **Unity 6.3 LTS (6000.3.11f1)**. Opening it in older versions (e.g. 2022.x or 2021.x) is **not recommended** and may cause errors. Newer Unity 6 patch versions should be compatible.

The following packages install **automatically** when you first open the project via Unity Hub:

- **TextMeshPro** — UI text rendering
- **2D Tilemap** — map and path tile system
- **2D Sprite** — sprite slicing and atlasing

Don't have Unity yet? Download Unity Hub from [unity.com/download](https://unity.com/download), then install the **Unity 6.3 LTS** (`6000.3.11f1`) editor from the *Installs* tab.

---

## Getting Started

1. Clone the repository:
   ```
   git clone https://github.com/oliinykam/tower-defense.git
   ```
2. Open **Unity Hub** → **Projects** tab → click **Open**.
3. Point it to the project folder (the one containing `Assets/` and `ProjectSettings/`).
4. Wait for the import to finish (may take 1–2 minutes on first open).
5. Open the main menu scene: `Assets/Scenes/MainMenu`.
6. Press **▶ Play**.

---

## Building for WebGL

1. Open **File → Build Settings**.
2. Select **WebGL** → click **Switch Platform**.
3. Open **Player Settings** and verify:
   - **Company Name** and **Product Name** are filled in.
   - **Publishing Settings → Compression Format** is set to `Gzip` or `Disabled` (required for GitHub Pages).
4. Click **Build** and choose an output folder (e.g. `Build/WebGL`).

### Running the build locally

Do **not** open `index.html` directly from the filesystem — browsers block this due to CORS. Use one of the following instead:

- **VS Code:** install the *Live Server* extension, right-click `index.html` → *Open with Live Server*.
- **Python:** run `python -m http.server 8080` in the build folder, then open `http://localhost:8080`.

---

## Project Structure

```
Assets/
├── Scenes/                  # MainMenu, Map (game scene)
├── Scripts/                 # All C# scripts
│   ├── GameManager.cs       # State machine, global events
│   ├── WaveManager.cs       # Wave generation, AI, enemy pool
│   ├── Tower.cs             # Targeting, shooting, freeze aura
│   ├── Enemy.cs             # Pathfinding (waypoints), slow stacking
│   ├── TowerPlacer.cs       # Ghost preview, tile validation, placement
│   ├── AttackerManager.cs   # PvP budget, queue, UI switching
│   ├── AudioManager.cs      # Audio source pool, BGM, anti-duplicate
│   └── ...
├── Prefabs/                 # Tower, Enemy, Projectile prefabs
├── ScriptableObjects/       # TowerData and EnemyData assets
├── Tilemaps/                # Map tiles and path tiles
├── Audio/                   # SFX and background music
└── Sprites/                 # All visual assets
```

---

## Key Scripts

| Script | Responsibility |
| :--- | :--- |
| `GameManager.cs` | Singleton. Manages `GameState` enum and publishes `OnGameStateChanged` event. |
| `GameUIManager.cs` | Subscribes to state events. Controls UI panels and time scale (1x / 2x / 4x). |
| `WaveManager.cs` | AI wave composition, enemy spawning, object pool, win/lose check. |
| `Tower.cs` | Target selection, shooting, freeze aura, range indicator. |
| `Enemy.cs` | Waypoint movement, damage handling, slow effect with diminishing returns. |
| `TowerPlacer.cs` | Ghost tower preview, tile validation, tower placement and refund. |
| `AttackerManager.cs` | PvP: attacker budget, enemy queue, defender UI hiding. |
| `AudioManager.cs` | AudioSource pool (15–30 sources), anti-duplicate guard, background music. |
| `PvPBuyButton.cs` | Shop button: LMB add, RMB remove, Shift for ×10. |

---

## Assets & Credits

| Category | Source |
| :--- | :--- |
| Sprites (tiles, towers, enemies, UI) | AI-generated (DALL-E / Midjourney) |
| Sound effects | freesound.org (CC0) |
| Background music | OpenGameArt.org (CC0) |

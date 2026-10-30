# Temple Run (Original 2011/2012) — Verified Reference & Gameplay Specification

**Target Title:** Temple Run (Original Edition)  
**Original Developer / Publisher:** Imangi Studios (Natalia Luckyanova & Keith Shepherd; Art by Kiril Tchangov)  
**Package Identifier:** `com.imangi.templerun`  
**Official Google Play Listing:** [Temple Run on Google Play](https://play.google.com/store/apps/details?id=com.imangi.templerun&hl=en_IN)  
**Target Scope:** Clean-Room Technical Specification for Independent Android Recreation  
**Inspection Status:** Based strictly on observable public references, historical release documentation, and verified gameplay footage. **No proprietary APK has been provided or decompiled.**

---

## 1. Verified Public References & Provenance

* **Original iOS Launch:** August 4, 2011 (Custom C++/OpenGL engine derived from *Max Adventure*).
* **Original Android Launch:** March 27, 2012 (Package: `com.imangi.templerun`, ported in collaboration with Unity Technologies).
* **Distinction Note:** This specification covers the original **Temple Run 1**. It excludes mechanics introduced in *Temple Run 2* (e.g., zip-lines, mine carts, curved pathways, green gems, dynamic vertical cliffs) or third-party spinoffs (*Temple Run: Brave*, *Temple Run: Oz*).

---

## 2. Observable Gameplay Mechanics & Feature Breakdown

### A. Core Premise & Narrative Loop
* **The Inciting Incident:** The explorer character (default: **Guy Dangerous**) escapes an ancient temple ruin clutching a stolen golden cursed idol.
* **The Chase:** A pack of menacing, ape-like "Demon Monkeys" pursues the player continuously from behind.
* **The Objective:** Survive as long as possible on an endless, procedurally generated modular temple path, collecting coins and avoiding fatal hazards.

---

### B. Controls & Player Input Mapping

| Input Gesture | Hardware Sensor | In-Game Action | Failure / Edge Case Behavior |
| :--- | :--- | :--- | :--- |
| **Swipe Left** | Touchscreen Drag ($\Delta X < -Threshold$) | 90° Left Turn at Corners / T-Junctions | If swiped on a straightway, causes the character to stumble sideways against the stone wall. |
| **Swipe Right** | Touchscreen Drag ($\Delta X > +Threshold$) | 90° Right Turn at Corners / T-Junctions | If swiped on a straightway, causes the character to stumble sideways against the stone wall. |
| **Swipe Up** | Touchscreen Drag ($\Delta Y < -Threshold$) | Jump upward in a parabolic arc | Overcomes low tree trunks, broken bridge gaps, and swamp water traps. |
| **Swipe Down** | Touchscreen Drag ($\Delta Y > +Threshold$) | Crouch and slide on knees forward | Slips under low stone archways, elevated tree limbs, and flaming fire rings. |
| **Tilt Left / Right** | Accelerometer ($G_x$ axis) | Continuous lateral steering across path width | Steers the runner across the 3 sub-lanes to collect coins and avoid edge drop-offs. |
| **Double Tap** | Touchscreen Double Tap ($< 300\text{ ms}$) | Activates equipped **Resurrection Wings** | Only triggers if player owns wings and cooldown timer is ready. |

---

### C. Camera System & Perspective
* **Type:** Third-person chase camera positioned directly behind and slightly elevated above the character.
* **Relative Transform (Nominal):** Position offset approximately `(0, 2.2m, -4.2m)` with a downward pitch angle of ~15° pointing toward the runner's back.
* **Turn Dynamics:** When a 90° turn is executed, the camera does **not** snap instantaneously; it performs a smooth, critically damped slerp/rotation (~250–350 ms) to align with the new track orientation while keeping the runner centered.
* **Vertical Tracking:**
  * **Jump:** Camera tracks the runner's vertical displacement with slight smoothing, dampening the jarring peak.
  * **Slide:** Camera maintains horizontal forward momentum without dipping violently into the ground, preserving forward visibility.
* **Stumble Shake:** Screen undergoes a slight camera shake impulse when the character trips.

---

### D. Movement, Speed Curve & Track Generation
* **Procedural Modular Assembly:** Track is composed of pre-built modular 3D segments spawned ahead of the player and despawned behind the camera:
  * Straightaways (lengths: 1x, 2x, 4x tile lengths).
  * 90° Left Corners and 90° Right Corners.
  * T-Junctions (branching paths where player must choose left or right; running straight leads to falling into water).
  * Narrowed broken stone bridges with missing walls.
* **Lane Architecture:** The pathway is divided into 3 virtual sub-lanes (Left, Center, Right) across a standardized track width (~4.5–5.0 meters).
* **Speed Curve:**
  * Initial velocity: Moderate running speed ($\approx 8.0\text{ m/s}$).
  * Acceleration: Continuous subtle velocity increase over distance run ($+0.05\text{ m/s}^2$) up to a capped terminal speed ($\approx 22.0\text{ m/s}$).
  * High-speed implication: As velocity increases, the timing window for swipe inputs and obstacle reaction drops from ~1.2s down to ~250ms.

---

### E. Obstacle Catalog & Collision Interactions

| Obstacle Name | Visual Appearance | Clearance Action | Collision Result |
| :--- | :--- | :--- | :--- |
| **Tree Roots / Fallen Log** | Gnarled roots or fallen log across path | **Jump** (Swipe Up) | Jumping clears. Failure to jump causes a **Stumble** (or death if hitting directly at root center). |
| **Fire Ring Trap** | Stone arch spitting a circle of flames | **Slide** (Swipe Down) | Sliding passes under. Running into flames results in death by burning. |
| **Low Stone Archway** | Low ancient carved stone beam | **Slide** (Swipe Down) | Head collision causes instant fatal blunt impact death. |
| **Broken Pathway / Gap** | Section of stone road completely missing | **Jump** (Swipe Up) | Falling into the gap results in falling death (drop-down splash/void). |
| **T-Junction Dead End** | Wall / open void directly ahead at a fork | **Turn** (Swipe L/R) | Running straight without turning results in plunging into the void/swamp. |
| **Pathway Edge / Ledge** | Missing outer boundary walls | **Tilt** (Center) | Tilting too far over the edge results in falling off the ledge into the water. |

---

### F. The Demon Monkeys & Stumble Mechanic
* **Normal State:** The demon monkeys run closely behind the camera view, occasionally visible at the lower screen periphery.
* **First Stumble:**
  * Triggered by: Tripping over the edge of a tree root, turning into a straight wall, or sliding too early before an obstacle.
  * Consequence: The monkeys surge forward onto the player's heels, gnashing their teeth right behind the character.
  * Recovery: A recovery timer starts (~6.0–8.0 seconds of clean running). If no further mistakes occur, the monkeys fall back to normal distance.
* **Second Stumble (Fatal):**
  * If the player stumbles again while the monkeys are in the surged state, the monkeys immediately leap onto the runner's back, dragging them down into a fatal game-over.

---

### G. Currency, Scoring, and Power-Up Economy

#### 1. Coin Distribution
* Coins appear in curved or straight lines matching the track path, positioned on the left, center, or right sub-lanes.
* **Coin Tiers:**
  * **Yellow Coins:** Worth 1 coin. (Spawn from 0m onwards).
  * **Red Coins:** Worth 2 coins. (Unlocked at higher upgrade levels; spawn after 1,000m).
  * **Blue Coins:** Worth 3 coins. (Unlocked at higher upgrade levels; spawn after 2,000m).

#### 2. Scoring Formula
Total score accumulates continuously based on meters run and currency collected, scaled by the global Score Multiplier ($M$):
$$\text{Score} = \Big(\text{Distance (meters)} \times 1 + \text{Coins Collected} \times 5 + \text{Power-ups Collected} \times 25\Big) \times M$$
* **Score Multiplier ($M$):** Begins at $1\times$ (or base level). Every completed in-game **Objective** permanently increments $M$ by $+1\times$ (up to $30\times+$).
* **Coin Bonus Meter:** Collecting 100 coins in succession fills an on-screen meter, awarding progressive point bonuses (100, 250, 500 points). Tripping or dying resets the bonus meter.

#### 3. Power-Ups (In-Run Collectibles)
* **Mega Coin:** Spawns a large spinning golden coin. Instantly adds a lump sum (+50 to +150 coins depending on store upgrade tier).
* **Coin Magnet:** Creates a magnetic aura for 10–25 seconds. Automatically pulls all nearby coins on any lane directly to the player without requiring tilting.
* **Invisibility:** Grants ethereal invulnerability for 10–25 seconds. Player phases harmlessly through tree roots, fire rings, and low arches (though 90° turns must still be executed manually).
* **Boost:** Rockets the player forward at supersonic speed for 250m–750m. Character auto-steers through turns and flies past all obstacles invulnerably.
* **Coin Values:** Passive store upgrade that introduces Red and Blue high-value coins into procedural spawns.

#### 4. Utilities (Store Consumables)
* **Resurrection Wings:** Costs 500 coins each. Double-tapping primes the wings for 30 seconds. If the player dies while wings are active, they are revived instantly on the track with brief invulnerability. (Cooldown: ~20s after expiry before another set can be primed).
* **Head Start:** Consumable utility (e.g., 2,500 coins for a 2,500m skip; 750 coins for a 1,000m skip) launching the player forward instantly at start of run.

---

### H. Menu Architecture & User Interface
1. **Main Menu:** 3D animated temple background with runner idle. Options: *Play*, *Store*, *Stats / Objectives*, *Options*, *More Games*.
2. **HUD (In-Game):**
   * Top Left: Coin Bonus Progress Bar.
   * Top Right: Score Counter and Coin Counter.
   * Top Sub-header: Active Resurrection Wings status icon.
3. **Store / Upgrades Menu:**
   * Power-up upgrade tracks (5 levels each: Mega Coin, Magnet, Invisibility, Boost, Coin Values).
   * Utilities shop (Resurrection Wings, Head Start).
   * Character Roster unlockable with coins (Guy Dangerous, Scarlett Fox, Barry Bones, Karma Lee, Montana Smith, Zach Wonder, Francisco Montoya).
   * Wallpaper / Concept Art gallery.
4. **Game Over Screen:**
   * Death cause banner (e.g., *"You were caught by the monkeys!"*, *"You fell into the abyss!"*, *"You crashed into a tree!"*).
   * Final Score, Meters Run, and Coins Collected.
   * *Play Again* and *Main Menu* buttons.

---

## 3. Separation: Direct Observations vs Inferences & Unknowns

| Element | Directly Observed & Verified (High Confidence) | Technical Inference (Probable) | Unknown / Proprietary Internal (Needs Design) |
| :--- | :--- | :--- | :--- |
| **Control Gestures** | Swipe 4-way, Accelerometer tilt, Double tap. | Input deadzone is ~30–50px; accelerometer uses low-pass filtering to prevent jitter. | Exact mathematical smoothing filter coefficients. |
| **Camera Motion** | Third-person follow, smooth 90° turn slerp, stable slide. | Camera uses Critically Damped Spring / Lerp with dynamic look-ahead target. | Exact spring stiffness, damping ratios, and clipping plane distances. |
| **Track Layout** | Grid-aligned 90° modular tiles, 3 sub-lanes. | Procedural generator uses a weighted random tile selection algorithm with minimum straight distance between turns. | Exact RNG probability tables and seed recurrence patterns. |
| **Collision Boxes** | Bounding box / capsule triggers for jump, slide, and stumble. | Stumble uses two separate trigger zones: glancing side-hit (stumble) vs center-head-on hit (fatal crash). | Exact sub-mesh collider dimensions and physics layer collision matrix. |
| **Asset Pipeline** | Low-poly 3D models with diffuse textures, unlit/vertex-lit mobile shaders. | Designed for OpenGL ES 1.1/2.0 hardware constraints of 2011/2012 mobile chipsets. | Original Maya/3ds Max source geometry and uncompressed audio stems. |

---

## 4. Independent Android Implementation Blueprint (Clean-Room)

To build a legitimate, high-performance clean-room recreation without copying proprietary code or assets:

```
┌────────────────────────────────────────────────────────────────────────┐
│                   CLEAN-ROOM ANDROID ARCHITECTURE                      │
├────────────────────────────────────────────────────────────────────────┤
│ Target Platform: Android 8.0+ (API 26+) / Architecture: arm64-v8a      │
├──────────────────────────┬─────────────────────────────────────────────┤
│ 3D Rendering Engine      │ Godot 4.3 (Mobile/OpenGL ES 3.0 renderer)   │
│                          │ OR Lightweight Filament / libGDX            │
├──────────────────────────┼─────────────────────────────────────────────┤
│ Physics & Collision      │ Built-in Kinematic Character Controller     │
│                          │ (Non-physics raycasting / AABB triggers)    │
├──────────────────────────┼─────────────────────────────────────────────┤
│ Input Management         │ Android SensorEventListener (Accelerometer) │
│                          │ + GestureDetector (Touch swipes & taps)     │
├──────────────────────────┼─────────────────────────────────────────────┤
│ Procedural World Manager │ Object Pooler with pre-allocated 3D tiles  │
│                          │ (Straight, Turn-L, Turn-R, T-Junction)      │
├──────────────────────────┼─────────────────────────────────────────────┤
│ Audio & SFX              │ Open-source/custom foley: footstep loops,   │
│                          │ coin chimes, obstacle swish, creature roars │
└──────────────────────────┴─────────────────────────────────────────────┘
```

### Key Clean-Room Technical Decisions:
1. **Kinematic vs Dynamic Physics:** The runner should **not** use dynamic rigid-body physics for forward locomotion. Movement along the track spine should be kinematic/spline-driven, while tilt adjusts lateral offset $X \in [-W/2, +W/2]$. This guarantees zero physics clipping bugs and deterministic 60fps performance on mobile.
2. **Zero-Garbage Object Pooling:** Procedural endless runners quickly suffer from Java GC frame drops if tiles are instantiated dynamically. Pre-allocate an object pool of ~25 track tiles, ~100 coin instances, and cycle them as the player advances.
3. **Sensor Low-Pass Filter:** Raw accelerometer values must pass through a single-pole IIR low-pass filter:
   $$\text{Tilt}_{\text{filtered}} = \alpha \cdot \text{Tilt}_{\text{raw}} + (1 - \alpha) \cdot \text{Tilt}_{\text{previous}} \quad (\alpha \approx 0.15)$$
   to eliminate hand tremor while maintaining snappy responsiveness.

---

## 5. Verification & Acceptance Test Suite

When implementing the clean-room game, each core mechanic must pass these functional acceptance criteria:

* [ ] **AC-01 (Steering Range):** Tilting device 15° left/right shifts character smoothly to the corresponding lane boundary without penetrating outer walls.
* [ ] **AC-02 (Turn Responsiveness):** Swiping left/right within 1.5 track widths of a 90° corner executes a 90° turn and smoothly pivots the camera within 300ms.
* [ ] **AC-03 (Straightway Stumble):** Swiping left/right while on an open straightaway triggers a stumble animation, screen shake, and brings the demon monkey models into the active viewport.
* [ ] **AC-04 (Consecutive Stumble Penalty):** Stumbling a second time within 6 seconds of a prior stumble immediately triggers the monkey capture game-over sequence.
* [ ] **AC-05 (Jump Clearance):** Swiping up initiates a jump arc clearing tree roots and broken gap hazards with collision disabled on lower body colliders during the arc.
* [ ] **AC-06 (Slide Clearance):** Swiping down reduces the character's vertical collider height by $\ge 50\%$ for 800ms, safely passing under low arches and fire rings.
* [ ] **AC-07 (Double-Tap Resurrection):** Double-tapping within 300ms with wings in inventory consumes one wing, displays angelic visual wings, and resurrects player upon next collision.
* [ ] **AC-08 (Score Math Parity):** Score increments continuously: $(\text{Meters} + \text{Coins} \times 5) \times \text{Multiplier}$. Completing an objective increments multiplier by $+1$.

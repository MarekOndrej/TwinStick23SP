# Audio in TwinStick23SP

The game uses a single `AudioManager` (`Assets/Scripts/AudioManager.cs`) that
subscribes to the global event bus and plays a sound for each gameplay event:

| Event | Plays |
|---|---|
| `onGunFired` | gunshot SFX |
| `onEnemyDefeated` | enemy death SFX |
| `onPlayerHealthChanged` (on decrease) | player damage SFX |
| `onGameOver` | game-over SFX |

Plus an optional looping music track.

## Procedural fallback (what you hear out of the box)

Every clip slot in `AudioManager` is optional. If you leave a slot empty, the
`AudioManager` generates a placeholder sound in code:

- **Gunshot** → short white-noise burst with sharp decay (`MakeNoiseBurst`).
- **Enemy death / player damage / game over** → descending tone with quick
  attack and exponential decay (`MakeDescendingTone`), each at different
  pitch and duration so they're distinguishable.

These are intentionally basic — they exist so the game is audible the first
time you press Play, not as final-quality assets.

## Swapping in real audio

### Step 1 — Get free audio
Kenney's free packs are the easiest starting point (CC0, no attribution
required):

- Shooter SFX: https://kenney.nl/assets/sci-fi-sounds (laser/zap noises good for the gun)
- Impact SFX: https://kenney.nl/assets/impact-sounds (enemy hits/deaths)
- UI / hurt sounds: https://kenney.nl/assets/ui-audio
- Music: https://opengameart.org (filter by license: CC0)

Or use anything you have rights to.

### Step 2 — Import into Unity
1. Drop the `.wav` / `.mp3` / `.ogg` files into `Assets/Audio/` (create the
   folder if it doesn't exist).
2. Unity auto-imports them. With one file selected, the Inspector shows
   import settings — defaults are fine for short SFX. For music, set
   **Load Type** to **Streaming** to save memory.

### Step 3 — Wire into AudioManager
1. In SampleScene's Hierarchy, select the **AudioManager** GameObject.
2. In the Inspector, find the **Audio Manager (Script)** component.
3. Each slot (Gun Fire Clip, Enemy Death Clip, Player Damage Clip, Game Over
   Clip, Music Clip) accepts an AudioClip via drag-and-drop.
4. Drag your imported audio files into the matching slots.
5. Save the scene (Ctrl+S).

Press Play and you should hear your real audio instead of the procedural
fallbacks.

### Step 4 — Tune volumes
Two sliders in the same component:
- `Sfx Volume` — applied to every PlayOneShot call (gunfire, hits, etc.).
- `Music Volume` — applied to the music AudioSource.

Both are 0–1. The default 0.6 / 0.25 mix keeps SFX punchy without burying
music.

## Architecture notes

- `AudioManager` lives in `SampleScene` only (not MainMenu — yet). When the
  scene reloads on Restart, a fresh `AudioManager` is created and resubscribes.
- It is a regular `MonoBehaviour`, not a singleton — the per-scene scope keeps
  event subscriptions clean.
- The `_lastPlayerHealth = -1f` sentinel is what stops the damage sound from
  firing on the very first `onPlayerHealthChanged` event (which is the
  `Damageable.Start` "I'm at full health" notification, not actual damage).
- Damage sound is also suppressed if the hit also triggers game-over —
  otherwise both the damage and game-over sounds would stack on the killing
  blow.

## Future improvements

- Per-event volume (so the game-over sting can be louder than the per-frame
  hit sound).
- AudioMixer with separate Master / Music / SFX groups (lets the player tweak
  mix from a Settings menu).
- 3D positional audio for projectile impacts (set `spatialBlend: 1` on the
  source).
- Music for MainMenu scene (separate AudioManager component).

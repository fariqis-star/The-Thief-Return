# The Thief Return

A 2D side-scrolling run-and-gun action shooter built in **Unity 6**.

## Scripts

All gameplay code lives under `Assets/Scripts/`.

### Player (`Assets/Scripts/Player/`)
- **PlayerInputReader.cs** — reads input (works with the new Input System or the legacy Input Manager). Bindings: `A`/`←` left, `D`/`→` right, `Space` jump, `Left Shift` dash, `J` shoot.
- **PlayerMotor.cs** — owns the Rigidbody2D: horizontal movement, jump (with coyote time), applies the dash.
- **PlayerFacing.cs** — two-direction facing (LEFT/RIGHT) and sprite flip.
- **PlayerDash.cs** — short horizontal dash with cooldown.
- **PlayerShooter.cs** — fires the currently equipped weapon in the facing direction.
- **GroundCheck.cs** — grounded detection via an overlap circle against a ground LayerMask.

### Combat (`Assets/Scripts/Combat/`)
- **PlayerWeapon.cs** — placeholder pistol; spawns bullets. Exposes fire rate / bullet speed.
- **Bullet.cs** — reusable horizontal projectile. `Hit Layers` controls what it can hit.

### Enemy (`Assets/Scripts/Enemy/`)
- **Enemy.cs** — self-contained enemy: patrols back and forth, stops and shoots the player when in detection range, resumes patrol when the player leaves.

## Controls

| Action | Key |
|--------|-----|
| Move left | `A` / `Left Arrow` |
| Move right | `D` / `Right Arrow` |
| Jump | `Space` |
| Shoot | `J` |
| Dash | `Left Shift` |

## Notes

- Uses Rigidbody2D + Collider2D physics.
- Player and enemy bullets use separate prefabs with different `Hit Layers` so there is no friendly fire.
- Health/damage, animations, weapon pickups, and levels are future work.

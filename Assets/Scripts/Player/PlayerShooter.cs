using RunAndGun.Combat;
using UnityEngine;

namespace RunAndGun.Player
{
    /// <summary>
    /// Bridges fire input to whatever weapon the player currently holds.
    /// Keeping this separate from the weapon itself is what makes weapon pickups easy later:
    /// a pickup just calls EquipWeapon() and nothing else has to change.
    /// </summary>
    [RequireComponent(typeof(PlayerInputReader))]
    [RequireComponent(typeof(PlayerFacing))]
    public class PlayerShooter : MonoBehaviour
    {
        [Header("Loadout")]
        [Tooltip("Weapon the player starts with. Assign the placeholder pistol here.")]
        [SerializeField] private PlayerWeapon _currentWeapon;

        private PlayerInputReader _input;
        private PlayerFacing _facing;

        public PlayerWeapon CurrentWeapon => _currentWeapon;

        private void Awake()
        {
            _input = GetComponent<PlayerInputReader>();
            _facing = GetComponent<PlayerFacing>();
        }

        private void Update()
        {
            if (_currentWeapon == null) return;
            if (!_input.FireHeld) return;

            // Bullets always travel along the player's facing direction.
            _currentWeapon.TryFire(_facing.Direction);
        }

        /// <summary>Hook for future weapon pickups.</summary>
        public void EquipWeapon(PlayerWeapon weapon)
        {
            _currentWeapon = weapon;
        }
    }
}

using UnityEngine;

namespace RunAndGun.Combat
{
    /// <summary>
    /// Placeholder pistol. Holds fire rate / bullet stats and spawns bullets.
    ///
    /// This is a plain MonoBehaviour on purpose -- to add a second weapon later, duplicate
    /// the GameObject, retune these values (or subclass if behaviour truly differs) and hand
    /// it to PlayerShooter.EquipWeapon(). No inheritance hierarchy needed yet.
    /// </summary>
    public class PlayerWeapon : MonoBehaviour
    {
        [Header("Projectile")]
        [Tooltip("Prefab with a Bullet component, a Rigidbody2D and a trigger Collider2D.")]
        [SerializeField] private Bullet _bulletPrefab;

        [Tooltip("Where bullets spawn. Parent this under the flipped transform so it mirrors with the player.")]
        [SerializeField] private Transform _muzzle;

        [Header("Stats")]
        [Tooltip("Shots per second.")]
        [SerializeField] private float _fireRate = 6f;

        [Tooltip("Bullet travel speed in units/second.")]
        [SerializeField] private float _bulletSpeed = 20f;

        [Tooltip("Seconds before a bullet despawns.")]
        [SerializeField] private float _bulletLifetime = 2f;

        private float _nextFireTime;

        /// <summary>
        /// Fires if the fire-rate cooldown has elapsed. Returns true if a shot was spawned.
        /// Applies no recoil force, so shooting never moves the player.
        /// </summary>
        /// <param name="facingDirection">-1 for left, +1 for right.</param>
        public bool TryFire(int facingDirection)
        {
            if (Time.time < _nextFireTime) return false;
            if (_bulletPrefab == null)
            {
                Debug.LogWarning($"{name}: no bullet prefab assigned.", this);
                return false;
            }

            _nextFireTime = Time.time + 1f / Mathf.Max(_fireRate, 0.01f);

            Vector3 spawnPosition = _muzzle != null ? _muzzle.position : transform.position;

            // Direction is taken from facing only -- never from the mouse position.
            Vector2 direction = new Vector2(facingDirection >= 0 ? 1f : -1f, 0f);

            Bullet bullet = Instantiate(_bulletPrefab, spawnPosition, Quaternion.identity);
            bullet.Launch(direction, _bulletSpeed, _bulletLifetime);
            return true;
        }
    }
}

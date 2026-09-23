using RunAndGun.Combat;
using UnityEngine;

namespace RunAndGun.Enemy
{
    /// <summary>
    /// A complete, self-contained enemy in ONE script. No other enemy scripts needed.
    ///
    /// Behaviour:
    ///   - Patrols left/right between two points using Rigidbody2D physics.
    ///   - When the player gets within Detection Range, it STOPS, faces the player,
    ///     and shoots horizontally on a timer (with a pause between shots).
    ///   - When the player leaves range, it goes back to patrolling.
    ///
    /// Put this one script on the Enemy GameObject and fill in the Inspector fields.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Enemy : MonoBehaviour
    {
        // ─────────────────────────────────────────────────────────────────────
        // PATROL
        // ─────────────────────────────────────────────────────────────────────
        [Header("Patrol")]
        [Tooltip("How far LEFT of the start position it walks, in units.")]
        [SerializeField] private float _leftRange = 3f;

        [Tooltip("How far RIGHT of the start position it walks, in units.")]
        [SerializeField] private float _rightRange = 3f;

        [Tooltip("Walking speed in units per second.")]
        [SerializeField] private float _moveSpeed = 2f;

        // ─────────────────────────────────────────────────────────────────────
        // DETECTION
        // ─────────────────────────────────────────────────────────────────────
        [Header("Detection")]
        [Tooltip("The player. Drag your Player GameObject here.")]
        [SerializeField] private Transform _player;

        [Tooltip("How close the player must be (in units) before the enemy starts shooting.")]
        [SerializeField] private float _detectionRange = 5f;

        // ─────────────────────────────────────────────────────────────────────
        // SHOOTING
        // ─────────────────────────────────────────────────────────────────────
        [Header("Shooting")]
        [Tooltip("The bullet prefab to fire. Use your EnemyBullet prefab.")]
        [SerializeField] private Bullet _bulletPrefab;

        [Tooltip("Point where bullets spawn. Usually an empty child at the gun barrel.")]
        [SerializeField] private Transform _muzzle;

        [Tooltip("Shots per second. Lower = longer pause between shots.")]
        [SerializeField] private float _fireRate = 1.5f;

        [Tooltip("How fast bullets travel, in units per second.")]
        [SerializeField] private float _bulletSpeed = 14f;

        [Tooltip("Seconds before a fired bullet disappears.")]
        [SerializeField] private float _bulletLifetime = 3f;

        // ─────────────────────────────────────────────────────────────────────
        // VISUALS
        // ─────────────────────────────────────────────────────────────────────
        [Header("Visuals")]
        [Tooltip("The enemy's SpriteRenderer, so it can flip to face left/right.")]
        [SerializeField] private SpriteRenderer _spriteRenderer;

        // ── private runtime state ────────────────────────────────────────────
        private Rigidbody2D _rigidbody;
        private float _leftBound;
        private float _rightBound;
        private int _facing = 1;       // +1 = right, -1 = left
        private float _nextFireTime;   // when we're next allowed to shoot

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();

            // Patrol boundaries are measured from wherever the enemy starts.
            _leftBound = transform.position.x - _leftRange;
            _rightBound = transform.position.x + _rightRange;

            if (_spriteRenderer == null)
                _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        private void FixedUpdate()
        {
            // Decide: is the player close enough to shoot?
            if (PlayerInRange())
            {
                ShootAtPlayer();
            }
            else
            {
                Patrol();
            }
        }

        // ── Is the player within detection range? ────────────────────────────
        private bool PlayerInRange()
        {
            if (_player == null) return false;
            return Vector2.Distance(transform.position, _player.position) <= _detectionRange;
        }

        // ── Walk back and forth between the two bounds ───────────────────────
        private void Patrol()
        {
            float x = transform.position.x;

            // Turn around at the edges.
            if (x >= _rightBound) _facing = -1;
            else if (x <= _leftBound) _facing = 1;

            Face(_facing);

            // Drive horizontal velocity; gravity keeps handling the vertical.
            Vector2 velocity = _rigidbody.linearVelocity;
            velocity.x = _facing * _moveSpeed;
            _rigidbody.linearVelocity = velocity;
        }

        [Header("Aiming")]
        [Tooltip("Dead zone (in units) around the enemy where it will NOT change facing. " +
                 "Prevents the twitchy flip when the player is nearly directly above/below.")]
        [SerializeField] private float _facingDeadZone = 0.2f;

        // ── Stop, face the player, and fire on the timer ─────────────────────
        private void ShootAtPlayer()
        {
            // Stop moving horizontally (stay in place while shooting).
            Vector2 velocity = _rigidbody.linearVelocity;
            velocity.x = 0f;
            _rigidbody.linearVelocity = velocity;

            // How far (and which way) the player is from the enemy on X.
            float xDiff = _player.position.x - transform.position.x;

            // Only flip when the player is CLEARLY to one side. Inside the dead zone we
            // keep the current facing, so the enemy no longer twitches / flips to the
            // wrong side when the player is right next to or slightly past its center.
            if (Mathf.Abs(xDiff) > _facingDeadZone)
            {
                int dirToPlayer = xDiff > 0f ? 1 : -1;
                Face(dirToPlayer);
            }

            // Fire in whatever direction we are ACTUALLY facing, so the bullet and the
            // sprite can never disagree.
            if (Time.time >= _nextFireTime)
            {
                Fire(_facing);
                _nextFireTime = Time.time + 1f / Mathf.Max(_fireRate, 0.01f);
            }
        }

        // ── Spawn one bullet traveling horizontally ──────────────────────────
        private void Fire(int direction)
        {
            if (_bulletPrefab == null) return;

            Vector3 spawnPos = _muzzle != null ? _muzzle.position : transform.position;
            Vector2 dir = new Vector2(direction >= 0 ? 1f : -1f, 0f);

            Bullet bullet = Instantiate(_bulletPrefab, spawnPos, Quaternion.identity);
            bullet.Launch(dir, _bulletSpeed, _bulletLifetime);
        }

        // ── Flip the sprite to face a direction ──────────────────────────────
        private void Face(int direction)
        {
            _facing = direction;
            if (_spriteRenderer != null)
                _spriteRenderer.flipX = direction < 0; // face left = flip
        }

        // ── Draw the detection range in the editor so you can see it ─────────
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _detectionRange);

            // Patrol span (cyan line).
            float left = transform.position.x - _leftRange;
            float right = transform.position.x + _rightRange;
            float y = transform.position.y;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(new Vector3(left, y, 0f), new Vector3(right, y, 0f));
        }
    }
}

using UnityEngine;

namespace RunAndGun.Combat
{
    /// <summary>
    /// Reusable projectile. Deliberately weapon-agnostic: whoever fires it passes in the
    /// direction and speed, so the same prefab works for any future weapon.
    ///
    /// Requires a Rigidbody2D and a trigger Collider2D. Gravity and continuous collision
    /// detection are configured in code so a misconfigured prefab cannot cause tunnelling.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Bullet : MonoBehaviour
    {
        [Header("Defaults")]
        [Tooltip("Used only if the bullet is spawned without Launch() being called.")]
        [SerializeField] private float _fallbackSpeed = 20f;

        [Tooltip("Seconds before the bullet despawns on its own, so strays never leak.")]
        [SerializeField] private float _lifetime = 2f;

        [Header("Collision")]
        [Tooltip("Layers this bullet reacts to. Leave the Player layer OUT so it never hits its owner.")]
        [SerializeField] private LayerMask _hitLayers;

        private Rigidbody2D _rigidbody;
        private float _despawnTime;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();

            // Projectiles fly straight and fast: no gravity, no spin, no tunnelling.
            _rigidbody.gravityScale = 0f;
            _rigidbody.freezeRotation = true;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        private void OnEnable()
        {
            _despawnTime = Time.time + _lifetime;
        }

        /// <summary>
        /// Sends the bullet flying. <paramref name="direction"/> is expected to be purely
        /// horizontal in this game, but the method does not assume it.
        /// </summary>
        public void Launch(Vector2 direction, float speed, float lifetime)
        {
            _lifetime = lifetime;
            _despawnTime = Time.time + _lifetime;

            _rigidbody.linearVelocity = direction.normalized * speed;
        }

        private void Update()
        {
            if (Time.time >= _despawnTime) Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Ignore anything not on a layer we care about (e.g. the player, other bullets).
            if ((_hitLayers.value & (1 << other.gameObject.layer)) == 0) return;

            // FUTURE: damage goes here once enemies/health exist. Intentionally left out for now.

            Destroy(gameObject);
        }

        // Keeps the fallback speed meaningful if a bullet is dropped into a scene manually.
        private void Start()
        {
            if (_rigidbody.linearVelocity.sqrMagnitude < 0.0001f)
                _rigidbody.linearVelocity = Vector2.right * _fallbackSpeed;
        }
    }
}

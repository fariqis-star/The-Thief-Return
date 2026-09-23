using System;
using UnityEngine;

namespace RunAndGun.Combat
{
    /// <summary>
    /// Reusable health for any damageable thing (player, enemy, ...).
    ///
    /// Put it on the Player with Max HP = 3 (three hearts), and on the Enemy with
    /// Max HP = 1 (dies in one hit). Bullets call TakeDamage() when they hit.
    ///
    /// Other scripts (like the hearts UI) listen to OnHealthChanged to update visuals,
    /// and OnDied to react to death.
    /// </summary>
    public class Health : MonoBehaviour
    {
        [Header("Health")]
        [Tooltip("Maximum hit points. Player = 3 (hearts). Enemy = 1 (one-hit kill).")]
        [SerializeField] private int _maxHealth = 3;

        [Tooltip("If true, this GameObject is destroyed when health reaches 0. " +
                 "Good for enemies. Turn OFF for the player if you want to handle death yourself.")]
        [SerializeField] private bool _destroyOnDeath = true;

        /// <summary>Current hit points.</summary>
        public int Current { get; private set; }

        /// <summary>Maximum hit points (e.g. number of hearts).</summary>
        public int Max => _maxHealth;

        /// <summary>True once health has hit 0.</summary>
        public bool IsDead { get; private set; }

        /// <summary>Fired whenever health changes. Args: (current, max). UI listens to this.</summary>
        public event Action<int, int> OnHealthChanged;

        /// <summary>Fired once when health reaches 0.</summary>
        public event Action OnDied;

        private void Awake()
        {
            Current = _maxHealth;
        }

        private void Start()
        {
            // Let listeners (UI) draw the starting state after they've had a chance to subscribe.
            OnHealthChanged?.Invoke(Current, _maxHealth);
        }

        /// <summary>Removes health. Ignored once dead or if amount is not positive.</summary>
        public void TakeDamage(int amount)
        {
            if (IsDead || amount <= 0) return;

            Current = Mathf.Max(Current - amount, 0);
            OnHealthChanged?.Invoke(Current, _maxHealth);

            if (Current == 0) Die();
        }

        /// <summary>Restores health up to the max (used later by health pickups).</summary>
        public void Heal(int amount)
        {
            if (IsDead || amount <= 0) return;

            Current = Mathf.Min(Current + amount, _maxHealth);
            OnHealthChanged?.Invoke(Current, _maxHealth);
        }

        private void Die()
        {
            IsDead = true;
            OnDied?.Invoke();

            if (_destroyOnDeath) Destroy(gameObject);
        }
    }
}

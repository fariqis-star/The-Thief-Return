using UnityEngine;

namespace RunAndGun.Player
{
    /// <summary>
    /// Pure state + timers for a short horizontal dash. This component deliberately does
    /// NOT touch the Rigidbody2D: PlayerMotor is the only script that writes velocity,
    /// which keeps the two from fighting each other. PlayerMotor drives Tick() so the
    /// ordering is deterministic.
    ///
    /// No invincibility, no roll animation -- just a fast horizontal burst.
    /// </summary>
    [RequireComponent(typeof(PlayerFacing))]
    public class PlayerDash : MonoBehaviour
    {
        [Header("Dash Tuning")]
        [Tooltip("Horizontal speed while dashing, in units/second.")]
        [SerializeField] private float _dashSpeed = 22f;

        [Tooltip("How long the dash lasts, in seconds. Keep this short.")]
        [SerializeField] private float _dashDuration = 0.15f;

        [Tooltip("Delay after a dash ENDS before another dash is allowed. Prevents spamming.")]
        [SerializeField] private float _dashCooldown = 0.6f;

        private PlayerFacing _facing;

        private float _dashTimeLeft;
        private float _cooldownTimeLeft;

        // Locked at dash start so the player cannot steer mid-dash.
        private int _lockedDirection = 1;

        public bool IsDashing => _dashTimeLeft > 0f;

        public bool CanDash => _dashTimeLeft <= 0f && _cooldownTimeLeft <= 0f;

        /// <summary>Horizontal velocity PlayerMotor should apply during the dash.</summary>
        public float DashVelocityX => _lockedDirection * _dashSpeed;

        private void Awake()
        {
            _facing = GetComponent<PlayerFacing>();
        }

        /// <summary>Called by PlayerMotor every physics step.</summary>
        public void Tick(float deltaTime)
        {
            if (_dashTimeLeft > 0f)
            {
                _dashTimeLeft -= deltaTime;

                // Cooldown only starts counting once the dash itself is over.
                if (_dashTimeLeft <= 0f) _cooldownTimeLeft = _dashCooldown;
            }
            else if (_cooldownTimeLeft > 0f)
            {
                _cooldownTimeLeft -= deltaTime;
            }
        }

        /// <summary>Starts a dash if allowed. Returns true if the dash actually began.</summary>
        public bool TryStartDash()
        {
            if (!CanDash) return false;

            // Snapshot facing now; ignoring input changes for the rest of the dash.
            _lockedDirection = _facing.Direction;
            _dashTimeLeft = _dashDuration;
            return true;
        }
    }
}

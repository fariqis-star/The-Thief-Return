using UnityEngine;

namespace RunAndGun.Player
{
    /// <summary>
    /// Owns the Rigidbody2D and is the ONLY script that writes to its velocity.
    /// Handles horizontal movement, jumping (with coyote time) and applying the dash.
    ///
    /// Movement is purely 2D -- Rigidbody2D physics cannot produce Z motion, and nothing
    /// here ever writes to transform.position.z.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerInputReader))]
    [RequireComponent(typeof(PlayerFacing))]
    [RequireComponent(typeof(GroundCheck))]
    [RequireComponent(typeof(PlayerDash))]
    public class PlayerMotor : MonoBehaviour
    {
        [Header("Horizontal Movement")]
        [Tooltip("Top running speed in units/second.")]
        [SerializeField] private float _moveSpeed = 8f;

        [Tooltip("How fast the player reaches top speed. Higher = more responsive.")]
        [SerializeField] private float _acceleration = 90f;

        [Tooltip("How fast the player stops when there is no input. Higher = snappier stop.")]
        [SerializeField] private float _deceleration = 120f;

        [Header("Jump")]
        [Tooltip("Upward velocity applied on jump, in units/second.")]
        [SerializeField] private float _jumpVelocity = 14f;

        [Tooltip("Grace period after walking off a ledge where a jump is still allowed. Set to 0 to disable.")]
        [SerializeField] private float _coyoteTime = 0.1f;

        [Tooltip("Ignores the ground check for this long right after jumping. Without it the " +
                 "feet are still overlapping the ground on the next step and a second jump would slip through.")]
        [SerializeField] private float _groundCheckLockoutAfterJump = 0.1f;

        private Rigidbody2D _rigidbody;
        private PlayerInputReader _input;
        private PlayerFacing _facing;
        private GroundCheck _groundCheck;
        private PlayerDash _dash;

        private float _coyoteTimeLeft;
        private float _groundLockoutLeft;

        /// <summary>True when standing on ground. Useful later for animations.</summary>
        public bool IsGrounded { get; private set; }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _input = GetComponent<PlayerInputReader>();
            _facing = GetComponent<PlayerFacing>();
            _groundCheck = GetComponent<GroundCheck>();
            _dash = GetComponent<PlayerDash>();
        }

        private void FixedUpdate()
        {
            float deltaTime = Time.fixedDeltaTime;

            _dash.Tick(deltaTime);
            UpdateGroundedState(deltaTime);

            // --- Dash input ---------------------------------------------------------
            if (_input.ConsumeDashPressed()) _dash.TryStartDash();

            // --- Facing -------------------------------------------------------------
            // Skipped while dashing so the player cannot turn around mid-dash.
            if (!_dash.IsDashing) _facing.SetDirectionFromInput(_input.MoveX);

            // --- Velocity -----------------------------------------------------------
            if (_dash.IsDashing)
            {
                ApplyDashVelocity();
            }
            else
            {
                ApplyHorizontalMovement(deltaTime);
                TryJump();
            }
        }

        private void UpdateGroundedState(float deltaTime)
        {
            if (_groundLockoutLeft > 0f) _groundLockoutLeft -= deltaTime;

            IsGrounded = _groundCheck.IsGrounded && _groundLockoutLeft <= 0f;

            // Refresh the coyote window while grounded, otherwise let it drain.
            if (IsGrounded) _coyoteTimeLeft = _coyoteTime;
            else _coyoteTimeLeft -= deltaTime;
        }

        /// <summary>
        /// Accelerates toward the target speed and decelerates to a full stop when input is
        /// released. Vertical velocity is untouched so gravity and jumping still work.
        /// </summary>
        private void ApplyHorizontalMovement(float deltaTime)
        {
            float targetSpeed = _input.MoveX * _moveSpeed;
            bool stopping = Mathf.Abs(targetSpeed) < 0.01f;
            float rate = stopping ? _deceleration : _acceleration;

            Vector2 velocity = _rigidbody.linearVelocity;
            velocity.x = Mathf.MoveTowards(velocity.x, targetSpeed, rate * deltaTime);
            _rigidbody.linearVelocity = velocity;
        }

        /// <summary>
        /// Jump is only possible from the ground (or within the coyote window). Zeroing the
        /// coyote timer and locking out the ground check together guarantee no double jump.
        /// </summary>
        private void TryJump()
        {
            if (!_input.ConsumeJumpPressed()) return;
            if (_coyoteTimeLeft <= 0f) return;

            Vector2 velocity = _rigidbody.linearVelocity;
            velocity.y = _jumpVelocity; // overwrite rather than add, for a consistent jump height
            _rigidbody.linearVelocity = velocity;

            _coyoteTimeLeft = 0f;
            _groundLockoutLeft = _groundCheckLockoutAfterJump;
        }

        /// <summary>
        /// Dash drives horizontal velocity and pins vertical velocity to zero, so the dash is
        /// perfectly flat. Because this sets VELOCITY (never transform.position), the dash is
        /// resolved by the physics engine and cannot tunnel through walls.
        /// </summary>
        private void ApplyDashVelocity()
        {
            _rigidbody.linearVelocity = new Vector2(_dash.DashVelocityX, 0f);
        }
    }
}

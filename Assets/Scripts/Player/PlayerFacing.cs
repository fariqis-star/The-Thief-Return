using UnityEngine;

namespace RunAndGun.Player
{
    /// <summary>
    /// Tracks which way the player faces. Only two states exist: LEFT (-1) and RIGHT (+1).
    /// Shooting and dashing both read <see cref="Direction"/>, so this is the single
    /// source of truth for orientation -- no mouse aiming, no 8-way directions.
    /// </summary>
    public class PlayerFacing : MonoBehaviour
    {
        [Header("Flip")]
        [Tooltip("Transform that gets mirrored on X. Leave empty to flip this GameObject. " +
                 "Parent the sprite AND the weapon muzzle under this so the muzzle mirrors too.")]
        [SerializeField] private Transform _flipTarget;

        [Tooltip("Direction the player faces when the scene starts.")]
        [SerializeField] private FacingDirection _startingDirection = FacingDirection.Right;

        public enum FacingDirection { Left = -1, Right = 1 }

        /// <summary>-1 when facing left, +1 when facing right.</summary>
        public int Direction { get; private set; }

        /// <summary>Convenience vector for shooting / dashing. Always purely horizontal.</summary>
        public Vector2 AimDirection => new Vector2(Direction, 0f);

        private void Awake()
        {
            if (_flipTarget == null) _flipTarget = transform;

            Direction = (int)_startingDirection;
            ApplyFlip();
        }

        /// <summary>
        /// Updates facing from horizontal input. Zero input leaves facing untouched,
        /// so the player keeps aiming the way they last moved.
        /// </summary>
        public void SetDirectionFromInput(float moveX)
        {
            if (Mathf.Abs(moveX) < 0.01f) return;

            int newDirection = moveX > 0f ? 1 : -1;
            if (newDirection == Direction) return;

            Direction = newDirection;
            ApplyFlip();
        }

        /// <summary>Mirrors the visual on X. Preserves whatever scale magnitude you authored.</summary>
        private void ApplyFlip()
        {
            Vector3 scale = _flipTarget.localScale;
            scale.x = Mathf.Abs(scale.x) * Direction;
            _flipTarget.localScale = scale;
        }
    }
}

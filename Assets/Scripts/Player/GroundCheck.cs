using UnityEngine;

namespace RunAndGun.Player
{
    /// <summary>
    /// Small reusable grounded test using an overlap circle against a LayerMask.
    /// Kept separate from movement so animations / future systems can reuse it.
    /// </summary>
    public class GroundCheck : MonoBehaviour
    {
        [Header("Ground Detection")]
        [Tooltip("Empty child transform placed at the player's feet. Falls back to this object's origin if unset.")]
        [SerializeField] private Transform _checkPoint;

        [Tooltip("Which layers count as ground. Assign your Ground layer here.")]
        [SerializeField] private LayerMask _groundLayers;

        [Tooltip("Radius of the overlap circle. Slightly narrower than the player's collider works best.")]
        [SerializeField] private float _checkRadius = 0.15f;

        /// <summary>
        /// Tests for ground right now. Cheap enough to call once per physics step.
        /// </summary>
        public bool IsGrounded =>
            Physics2D.OverlapCircle(CheckPosition, _checkRadius, _groundLayers) != null;

        private Vector2 CheckPosition => _checkPoint != null ? _checkPoint.position : transform.position;

        // Visualises the probe in the editor: green when grounded, red when airborne.
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(CheckPosition, _checkRadius);
        }
    }
}

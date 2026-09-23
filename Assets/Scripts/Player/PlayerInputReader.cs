using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace RunAndGun.Player
{
    /// <summary>
    /// Single place where raw hardware input is read. Every other player component asks
    /// this class for input instead of touching the keyboard/mouse directly.
    ///
    /// This compiles against the new Input System when the package is installed
    /// (Unity defines ENABLE_INPUT_SYSTEM automatically) and silently falls back to the
    /// legacy Input Manager otherwise. No .inputactions asset is required, so there is
    /// nothing to configure and no extra package to install.
    ///
    /// Bindings: A / Left Arrow = left, D / Right Arrow = right,
    ///           Space = jump, Left Shift = dash, Left Mouse Button = shoot.
    /// </summary>
    public class PlayerInputReader : MonoBehaviour
    {
        /// <summary>
        /// How long a "pressed" event stays available to be consumed. Input is read in
        /// Update but consumed in FixedUpdate, and those do not run at the same rate --
        /// without this short latch a press that lands between two physics steps is lost.
        /// This is deliberately tiny: it is NOT a jump buffer.
        /// </summary>
        private const float PressLatchDuration = 0.08f;

        /// <summary>-1 = left, 0 = no input, +1 = right.</summary>
        public float MoveX { get; private set; }

        /// <summary>True while the fire button is held (placeholder pistol is automatic-friendly).</summary>
        public bool FireHeld { get; private set; }

        // Timestamps of the last press, or a negative value when already consumed/expired.
        private float _jumpPressedAt = -1f;
        private float _dashPressedAt = -1f;

        private void Update()
        {
            ReadDevices(out float moveX, out bool jumpPressed, out bool dashPressed, out bool fireHeld);

            MoveX = moveX;
            FireHeld = fireHeld;

            if (jumpPressed) _jumpPressedAt = Time.time;
            if (dashPressed) _dashPressedAt = Time.time;
        }

        /// <summary>Returns true once per jump press, then clears it.</summary>
        public bool ConsumeJumpPressed() => ConsumeLatch(ref _jumpPressedAt);

        /// <summary>Returns true once per dash press, then clears it.</summary>
        public bool ConsumeDashPressed() => ConsumeLatch(ref _dashPressedAt);

        private static bool ConsumeLatch(ref float pressedAt)
        {
            if (pressedAt < 0f) return false;

            bool isFresh = Time.time - pressedAt <= PressLatchDuration;
            pressedAt = -1f; // consumed either way, so a stale press never fires later
            return isFresh;
        }

        /// <summary>
        /// The only method that differs between the two input backends.
        /// </summary>
        private static void ReadDevices(out float moveX, out bool jumpPressed, out bool dashPressed, out bool fireHeld)
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            Mouse mouse = Mouse.current;

            bool left = keyboard != null && (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed);
            bool right = keyboard != null && (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed);

            jumpPressed = keyboard != null && keyboard.spaceKey.wasPressedThisFrame;
            dashPressed = keyboard != null && keyboard.leftShiftKey.wasPressedThisFrame;
            fireHeld = mouse != null && mouse.leftButton.isPressed;
#else
            bool left = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
            bool right = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);

            jumpPressed = Input.GetKeyDown(KeyCode.Space);
            dashPressed = Input.GetKeyDown(KeyCode.LeftShift);
            fireHeld = Input.GetMouseButton(0);
#endif

            // Holding both directions cancels out, which also means facing is left unchanged.
            moveX = (left ? -1f : 0f) + (right ? 1f : 0f);
        }
    }
}

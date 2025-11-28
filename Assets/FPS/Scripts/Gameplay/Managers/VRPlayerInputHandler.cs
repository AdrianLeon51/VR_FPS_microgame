using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class VRPlayerInputHandler : MonoBehaviour
    {
        [Header("VR Controller References")]
        [Tooltip("Right hand anchor (OVRCameraRig > TrackingSpace > RightHandAnchor)")]
        public Transform RightHandAnchor;
        
        [Header("VR Controller References")]
        [Tooltip("Left OVR Controller")]
        public OVRInput.Controller LeftController = OVRInput.Controller.LTouch;

        [Tooltip("Right OVR Controller")]
        public OVRInput.Controller RightController = OVRInput.Controller.RTouch;

        [Header("Movement Settings")]
        [Tooltip("Thumbstick deadzone")]
        public float ThumbstickDeadzone = 0.2f;

        // NEW: Optional gaze-based firing
        [Header("Gaze Fire Settings (Optional)")]
        [Tooltip("Enable gaze-based firing through SelectionInputManager")]
        public bool enableGazeFiring = false;
        
        [Tooltip("Reference to SelectionInputManager (optional, for gaze-based firing)")]
        public SelectionInputManager selectionInputManager;

        GameFlowManager m_GameFlowManager;
        PlayerCharacterController m_PlayerCharacterController;
        bool m_FireInputWasHeld;
        float m_SmoothTurnInput;

        // NEW: Track gaze fire state
        private bool m_GazeFireTriggered;
        private bool m_GazeFireWasHeld;

        public enum MovementMode
        {
            Thumbstick,
            HumanJoystick
        }

        [Header("Movement Input Mode")]
        public MovementMode movementMode = MovementMode.Thumbstick;

        [Header("Optional Reference")]
        public HumanJoystickTranslation humanJoystick; // drag in inspector


        void Start()
        {
            
            m_PlayerCharacterController = GetComponent<PlayerCharacterController>();
            DebugUtility.HandleErrorIfNullGetComponent<PlayerCharacterController, VRPlayerInputHandler>(
                m_PlayerCharacterController, this, gameObject);
            m_GameFlowManager = FindObjectOfType<GameFlowManager>();
            DebugUtility.HandleErrorIfNullFindObject<GameFlowManager, VRPlayerInputHandler>(m_GameFlowManager, this);

            // Validate right hand anchor
            if (RightHandAnchor == null)
            {
                Debug.LogError("VRPlayerInputHandler: RightHandAnchor is not assigned! Assign OVRCameraRig/TrackingSpace/RightHandAnchor in inspector.");
            }

            // NEW: Subscribe to gaze selection events if enabled
            if (enableGazeFiring && selectionInputManager != null)
            {
                selectionInputManager.OnObjectSelected += HandleGazeFireTrigger;
            }
        }

        void OnDestroy()
        {
            // NEW: Clean up event subscription
            if (enableGazeFiring && selectionInputManager != null)
            {
                selectionInputManager.OnObjectSelected -= HandleGazeFireTrigger;
            }
        }

        // NEW: Handle gaze-based fire trigger
        private void HandleGazeFireTrigger(GameObject target)
        {
            // Only trigger fire for valid enemy targets (you can customize this check)
            if (target != null && target.CompareTag("Enemy"))
            {
                m_GazeFireTriggered = true;
            }
        }

        void LateUpdate()
        {
            m_FireInputWasHeld = GetFireInputHeld();
            
            // NEW: Reset gaze fire trigger after frame
            m_GazeFireWasHeld = m_GazeFireTriggered;
            m_GazeFireTriggered = false;
        }

        public bool CanProcessInput()
        {
            return !m_GameFlowManager.GameIsEnding;
        }

        // Movement from left thumbstick
        public Vector3 GetMoveInput()
        {
            if (!CanProcessInput())
                return Vector3.zero;

            switch (movementMode)
            {
                case MovementMode.Thumbstick:
                    return GetThumbstickMove();

                case MovementMode.HumanJoystick:
                    return GetHumanJoystickMove();

                default:
                    return Vector3.zero;
            }
        }

        private Vector3 GetThumbstickMove()
        {
            Vector2 thumbstick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, LeftController);

            // Deadzone handling
            if (thumbstick.magnitude < ThumbstickDeadzone)
                thumbstick = Vector2.zero;

            // Convert to world-space movement
            Vector3 move = new Vector3(thumbstick.x, 0f, thumbstick.y);

            // Prevent diagonal speed boost
            move = Vector3.ClampMagnitude(move, 1f);

            return move;
        }

        private Vector3 GetHumanJoystickMove()
        {
            return Vector3.zero; // Don't feed PlayerCharacterController
        }

        public bool IsUsingHumanJoystick()
        {
            return movementMode == MovementMode.HumanJoystick && humanJoystick != null;
        }

        // Smooth turn from right thumbstick horizontal
        public float GetLookInputsHorizontal()
        {
            if (CanProcessInput())
            {
                Vector2 thumbstick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, RightController);

                float raw = thumbstick.x;
                
                // Simple deadzone
                if (Mathf.Abs(raw) < ThumbstickDeadzone)
                {
                    return 0f;
                }

                // Remap deadzone to full range
                float sign = Mathf.Sign(raw);
                raw = (Mathf.Abs(raw) - ThumbstickDeadzone) / (1f - ThumbstickDeadzone) * sign;

                // Return RAW input - no deltaTime, no smoothing, no speed multiplier
                // PlayerCharacterController will handle all of that
                return raw;
            }

            return 0f;
        }

        // VR uses physical head movement, return 0
        public float GetLookInputsVertical()
        {
            return 0f; // Not needed in VR - players look with their head
        }

        // Jump - A button (right controller)
        public bool GetJumpInputDown()
        {
            if (CanProcessInput())
            {
                return OVRInput.GetDown(OVRInput.Button.One, RightController);
            }
            return false;
        }

        public bool GetJumpInputHeld()
        {
            if (CanProcessInput())
            {
                return OVRInput.Get(OVRInput.Button.One, RightController);
            }
            return false;
        }

        // Fire - Right trigger OR gaze selection
        public bool GetFireInputDown()
        {
            bool triggerFire = GetFireInputHeld() && !m_FireInputWasHeld;
            
            // NEW: Add gaze fire option
            if (enableGazeFiring)
            {
                bool gazeFire = m_GazeFireTriggered && !m_GazeFireWasHeld;
                return triggerFire || gazeFire;
            }
            
            return triggerFire;
        }

        public bool GetFireInputReleased()
        {
            return !GetFireInputHeld() && m_FireInputWasHeld;
        }

        public bool GetFireInputHeld()
        {
            if (CanProcessInput())
            {
                // NEW: Combine trigger and gaze input
                bool triggerHeld = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, RightController) > 0.5f;
                
                if (enableGazeFiring)
                {
                    return triggerHeld || m_GazeFireTriggered;
                }
                
                return triggerHeld;
            }
            return false;
        }

        // NEW: Get the forward direction the controller is pointing
        public Vector3 GetAimDirection()
        {
            if (RightHandAnchor != null)
                return RightHandAnchor.forward;
            
            return transform.forward; // Fallback
        }
        
        // Aim - Right grip
        public bool GetAimInputHeld()
        {
            if (CanProcessInput())
            {
                return OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, RightController) > 0.5f;
            }
            return false;
        }

        // Sprint - Left thumbstick click
        public bool GetSprintInputHeld()
        {
            if (CanProcessInput())
            {
                return OVRInput.Get(OVRInput.Button.PrimaryThumbstick, LeftController);
            }
            return false;
        }

        // Crouch - B button (right controller)
        public bool GetCrouchInputDown()
        {
            if (CanProcessInput())
            {
                return OVRInput.GetDown(OVRInput.Button.Two, RightController);
            }
            return false;
        }

        public bool GetCrouchInputReleased()
        {
            if (CanProcessInput())
            {
                return OVRInput.GetUp(OVRInput.Button.Two, RightController);
            }
            return false;
        }

        // Reload - X button (left controller)
        public bool GetReloadButtonDown()
        {
            if (CanProcessInput())
            {
                return OVRInput.GetDown(OVRInput.Button.One, LeftController);
            }
            return false;
        }

        // Weapon switch - Right thumbstick up/down
        public int GetSwitchWeaponInput()
        {
            if (CanProcessInput())
            {
                Vector2 thumbstick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, RightController);

                if (thumbstick.y > 0.7f)
                    return 1;  // Next weapon
                else if (thumbstick.y < -0.7f)
                    return -1; // Previous weapon
            }

            return 0;
        }

        // Direct weapon select - not practical in VR, return 0
        public int GetSelectWeaponInput()
        {
            // VR uses GetSwitchWeaponInput instead
            return 0;
        }
    }
}
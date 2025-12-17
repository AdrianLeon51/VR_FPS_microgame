using UnityEngine;

namespace Unity.FPS.Gameplay
{
    /// <summary>
    /// Makes WeaponCamera follow RightHandAnchor without reparenting.
    /// Attach this to WeaponCamera (wherever it currently is in hierarchy).
    /// LEAST INVASIVE - No hierarchy changes needed!
    /// </summary>
    public class VRWeaponTracker : MonoBehaviour
    {
        [Header("Auto-Find References")]
        [Tooltip("Leave empty - will auto-find RightHandAnchor")]
        public Transform RightHandAnchor;

        [Header("Offset Tuning")]
        [Tooltip("Position offset from controller (adjust in Play Mode, then copy values)")]
        public Vector3 PositionOffset = new Vector3(0f, -0.1f, 0.2f);

        [Tooltip("Rotation offset from controller in degrees")]
        public Vector3 RotationOffset = new Vector3(-10f, 0f, 0f);

        [Header("Smoothing (Optional)")]
        [Tooltip("Smooth position? Usually not needed for hand tracking")]
        public bool SmoothPosition = false;

        [Tooltip("Smooth rotation? Can reduce controller jitter")]
        public bool SmoothRotation = true;

        [Tooltip("Smoothing speed (higher = less smooth but more responsive)")]
        [Range(5f, 30f)]
        public float SmoothingSpeed = 20f;

        [Header("Debug")]
        [Tooltip("Show debug line showing aim direction")]
        public bool ShowAimDebug = true;

        private Quaternion offsetRotation;

        [Header("Optional Eye Gaze")]
        public EyeGaze eyeGaze;

        public VRPlayerInputHandler InputHandler;


        void Start()
        {
            // Auto-find RightHandAnchor if not assigned
            if (RightHandAnchor == null)
            {
                // Search for it in the player hierarchy
                RightHandAnchor = GameObject.Find("RightHandAnchor")?.transform;

                if (RightHandAnchor == null)
                {
                    Debug.LogError("VRWeaponTracker: Could not find RightHandAnchor! Please assign manually in inspector.");
                    enabled = false;
                    return;
                }

                Debug.Log($"VRWeaponTracker: Auto-found RightHandAnchor at {RightHandAnchor.name}");
            }

            offsetRotation = Quaternion.Euler(RotationOffset);
        }

        void LateUpdate()
        {
            bool useEyeGaze =
                InputHandler != null &&
                VRPlayerInputHandler.movementMode == VRPlayerInputHandler.MovementMode.HumanJoystick &&
                eyeGaze != null;

            Vector3 targetPosition;
            Quaternion targetRotation;

            if (useEyeGaze)
            {
                // Eye gaze based aiming
                targetPosition = transform.position;
                Vector3 newForward = eyeGaze._lastHitObjectPos - transform.position;
                targetRotation = Quaternion.LookRotation(
                    newForward.normalized,
                    Vector3.up
                ) * offsetRotation;
            }
            else
            {
                // Controller based aiming (default behavior)
                if (RightHandAnchor == null) return;

                targetRotation = RightHandAnchor.rotation * offsetRotation;
                targetPosition = RightHandAnchor.position + (RightHandAnchor.rotation * PositionOffset);
            }

            // Apply with optional smoothing
            if (SmoothPosition)
            {
                transform.position = Vector3.Lerp(
                    transform.position,
                    targetPosition,
                    1f - Mathf.Exp(-SmoothingSpeed * Time.deltaTime)
                );
            }
            else
            {
                transform.position = targetPosition;
            }

            if (SmoothRotation)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    1f - Mathf.Exp(-SmoothingSpeed * Time.deltaTime)
                );
            }
            else
            {
                transform.rotation = targetRotation;
            }
        }


        void OnDrawGizmos()
        {
            if (!ShowAimDebug || !Application.isPlaying) return;

            // Draw aim direction
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, eyeGaze._lastHitObjectPos * 5f);

            // Draw connection to hand
            if (RightHandAnchor != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(RightHandAnchor.position, transform.position);
                Gizmos.DrawWireSphere(RightHandAnchor.position, 0.02f);
            }
        }

        // Public API for weapon scripts to use
        public Vector3 GetAimOrigin() => transform.position;
        public Vector3 GetAimDirection() => eyeGaze.GetGazeDirection();
        public Ray GetAimRay() => new Ray(transform.position, eyeGaze.GetGazeDirection());

#if UNITY_EDITOR
        [Header("Play Mode Tuning Helper")]
        [Tooltip("Press this button in Play Mode to print current offset values")]
        public bool PrintCurrentOffset;

        void Update()
        {
            if (PrintCurrentOffset && Application.isPlaying && RightHandAnchor != null)
            {
                PrintCurrentOffset = false;

                // Calculate what the current offsets would need to be
                Vector3 localPos = RightHandAnchor.InverseTransformPoint(transform.position);
                Quaternion localRot = Quaternion.Inverse(RightHandAnchor.rotation) * transform.rotation;
                Vector3 localEuler = localRot.eulerAngles;

                Debug.Log($"=== CURRENT OFFSET VALUES ===\n" +
                         $"PositionOffset: ({localPos.x:F3}, {localPos.y:F3}, {localPos.z:F3})\n" +
                         $"RotationOffset: ({localEuler.x:F1}, {localEuler.y:F1}, {localEuler.z:F1})\n" +
                         $"Copy these values to the inspector!");
            }
        }
#endif
    }
}
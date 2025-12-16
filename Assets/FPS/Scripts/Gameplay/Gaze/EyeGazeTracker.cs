using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Gameplay
{
    public class EyeGazeTracker : MonoBehaviour
    {
        [Header("Eye Gaze Source")]
        public EyeGaze EyeGaze;

        [Header("Offset Tuning")]
        [Tooltip("Local offset from gaze origin")]
        public Vector3 PositionOffset = Vector3.zero;

        [Tooltip("Rotation offset from gaze direction in degrees")]
        public Vector3 RotationOffset = Vector3.zero;

        [Header("Smoothing")]
        public bool SmoothPosition = false;
        public bool SmoothRotation = true;

        [Range(5f, 30f)]
        public float SmoothingSpeed = 20f;

        [Header("Debug")]
        public bool ShowGazeDebug = true;

        private Quaternion offsetRotation;

        void Start()
        {
            if (EyeGaze == null)
            {
                Debug.LogError("EyeGazeTracker: EyeGaze reference not assigned!");
                enabled = false;
                return;
            }

            offsetRotation = Quaternion.Euler(RotationOffset);
        }

        void LateUpdate()
        {
            Vector3 gazeOrigin = EyeGaze.GetGazeOrigin();
            Vector3 gazeDirection = EyeGaze.GetGazeDirection();

            if (gazeDirection.sqrMagnitude < 0.0001f)
                return;

            // Base rotation from gaze direction
            Quaternion gazeRotation = Quaternion.LookRotation(gazeDirection, Vector3.up);

            // Apply offsets
            Quaternion targetRotation = gazeRotation * offsetRotation;
            Vector3 targetPosition = gazeOrigin + (gazeRotation * PositionOffset);

            // Position
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

            // Rotation
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
            if (!ShowGazeDebug || EyeGaze == null || !Application.isPlaying)
                return;

            Vector3 origin = EyeGaze.GetGazeOrigin();
            Vector3 dir = EyeGaze.GetGazeDirection();

            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(origin, dir * 5f);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(origin, 0.01f);

            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, transform.forward * 2f);
        }

        // Public API (mirrors VRWeaponTracker)
        public Vector3 GetAimOrigin() => transform.position;
        public Vector3 GetAimDirection() => transform.forward;
        public Ray GetAimRay() => new Ray(transform.position, transform.forward);
    }
}
using System;
using UnityEngine;

public class HeadGainManager : MonoBehaviour
{
    [Header("Foveated View Filter (OVRVignette)")]
    public OVRVignette ovrVignette;               // Assign CenterEyeAnchor's OVRVignette here
    public float minVignetteFOV = 30f;             // Strong vignette: small clear center
    public float maxVignetteFOV = 60f;             // Weak vignette: large clear center
    public float vignetteSmoothing = 5f;           // How smooth vignette changes

    private float smoothCurrentSpeed; // stores smoothed currentSpeed


    [Header("Rotation Amplification")]
    public Transform playerObject;          // The tracked head or source transform
    public Transform targetObject;          // The tracked head or source transform
    public float thresholdRotation = 1f;    // Minimum yaw angle to trigger rotation
    public float rotationSpeed = 3f;      // Equivalent to movementSpeed in original code
    public bool activateRotation = true;
    public float highRotationSpeed = 3f;
    public float angleSpeedChange = 40f;

    private Quaternion originalRotation;

    public static bool rotationIdleActive = false;

    private void Start()
    {
        originalRotation = transform.rotation;

    }
    void Update()
    {
        if (!targetObject || !activateRotation)
            return;

        // Convert yaw to -180..180
        float rawYaw = targetObject.localEulerAngles.y;
        float yaw = (rawYaw > 180f) ? rawYaw - 360f : rawYaw;

        


        Quaternion targetRotation = targetObject.localRotation;

        // Check yaw angle
        if (Math.Abs(yaw) >= thresholdRotation)
        {
            // Choose speed based on yaw range
            float currentSpeed =
                (Mathf.Abs(yaw) <= angleSpeedChange) ? highRotationSpeed : rotationSpeed;

            Quaternion rotateDirection =
                Quaternion.Euler(0f, targetObject.eulerAngles.y, 0f);

            if (targetObject != null)
            {
                UpdateVignetteWithYawSpeed(Math.Abs(yaw));
            }

            //Quaternion currentRotationVelocity = Quaternion.Slerp(transform.rotation, rotateDirection, rotationSpeed * Time.deltaTime);

            playerObject.rotation = Quaternion.Slerp(playerObject.rotation, rotateDirection, Time.deltaTime * currentSpeed);


            rotationIdleActive = false;
        }
        else
        {
            rotationIdleActive = true;

            // Smoothly restore vignette when idle
            if (ovrVignette != null)
            {
                smoothCurrentSpeed = Mathf.Lerp(smoothCurrentSpeed, 0f, Time.deltaTime * vignetteSmoothing);
                float tIdle = Mathf.Clamp01(smoothCurrentSpeed / Mathf.Max(rotationSpeed, highRotationSpeed));
                ovrVignette.VignetteFieldOfView = Mathf.Lerp(maxVignetteFOV, minVignetteFOV, tIdle);
            }
        }
    }

    public void ResetRotation()
    {
        float elapsed = 0f;
        Quaternion startRot = transform.rotation;

        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / 0.5f;

            transform.rotation = Quaternion.Lerp(startRot, originalRotation, t);

        }
        //Vector3 angles = transform.rotation.eulerAngles;
        //angles.y = targetObject.localRotation.eulerAngles.y;
        //transform.rotation = Quaternion.Euler(angles);
    }

    private void UpdateVignetteWithYawSpeed(float currentYaw)
    {
        if (ovrVignette == null)
            return;

        float t = Mathf.Clamp01(currentYaw / angleSpeedChange);

        // Map to vignette FOV (higher speed = more foveated)
        float vignetteFOV = Mathf.Lerp(maxVignetteFOV, minVignetteFOV, t);

        // Apply
        ovrVignette.VignetteFieldOfView = vignetteFOV;
    }






}

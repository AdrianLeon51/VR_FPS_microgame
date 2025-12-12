using System;
using UnityEngine;

public class HeadGainManager : MonoBehaviour
{
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

            //Quaternion currentRotationVelocity = Quaternion.Slerp(transform.rotation, rotateDirection, rotationSpeed * Time.deltaTime);

            playerObject.rotation = Quaternion.Slerp(playerObject.rotation, rotateDirection, Time.deltaTime * currentSpeed);

            rotationIdleActive = false;
        }
        else
        {
            rotationIdleActive = true;
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
}

using UnityEngine;

public class HeadGainManager : MonoBehaviour
{
    [Header("Rotation Amplification")]
    public Transform targetObject;          // The tracked head or source transform
    public float thresholdRotation = 1f;    // Minimum yaw angle to trigger rotation
    public float rotationSpeed = 0.8f;      // Equivalent to movementSpeed in original code
    public bool activateRotation = true;

    public static bool rotationIdleActive = false;

    void Update()
    {
        if (!targetObject || !activateRotation)
            return;

        Quaternion targetRotation = targetObject.localRotation;

        // Check yaw angle
        if (targetRotation.eulerAngles.y >= thresholdRotation)
        {
            Quaternion rotateDirection =
                Quaternion.Euler(0f, targetObject.eulerAngles.y, 0f);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                rotateDirection,
                Time.deltaTime * rotationSpeed
            );

            rotationIdleActive = false;
        }
        else
        {
            rotationIdleActive = true;
        }
    }

    public void ResetRotation()
    {
        Vector3 angles = transform.rotation.eulerAngles;
        angles.y = targetObject.localRotation.eulerAngles.y;
        transform.rotation = Quaternion.Euler(angles);
    }
}

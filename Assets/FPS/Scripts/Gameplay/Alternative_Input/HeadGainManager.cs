using UnityEngine;

public class HeadGainManager : MonoBehaviour
{
    [Header("Rotation Amplification")]
    public Transform targetObject;          // The VR camera/head transform
    public float thresholdRotation = 15f;   // Degrees of head turn before body follows
    public float rotationSpeed = 2f;        // How fast body catches up to head
    public bool activateRotation = true;
    public static bool rotationIdleActive = false;

    private float referenceYaw = 0f;        // Reference world yaw when head was centered

    void Start()
    {
        if (targetObject == null)
        {
            Debug.LogError("HeadGainManager: targetObject is not assigned!");
            return;
        }

        // Store the initial world yaw of the head
        referenceYaw = targetObject.eulerAngles.y;
        Debug.Log($"HeadGainManager initialized. Reference yaw: {referenceYaw:F1}°");
    }

    void LateUpdate()
    {
        if (!targetObject || !activateRotation)
        {
            rotationIdleActive = true;
            return;
        }

        // Calculate how far the head has turned in world space from reference
        float currentHeadYaw = targetObject.eulerAngles.y;
        float headRotationDelta = Mathf.DeltaAngle(referenceYaw, currentHeadYaw);

        Debug.Log($"Current: {currentHeadYaw:F1}°, Reference: {referenceYaw:F1}°, Delta: {headRotationDelta:F1}°");

        // Check if head rotation exceeds threshold
        if (Mathf.Abs(headRotationDelta) >= thresholdRotation)
        {
            Debug.Log($"<color=green>Head turned {headRotationDelta:F1}° - ROTATING BODY</color>");

            // Rotate the Player body toward the head direction
            float rotationAmount = headRotationDelta * rotationSpeed * Time.deltaTime;
            transform.Rotate(0f, rotationAmount, 0f, Space.World);

            // Update reference to current head position
            referenceYaw = currentHeadYaw;

            rotationIdleActive = false;
        }
        else
        {
            Debug.Log($"Head Delta {headRotationDelta:F1}° - below threshold");
            rotationIdleActive = true;
        }
    }

    public void ResetRotation()
    {
        if (targetObject)
        {
            referenceYaw = targetObject.eulerAngles.y;
            Debug.Log($"Rotation reset. New reference: {referenceYaw:F1}°");
        }
    }
}
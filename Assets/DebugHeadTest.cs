using System.Collections;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit.SceneDecorator;
using UnityEngine;

public class DebugHeadTest : MonoBehaviour
{
    Vector3 headForward;
    // Start is called before the first frame update
    void Start()
    {
        headForward = transform.forward;
        headForward.y = 0; // Project to horizontal plane
    }

    // Update is called once per frame
    void Update()
    {
        // Calculate the angle difference between head and body
        float angleDifference = Vector3.SignedAngle(transform.forward, headForward, Vector3.up);
        // Debug visualization
        if (Time.frameCount % 30 == 0) // Log twice per second
        {
            Debug.Log($"HeadGain - Head vs Body angle: {angleDifference:F1}°");
            Debug.Log($"HEAD DEBUG: Local={transform.localEulerAngles.y:F1}° World={transform.eulerAngles.y:F1}°");
        }

        // Draw debug rays
        Debug.DrawRay(transform.localPosition + Vector3.up, transform.forward * 2f, Color.blue, 0.1f); // Player body
        
    }
}

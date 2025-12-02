using UnityEngine;

public class HeadRotationTest : MonoBehaviour
{
    void Update()
    {
        Debug.Log($"HEAD ROT: Local={transform.localEulerAngles.y:F1}° World={transform.eulerAngles.y:F1}°");
    }
}
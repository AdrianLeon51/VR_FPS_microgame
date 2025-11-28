using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceGestureSelection : MonoBehaviour
{

    public OVRFaceExpressions faceExpressions;
    public float threshold = 0.8f;

    public EyeGaze eyeGaze;

    public event System.Action<GameObject> OnGestureSelected;
    private GameObject target;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        target = eyeGaze.CurrentGazeTarget;

        // Example: Get the weight (blend shape coefficient) for a specific expression
        float tongueWeight = faceExpressions.GetWeight(OVRFaceExpressions.FaceExpression.TongueOut);
        Debug.Log("JawWeight: " + tongueWeight);
        if (tongueWeight >= threshold)
        {
            OnGestureSelected.Invoke(target);
        }
    }
}

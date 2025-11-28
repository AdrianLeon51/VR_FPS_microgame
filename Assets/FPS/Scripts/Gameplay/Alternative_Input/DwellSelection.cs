using UnityEngine;

public class DwellSelection : MonoBehaviour
{
    public EyeGaze eyeGaze;
    public float dwellTime = 2f;

    private float timer = 0f;

    public event System.Action<GameObject> OnDwellSelected;

    private void OnEnable()
    {
        timer = 0f;
    }

    void Update()
    {
        GameObject target = eyeGaze.CurrentGazeTarget;

        if (target == null)
        {
            timer = 0f;
            return;
        }

        timer += Time.deltaTime;

        if (timer >= dwellTime)
        {
            OnDwellSelected?.Invoke(target);
            timer = 0f; // Reset so it can select again if needed
        }
    }
}

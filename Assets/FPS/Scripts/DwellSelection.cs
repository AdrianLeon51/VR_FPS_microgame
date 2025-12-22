using UnityEngine;
using Unity.FPS.Gameplay;

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

        if (timer >= dwellTime && (target.name == "HitBox" || target.name == "Hitbox Top" || target.name == "Hitbox Base"))
        {
            OnDwellSelected?.Invoke(target);
            Debug.Log("Select dwell");
            timer = 0f; // Reset so it can select again if needed
        }
    }
}

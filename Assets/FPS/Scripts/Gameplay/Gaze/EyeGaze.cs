using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.Gameplay
{

    public class EyeGaze : MonoBehaviour
    {
        public OVREyeGaze LeftEye, RightEye;

        private Vector3 _combinedGazeOrigin, _combinedGazeDir;

        private List<Quaternion> _headRotationBuffer;

        [Header("Gaze Dwell Settings")]
        public float dwellTime = 1.5f;     // seconds required to trigger
        private float _gazeTimer = 0f;
        public bool firstGaze = false;
        public GameObject currentDwellTarget { get; private set; }
        public event System.Action<GameObject> OnDwellComplete;


        [Header("One Euro Filter")]

        public bool FilteringGaze = true;

        public float FilterFrequency = 90f;
        public float FilterMinCutOff = 0.05f;
        public float FilterBeta = 10f;
        public float FitlerDcutoff = 1f;

        private OneEuroFilter<Vector3> _gazeDirFilter;
        private OneEuroFilter<Vector3> _gazePosFilter;

        [Header("Gaze Correction")]
        public bool CorrectGaze;
        public int FrameOffset = 7;

        [Header("Gaze Interaction")]
        public float gazeDistance = 30f;
        public Color gazeColor = Color.red;

        [Header("Dynamic Gaze Direction")]
        public bool SnapGazeToHit = true;

        [Tooltip("Layers that can redirect gaze direction")]
        public LayerMask gazeSnapLayer;


        public bool activateGazeRay = false;
        private GameObject _lastHitObject;
        public Vector3 _lastHitObjectPos;
        private Material _originalMaterial;

        public GameObject CurrentGazeTarget { get; private set; }
        public event System.Action<GameObject> OnGazeObjectChanged;

        [Tooltip("Only objects on this layer will respond to gaze interaction.")]
        public LayerMask interactableLayer;

        void Awake()
        {
            _gazeDirFilter = new OneEuroFilter<Vector3>(FilterFrequency);
            _gazePosFilter = new OneEuroFilter<Vector3>(FilterFrequency);

            _headRotationBuffer = new List<Quaternion>();
        }


        void Update()
        {
            _combinedGazeOrigin = Vector3.Lerp(LeftEye.transform.position, RightEye.transform.position, 0.5f);
            _combinedGazeDir = Quaternion.Slerp(LeftEye.transform.rotation, RightEye.transform.rotation, 0.5f).normalized * Vector3.forward;

            if (FilteringGaze)
            {
                _gazeDirFilter.UpdateParams(FilterFrequency, FilterMinCutOff, FilterBeta, FitlerDcutoff);
                _gazePosFilter.UpdateParams(FilterFrequency, FilterMinCutOff, FilterBeta, FitlerDcutoff);

                _combinedGazeDir = _gazeDirFilter.Filter(_combinedGazeDir);
                _combinedGazeOrigin = _gazePosFilter.Filter(_combinedGazeOrigin);
            }

            if (CorrectGaze && _headRotationBuffer.Count == FrameOffset)
            {
                Quaternion headRotOffset = _headRotationBuffer[0] * Quaternion.Inverse(_headRotationBuffer[_headRotationBuffer.Count - 1]);
                _combinedGazeDir = headRotOffset * _combinedGazeDir;
            }

            //TrySnapGazeDirection();

            UpdateHeadRotationBuffer();


            if (activateGazeRay)
            {
                HandleGazeRaycast();
            }

        }

        void UpdateHeadRotationBuffer()
        {
            Quaternion currentHeadRotation = Camera.main.transform.rotation;
            _headRotationBuffer.Add(currentHeadRotation);
            if (_headRotationBuffer.Count > FrameOffset)
            {
                _headRotationBuffer.RemoveAt(0);
            }
        }

        public Ray GetGazeRay()
        {
            return new Ray(_combinedGazeOrigin, _combinedGazeDir);
        }

        public Vector3 GetGazeOrigin()
        {
            return _combinedGazeOrigin;
        }

        public Vector3 GetGazeDirection()
        {
            return _combinedGazeDir;
        }

        void TrySnapGazeDirection()
        {
            Vector3 dir = _combinedGazeDir.normalized;
            Vector3 origin = _combinedGazeOrigin + dir * 0.02f;

            Debug.DrawRay(origin, dir * gazeDistance, Color.green);

            if (Physics.Raycast(
                origin,
                _combinedGazeDir,
                out RaycastHit hit,
                gazeDistance,
                gazeSnapLayer,
                QueryTriggerInteraction.Collide))
            {
                Vector3 snappedDir = (hit.point - _combinedGazeOrigin).normalized;

                if (FilteringGaze)
                {
                    snappedDir = _gazeDirFilter.Filter(snappedDir);
                }

                _combinedGazeDir = snappedDir;
                _lastHitObjectPos = hit.point;

            }
        }



        void HandleGazeRaycast()
        {
            Ray gazeRay = GetGazeRay();
            RaycastHit hit;

            if (Physics.Raycast(gazeRay, out hit, gazeDistance, interactableLayer))

            {
                GameObject hitObject = hit.collider.gameObject;
                _lastHitObjectPos = hit.point;

                if (hitObject != _lastHitObject)
                {
                    // Reset dwell timer when switching targets
                    _gazeTimer = 0f;
                    currentDwellTarget = null;
                    VRPlayerInputHandler.fireSucceed = false;

                    Debug.Log("Chhanged dwell to: " + hitObject.name);
                    // Restore previous object's color
                    if (_lastHitObject != null && _originalMaterial != null)
                    {
                        Renderer lastRenderer = _lastHitObject.GetComponent<Renderer>();
                        if (lastRenderer != null)
                            lastRenderer.material.color = _originalMaterial.color;
                    }

                    // Change color of current object
                    Renderer renderer = hitObject.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        if (!firstGaze && hitObject.name != "CenterDot")
                        {
                            float distance = Vector3.Distance(hitObject.transform.position, _combinedGazeOrigin);
                            Debug.Log("Gazing at: " + hitObject.name + " distance:" + distance);
                            firstGaze = true;
                        }

                        _originalMaterial = new Material(renderer.material); // make a copy
                        //renderer.material.color = gazeColor;
                    }

                    _lastHitObject = hitObject;
                    CurrentGazeTarget = hitObject;

                    OnGazeObjectChanged?.Invoke(CurrentGazeTarget);
                }
                else
                {
                    // Increase dwell timer only while looking at same target
                    _gazeTimer += Time.deltaTime;

                    if (hitObject.name == "HitBox" || hitObject.name == "HitBox Top" || hitObject.name == "HitBox Base")
                    {
                        if (currentDwellTarget == null && _gazeTimer >= dwellTime)
                        {
                            //Debug.Log("Dwelling at: " + hitObject.name);
                            currentDwellTarget = hitObject;

                            // Notify other scripts
                            OnDwellComplete?.Invoke(hitObject);

                            Debug.Log("DWELL COMPLETE on: " + hitObject.name);
                        }
                    }

                }
            }
            else
            {
                // No hit, reset last object
                if (_lastHitObject != null && _originalMaterial != null)
                {
                    Renderer lastRenderer = _lastHitObject.GetComponent<Renderer>();
                    if (lastRenderer != null)
                        lastRenderer.material.color = _originalMaterial.color;
                }

                _lastHitObject = null;
                CurrentGazeTarget = null;

                OnGazeObjectChanged?.Invoke(null);

                if (currentDwellTarget != null)
                {
                    //Dwell variables restart
                    _gazeTimer = 0f;
                    currentDwellTarget = null;
                }

            }
        }


    }
}

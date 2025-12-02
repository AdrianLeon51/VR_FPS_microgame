using System;
using UnityEngine;

public class HumanJoystickTranslation : MonoBehaviour
{
    #region Public Fields
    [Header("Locomotion Settings")]
    [Tooltip("Changes the maximum speed in m/s of forward and backward translation.")]
    public float _maxTranslationSpeed = 3f;

    [Tooltip("Leaning forward dead-zone in percent.")]
    [Range(0f, 0.9f)]
    public float _deadzone = 0f;

    [Tooltip("Leaning forward dead-zone in percent.")]
    public float _offsetHeadPivot = 0.09f;

    [Tooltip("Power of the exponetial Function")]
    [Range(1f, 2f)]
    private float _exponentialTransferFunctionPower = 1.53f;

    [Tooltip("Define the distance from center which results in maximum axis deviation.")]
    public float _bodyOffsetForMaxSpeed = 0.4f;

    public LayerMask _terrainLayer;

    [Header("Transfer Function")]
    [Tooltip("Sensitivity of leaning (inside the exponential function)")]
    [Range(0f, 5f)]
    public float _transferSensitivity = 1f;

    [Tooltip("(outside of the exponential function)")]
    [Range(0f, 10f)]
    // In Summary --> input = transferFactor * (leaningMag * speedSensitivity)^(exponential)
    public float _transferFactor = 1f;

    public Transform playerTransform;
    
    #endregion

    [Header("Movement Control")]
    GameObject _headJoint;
    private GameObject _camera;
    private Vector3 _leaningRefPosition = Vector3.zero;
    private float _velocityAxis = 0;
    private Vector3 _tiltingDirectionLocal;
    private float _lastTriggerState = 0;

    [Header("Movement Control")]
    [Tooltip("Toggle whether movement is allowed at all.")]
    public bool movementEnabled = true;

    private bool _calibrated = false;

    void Start()
    {
        _camera = GameObject.Find("CenterEyeAnchor");
        // Create a child object on the camera that represents the pivot point for the
        // player's yaw/lean pivot. Avoid creating an orphaned GameObject at world origin
        // by not calling Instantiate() on a temporary and instead creating a single
        // GameObject and parenting it immediately.
        Vector3 centerOfYawRotationPosition = _camera.transform.position - (_camera.transform.forward * _offsetHeadPivot);
        GameObject centerOfYawRotation = new GameObject("CenterOfYawRotation");
        _headJoint = centerOfYawRotation;
        // parent to the camera and keep world position
        _headJoint.transform.SetParent(_camera.transform, true);
        _headJoint.transform.position = centerOfYawRotationPosition;

        CalibrateLeaningKS();
    }

    void Update()
    {
        OVRInput.Update();
        float triggerAxis = MathF.Max(OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger), OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger));

        // calibrate leaning each time the interface is activated
        if (OnTriggerDown(triggerAxis))
        {
            Debug.Log("Calibration");
            CalibrateLeaningKS();
        }

        UpdateLeaningInputs();

        if (movementEnabled && _calibrated)
        {
            // Calculate movement
            Vector3 movementDirection = GetMovementDirection();
            float speed = _velocityAxis * _maxTranslationSpeed;
            Vector3 movement = movementDirection * speed * Time.deltaTime;
            
            // DIRECTLY MOVE THE PLAYER
            playerTransform.position += movement;
            
            //float distanceToTravel = _velocityAxis * _maxTranslationSpeed;
            //transform.position += distanceToTravel * Time.deltaTime * GetMovementDirection();
        }

        // Print ONLY when pressing another button (A or X)
        if (OVRInput.Get(OVRInput.Button.One))
        {
            Debug.Log("One Pressed");
            PrintMovementDebugInfo();
        }


        MoveTransformToTerrain();
    }

    // Set the reference point for the leaning interface
    public void CalibrateLeaningKS()
    {
        _leaningRefPosition = this.transform.InverseTransformPoint(_headJoint.transform.position);
        //_leaningCalibKit.transform.rotation = Quaternion.identity;
        _calibrated = true;
        //Debug.Log("Calibration true");
    }
    
    private Vector3 RecordCalibratedPosition()
    {
        Vector3 calibPosition = new Vector3(0,2.5f,-15);
        return calibPosition;
    }
    public void MoveToCalibPosition()
    {
        gameObject.transform.position = RecordCalibratedPosition();
    }

    public void MovementEnable()
    {
        movementEnabled = true; 
    }
    public void MovementDisable()
    {
        movementEnabled = false;
    }
    // I do not want to use gravity in VR because of potentially awkward physics behaviour, thus we have to manually set the platform to the ground for ground based travel
    private void MoveTransformToTerrain()
    {
        RaycastHit terrainHit;
        bool hit = Physics.Raycast(transform.position + Vector3.up * 100f, Vector3.down, out terrainHit, Mathf.Infinity, _terrainLayer);

        // Only update the transform position if a terrain point was actually hit. If no
        // hit occurs we avoid moving the object to Vector3.zero (the default RaycastHit)
        // which would unintentionally teleport the player to the scene origin.
        if (hit)
        {
            transform.position = terrainHit.point;
        }
    }

    private void UpdateLeaningInputs()
    {
        Vector3 diff = this.transform.InverseTransformPoint(_headJoint.transform.position) - _leaningRefPosition;
        _velocityAxis = diff.magnitude;
        diff = Vector3.ProjectOnPlane(diff, Vector3.up);
        _tiltingDirectionLocal = diff.normalized;
        
        // clamp body tilt to an axis
        _velocityAxis = Mathf.Clamp01(_velocityAxis / _bodyOffsetForMaxSpeed);

        // apply transform function
        float adjustedInput = Mathf.Clamp01((_velocityAxis - _deadzone) / (1f - _deadzone));
        float transformFunc = Mathf.Pow(adjustedInput * _transferSensitivity, _exponentialTransferFunctionPower) * _transferFactor;
        
        _velocityAxis = _velocityAxis * transformFunc;
    }
    
    public Vector3 GetMovementDirection()
    {
        return transform.localToWorldMatrix * _tiltingDirectionLocal;
    }

    public Vector3 GetVelocityVector()
    {
        // full world-space velocity including speed
        return _velocityAxis * _maxTranslationSpeed * GetMovementDirection();
    }


    public Vector2 GetAxis2D()
    {
        Vector3 axis3D = _velocityAxis * _tiltingDirectionLocal;
        return new Vector2(axis3D.x, axis3D.z);
    }

    
    private Boolean OnTriggerDown(float val)
    {
        if (val > 0f && _lastTriggerState == 0f)
        {
            _lastTriggerState = val;
            return true;
        }

        _lastTriggerState = val;
        return false;
    }

    //For Debug Purposes
    private void PrintMovementDebugInfo()
    {
        // Distance the head has moved relative to calibration
        float distance = _velocityAxis * _bodyOffsetForMaxSpeed;

        // Actual movement speed (m/s)
        float movementSpeed = _velocityAxis * _maxTranslationSpeed;

        // Direction in world space
        Vector3 direction = GetMovementDirection();

        Debug.Log(
            $"[HumanJoystick] Distance: {distance:F3} m | " +
            $"Speed: {movementSpeed:F3} m/s | " +
            $"Direction: {direction}"
        );
    }


}

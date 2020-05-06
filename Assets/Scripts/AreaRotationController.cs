using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class AreaRotationController : MonoBehaviour {
    private bool _isRotating;
    private Quaternion _initialRotation;
    private Vector3 _controllerReference;
    private InputDevice _leftHand;

    public float triggerThreshold = .2f;
    public GameObject cameraContainer;

    private void Update() {
        _leftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        
        _leftHand.TryGetFeatureValue(CommonUsages.trigger, out var triggerVal);
        
        if (triggerVal >= triggerThreshold && !_isRotating) {
            OnTriggerDown();
        }

        if (triggerVal < triggerThreshold && _isRotating) {
            OnTriggerUp();
        }

        if (!_isRotating) return;

        _leftHand.TryGetFeatureValue(CommonUsages.devicePosition, out var currentPosition);
        currentPosition += cameraContainer.transform.position;
        var tmpRotation = Quaternion.FromToRotation(_controllerReference, currentPosition);
        transform.Rotate(tmpRotation.eulerAngles, Space.World);
        _controllerReference = currentPosition;
    }

    private void OnTriggerDown() {
        _isRotating = true;
        _leftHand.TryGetFeatureValue(CommonUsages.devicePosition, out _controllerReference);
        _controllerReference += cameraContainer.transform.position;
    }

    private void OnTriggerUp() {
        _isRotating = false;
    }
}
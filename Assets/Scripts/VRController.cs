using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class VRController : MonoBehaviour  {
    private InputDevice _head;
    private InputDevice _leftHand;
    private InputDevice _rightHand;

    public GameObject cameraContainer;
    public float playerDistance = 2f;
    public float playerHeight = 1f;
    public GameObject movementStick;
    public GameObject rotationStick;
    public Canvas messagesCanvas;

    private void Recenter() {
        _head.TryGetFeatureValue(CommonUsages.deviceRotation, out var headRotation);

        // convert a quaternion to simple vector
        var newPosition = new Vector3 {
            x = 2 * (headRotation.x * headRotation.z + headRotation.w * headRotation.y),
            // y = 2 * (headRotation.y * headRotation.z + headRotation.w * headRotation.x),
            z = 1 - 2 * (headRotation.x * headRotation.x + headRotation.y * headRotation.y)
        };
        newPosition.Normalize();
        newPosition *= -playerDistance;
        newPosition -= Camera.main.transform.localPosition;
        newPosition.y = playerHeight;

        cameraContainer.transform.position = newPosition;
    }
    
    private void Start() {
        _head = InputDevices.GetDeviceAtXRNode(XRNode.Head);
        Recenter();
    }

    private void Update() {
        _head = InputDevices.GetDeviceAtXRNode(XRNode.Head);
        _leftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        _rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        
        if (Input.GetKey(KeyCode.Tab)) {
            Recenter();
        }

        var cameraTransform = Camera.main.transform;
        messagesCanvas.transform.LookAt(cameraTransform.position);
        messagesCanvas.transform.Rotate(0, 180, 0);

        _leftHand.TryGetFeatureValue(CommonUsages.deviceRotation, out var leftHandRotation);
        _leftHand.TryGetFeatureValue(CommonUsages.devicePosition, out var leftHandPosition);
        _rightHand.TryGetFeatureValue(CommonUsages.deviceRotation, out var rightHandRotation);
        _rightHand.TryGetFeatureValue(CommonUsages.devicePosition, out var rightHandPosition);

        movementStick.transform.localRotation = rightHandRotation;
        movementStick.transform.localPosition = rightHandPosition;
        rotationStick.transform.localRotation = leftHandRotation;
        rotationStick.transform.localPosition = leftHandPosition;
    }
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.XR;

public class PlayerController : MonoBehaviour {
    private readonly List<GameObject> _parts = new List<GameObject>();
    private readonly List<Vector3> _path = new List<Vector3>();
    private GameObject _apple;
    private bool _gameOver = true;
    private int _score;
    private InputDevice _rightHand;

    public float speed = .0001f;
    public float rotationSpeed = .1f;
    public int length = 10;
    public float distance = .01f;
    public Material material;
    public Material appleMaterial;
    public GameObject boundary;
    public Text scoreText;
    public Text startText;
    public Text gameOverText;
    public float triggerThreshold = .2f;
    public GameObject cameraContainer;

    private GameObject GenerateApple() {
        var obj = new GameObject();
        
        obj.AddComponent<MeshFilter>().sharedMesh = GetComponent<MeshFilter>().sharedMesh;
        obj.AddComponent<MeshRenderer>().material = appleMaterial;
        obj.name = "Apple";
        obj.transform.SetParent(transform.parent);
        obj.transform.localPosition = new Vector3(Random.Range(-.4f, .4f), Random.Range(-.3f, .3f), Random.Range(-.3f, .3f));
        obj.transform.localScale = transform.localScale;
        var objCollider = obj.AddComponent<SphereCollider>();
        objCollider.radius = .01f;
        objCollider.isTrigger = true;
        return obj;
    }

    private void AddPart(int i) {
        var selfTransform = transform;
        
        var part = new GameObject();
        var partTransform = part.transform;
        
        part.AddComponent<MeshFilter>().sharedMesh = GetComponent<MeshFilter>().sharedMesh;
        
        var sphereCollider = part.AddComponent<SphereCollider>();
        sphereCollider.radius = .01f;
        sphereCollider.isTrigger = true;

        part.AddComponent<MeshRenderer>().material = material;

        partTransform.SetParent(selfTransform.parent);
        partTransform.localScale = selfTransform.localScale;
        partTransform.localPosition = selfTransform.localPosition - (selfTransform.localRotation * Vector3.right * (distance * i));
        part.name = "BodyPart";
        
        _path.Add(partTransform.localPosition);
        _parts.Add(part);
    }

    private void Start() {
        gameOverText.enabled = false;
    }

    private void StartGame() {
        var selfTransform = transform;
        selfTransform.localPosition = new Vector3(0, 0, 0);
        selfTransform.localRotation = new Quaternion(0, 0, 0, 0);
        foreach (var part in _parts) {
            Destroy(part);
        }
        _parts.Clear();
        _path.Clear();
        Destroy(_apple);
        _gameOver = false;
        gameOverText.enabled = false;
        startText.enabled = false;

        _path.Insert(0, selfTransform.localPosition);

        for (var i=1; i < length; i++) {
            AddPart(i);
        }

        _apple = GenerateApple();
    }

    void Update() {
        _rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        _rightHand.TryGetFeatureValue(CommonUsages.primaryButton, out var menuButton);
        
        if (Input.GetKey(KeyCode.Escape)) {
            Application.Quit();
        }

        if (_gameOver) {
            if (menuButton) {
                StartGame();
            }

            return;
        }

        var selfTransform = transform;
        scoreText.text = "Score: " + _score;

        _rightHand.TryGetFeatureValue(CommonUsages.trigger, out var triggerValue);
        if (triggerValue >= triggerThreshold) {
            _rightHand.TryGetFeatureValue(CommonUsages.devicePosition, out var controllerPosition);
            controllerPosition += cameraContainer.transform.position - selfTransform.position;
            var selfRotation = selfTransform.rotation;
            selfTransform.rotation = Quaternion.RotateTowards(
                selfRotation,
                Quaternion.FromToRotation(Vector3.right, controllerPosition),
                rotationSpeed * Time.deltaTime
            );
        }

        selfTransform.localPosition += (selfTransform.localRotation * Vector3.right).normalized * (speed * Time.deltaTime);

        for (var i = 0; i < _parts.Count; i++) {
            var targetLength = distance * (i + 1);
            var accumulatedDistance = 0f;
            Vector3 interpolationPoint1 = new Vector3(),
                    interpolationPoint2 = new Vector3();
            var interpolationFraction = 0f;
            var found = false;
            int j;
            for (j = 0; j < _path.Count - 1; j++) {
                interpolationPoint1 = _path[j];
                interpolationPoint2 = _path[j + 1];
                var segmentLength = (interpolationPoint2 - interpolationPoint1).magnitude;
                if (accumulatedDistance <= targetLength && accumulatedDistance + segmentLength > targetLength) {
                    interpolationFraction = (targetLength - accumulatedDistance) / segmentLength;
                    found = true;
                    break;
                }
                accumulatedDistance += segmentLength;
            }

            if (!found) {
                continue;
            }

            _parts[i].transform.localPosition = Vector3.Lerp(interpolationPoint1, interpolationPoint2, interpolationFraction);

            if (i == _parts.Count - 1 && j < _path.Count - 2) {
                _path.RemoveRange(j + 2, _path.Count - (j + 2));
            }
        }
        
        _path.Insert(0, selfTransform.localPosition);
    }

    private void OnTriggerExit(Collider other) {
        if (other.gameObject == boundary) {
            Debug.Log("Boundary exit");
            gameOverText.enabled = true;
            _gameOver = true;
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.name == "BodyPart") {
            if (!_parts.GetRange(0, 5).Contains(other.gameObject)) {
                Debug.Log("Ate yourself");
                gameOverText.enabled = true;
                _gameOver = true;
            }
        } else if (other.gameObject == _apple) {
            Debug.Log("Ate apple");
            _score++;
            AddPart(100000);
            Destroy(_apple);
            _apple = GenerateApple();
        }
    }
}

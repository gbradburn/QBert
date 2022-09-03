using System;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    Camera _mainCamera;
    Transform _transform;

    void Awake()
    {
        _mainCamera = Camera.main;
        _transform = transform;
    }

    void LateUpdate()
    {
        _transform.LookAt(_mainCamera.transform);
    }
}

using System;
using DG.Tweening;
using UnityEngine;

public class TransportDisc : MonoBehaviour
{
    [SerializeField] float _rotateSpeed = 500f;
    [SerializeField] Vector3 _destinationLeft, _destinationRight;
    Vector3 _destination;
    Transform _transform, _disc;
    public bool _isActive;
    QBert _qBert;

    bool ReachedDestination => Mathf.Approximately(0f, Vector3.Distance(_transform.position, _destination));

    void Awake()
    {
        _transform = transform;
        _disc = _transform.GetChild(0);
    }

    void Update()
    {
        if (!_isActive) return;
        _disc.Rotate(0f, _rotateSpeed * Time.deltaTime, 0f, Space.Self);
        if (!ReachedDestination) return;
        _qBert.JumpToPlatform();
        _isActive = false;
        gameObject.SetActive(false);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.gameObject.TryGetComponent(out _qBert)) return;
        _qBert.transform.SetParent(_transform);
        ActivateDisc();
    }

    void ActivateDisc()
    {
        _destination = _transform.position.z > 10 ? _destinationRight : _destinationLeft; 
        _isActive = true;
        _transform.DOMove(_destination, 2f)
            .SetEase(Ease.Linear)
            .SetAutoKill(true);
    }
}

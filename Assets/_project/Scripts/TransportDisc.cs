using System;
using DG.Tweening;
using UnityEngine;

public class TransportDisc : MonoBehaviour
{
    [SerializeField] float _rotateSpeed = 500f;
    [SerializeField] Vector3 _destinationLeft, _destinationRight;
    Vector3 _destination;
    Transform _transform, _disc;
    public bool IsActive { get; private set; }
    QBert _qBert;

    bool ReachedDestination => Mathf.Approximately(0f, Vector3.Distance(_transform.position, _destination));
    public bool LeftDisc => _destination == _destinationLeft;

    void Awake()
    {
        _transform = transform;
        _disc = _transform.GetChild(0);
    }

    void Update()
    {
        if (!IsActive) return;
        _disc.Rotate(0f, _rotateSpeed * Time.deltaTime, 0f, Space.Self);
        if (!ReachedDestination) return;
        _qBert.JumpToPlatform();
        IsActive = false;
        gameObject.SetActive(false);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.gameObject.TryGetComponent(out _qBert)) return;
        _qBert.transform.SetParent(_transform);
        ActivateDisc();
        _qBert.LandedOnDisc(_transform.position.x < 0);
    }

    void ActivateDisc()
    {
        _destination = _transform.position.z > 10 ? _destinationRight : _destinationLeft; 
        IsActive = true;
        ScoreManager.Instance.UseTransportDisc();
        _transform.DOMove(_destination, 2f)
            .SetEase(Ease.Linear)
            .SetAutoKill(true);
    }
}

using DG.Tweening;
using UnityEngine;

public class Ugg : EnemyBase
{
    [SerializeField] Vector3[] _spawnPositions;

    protected override void OnEnable()
    {
        _transform.position = _spawnPositions[Random.Range(0, _spawnPositions.Length)];
        base.OnEnable();
    }

    protected override Vector3 GetRandomJumpLocation()
    {
        Vector3 position = _transform.position;
        Vector3 facing = _transform.eulerAngles;


        int legalLocations = GameManager.Instance.BoardManager.LegalJumpLocations(_legalJumpLocations, position, false, true);
        position = _legalJumpLocations[Random.Range(0, legalLocations)];
        
        return position;
    }

    protected override void Jump()
    {
        _jumping = true;
        _animator.SetBool(Jumping, true);
        Vector3 location = GetRandomJumpLocation();
        if (BoardManager.LandingOutOfBounds(location, true))
        {
            _dead = true;
            location = LocationOffEdge(location);
            transform.DOJump(location, 25, 1, 1).SetEase(Ease.InSine).SetAutoKill();
            EnemyDied.Invoke(this);
            return;
        }

        transform.DOJump(location, 4, 1, 0.5f).SetEase(Ease.Linear).SetAutoKill();
    }

    protected override Vector3 LocationOffEdge(Vector3 location)
    {
        Vector3 offEdgeLocation = location;
        offEdgeLocation.y = -10;
        if (location.z > 17)
        {
            offEdgeLocation.z = 30;
        }
        else if (location.z < 0)
        {
            offEdgeLocation.z = -10;
        }
        else if (location.x < 0)
        {
            offEdgeLocation.x = -10;
        }
        else if (location.x > 18)
        {
            offEdgeLocation.x = 28;
        }
        else
        {
            if (_transform.eulerAngles == BoardManager.SouthEast)
            {
                offEdgeLocation.x += 10;
            }
            else
            {
                offEdgeLocation.z -= 10;
            }
        }

        return offEdgeLocation;    
    }
}

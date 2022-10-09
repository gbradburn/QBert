using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class Coily : EnemyBase
{
    [SerializeField] GameObject _egg, _snake;

    protected override void OnEnable()
    {
        base.OnEnable();
        _egg.SetActive(true);
        _snake.SetActive(false);
    }
    protected override void OnCollisionEnter(Collision other)
    {
        base.OnCollisionEnter(other);
        if (_dead || !(_transform.position.y < 4) || !_egg.activeSelf) return;
        _egg.SetActive(false);
        _snake.SetActive(true);
    }
    
    protected override Vector3 GetRandomJumpLocation()
    {
        Vector3 position = _transform.position;
        Vector3 facing = _transform.eulerAngles;
        
        // Egg always jumps down
        if (_egg.activeSelf)
        {
            int locations = GameManager.Instance.BoardManager.LegalJumpLocations(_legalJumpLocations, _transform.position);
            return _legalJumpLocations[Random.Range(0, locations)];
        }
        
        // Is Q*Bert on a transport disc
        var transportDisc = GameManager.Instance.BoardManager.ActiveTransportDisc;
        if (transportDisc)
        {
            position.y += 3;
            if (transportDisc.LeftDisc)
            {
                position.x -= 3;
                facing = BoardManager.NorthWest;
            }
            else
            {
                position.z += 3;
                facing = BoardManager.NorthEast;
            }
        }
        else
        {
            // Snake always jumps toward player (unless disc is activated)
            int yOffset = GameManager.Instance.Qbert.position.y < position.y ? -3 : 3;
            position.y += yOffset;
            switch (yOffset)
            {
                // Q*Bert is below and to the left of snake
                case < 0 when GameManager.Instance.Qbert.position.z < position.z:
                    position.z -= 3;
                    facing = BoardManager.SouthWest;
                    break;
                // Q*Bert is below and to the right 
                case < 0:
                    position.x += 3;
                    facing = BoardManager.SouthEast;
                    break;
                // Q*Bert if above and to the right of snake
                case > 0 when GameManager.Instance.Qbert.position.z > position.z:
                    position.z += 3;
                    facing = BoardManager.NorthEast;
                    break;
                // Q*Bert is above and to the left of snake
                case > 0:
                    position.x -= 3;
                    facing = BoardManager.NorthWest;
                    break;
                case 0:
                    int legalLocations = GameManager.Instance.BoardManager.LegalJumpLocations(_legalJumpLocations,
                        _transform.position, _transform.position.y > 6);
                    position = _legalJumpLocations[Random.Range(0, legalLocations)];
                    break;
            }
        }

        transform.DORotate(facing, 0.25f).SetEase(Ease.Linear).SetAutoKill();
        
        return position;
    }

 
}

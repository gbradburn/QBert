using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [SerializeField] GameObject _squarePrefab;
    [SerializeField] TransportDisc _transportDiscPrefab;
    [SerializeField] Vector3[] _transportDiscPositions;

    List<IPlatform> _platforms;
    List<TransportDisc> _transportDiscs;
    Transform _transform;
    List<Vector3> _downDirections, _upDirections;

    void Awake()
    {
        _transform = transform;
        _transportDiscs = new List<TransportDisc>();
        _downDirections = new List<Vector3>
        {
            new Vector3(3, -3, 0),
            new Vector3(0, -3, -3)
        };
        _upDirections = new List<Vector3>
        {
            new Vector3(-3, 3, 0),
            new Vector3(0, 3, 3)
        };
        
    }

    public int UnFlippedPlatforms => _platforms.Count(p => !p.Flipped);

    public static readonly Vector3 NoChange = Vector3.up;
    public static readonly Vector3 NorthEast = Vector3.zero;
    public static readonly Vector3 NorthWest = new Vector3(0, 270, 0);
    public static readonly Vector3 SouthEast = new Vector3(0, 90, 0);
    public static readonly Vector3 SouthWest = new Vector3(0, 180, 0);    
    
    public static bool LandingOutOfBounds(Vector3 targetPosition)
    {
        return targetPosition.z is > 20 or < 0 || targetPosition.x is < 0 or > 18 || targetPosition.y < 2.5f;
    }
    
    public int LegalJumpLocations(List<Vector3> legalJumpPositions, Vector3 position, bool down = true)
    {
        legalJumpPositions.Clear();

        if (down)
        {
            foreach (var vector in _downDirections)
            {
                Vector3 landingPosition = position + vector;
                if (!BoardManager.LandingOutOfBounds(landingPosition))
                {
                    legalJumpPositions.Add(landingPosition);
                }
            }

            return legalJumpPositions.Count;
        }

        foreach (var vector in _upDirections)
        {
            Vector3 landingPosition = position + vector;
            if (!BoardManager.LandingOutOfBounds(position))
            {
                legalJumpPositions.Add(position);
            }
        }

        return legalJumpPositions.Count;
    }     

    public TransportDisc TransportDiscAtPosition(Vector3 position)
    {
        return _transportDiscs.FirstOrDefault(disc => Vector3.Distance(position, disc.transform.position) < 3f);
    }
    
    public void SetUpBoard()
    {
        if (_transportDiscs.Count > 0)
        {
            ResetTransportDiscs();
        }
        else
        {
            CreateTransportDiscs();
        }
        if (_platforms?.Count > 0)
        {
            ResetPlatforms();
            return;
        }
        _platforms = new List<IPlatform>();
        int blocksPerRow = 7;
        int y = 0;
        int startZ = 0;
        while (blocksPerRow > 0)
        {
            for (int i = 0; i < blocksPerRow; ++i)
            {
                GameObject square = Instantiate(_squarePrefab, _transform);
                square.transform.localPosition = new Vector3(i * 3, y, startZ + i * 3);
                _platforms.Add(square.GetComponentInChildren<IPlatform>());
            }

            --blocksPerRow;
            startZ += 3;
            y += 3;
        }
    }

    public void ShowVictoryEffect()
    {
        StartCoroutine(FlashPlatformColors(8.5f));
    }

    IEnumerator FlashPlatformColors(float duration)
    {
        while (duration > 0f)
        {
            duration -= Time.deltaTime;
            foreach (var platform in _platforms)
            {
                platform.SetPlatformColor(Random.ColorHSV());
            }

            yield return null;
        }
    }
    
    void ResetPlatforms()
    {
        if (UnFlippedPlatforms == _platforms.Count) return;
        foreach (var platform in _platforms.Where(platform => platform.Flipped))
        {
            platform.SetFlippedState(false);
        }
    }
    
    void CreateTransportDiscs()
    {
        foreach (Vector3 discPosition in _transportDiscPositions)
        {
            var disc = Instantiate(_transportDiscPrefab, discPosition, Quaternion.identity)
                .GetComponent<TransportDisc>();
            _transportDiscs.Add(disc);
            disc.transform.SetParent(_transform);
        }
    }
    
    void ResetTransportDiscs()
    {
        for (var i = 0; i < _transportDiscPositions.Length; ++i)
        {
            _transportDiscs[i].transform.position = _transportDiscPositions[i];
            _transportDiscs[i].gameObject.SetActive(true);
        }
    }
}

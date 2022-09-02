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

    void Awake()
    {
        _transform = transform;
        _transportDiscs = new List<TransportDisc>();
    }

    public int UnFlippedPlatforms => _platforms.Count(p => !p.Flipped);

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

    public void ResetPlatforms()
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

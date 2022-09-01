using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlatform
{
    bool Flipped { get; }
    void SetFlippedState(bool flipped);
}

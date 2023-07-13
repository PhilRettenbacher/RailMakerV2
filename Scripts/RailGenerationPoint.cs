using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct RailGenerationPoint
{
    public Vector3 Position;
    public Quaternion PositionDirection;
    public Quaternion NormalDirection;
    public float Radius;
    public bool ConnectToNext;
}

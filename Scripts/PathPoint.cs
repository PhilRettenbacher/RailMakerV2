using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct PathPoint
{
    
    public float radius;
    public float angle; //Full angle between entry and exit (Straight = PI Radians)
    public bool hasRadius;
    public Vector3 position;
    public Vector3 center;
    public Vector3 startPoint;
    public Vector3 endPoint;

    public PathPoint(Vector3 pos)
    {
        position = pos;
        radius = 0;
        angle = 0;
        center = pos;
        startPoint = pos;
        endPoint = pos;
        hasRadius = false;
    }

    public PathPoint(float radius, float angle, Vector3 position, Vector3 center, Vector3 startPoint, Vector3 endPoint)
    {
        this.radius = radius;
        this.angle = angle;
        this.hasRadius = !Mathf.Approximately(radius, 0);
        this.position = position;
        this.center = center;
        this.startPoint = startPoint;
        this.endPoint = endPoint;
    }

}

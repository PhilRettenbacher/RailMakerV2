using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Path
{
    public List<PathPoint> points = new List<PathPoint>();

    public Path()
    { }
    public Path(List<PathPoint> points)
    {
        this.points = points;
    }

    public Path(Vector3 start, Vector3 end)
    {
        points.Add(new PathPoint() { position = start });
        points.Add(new PathPoint() { position = end });
    }

    public PathPoint RecalculatePoint(int index)
    {
        var currPoint = points[index];

        float angle;
        Vector3 center;
        Vector3 startPoint;
        Vector3 endPoint; 

        if(index == 0 || index == points.Count-1 || !currPoint.hasRadius)
        {
            angle = 0;
            center = currPoint.position;
            startPoint = currPoint.position;
            endPoint = currPoint.position;
            return new PathPoint(0, angle, currPoint.position, center, startPoint, endPoint);
        }

        var previousPoint = points[index - 1];
        var nextPoint = points[index + 1];

        var entryDirection = (currPoint.position - previousPoint.position).normalized;
        var exitDirection = (nextPoint.position - currPoint.position).normalized;

        angle = Vector3.Angle(-entryDirection, exitDirection) * Mathf.Deg2Rad;

        float distance = currPoint.radius / Mathf.Tan(angle / 2);

        startPoint = currPoint.position - entryDirection * distance;
        endPoint = currPoint.position + exitDirection * distance;

        Vector3 centerDirection = exitDirection - entryDirection;
        float centerLength = distance / Mathf.Cos(angle / 2);

        center = currPoint.position + centerDirection.normalized * centerLength;

        return new PathPoint(currPoint.radius, angle, currPoint.position, center, startPoint, endPoint);
    }
}

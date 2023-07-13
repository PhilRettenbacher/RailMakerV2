using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[SelectionBase]
public class Rail : MonoBehaviour
{
    public Path railPath;
    [SerializeField, HideInInspector]
    private MeshFilter mf;
    [SerializeField, HideInInspector]
    private GameObject meshGM;
    [SerializeField, HideInInspector]
    private MeshRenderer mr;

    public Mesh railMesh;
    public int pointCount => railPath.points.Count;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void RegenerateMesh()
    {
        if (!meshGM)
        {
            meshGM = new GameObject("RailMesh");
            meshGM.transform.SetParent(transform, false);
        }
        if (!mf)
        {
            mf = meshGM.AddComponent<MeshFilter>();
            mf.sharedMesh = railMesh;
        }
        if (!mr)
        {
            mr = meshGM.AddComponent<MeshRenderer>();
        }

        var genPoints = railPath.points.SelectMany(GetGenerationPoints).ToList();

        MeshGenerator.GenerateMesh(railMesh, genPoints, RailShape.GenerateCircle(10));
    }

    private List<RailGenerationPoint> GetGenerationPoints(PathPoint point, int index)
    {
        List<RailGenerationPoint> genPoints = new List<RailGenerationPoint>();

        Quaternion directionMean = Quaternion.identity;
        Quaternion directionTo = Quaternion.identity;
        Quaternion directionFrom = Quaternion.identity;

        bool isFirst = index == 0;
        bool isLast = index == pointCount - 1;


        if (!isFirst)
        {
            directionTo = Quaternion.LookRotation(point.position - railPath.points[index - 1].position);
        }
        if (!isLast)
        {
            directionFrom = Quaternion.LookRotation(railPath.points[index + 1].position - point.position);
        }

        if (isFirst || isLast)
        {
            Quaternion dir = isFirst ? directionFrom : directionTo;

            genPoints.Add(new RailGenerationPoint() { Position = point.position, Radius = 1, PositionDirection = dir, NormalDirection = dir, ConnectToNext = isFirst });
            return genPoints;
        }

        Vector3 from = railPath.points[index + 1].position - point.position;
        Vector3 to = point.position - railPath.points[index - 1].position;

        directionMean = Quaternion.LookRotation(from.normalized + to.normalized);

        if (!point.hasRadius)
        {
            genPoints.Add(new RailGenerationPoint()
            {
                Position = point.position,
                Radius = 1,
                PositionDirection = directionMean,
                NormalDirection = directionTo,
                ConnectToNext = false });
            genPoints.Add(new RailGenerationPoint() 
            { 
                Position = point.position, 
                Radius = 1, 
                PositionDirection = directionMean,
                NormalDirection = directionFrom,
                ConnectToNext = true });
        }
        else
        {
            //multiple points...

            float bendAngle = Mathf.PI - point.angle;

            int pointCount = Mathf.FloorToInt(bendAngle * point.radius) + 2;

            Vector3 centerToStart = point.startPoint - point.center;

            Quaternion centerAxis = Quaternion.LookRotation(Vector3.Cross(from, -to), centerToStart);

            float anglePerStep = bendAngle / (pointCount - 1);

            for(int i = 0; i<pointCount; i++)
            {
                Quaternion localRot = Quaternion.Euler(0, 0, i * anglePerStep * Mathf.Rad2Deg);

                Vector3 pos = centerAxis * localRot * Vector3.up * point.radius + point.center;
                genPoints.Add(new RailGenerationPoint()
                {
                    Position = pos,
                    Radius = 1,
                    PositionDirection = directionMean,
                    NormalDirection = directionFrom,
                    ConnectToNext = true
                });
            }

        }
        return genPoints;
    }

    private void Reset()
    {
        railPath = new Path(Vector3.zero, Vector3.forward * 5);

    }
}

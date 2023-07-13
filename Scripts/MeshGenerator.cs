using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public static class MeshGenerator
{
    public static void GenerateMesh(Mesh mesh, List<RailGenerationPoint> points, RailShape shape)
    {
        int segmentCount = points.Count(x => x.ConnectToNext);

        int vertexCount = shape.vertices.Count * points.Count;
        int triangleCount = shape.lines.Count * (segmentCount) * 6;


        Vector3[] vertices = new Vector3[vertexCount];
        Vector2[] uvs = new Vector2[vertexCount];
        Vector3[] normals = new Vector3[vertexCount];
        int[] tris = new int[triangleCount];

        for (int i = 0; i < points.Count; i++)
        {
            GenerateRailPointMesh(points[i], shape, vertices, normals, uvs, i);
            //Quaternion localRailDirection = Quaternion.identity;
            //Vector2 scewVector = Vector2.zero;

            //if (i == 0)
            //    localRailDirection = Quaternion.LookRotation(points[i + 1].Position - points[i].Position);
            //else if (i == points.Count - 1)
            //{
            //    localRailDirection = Quaternion.LookRotation(points[i].Position - points[i - 1].Position);
            //}
            //else
            //{
            //    Vector3 entry = (points[i].Position - points[i - 1].Position).normalized;
            //    Vector3 exit = (points[i + 1].Position - points[i].Position).normalized;
            //    localRailDirection = Quaternion.LookRotation(points[i].Position - points[i - 1].Position);

            //    Quaternion targetRailDirection = Quaternion.LookRotation(entry.normalized + exit.normalized);

            //    float angle = (180 - Vector3.Angle(entry, exit)) / 2;
            //    float skewedLength = 1 / Mathf.Sin(angle * Mathf.Deg2Rad);
            //    float skewedDiff = Mathf.Tan(Mathf.PI / 2 - angle * Mathf.Deg2Rad);
            //    scewVector = (Quaternion.Inverse(targetRailDirection) * (entry - exit)).normalized * (skewedDiff);

            //    //Debug.Log("direction: " + scewVector);
            //    //Debug.Log("skewedLength: " + skewedLength);
            //    //Debug.Log("skewedDiff: " + skewedDiff);
            //    //Debug.Log("Angle: " + angle);
            //}

            //Vector3 currCenterPos = points[i].Position;

            //float cosAngle = Mathf.Cos(-points[i].Angle * Mathf.Deg2Rad);
            //float sinAngle = Mathf.Sin(-points[i].Angle * Mathf.Deg2Rad);

            //bool isEdge = points[i].IsEdge && i > 0 && i < points.Count - 1;

            //for (int k = 0; k < (isEdge ? 2 : 1); k++)
            //{
            //    for (int j = 0; j < shape.vertices.Count; j++)
            //    {
            //        Vector2 pos = shape.vertices[j];
            //        Vector2 normal = shape.normals[j];

            //        if (!Mathf.Approximately(points[i].angle, 0))
            //        {
            //            pos = new Vector2(pos.x * cosAngle - pos.y * sinAngle, pos.x * sinAngle + pos.y * cosAngle);
            //            normal = new Vector2(normal.x * cosAngle - normal.y * sinAngle, normal.x * sinAngle + normal.y * cosAngle);
            //        }

            //        Quaternion normalRot = localRailDirection;
            //        if (k == 1)
            //            normalRot = Quaternion.LookRotation(points[i + 1].pos - points[i].pos);

            //        Vector3 newVert = currCenterPos + localRailDirection * (new Vector3(pos.x, pos.y, Vector2.Dot(pos, scewVector)) * radius);
            //        Vector3 newNormal = normalRot * normal;

            //        vertices.Add(newVert);
            //        normals.Add(newNormal.normalized);
            //        uvs.Add(new Vector2(shape.us[j], currDistance / uvTiling));

            //        if (addCap)
            //        {
            //            bool inFront = i == 0;
            //            capVerts.Add(newVert);
            //            capNorms.Add(localRailDirection * (inFront ? Vector3.back : Vector3.forward));
            //            capUvs.Add(new Vector2(shape.us[j], currDistance / uvTiling));
            //        }
            //    }
            //    newPointCount++;
            //}

            //if (i != points.Count - 1)
            //    currDistance += Vector3.Distance(points[i].pos, points[i + 1].pos);
        }

        int currSegmentCount = 0;

        for (int pointIdx = 0; pointIdx < points.Count - 1; pointIdx++)
        {
            if(!points[pointIdx].ConnectToNext)
            {
                continue;
            }

            for (int i = 0; i < shape.lines.Count / 2; i++)
            {
                int VertSegmentCount = shape.vertices.Count;

                int currTriIdx = currSegmentCount * shape.lines.Count * 3 + i * 6;

                int vert1 = shape.lines[i * 2] + pointIdx * VertSegmentCount;
                int vert2 = shape.lines[(i * 2 + 1) % shape.lines.Count] + pointIdx * VertSegmentCount;
                int vert3 = shape.lines[i * 2] + (1 + pointIdx) * VertSegmentCount;
                int vert4 = shape.lines[(i * 2 + 1) % shape.lines.Count] + (1 + pointIdx) * VertSegmentCount;

                //Debug.Log("vert1: " + vertices[vert1]);
                //Debug.Log("vert2: " + vertices[vert2]);
                //Debug.Log("vert3: " + vertices[vert3]);
                //Debug.Log("vert4: " + vertices[vert4]);

                tris[currTriIdx] = vert2;
                tris[currTriIdx + 1] = vert1;
                tris[currTriIdx + 2] = vert3;

                tris[currTriIdx + 3] = vert4;
                tris[currTriIdx + 4] = vert2;
                tris[currTriIdx + 5] = vert3;
            }
            currSegmentCount++;
        }

        //int vertStartPoint = vertices.Count;
        //int triStartPoint = (newPointCount - 1) * shape.lines.Count * 3;



        //Caps
        //for (int j = 0; j < 2; j++)
        //{
        //    for (int i = 0; i < shape.capTris.Count; i++)
        //    {
        //        tris[triStartPoint + j * shape.capTris.Count + i] = vertStartPoint + j * shape.vertices.Count + shape.capTris[j == 0 ? i : shape.capTris.Count - i - 1];
        //    }
        //}


        //vertices.AddRange(capVerts);
        //normals.AddRange(capNorms);
        //uvs.AddRange(capUvs);

        mesh.Clear();

        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.triangles = tris;
        mesh.SetUVs(0, uvs);
    }

    private static void GenerateRailPointMesh(RailGenerationPoint point, RailShape shape, Vector3[] vertices, Vector3[] normals, Vector2[] uvs, int index)
    {
        int offset = shape.vertices.Count * index;

        for(int i = 0; i<shape.vertices.Count; i++)
        {
            vertices[offset + i] = point.Position + point.PositionDirection * (shape.vertices[i] * point.Radius);
            normals[offset + i] = point.NormalDirection * shape.normals[i];
            uvs[offset + i] = new Vector2(shape.us[i], index);
        }
    }
}

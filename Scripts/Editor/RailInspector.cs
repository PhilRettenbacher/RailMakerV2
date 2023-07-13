using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Rail))]
public class RailInspector : Editor
{
    const int START_HANDLE_INDEX = 50;
    const float LINE_DISTANCE_PIXEL_THRESHOLD = 10;

    readonly Color HANDLE_COLOR = Color.white;
    readonly Color HANDLE_HIGHLIGHTED_COLOR = Color.yellow;
    readonly Color HANDLE_DELETE_COLOR = Color.red;

    readonly Color PATH_COLOR = Color.grey;
    readonly Color PATH_HIGHLIGHTED_COLOR = Color.green;

    Selection selection;

    Vector2 previousMousePosition;
    Rail rail;
    bool needsRepaint;

    SerializedProperty m_railPath;
    SerializedProperty m_points;

    private void OnEnable()
    {
        rail = target as Rail;
        selection = new Selection();

        m_railPath = serializedObject.FindProperty(nameof(rail.railPath));
        m_points = m_railPath.FindPropertyRelative(nameof(rail.railPath.points));
    }
    private void OnSceneGUI()
    {
        Event guiEvent = Event.current;



        if (guiEvent.type == EventType.Repaint)
        {
            Draw(guiEvent.type);
            needsRepaint = false;
        }
        else if (guiEvent.type == EventType.Layout)
        {
            selection.nearestLine = -1;
            Draw(guiEvent.type);
            selection.defaultControlId = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(selection.defaultControlId);
        }
        else if (guiEvent.type == EventType.MouseDown)
        {
            if (guiEvent.button == 0)
            {           
                int closestHandle = HandleUtility.nearestControl;
                if (IsNearestHandleRailPoint(closestHandle))
                {
                    selection.selectedHandle = closestHandle - START_HANDLE_INDEX;
                }
                else
                {
                    selection.selectedHandle = -1;
                }
                previousMousePosition = Event.current.mousePosition;

                selection.selectedLine = -1;

                if (HandleUtility.nearestControl == selection.defaultControlId && selection.nearestLine != -1)
                {
                    selection.selectedLine = selection.nearestLine;
                }
            }
        }
        else if (guiEvent.type == EventType.MouseUp)
        {
            if (guiEvent.button == 0)
            {
                previousMousePosition = Vector2.zero;

                if (guiEvent.modifiers.HasFlag(EventModifiers.Control) && selection.selectedHandle != -1 && rail.pointCount > 2)
                {
                    Debug.Log("Delete Point");

                    DeletePointAt(selection.selectedHandle);

                    serializedObject.ApplyModifiedProperties();

                    selection.selectedHandle = -1;
                    selection.nearestLine = -1;
                    selection.selectedLine = -1;
                    needsRepaint = true;
                }

                selection.selectedHandle = -1;


                if (selection.nearestLine == selection.selectedLine && selection.selectedLine != -1)
                {
                    Vector3 newPoint = HandleUtility.ClosestPointToPolyLine(GetLineSegment(selection.nearestLine));
                    Debug.Log("Place Point");

                    InsertPointAt(selection.nearestLine + 1, rail.transform.InverseTransformPoint(newPoint));
                }
            }
        }
        else if (guiEvent.type == EventType.MouseDrag)
        {
            if (selection.selectedHandle != -1)
            {
                int pointIndex = selection.selectedHandle;
                Ray mouseRay = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
                Plane movementPlane = GetPointMovementPlane(pointIndex);

                if (movementPlane.Raycast(mouseRay, out float distance))
                {

                    m_points.GetArrayElementAtIndex(pointIndex).FindPropertyRelative("position").vector3Value = rail.transform.InverseTransformPoint(mouseRay.GetPoint(distance));
                    serializedObject.ApplyModifiedProperties();
                    RecalculateAdjacentPoints(pointIndex);

                    needsRepaint = true;

                }
            }
        }

        if (needsRepaint)
        {
            HandleUtility.Repaint();
        }
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if(GUILayout.Button("Regenerate"))
        {
            rail.RegenerateMesh();
        }
    }

    void Draw(EventType eventType)
    {
        if(IsNearestHandleRailPoint(HandleUtility.nearestControl))
        {
            selection.nearestHandle = HandleUtility.nearestControl - START_HANDLE_INDEX;
        }
        else
        {
            selection.nearestHandle = -1;
        }

        for (int i = 0; i < rail.pointCount; i++)
        {
            DrawPathPoint(i, eventType);
        }
    }

    void DrawPathPoint(int index, EventType eventType)
    {
        var prevColor = Handles.color;



        var point = rail.railPath.points[index];

        var pointPos = rail.transform.TransformPoint(point.position);


        if (index < rail.pointCount - 1)
        {
            var lineSegment = GetLineSegment(index);
            if (eventType == EventType.Repaint)
            {
                if (selection.nearestLine == index)
                {
                    Handles.color = PATH_HIGHLIGHTED_COLOR;
                }
                else
                {
                    Handles.color = PATH_COLOR;
                }
                Handles.DrawAAPolyLine(lineSegment);

                if(point.hasRadius)
                {
                    Vector3 firstTangent = point.startPoint - point.center;
                    Vector3 cross = Vector3.Cross(firstTangent, point.endPoint - point.center);
                    Handles.SphereHandleCap(0, rail.transform.TransformPoint(point.center), Quaternion.identity, 0.2f, Event.current.type);
                    Handles.DrawWireArc(rail.transform.TransformPoint(point.center), rail.transform.TransformDirection(cross), rail.transform.TransformDirection(firstTangent), Vector3.Angle(point.position - point.startPoint, point.endPoint - point.position), point.radius);
                }
            }
            else if (eventType == EventType.Layout)
            {
                var lineDistance = HandleUtility.DistanceToLine(lineSegment[0], lineSegment[1]);
                if (lineDistance < LINE_DISTANCE_PIXEL_THRESHOLD)
                {
                    selection.nearestLine = index;
                    needsRepaint = true;
                }
            }
        }

        var handleSize = HandleUtility.GetHandleSize(pointPos) * 0.2f;

        if (selection.nearestHandle == index)
        {
            if (Event.current.modifiers.HasFlag(EventModifiers.Control))
            {
                Handles.color = HANDLE_DELETE_COLOR;
            }
            else
            {
                Handles.color = HANDLE_HIGHLIGHTED_COLOR;
            }
        }
        else
        {
            Handles.color = HANDLE_COLOR;
        }

        CreateHandleCap(START_HANDLE_INDEX + index, pointPos, Quaternion.identity, handleSize, eventType);

        Handles.color = prevColor;
    }

    void CreateHandleCap(int id, Vector3 position, Quaternion rotation, float size, EventType eventType)
    {
        Handles.SphereHandleCap(id, position, rotation, size, eventType);
    }

    bool IsNearestHandleRailPoint(int nearestHandleId)
    {
        return nearestHandleId >= START_HANDLE_INDEX && nearestHandleId < START_HANDLE_INDEX + rail.pointCount;
    }

    void InsertPointAt(int index, Vector3 position)
    {
        m_points.InsertArrayElementAtIndex(index);
        var newPointProperty = m_points.GetArrayElementAtIndex(index);
        newPointProperty.FindPropertyRelative("position").vector3Value = position;
        newPointProperty.FindPropertyRelative("radius").floatValue = 0;

        serializedObject.ApplyModifiedProperties();
        RecalculateAdjacentPoints(index);
    }

    void RecalculateAdjacentPoints(int index)
    {
        if (index > 0)
        {
            var prevPointProperty = m_points.GetArrayElementAtIndex(index - 1);
            SetSerializedPropertyToPoint(prevPointProperty, rail.railPath.RecalculatePoint(index - 1));
        }

        if (index < rail.pointCount - 1)
        {
            var prevPointProperty = m_points.GetArrayElementAtIndex(index + 1);
            SetSerializedPropertyToPoint(prevPointProperty, rail.railPath.RecalculatePoint(index + 1));
        }

        var newPointProperty = m_points.GetArrayElementAtIndex(index);

        var newPoint = rail.railPath.RecalculatePoint(index);

        SetSerializedPropertyToPoint(newPointProperty, newPoint);

        serializedObject.ApplyModifiedProperties();
    }

    void DeletePointAt(int index)
    {
        m_points.DeleteArrayElementAtIndex(index);
    }

    Vector3[] GetLineSegment(int index)
    {
        if (index < 0 || index >= rail.pointCount - 1)
            return null;

        var pointPos = GetWorldPoint(index);
        var endPos = GetWorldPoint(index + 1);

        return new[] { pointPos, endPos };
    }

    Vector3 GetWorldPoint(int index)
    {
        return rail.transform.TransformPoint(rail.railPath.points[index].position);
    }

    Plane GetPointMovementPlane(int index)
    {
        if (index != rail.pointCount - 1)
        {
            return new Plane(GetWorldPoint(index), GetWorldPoint(index + 1), GetWorldPoint(index) + Vector3.up);
        }
        else if (index != 0)
        {
            return GetPointMovementPlane(index - 1);
        }
        return new Plane();
    }

    void SetSerializedPropertyToPoint(SerializedProperty pointProperty, PathPoint point)
    {
        pointProperty.FindPropertyRelative(nameof(point.position)).vector3Value = point.position;
        pointProperty.FindPropertyRelative(nameof(point.center)).vector3Value = point.center;
        pointProperty.FindPropertyRelative(nameof(point.startPoint)).vector3Value = point.startPoint;
        pointProperty.FindPropertyRelative(nameof(point.endPoint)).vector3Value = point.endPoint;

        pointProperty.FindPropertyRelative(nameof(point.angle)).floatValue = point.angle;
        pointProperty.FindPropertyRelative(nameof(point.radius)).floatValue = point.radius;

        pointProperty.FindPropertyRelative(nameof(point.hasRadius)).boolValue = point.hasRadius;
    }

    class Selection
    {
        internal int nearestHandle = -1;
        internal int selectedHandle = -1;
        internal int nearestLine = -1;
        internal int selectedLine = -1;
        internal int defaultControlId;
    }
}

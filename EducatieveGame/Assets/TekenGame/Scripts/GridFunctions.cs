using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridFunctions : MonoBehaviour
{
    private GridGenerator _gridGen;

    private GridGenerator GridGen { get => _gridGen; set => _gridGen = value; }

    private void Awake()
    {
        GridGen = GetComponent<GridGenerator>();
    }

    public bool MouseInGrid(Vector3 position) //kijkt na of de muis zich in het grid bevindt
    {
        float minX = GridGen.GridPoints[0].x;
        float minY = GridGen.GridPoints[0].y;
        float maxX = GridGen.GridPoints[^1].x;
        float maxY = GridGen.GridPoints[^1].y;
        return position.x >= minX && position.x <= maxX && position.y >= minY && position.y <= maxY;
    }

    public bool MouseInGridCell(Vector3 position, Vector3 gridCellLeftBottom, Vector3 gridCellRightTop)
    {
        float minX = gridCellLeftBottom.x;
        float minY = gridCellLeftBottom.y;
        float maxX = gridCellRightTop.x;
        float maxY = gridCellRightTop.y;
        return position.x >= minX && position.x <= maxX && position.y >= minY && position.y <= maxY;
    }

    public Vector3[] CenterGridPoints(int cellSize)
    {
        List<Vector3> centerPoints = new List<Vector3>();

        HashSet<float> uniqueXValues = new HashSet<float>();
        HashSet<float> uniqueYValues = new HashSet<float>();

        foreach (Vector3 point in GridGen.GridPoints)
        {
            uniqueXValues.Add(point.x);
            uniqueYValues.Add(point.y);
        }

        float maxX = uniqueXValues.Max();
        float maxY = uniqueYValues.Max();

        foreach (Vector3 cornerPoint in GridGen.GridPoints)
        {
            if (cornerPoint.x != maxX && cornerPoint.y != maxY)
            {
                Vector3 centerPoint = new Vector3(cornerPoint.x + 0.5f * cellSize, cornerPoint.y + 0.5f * cellSize, 0);

                centerPoints.Add(centerPoint);
            }
        }

        return centerPoints.ToArray();
    }

    public Vector3 ClosestPositionOnGrid(Vector3 position) //berekent de dichtste plaats op het grid voor het startpunt
    {
        Vector3 closestGridPoint = GridGen.GridPoints[0];
        float closestDistance = Vector3.Distance(position, closestGridPoint);

        for (int i = 1; i < GridGen.GridPoints.Length; i++)
        {
            float distance = Vector3.Distance(position, GridGen.GridPoints[i]);

            if (distance < closestDistance)
            {
                closestGridPoint = GridGen.GridPoints[i];
                closestDistance = distance;
            }
        }

        return closestGridPoint;
    }

    public Vector3 ClosestPosition(Vector3 position, Vector3 targetGridPoint, int cellSize) //berekent de dichtste plaats op het grid voor de volgende lijn
    {
        Vector3 closestGridPoint = Vector3.zero;
        float closestDistanceToMouse = Vector3.Distance(position, closestGridPoint);

        for (int i = 0; i < GridGen.GridPoints.Length; i++)
        {
            Vector3 gridPoint = GridGen.GridPoints[i];
            float distanceToMouse = Vector3.Distance(position, gridPoint);

            if (Mathf.Abs(gridPoint.x - targetGridPoint.x) <= cellSize && Mathf.Abs(gridPoint.y - targetGridPoint.y) <= cellSize)
            {
                if (distanceToMouse < closestDistanceToMouse)
                {
                    closestGridPoint = gridPoint;
                    closestDistanceToMouse = distanceToMouse;
                }
            }
        }

        return closestGridPoint;
    }
}

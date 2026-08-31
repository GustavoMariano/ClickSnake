using System.Collections.Generic;
using UnityEngine;

public class FoodController : MonoBehaviour
{
    public Vector2Int GridPosition => new(
        Mathf.RoundToInt(transform.position.x),
        Mathf.RoundToInt(transform.position.y));

    public bool MoveToRandomPosition(
        int minX,
        int maxX,
        int minY,
        int maxY,
        HashSet<Vector2Int> occupiedPositions)
    {
        List<Vector2Int> availablePositions = new();

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                Vector2Int position = new(x, y);

                if (!occupiedPositions.Contains(position))
                    availablePositions.Add(position);
            }
        }

        if (availablePositions.Count == 0)
            return false;

        Vector2Int newPosition =
            availablePositions[Random.Range(0, availablePositions.Count)];

        transform.position = new Vector3(
            newPosition.x,
            newPosition.y,
            0);

        return true;
    }
}
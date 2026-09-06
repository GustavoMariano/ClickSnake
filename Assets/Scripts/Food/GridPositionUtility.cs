using System.Collections.Generic;
using UnityEngine;

public static class GridPositionUtility
{
    public static bool TryGetRandomAvailablePosition(
        int minX,
        int maxX,
        int minY,
        int maxY,
        HashSet<Vector2Int> occupiedPositions,
        out Vector2Int position)
    {
        List<Vector2Int> availablePositions = new();

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                Vector2Int candidate = new(x, y);

                if (!occupiedPositions.Contains(candidate))
                    availablePositions.Add(candidate);
            }
        }

        if (availablePositions.Count == 0)
        {
            position = default;
            return false;
        }

        position = availablePositions[Random.Range(0, availablePositions.Count)];
        return true;
    }
}

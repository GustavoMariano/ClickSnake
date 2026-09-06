using System.Collections.Generic;
using UnityEngine;

public class BadFoodController : MonoBehaviour
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
        if (!GridPositionUtility.TryGetRandomAvailablePosition(
                minX,
                maxX,
                minY,
                maxY,
                occupiedPositions,
                out Vector2Int newPosition))
        {
            return false;
        }

        transform.position = new Vector3(
            newPosition.x,
            newPosition.y,
            0);

        return true;
    }
}

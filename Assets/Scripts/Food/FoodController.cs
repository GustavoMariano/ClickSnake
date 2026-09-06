using System.Collections.Generic;
using UnityEngine;

public enum FoodType
{
    Normal,
    Special
}

public class FoodController : MonoBehaviour
{
    private const float SpecialFoodChance = 0.05f;

    private SpriteRenderer _spriteRenderer;
    private Color _normalColor;

    public FoodType CurrentType { get; private set; }
    public int ScoreAmount => CurrentType == FoodType.Special ? 2 : 1;
    public int GrowthAmount => 1;

    public Vector2Int GridPosition => new(
        Mathf.RoundToInt(transform.position.x),
        Mathf.RoundToInt(transform.position.y));

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_spriteRenderer != null)
            _normalColor = _spriteRenderer.color;

        RandomizeType();
    }

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

        RandomizeType();

        return true;
    }

    private void RandomizeType()
    {
        CurrentType = Random.value < SpecialFoodChance
            ? FoodType.Special
            : FoodType.Normal;

        if (_spriteRenderer == null)
            return;

        _spriteRenderer.color = CurrentType == FoodType.Special
            ? Color.yellow
            : _normalColor;
    }
}

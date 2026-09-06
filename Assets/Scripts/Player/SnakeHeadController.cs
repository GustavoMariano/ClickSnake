using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SnakeHeadController : MonoBehaviour
{
    private const int BadFoodSpawnThreshold = 8;
    private const int BadFoodScorePenalty = 2;
    private const float BadFoodSpawnChance = 0.5f;
    private const float InitialFoodTime = 20f;
    private const float FoodTimeDecrease = 0.5f;
    private const float MinimumFoodTime = 3f;

    private static readonly Vector2Int[] Directions =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    [SerializeField] private int _minX = -7;
    [SerializeField] private int _maxX = 7;
    [SerializeField] private int _minY = -4;
    [SerializeField] private int _maxY = 4;

    [SerializeField] private FoodController _food;
    [SerializeField] private BadFoodController _badFood;
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private Transform _bodySegmentPrefab;
    [SerializeField] private List<Transform> _bodySegments = new();

    private Vector2Int _gridPosition;
    private int _pendingGrowth;
    private bool _badFoodActive;
    private float _currentFoodTimeLimit;
    private float _foodTimeRemaining;

    private void Awake()
    {
        _pendingGrowth = 0;
        _badFoodActive = false;
        _currentFoodTimeLimit = InitialFoodTime;
        ResetFoodTimer();

        if (_badFood != null)
            _badFood.gameObject.SetActive(false);

        _gridPosition = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.y));
    }

    private void Update()
    {
        if (_gameManager != null && _gameManager.IsGameOver)
            return;

        UpdateFoodTimer();

        Vector2Int direction = GetDirection();

        if (direction == Vector2Int.zero)
            return;

        Move(direction);
    }

    private Vector2Int GetDirection()
    {
        if (Keyboard.current.wKey.wasPressedThisFrame)
            return Vector2Int.up;

        if (Keyboard.current.sKey.wasPressedThisFrame)
            return Vector2Int.down;

        if (Keyboard.current.aKey.wasPressedThisFrame)
            return Vector2Int.left;

        if (Keyboard.current.dKey.wasPressedThisFrame)
            return Vector2Int.right;

        return Vector2Int.zero;
    }

    private void Move(Vector2Int direction)
    {
        Vector2Int newPosition = _gridPosition + direction;

        if (!CanMoveTo(newPosition))
            return;

        Vector2Int previousPosition = _gridPosition;

        _gridPosition = newPosition;

        transform.position = new Vector3(
            _gridPosition.x,
            _gridPosition.y,
            0);

        Vector2Int tailPreviousPosition = MoveBody(previousPosition);
        ApplyPendingGrowth(tailPreviousPosition);

        if (IsGoodFoodAtHead())
            CollectGoodFood();
        else if (IsBadFoodAtHead())
            CollectBadFood();

        if (!HasAvailableMove())
            _gameManager?.GameOver();
    }

    private bool CanMoveTo(Vector2Int position)
    {
        return IsInsideBoard(position) && !IsBodyPosition(position);
    }

    private bool HasAvailableMove()
    {
        foreach (Vector2Int direction in Directions)
        {
            if (CanMoveTo(_gridPosition + direction))
                return true;
        }

        return false;
    }

    private bool IsInsideBoard(Vector2Int position)
    {
        return position.x >= _minX &&
               position.x <= _maxX &&
               position.y >= _minY &&
               position.y <= _maxY;
    }

    private bool IsBodyPosition(Vector2Int position)
    {
        foreach (Transform segment in _bodySegments)
        {
            Vector2Int segmentPosition = new(
                Mathf.RoundToInt(segment.position.x),
                Mathf.RoundToInt(segment.position.y));

            if (segmentPosition == position)
                return true;
        }

        return false;
    }

    private Vector2Int MoveBody(Vector2Int previousPosition)
    {
        Vector2Int tailPreviousPosition = previousPosition;

        foreach (Transform segment in _bodySegments)
        {
            Vector2Int currentPosition = new(
                Mathf.RoundToInt(segment.position.x),
                Mathf.RoundToInt(segment.position.y));

            segment.position = new Vector3(
                previousPosition.x,
                previousPosition.y,
                0);

            previousPosition = currentPosition;
            tailPreviousPosition = currentPosition;
        }

        return tailPreviousPosition;
    }

    private bool IsGoodFoodAtHead()
    {
        return _food != null && _gridPosition == _food.GridPosition;
    }

    private bool IsBadFoodAtHead()
    {
        return _badFoodActive &&
               _badFood != null &&
               _gridPosition == _badFood.GridPosition;
    }

    private void CollectGoodFood()
    {
        Grow(_food.GrowthAmount);
        _gameManager?.AddScore(_food.ScoreAmount);
        _gameManager?.RegisterGoodFoodCollected();

        _currentFoodTimeLimit = Mathf.Max(
            MinimumFoodTime,
            _currentFoodTimeLimit - FoodTimeDecrease);

        RespawnFood();
        RollBadFoodForNextCycle();
    }

    private void CollectBadFood()
    {
        _gameManager?.AddScore(-BadFoodScorePenalty);

        RespawnFood();
        SetBadFoodActive(false);
    }

    private void UpdateFoodTimer()
    {
        if (_food == null)
            return;

        _foodTimeRemaining -= Time.deltaTime;

        if (_foodTimeRemaining <= 0f)
            RespawnFood();
    }

    private void RespawnFood()
    {
        if (_food == null)
            return;

        _food.MoveToRandomPosition(
            _minX,
            _maxX,
            _minY,
            _maxY,
            GetOccupiedPositionsIncludingBadFood());

        ResetFoodTimer();
    }

    private void RollBadFoodForNextCycle()
    {
        if (_badFood == null)
            return;

        bool shouldSpawn =
            _gameManager != null &&
            _gameManager.GoodFoodsCollected >= BadFoodSpawnThreshold &&
            Random.value < BadFoodSpawnChance;

        if (!shouldSpawn)
        {
            SetBadFoodActive(false);
            return;
        }

        HashSet<Vector2Int> occupiedPositions = GetOccupiedPositions();

        if (_food != null)
            occupiedPositions.Add(_food.GridPosition);

        occupiedPositions.Add(_badFood.GridPosition);

        bool movedToValidPosition = _badFood.MoveToRandomPosition(
            _minX,
            _maxX,
            _minY,
            _maxY,
            occupiedPositions);

        SetBadFoodActive(movedToValidPosition);
    }

    private void SetBadFoodActive(bool isActive)
    {
        _badFoodActive = isActive;

        if (_badFood != null)
            _badFood.gameObject.SetActive(isActive);
    }

    private void ResetFoodTimer()
    {
        _foodTimeRemaining = _currentFoodTimeLimit;
    }

    private void Grow(int amount)
    {
        if (amount <= 0)
            return;

        _pendingGrowth += amount;
    }

    private void ApplyPendingGrowth(Vector2Int position)
    {
        if (_pendingGrowth <= 0)
            return;

        Transform newSegment = Instantiate(
            _bodySegmentPrefab,
            new Vector3(position.x, position.y, 0),
            Quaternion.identity);

        newSegment.name = $"SnakeBody_{_bodySegments.Count + 1}";

        _bodySegments.Add(newSegment);
        _pendingGrowth--;
    }

    private HashSet<Vector2Int> GetOccupiedPositions()
    {
        HashSet<Vector2Int> occupiedPositions = new()
    {
        _gridPosition
    };

        foreach (Transform segment in _bodySegments)
        {
            Vector2Int segmentPosition = new(
                Mathf.RoundToInt(segment.position.x),
                Mathf.RoundToInt(segment.position.y));

            occupiedPositions.Add(segmentPosition);
        }

        return occupiedPositions;
    }

    private HashSet<Vector2Int> GetOccupiedPositionsIncludingBadFood()
    {
        HashSet<Vector2Int> occupiedPositions = GetOccupiedPositions();

        if (_food != null)
            occupiedPositions.Add(_food.GridPosition);

        if (_badFoodActive && _badFood != null)
            occupiedPositions.Add(_badFood.GridPosition);

        return occupiedPositions;
    }
}

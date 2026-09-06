using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SnakeHeadController : MonoBehaviour
{
    [SerializeField] private int _minX = -7;
    [SerializeField] private int _maxX = 7;
    [SerializeField] private int _minY = -4;
    [SerializeField] private int _maxY = 4;

    [SerializeField] private FoodController _food;
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private Transform _bodySegmentPrefab;
    [SerializeField] private List<Transform> _bodySegments = new();

    private Vector2Int _gridPosition;

    private void Awake()
    {
        _gridPosition = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.y));
    }

    private void Update()
    {
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

        if (!IsInsideBoard(newPosition))
            return;

        if (IsBodyPosition(newPosition))
            return;

        Vector2Int previousPosition = _gridPosition;

        _gridPosition = newPosition;

        transform.position = new Vector3(
            _gridPosition.x,
            _gridPosition.y,
            0);

        Vector2Int tailPreviousPosition = MoveBody(previousPosition);

        CheckFood(tailPreviousPosition);
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

    private void CheckFood(Vector2Int newSegmentPosition)
    {
        if (_food == null)
            return;

        if (_gridPosition != _food.GridPosition)
            return;

        Grow(newSegmentPosition);
        _gameManager?.AddScore(1);

        _food.MoveToRandomPosition(
            _minX,
            _maxX,
            _minY,
            _maxY,
            GetOccupiedPositions());
    }

    private void Grow(Vector2Int position)
    {
        Transform newSegment = Instantiate(
            _bodySegmentPrefab,
            new Vector3(position.x, position.y, 0),
            Quaternion.identity);

        newSegment.name = $"SnakeBody_{_bodySegments.Count + 1}";

        _bodySegments.Add(newSegment);
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
}

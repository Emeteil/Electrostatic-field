using UnityEngine;

public struct FixedPoint
{
    public Vector2Int Position;
    public float Value;

    public FixedPoint(Vector2Int position, float value)
    {
        Position = position;
        Value = value;
    }
}
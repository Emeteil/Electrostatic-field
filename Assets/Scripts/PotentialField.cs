using System.Collections.Generic;
using UnityEngine;

public class PotentialField
{
    private readonly Vector2 _size;
    private float[,] _potential;
    private List<FixedPoint> _fixedPoints = new List<FixedPoint>();

    public PotentialField(Vector2 size)
    {
        _size = size;
        InitializePotential();
    }

    private void InitializePotential()
    {
        int width = (int)_size.x + 1;
        int height = (int)_size.y + 1;
        _potential = new float[width, height];
    }

    public void SetFixedPoint(Vector2Int position, float value)
    {
        _potential[position.x, position.y] = value;
        _fixedPoints.Add(new FixedPoint(position, value));
    }

    public void CalculatePotential()
    {
        int width = (int)_size.x + 1;
        int height = (int)_size.y + 1;

        for (int iteration = 0; iteration < 2000; iteration++)
        {
            float[,] newPotential = (float[,])_potential.Clone();

            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    if (IsFixedPoint(x, y)) continue;

                    newPotential[x, y] = (
                        _potential[x + 1, y] + _potential[x - 1, y] +
                        _potential[x, y + 1] + _potential[x, y - 1]
                    ) / 4f;
                }
            }

            UpdateBoundaries(width, height, newPotential);
            _potential = newPotential;
        }
    }

    private void UpdateBoundaries(int width, int height, float[,] newPotential)
    {
        for (int y = 0; y < height; y++)
        {
            if (!IsFixedPoint(0, y))
                newPotential[0, y] = newPotential[1, y];
            newPotential[width - 1, y] = newPotential[width - 2, y];
        }

        for (int x = 0; x < width; x++)
        {
            newPotential[x, 0] = newPotential[x, 1];
            newPotential[x, height - 1] = newPotential[x, height - 2];
        }
    }

    public float GetPotential(float x, float y)
    {
        int i = Mathf.FloorToInt(x);
        int j = Mathf.FloorToInt(y);
        float dx = x - i;
        float dy = y - j;

        if (i < 0 || i >= _potential.GetLength(0) - 1 || j < 0 || j >= _potential.GetLength(1) - 1)
            return 0f;

        return BilinearInterpolation(
            _potential[i, j],
            _potential[i + 1, j],
            _potential[i, j + 1],
            _potential[i + 1, j + 1],
            dx,
            dy
        );
    }

    private float BilinearInterpolation(float v00, float v10, float v01, float v11, float dx, float dy)
    {
        return v00 * (1 - dx) * (1 - dy) +
               v10 * dx * (1 - dy) +
               v01 * (1 - dx) * dy +
               v11 * dx * dy;
    }

    private bool IsFixedPoint(int x, int y)
    {
        foreach (var point in _fixedPoints)
        {
            if (point.Position.x == x && point.Position.y == y)
                return true;
        }
        return false;
    }
}
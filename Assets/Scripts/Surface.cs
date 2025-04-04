using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshRenderer))]
public class Surface : MonoBehaviour
{
    [SerializeField] private Vector2 localTopRight = new Vector2(20, 16);

    private Vector2 lastHoverPosition;
    private Collider boxCollider;
    private float[,] potential;
    private List<(int x, int y, float value)> fixedPoints = new List<(int x, int y, float value)>();

    private Material material;
    private Renderer rendererO;
    private Texture2D gridTexture;
    private Texture2D heatMap;

    private void Start()
    {
        boxCollider = GetComponent<Collider>();
        material = new Material(Shader.Find("Standard"));
        rendererO = GetComponent<Renderer>();

        CreateGridTexture(gridSize: localTopRight);
        InitializePotential();
        CalculatePotential();
        heatMap = CreateHeatmapTexture(0, 10, 1024, grid: true);
    }

    private void CreateGridTexture(
        int textureSize = 1024,
        Vector2 gridSize = default,
        Color backgroundColor = default,
        Color lineColor = default,
        float lineThickness = 3f)
    {
        if (gridSize == default) gridSize = new Vector2(10, 10);
        if (backgroundColor == default) backgroundColor = Color.white;
        if (lineColor == default) lineColor = Color.black;

        gridTexture = new Texture2D(textureSize, textureSize);
        Color[] pixels = new Color[textureSize * textureSize];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = backgroundColor;

        Vector2 cellSize = new Vector2(
            textureSize / gridSize.x,
            textureSize / gridSize.y
        );

        for (int y = 0; y <= gridSize.y; y++)
        {
            int pixelY = Mathf.RoundToInt(y * cellSize.y);

            for (int dy = -Mathf.FloorToInt(lineThickness / 2); dy <= Mathf.CeilToInt(lineThickness / 2); dy++)
            {
                int currentY = pixelY + dy;
                if (!(currentY >= 0 && currentY < textureSize)) continue;

                for (int x = 0; x < textureSize; x++)
                    pixels[currentY * textureSize + x] = lineColor;
            }
        }

        for (int x = 0; x <= gridSize.x; x++)
        {
            int pixelX = Mathf.RoundToInt(x * cellSize.x);

            for (int dx = -Mathf.FloorToInt(lineThickness / 2); dx <= Mathf.CeilToInt(lineThickness / 2); dx++)
            {
                int currentX = pixelX + dx;
                if (!(currentX >= 0 && currentX < textureSize)) continue;

                for (int y = 0; y < textureSize; y++)
                    pixels[y * textureSize + currentX] = lineColor;
            }
        }

        gridTexture.SetPixels(pixels);
        gridTexture.Apply();

        material.mainTexture = gridTexture;
        rendererO.material = material;
    }

    private Texture2D CreateHeatmapTexture(
        float minPotential,
        float maxPotential,
        int textureSize,
        bool grid = false,
        float lineThickness = 3f
    )
    {
        Texture2D texture = new Texture2D(textureSize, textureSize);
        Color[] pixels = new Color[textureSize * textureSize];

        float pixelToGridX = localTopRight.x / textureSize;
        float pixelToGridY = localTopRight.y / textureSize;

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float gridX = x * pixelToGridX;
                float gridY = y * pixelToGridY;
                float potentialValue = GetPotential(gridX, gridY);
                float t = Mathf.InverseLerp(minPotential, maxPotential, potentialValue);

                Color color = Color.Lerp(Color.blue, Color.red, t);

                pixels[y * textureSize + x] = color;
            }
        }

        if (!grid)
        {
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        Vector2 cellSize = new Vector2(
            textureSize / localTopRight.x,
            textureSize / localTopRight.y
        );

        for (int y = 0; y <= localTopRight.y; y++)
        {
            int pixelY = Mathf.RoundToInt(y * cellSize.y);

            for (int dy = -Mathf.FloorToInt(lineThickness / 2); dy <= Mathf.CeilToInt(lineThickness / 2); dy++)
            {
                int currentY = pixelY + dy;
                if (!(currentY >= 0 && currentY < textureSize)) continue;

                for (int x = 0; x < textureSize; x++)
                    pixels[currentY * textureSize + x] = Color.black;
            }
        }

        for (int x = 0; x <= localTopRight.x; x++)
        {
            int pixelX = Mathf.RoundToInt(x * cellSize.x);

            for (int dx = -Mathf.FloorToInt(lineThickness / 2); dx <= Mathf.CeilToInt(lineThickness / 2); dx++)
            {
                int currentX = pixelX + dx;
                if (!(currentX >= 0 && currentX < textureSize)) continue;

                for (int y = 0; y < textureSize; y++)
                    pixels[y * textureSize + currentX] = Color.black;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    private void SetFixedPoint(int x, int y, float value)
    {
        potential[x, y] = value;
        fixedPoints.Add((x, y, value));
    }

    private void InitializePotential()
    {
        int width = (int)localTopRight.x + 1;
        int height = (int)localTopRight.y + 1;
        potential = new float[width, height];

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                potential[x, y] = 0f;

        SetFixedPoint(1, 8, 10f); // Положительный электрод
        SetFixedPoint(19, 8, 0f); // Отрицательный электрод

        // for (int y = 0; y < height; y++)
        //     SetFixedPoint(0, y, 0f); // Плоский отрицательный электрод
        // SetFixedPoint(19, 8, 10f); // Положительный электрод
    }

    // Только для кратных точек по сантиметрам
    private void CalculatePotential()
    {
        int width = (int)localTopRight.x + 1;
        int height = (int)localTopRight.y + 1;

        for (int iteration = 0; iteration < 2000; iteration++)
        {
            float[,] newPotential = (float[,])potential.Clone();

            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    if (IsFixedPoint(x, y)) continue;

                    newPotential[x, y] = (
                        potential[x + 1, y] + potential[x - 1, y] +
                        potential[x, y + 1] + potential[x, y - 1]
                    ) / 4f;
                }
            }

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

            potential = newPotential;
        }
    }

    private bool IsFixedPoint(int x, int y)
    {
        foreach (var point in fixedPoints)
        {
            if (point.x == x && point.y == y)
                return true;
        }
        return false;
    }

    // Билинейная интерполяция
    private float GetPotential(float x, float y)
    {
        int i = Mathf.FloorToInt(x);
        int j = Mathf.FloorToInt(y);
        float dx = x - i;
        float dy = y - j;

        if (i < 0 || i >= potential.GetLength(0) - 1 || j < 0 || j >= potential.GetLength(1) - 1)
            return 0f;

        float v00 = potential[i, j];
        float v10 = potential[i + 1, j];
        float v01 = potential[i, j + 1];
        float v11 = potential[i + 1, j + 1];

        return (
            v00 * (1 - dx) * (1 - dy) +
            v10 * dx * (1 - dy) +
            v01 * (1 - dx) * dy +
            v11 * dx * dy
        );
    }

    private void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        float maxDistance = float.MaxValue;

        if (boxCollider.Raycast(ray, out hit, maxDistance))
        {
            lastHoverPosition = new Vector2(
                hit.textureCoord.x * localTopRight.x,
                hit.textureCoord.y * localTopRight.y
            );
        }

        // Debug
        if (Input.GetKey(KeyCode.Q))
        {
            material.mainTexture = heatMap;
        }
        if (Input.GetKeyUp(KeyCode.Q))
        {
            material.mainTexture = gridTexture;
        }
    }

    private void OnGUI()
    {
        // Debug
        float potentialValue = GetPotential(lastHoverPosition.x, lastHoverPosition.y);
        GUI.Label(new Rect(10, 10, 300, 20), $"Координата: {lastHoverPosition}, Потенциал: {potentialValue:F2} В");
    }
}
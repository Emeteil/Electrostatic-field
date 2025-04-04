using UnityEngine;

public class TextureRenderer
{
    private readonly Renderer _renderer;
    private readonly Vector2 _gridSize;
    private Material _material;
    private Texture2D _gridTexture;
    private Texture2D _heatMapTexture;

    public TextureRenderer(Renderer renderer, Vector2 gridSize)
    {
        _renderer = renderer;
        _gridSize = gridSize;
        _material = new Material(Shader.Find("Standard"));
        _renderer.material = _material;
    }

    public void CreateGridTexture(
        int textureSize = 1024,
        Color backgroundColor = default,
        Color lineColor = default,
        float lineThickness = 3f)
    {
        if (backgroundColor == default) backgroundColor = Color.white;
        if (lineColor == default) lineColor = Color.black;

        _gridTexture = new Texture2D(textureSize, textureSize);
        Color[] pixels = new Color[textureSize * textureSize];

        FillBackground(pixels, backgroundColor);
        DrawGridLines(pixels, textureSize, lineColor, lineThickness);

        _gridTexture.SetPixels(pixels);
        _gridTexture.Apply();
    }

    public void CreateHeatmapTexture(
        PotentialField potentialField,
        float minPotential = 0f,
        float maxPotential = 10f,
        int textureSize = 1024,
        bool drawGrid = true,
        float lineThickness = 3f)
    {
        _heatMapTexture = new Texture2D(textureSize, textureSize);
        Color[] pixels = new Color[textureSize * textureSize];

        CalculateHeatmapColors(pixels, potentialField, textureSize, minPotential, maxPotential);

        if (drawGrid)
        {
            DrawGridLines(pixels, textureSize, Color.black, lineThickness);
        }

        _heatMapTexture.SetPixels(pixels);
        _heatMapTexture.Apply();
    }

    public void UpdateMainTexture(bool heatMap = false) {
        _material.mainTexture = heatMap ? _heatMapTexture : _gridTexture;
    }

    public void ShowHeatmap()
    {
        if (_heatMapTexture != null)
            _material.mainTexture = _heatMapTexture;
    }

    public void ShowGrid()
    {
        if (_gridTexture != null)
            _material.mainTexture = _gridTexture;
    }

    private void FillBackground(Color[] pixels, Color color)
    {
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = color;
    }

    private void CalculateHeatmapColors(Color[] pixels, PotentialField potentialField, int textureSize, float minPotential, float maxPotential)
    {
        float pixelToGridX = _gridSize.x / textureSize;
        float pixelToGridY = _gridSize.y / textureSize;

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float gridX = x * pixelToGridX;
                float gridY = y * pixelToGridY;
                float potentialValue = potentialField.GetPotential(gridX, gridY);
                float t = Mathf.InverseLerp(minPotential, maxPotential, potentialValue);

                pixels[y * textureSize + x] = Color.Lerp(Color.blue, Color.red, t);
            }
        }
    }

    private void DrawGridLines(Color[] pixels, int textureSize, Color lineColor, float lineThickness)
    {
        Vector2 cellSize = new Vector2(
            textureSize / _gridSize.x,
            textureSize / _gridSize.y
        );

        DrawHorizontalGridLines(pixels, textureSize, lineColor, lineThickness, cellSize);
        DrawVerticalGridLines(pixels, textureSize, lineColor, lineThickness, cellSize);
    }

    private void DrawHorizontalGridLines(Color[] pixels, int textureSize, Color lineColor, float lineThickness, Vector2 cellSize)
    {
        for (int y = 0; y <= _gridSize.y; y++)
        {
            int pixelY = Mathf.RoundToInt(y * cellSize.y);
            DrawLine(pixels, textureSize, 0, textureSize - 1, pixelY, lineColor, lineThickness, true);
        }
    }

    private void DrawVerticalGridLines(Color[] pixels, int textureSize, Color lineColor, float lineThickness, Vector2 cellSize)
    {
        for (int x = 0; x <= _gridSize.x; x++)
        {
            int pixelX = Mathf.RoundToInt(x * cellSize.x);
            DrawLine(pixels, textureSize, pixelX, pixelX, 0, lineColor, lineThickness, false);
        }
    }

    private void DrawLine(Color[] pixels, int textureSize, int startX, int endX, int startY, Color color, float thickness, bool isHorizontal)
    {
        int halfThickness = Mathf.FloorToInt(thickness / 2f);
        
        for (int d = -halfThickness; d <= halfThickness; d++)
        {
            if (isHorizontal)
            {
                int currentY = startY + d;
                if (currentY < 0 || currentY >= textureSize) continue;

                for (int x = Mathf.Max(0, startX); x <= Mathf.Min(textureSize - 1, endX); x++)
                {
                    pixels[currentY * textureSize + x] = color;
                }
            }
            else
            {
                int currentX = startX + d;
                if (currentX < 0 || currentX >= textureSize) continue;

                for (int y = Mathf.Max(0, startY); y < textureSize; y++)
                {
                    pixels[y * textureSize + currentX] = color;
                }
            }
        }
    }
}
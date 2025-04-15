using UnityEngine;

public class TextureRenderer
{
    private readonly Renderer _renderer;
    private readonly Vector2 _gridSize;
    private Material _material;
    private Texture2D _gridTexture;
    private Texture2D _heatMapTexture;

    public TextureRenderer(Renderer renderer, Vector2 gridSize, Material material = null)
    {
        _renderer = renderer;
        _gridSize = gridSize;
        _material = material == null ? new Material(Shader.Find("Standard")) : material;
        _renderer.material = _material;
    }

    public void CreateGridTexture(
        Electrode[] electrodes,
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
        DrawElectrodes(electrodes, pixels, textureSize, lineThickness * 5);

        _gridTexture.SetPixels(pixels);
        _gridTexture.Apply();
    }

    public void CreateHeatmapTexture(
        PotentialField potentialField,
        Electrode[] electrodes,
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
        DrawElectrodes(electrodes, pixels, textureSize, lineThickness * 5);

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

    

    private void DrawElectrodes(Electrode[] electrodes, Color[] pixels, int textureSize, float lineThickness)
    {
        foreach (var electrode in electrodes)
        {
            if (electrode.IsFlatElectrode)
                DrawFlatElectrode(electrode, pixels, textureSize, lineThickness);
            else
                DrawPointElectrode(electrode, pixels, textureSize);
        }
    }

    private void DrawPointElectrode(Electrode electrode, Color[] pixels, int textureSize)
    {
        Color fillColor = electrode.Value > 5f ? Color.red : Color.blue;
        Color outlineColor = Color.black;

        float cellWidth = 30;

        int centerX = Mathf.RoundToInt(electrode.Position.x * (textureSize / _gridSize.x));
        int centerY = Mathf.RoundToInt(electrode.Position.y * (textureSize / _gridSize.y));

        float radius = cellWidth;
        float outlineThickness = 2f;

        int startX = Mathf.Max(0, centerX - (int)(radius + outlineThickness));
        int endX = Mathf.Min(textureSize - 1, centerX + (int)(radius + outlineThickness));
        int startY = Mathf.Max(0, centerY - (int)(radius + outlineThickness));
        int endY = Mathf.Min(textureSize - 1, centerY + (int)(radius + outlineThickness));

        for (int y = startY; y <= endY; y++)
        {
            for (int x = startX; x <= endX; x++)
            {
                float dx = x - centerX;
                float dy = y - centerY;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);

                if (distance <= radius + outlineThickness)
                {
                    if (distance <= radius)
                    {
                        pixels[y * textureSize + x] = fillColor;
                    }
                    else
                    {
                        pixels[y * textureSize + x] = outlineColor;
                    }
                }
            }
        }
    }

    private void DrawFlatElectrode(Electrode electrode, Color[] pixels, int textureSize, float lineThickness)
    {
        Color color = electrode.Value > 5f ? Color.red : Color.blue;

        if (electrode.IsHorizontal)
        {
            int yPos = Mathf.RoundToInt(electrode.Position.y * (textureSize / _gridSize.y));
            DrawThickLineHorizontal(pixels, textureSize, yPos, color, lineThickness);
        }
        else
        {
            int xPos = Mathf.RoundToInt(electrode.Position.x * (textureSize / _gridSize.x));
            DrawThickLineVertical(pixels, textureSize, xPos, color, lineThickness);
        }
    }

    private void DrawThickLineHorizontal(Color[] pixels, int textureSize, int centerY, Color color, float thickness)
    {
        int halfThick = Mathf.FloorToInt(thickness / 2f);
        for (int dy = -halfThick; dy <= halfThick; dy++)
        {
            int y = centerY + dy;
            if (y < 0 || y >= textureSize) continue;
            for (int x = 0; x < textureSize; x++)
            {
                pixels[y * textureSize + x] = color;
            }
        }
    }

    private void DrawThickLineVertical(Color[] pixels, int textureSize, int centerX, Color color, float thickness)
    {
        int halfThick = Mathf.FloorToInt(thickness / 2f);
        for (int dx = -halfThick; dx <= halfThick; dx++)
        {
            int x = centerX + dx;
            if (x < 0 || x >= textureSize) continue;
            for (int y = 0; y < textureSize; y++)
            {
                pixels[y * textureSize + x] = color;
            }
        }
    }
}
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class TextureDrawer : MonoBehaviour
{
    [SerializeField] private Color _brushColor = Color.black;
    [SerializeField] private float _brushWidth = 10f;
    [SerializeField] private float _eraserWidth = 20f;

    private MeshRenderer _meshRenderer;
    private Collider _collider;
    private Texture2D _texture;
    private Vector2 _previousPoint;
    private bool _isDrawing;
    private Color[] _pixels;
    private bool _textureNeedsUpdate;

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _collider = GetComponent<Collider>();
        InitializeTexture();
    }

    private void InitializeTexture()
    {
        Material material = _meshRenderer.material;
        _texture = new Texture2D(512, 512);
        _pixels = _texture.GetPixels();
        for (int i = 0; i < _pixels.Length; i++) _pixels[i] = Color.clear;
        _texture.SetPixels(_pixels);
        _texture.Apply();
        material.mainTexture = _texture;
    }

    private void Update()
    {
        HandleInput();
        if (_textureNeedsUpdate)
        {
            _texture.SetPixels(_pixels);
            _texture.Apply();
            _textureNeedsUpdate = false;
        }
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            _isDrawing = true;
            _previousPoint = GetTextureCoordinate();
        }

        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
        {
            _isDrawing = false;
        }

        if (_isDrawing)
        {
            Vector2 currentPoint = GetTextureCoordinate();
            if (Input.GetMouseButton(0)) DrawLine(_previousPoint, currentPoint, _brushColor, _brushWidth);
            if (Input.GetMouseButton(1)) DrawLine(_previousPoint, currentPoint, Color.clear, _eraserWidth);
            _previousPoint = currentPoint;
            _textureNeedsUpdate = true;
        }
    }

    private Vector2 GetTextureCoordinate()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (_collider.Raycast(ray, out RaycastHit hit, 1000000f))
        {
            Vector2 pixelUV = hit.textureCoord;
            pixelUV.x *= _texture.width;
            pixelUV.y *= _texture.height;
            return pixelUV;
        }
        return _previousPoint;
    }

    private void DrawLine(Vector2 start, Vector2 end, Color color, float width)
    {
        float distance = Vector2.Distance(start, end);
        Vector2 direction = (end - start).normalized;
        for (float i = 0; i <= distance; i += 1f)
        {
            Vector2 point = start + direction * i;
            DrawCircle(point, width, color);
        }
    }

    private void DrawCircle(Vector2 center, float radius, Color color)
    {
        int radiusInt = Mathf.RoundToInt(radius);
        for (int x = -radiusInt; x <= radiusInt; x++)
        {
            for (int y = -radiusInt; y <= radiusInt; y++)
            {
                if (x * x + y * y <= radius * radius)
                {
                    int texX = Mathf.RoundToInt(center.x + x);
                    int texY = Mathf.RoundToInt(center.y + y);
                    if (texX >= 0 && texX < _texture.width && texY >= 0 && texY < _texture.height)
                    {
                        int index = texY * _texture.width + texX;
                        _pixels[index] = color;
                    }
                }
            }
        }
    }
}
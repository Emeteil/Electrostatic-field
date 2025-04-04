using UnityEngine;

public class SurfaceInteraction
{
    private readonly Collider _collider;
    private readonly Vector2 _gridSize;
    private readonly PotentialField _potentialField;
    
    private Vector2 _lastHoverPosition;

    public SurfaceInteraction(Collider collider, Vector2 gridSize, PotentialField potentialField)
    {
        _collider = collider;
        _gridSize = gridSize;
        _potentialField = potentialField;
    }

    public void HandleInteraction()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (_collider.Raycast(ray, out RaycastHit hit, float.MaxValue))
        {
            _lastHoverPosition = new Vector2(
                hit.textureCoord.x * _gridSize.x,
                hit.textureCoord.y * _gridSize.y
            );
        }
    }

    public void DrawDebugInfo()
    {
        float potentialValue = _potentialField.GetPotential(_lastHoverPosition.x, _lastHoverPosition.y);
        GUI.Label(new Rect(10, 10, 300, 20), 
            $"Координата: {_lastHoverPosition}, Потенциал: {potentialValue:F2} В");
    }
}
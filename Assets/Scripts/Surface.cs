using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(Collider))]
public class Surface : MonoBehaviour
{
    [SerializeField] private Vector2 _localTopRight = new Vector2(20, 16);
    [SerializeField] private Electrode[] _electrodes;

    private TextureRenderer _textureRenderer;
    private PotentialField _potentialField;
    private SurfaceInteraction _surfaceInteraction;

    private void Start()
    {
        FullRecalculetePotential();
    }

    [ContextMenu("FullRecalculetePotential")]
    private void FullRecalculetePotential() {
        _potentialField = new PotentialField(_localTopRight);
        _textureRenderer = new TextureRenderer(GetComponent<Renderer>(), _localTopRight);
        _surfaceInteraction = new SurfaceInteraction(GetComponent<Collider>(), _localTopRight, _potentialField);
        
        InitializeElectrodes();
        _potentialField.CalculatePotential();
        _textureRenderer.CreateGridTexture();
        _textureRenderer.CreateHeatmapTexture(_potentialField);
        _textureRenderer.UpdateMainTexture();
    }

    private void InitializeElectrodes()
    {
        foreach (var electrode in _electrodes)
        {
            if (!electrode.IsFlatElectrode)
            {
                _potentialField.SetFixedPoint(electrode.Position, electrode.Value);
                continue;
            }

            if (electrode.IsHorizontal)
                for (int x = 0; x <= _localTopRight.x; x++)
                    _potentialField.SetFixedPoint(new Vector2Int(x, electrode.Position.y), electrode.Value);
            else
                for (int y = 0; y <= _localTopRight.y; y++)
                    _potentialField.SetFixedPoint(new Vector2Int(electrode.Position.x, y), electrode.Value);
        }
    }

    private void Update()
    {
        _surfaceInteraction.HandleInteraction();

        if (Input.GetKey(KeyCode.Q))
            _textureRenderer.ShowHeatmap();
        if (Input.GetKeyUp(KeyCode.Q))
            _textureRenderer.ShowGrid();
    }

    private void OnGUI()
    {
        _surfaceInteraction.DrawDebugInfo();
    }
}
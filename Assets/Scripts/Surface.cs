using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(Collider))]
public class Surface : MonoBehaviour
{
    [SerializeField] public Vector2 localTopRight = new Vector2(20, 16);
    [SerializeField] public Material material;
    [SerializeField] public Electrode[] electrodes;
    [SerializeField] public CoordinateNumbering x_axis;
    [SerializeField] public CoordinateNumbering y_axis;

    public TextureRenderer textureRenderer;
    public PotentialField potentialField;

    private void Start()
    {
        FullRecalculetePotential();
    }

    [ContextMenu("FullRecalculetePotential")]
    public void FullRecalculetePotential(bool _heatMap = false) {
        potentialField = new PotentialField(localTopRight);
        textureRenderer = new TextureRenderer(GetComponent<Renderer>(), localTopRight, material);
        
        x_axis.GenerateNumberingCount((int)localTopRight.x + 1);
        y_axis.GenerateNumberingCount((int)localTopRight.y + 1);

        InitializeElectrodes();
        potentialField.CalculatePotential();
        textureRenderer.CreateGridTexture(electrodes);
        textureRenderer.CreateHeatmapTexture(potentialField, electrodes);
        textureRenderer.UpdateMainTexture(_heatMap);
    }

    private void InitializeElectrodes()
    {
        foreach (var electrode in electrodes)
        {
            if (!electrode.IsFlatElectrode)
            {
                potentialField.SetFixedPoint(electrode.Position, electrode.Value);
                continue;
            }

            if (electrode.IsHorizontal)
                for (int x = 0; x <= localTopRight.x; x++)
                    potentialField.SetFixedPoint(new Vector2Int(x, electrode.Position.y), electrode.Value);
            else
                for (int y = 0; y <= localTopRight.y; y++)
                    potentialField.SetFixedPoint(new Vector2Int(electrode.Position.x, y), electrode.Value);
        }
    }
}
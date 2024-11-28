// HexTile.cs
using Godot;

public partial class HexTile : Node3D
{
	[Export] public MeshInstance3D Mesh { get; set; }

	private StandardMaterial3D _defaultMaterial;
	private StandardMaterial3D _highlightMaterial;

	public TileType TileType { get; private set; } = TileType.Floor;
	private Label3D _coordinateLabel;
	private Vector3I _hexCoord;

	public override void _Ready()
	{
		_defaultMaterial = new StandardMaterial3D
		{
			AlbedoColor = Colors.Gray
		};

		_highlightMaterial = new StandardMaterial3D
		{
			AlbedoColor = Colors.Green,
			EmissionEnabled = true,
			Emission = Colors.Green,
			EmissionEnergyMultiplier = 0.5f
		};

		if (Mesh != null)
		{
			Mesh.MaterialOverride = _defaultMaterial;
		}

		// Add coordinate display
		_coordinateLabel = new Label3D
		{
			Text = "0,0,0",
			FontSize = 34,
			PixelSize = 0.01f,
			Billboard = BaseMaterial3D.BillboardModeEnum.Enabled,
			Position = new Vector3(0.5f, 0.8f, 1.1f),
			// RotationDegrees = new Vector3(-40, 15, 0),
			Modulate = Colors.Black
		};
		AddChild(_coordinateLabel);
	}

	public void OnInputEvent(Node camera, InputEvent @event, Vector3 position, Vector3 normal, int shapeIdx)
	{
		if (@event is InputEventMouseButton mouseEvent)
		{
			if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
			{
				SignalBus.Instance.EmitSignal(SignalBus.SignalName.TileClicked, this);
			}
		}
	}

	public void SetHexCoordinate(Vector3I coord)
	{
		_hexCoord = coord;
		if (_coordinateLabel != null)
		{
			_coordinateLabel.Text = $"{coord.X},{coord.Y},{coord.Z}";
		}
	}

	public void SetTileType(TileType type)
	{
		TileType = type;
		UpdateVisuals();
	}

	public void Highlight(bool enabled)
	{
		if (Mesh != null)
		{
			Mesh.MaterialOverride = enabled ? _highlightMaterial : _defaultMaterial;
		}
	}

	private void UpdateVisuals()
	{
		if (Mesh == null) return;

		var material = new StandardMaterial3D();

		switch (TileType)
		{
			case TileType.Empty:
				material.AlbedoColor = Colors.Transparent;
				material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
				break;
			case TileType.Floor:
				material.AlbedoColor = Colors.Gray;
				break;
			case TileType.Lava:
				material.AlbedoColor = Colors.DarkRed;
				material.EmissionEnabled = true;
				material.Emission = Colors.Red;
				material.EmissionEnergyMultiplier = 1.0f;
				break;
			case TileType.Wall:
				material.AlbedoColor = Colors.DarkGray;
				break;
		}

		Mesh.MaterialOverride = material;
	}
}

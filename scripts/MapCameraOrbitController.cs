using Godot;

public partial class MapCameraOrbitController : Node3D
{
	[ExportCategory("Target Settings")]
	[Export] public Node3D Target { get; set; }

	[ExportCategory("Orbit Speeds & Limits")]
	[Export] public float Distance { get; set; } = 50f;
	[Export] public float MinDistance { get; set; } = 5f;
	[Export] public float MaxDistance { get; set; } = 1000f;
	[Export] public float XSpeed { get; set; } = 250f;
	[Export] public float YSpeed { get; set; } = 120f;
	[Export] public float YMinLimit { get; set; } = -80f;
	[Export] public float YMaxLimit { get; set; } = 80f;
	[Export] public float ScrollSpeed { get; set; } = 20f;

	private float _x = 0.0f;
	private float _y = 0.0f;
	private MapViewManager _mapViewManager;
	private bool _isRightMouseDown = false;

	public override void _Ready()
	{
		_mapViewManager = GetTree().Root.FindChild("MapViewManager", true, false) as MapViewManager;
		GD.Print($"[MapCameraOrbitController] Ready. MapViewManager found: {_mapViewManager != null}");

		Vector3 angles = RotationDegrees;
		_x = angles.Y;
		_y = angles.X;

		if (Target != null)
		{
			Distance = GlobalPosition.DistanceTo(Target.GlobalPosition);
			GD.Print($"[MapCameraOrbitController] Target assigned: {Target.Name}. Initial camera distance: {Distance}");
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseButton)
		{
			if (mouseButton.ButtonIndex == MouseButton.Right)
			{
				_isRightMouseDown = mouseButton.Pressed;
				GD.Print($"[MapCameraOrbitController] Right Mouse Drag Active: {_isRightMouseDown}");
			}
			else if (mouseButton.Pressed)
			{
				if (mouseButton.ButtonIndex == MouseButton.WheelUp)
				{
					Zoom(-1f);
				}
				else if (mouseButton.ButtonIndex == MouseButton.WheelDown)
				{
					Zoom(1f);
				}
			}
		}
		else if (@event is InputEventMouseMotion mouseMotion && _isRightMouseDown)
		{
			_x += mouseMotion.Relative.X * XSpeed * 0.002f;
			_y -= mouseMotion.Relative.Y * YSpeed * 0.002f;
			_y = Mathf.Clamp(_y, YMinLimit, YMaxLimit);
		}
	}

	private void Zoom(float direction)
	{
		Distance -= direction * ScrollSpeed * (Distance * 0.1f);
		Distance = Mathf.Clamp(Distance, MinDistance, MaxDistance);
		GD.Print($"[MapCameraOrbitController] Zoomed camera. New Distance: {Distance:F2}");
	}

	public override void _Process(double delta)
	{
		if (_mapViewManager != null && !_mapViewManager.IsMapViewActive)
			return;

		if (Target == null) return;

		Quaternion rotation = Quaternion.FromEuler(new Vector3(Mathf.DegToRad(_y), Mathf.DegToRad(_x), 0f));
		Vector3 negDistance = new Vector3(0.0f, 0.0f, Distance);
		Vector3 position = Target.GlobalPosition + (rotation * negDistance);

		GlobalTransform = new Transform3D(new Basis(rotation), position);
	}
}

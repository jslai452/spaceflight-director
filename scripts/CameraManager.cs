using Godot;

public partial class CameraManager : Node3D
{
	[Export(PropertyHint.NodePathValidTypes, "Node3D")]
	public NodePath TargetPath { get; set; }

	[Export(PropertyHint.NodePathValidTypes, "Node3D")]
	public NodePath CameraPath { get; set; }

	[Export(PropertyHint.NodePathValidTypes, "Node3D")]
	public NodePath PlanetCenterPath { get; set; } // Optional: auto-detected if left empty

	[ExportGroup("Orbit Settings")]
	[Export] public float Distance { get; set; } = 15.0f;
	[Export] public float MinDistance { get; set; } = 2.0f;
	[Export] public float MaxDistance { get; set; } = 500f;
	[Export] public float ZoomSensitivity { get; set; } = 20f;
	[Export] public float SensitivityX { get; set; } = 4.0f;
	[Export] public float SensitivityY { get; set; } = 4.0f;
	[Export] public float MinPitch { get; set; } = -85.0f;
	[Export] public float MaxPitch { get; set; } = 85.0f;

	[ExportGroup("Surface / Planet Mode")]
	[Export] public bool UsePlanetSurfaceGravityAlignment { get; set; } = true;

	private Node3D _targetNode;
	private Node3D _cameraNode;
	private Node3D _planetCenterNode;

	private float _currentX = 0.0f;
	private float _currentY = 20.0f;
	private bool _isRightMouseDown = false;
	private Vector2 _mouseDelta = Vector2.Zero;
	private float _scrollDelta = 0.0f;

	public override void _Ready()
	{
		if (!string.IsNullOrEmpty(TargetPath))
		{
			_targetNode = GetNodeOrNull<Node3D>(TargetPath);
		}

		if (!string.IsNullOrEmpty(CameraPath))
		{
			_cameraNode = GetNodeOrNull<Node3D>(CameraPath);
		}

		// 1. Explicit PlanetCenterPath assignment check
		if (!string.IsNullOrEmpty(PlanetCenterPath))
		{
			_planetCenterNode = GetNodeOrNull<Node3D>(PlanetCenterPath);
		}
		// 2. Fallback: Auto-detect PlanetCenter from CelestialBodyGravity on the target
		else if (_targetNode != null)
		{
			var gravityComp = _targetNode.GetNodeOrNull<CelestialBodyGravity>("CelestialBodyGravity") 
							  ?? _targetNode.FindChild("CelestialBodyGravity", true, false) as CelestialBodyGravity;
			
			if (gravityComp != null && gravityComp.PlanetCenter != null)
			{
				_planetCenterNode = gravityComp.PlanetCenter;
				//GD.Print($"[CameraManager] Auto-detected PlanetCenter from CelestialBodyGravity: {_planetCenterNode.Name}");
			}
		}

		Vector3 angles = RotationDegrees;
		_currentX = angles.Y;
		_currentY = angles.X;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseButton)
		{
			if (mouseButton.ButtonIndex == MouseButton.Right)
			{
				_isRightMouseDown = mouseButton.Pressed;
			}
			else if (mouseButton.Pressed)
			{
				if (mouseButton.ButtonIndex == MouseButton.WheelUp)
				{
					_scrollDelta = 1.0f;
				}
				else if (mouseButton.ButtonIndex == MouseButton.WheelDown)
				{
					_scrollDelta = -1.0f;
				}
			}
		}
		else if (@event is InputEventMouseMotion mouseMotion && _isRightMouseDown)
		{
			_mouseDelta += mouseMotion.Relative;
		}
	}

	public override void _Process(double delta)
	{
		if (_targetNode == null || _cameraNode == null) return;

		// Handle Mouse Look (Yaw / Pitch)
		if (_isRightMouseDown && _mouseDelta != Vector2.Zero)
		{
			_currentX -= _mouseDelta.X * SensitivityX * 0.05f;
			_currentY -= _mouseDelta.Y * SensitivityY * 0.05f;
			_currentY = Mathf.Clamp(_currentY, MinPitch, MaxPitch);
			_mouseDelta = Vector2.Zero;
		}

		// Handle Zoom Scroll
		if (Mathf.Abs(_scrollDelta) > 0.01f)
		{
			Distance -= _scrollDelta * ZoomSensitivity;
			Distance = Mathf.Clamp(Distance, MinDistance, MaxDistance);
			_scrollDelta = 0.0f;
		}

		UpdateCameraTransform();
	}

	private void UpdateCameraTransform()
{
	if (_targetNode == null || _cameraNode == null) return;

	if (UsePlanetSurfaceGravityAlignment && _planetCenterNode != null)
	{
		// 1. Calculate local surface Up vector (pointing away from planet center to craft)
		Vector3 localUp = (_targetNode.GlobalPosition - _planetCenterNode.GlobalPosition).Normalized();
		if (localUp.LengthSquared() < 0.0001f) localUp = Vector3.Up;

		// 2. Compute stable tangent basis vectors
		Vector3 worldUpRef = Mathf.Abs(localUp.Dot(Vector3.Up)) > 0.99f ? Vector3.Forward : Vector3.Up;
		Vector3 localForward = (localUp.Cross(worldUpRef)).Normalized();
		Vector3 localRight = localForward.Cross(localUp).Normalized();

		// 3. Construct spherical rotations relative to local surface frame
		Quaternion yawRotation = new Quaternion(localUp, Mathf.DegToRad(_currentX));
		Quaternion pitchRotation = new Quaternion(localRight, Mathf.DegToRad(_currentY));
		Quaternion totalRotation = yawRotation * pitchRotation;

		// 4. Position the camera at distance around the target
		Vector3 offsetDirection = totalRotation * localUp;
		Vector3 position = _targetNode.GlobalPosition + offsetDirection * Distance;

		// 5. Safely orient camera looking at target with dynamic up-reference fallback to prevent polar flips
		Vector3 targetPos = _targetNode.GlobalPosition;
		Vector3 viewDir = (targetPos - position).Normalized();
		
		// If camera is looking almost straight down/up, switch reference up to avoid gimbal lock/inversion
		Vector3 safeUp = localUp;
		if (Mathf.Abs(viewDir.Dot(safeUp)) > 0.95f)
		{
			safeUp = Mathf.Abs(viewDir.Dot(Vector3.Forward)) > 0.95f ? Vector3.Right : Vector3.Forward;
		}

		_cameraNode.GlobalPosition = position;
		_cameraNode.LookAt(targetPos, safeUp);
	}
	else
	{
		// Fallback to standard world-space orbit camera if no planet reference is found
		Quaternion rotation = Quaternion.FromEuler(new Vector3(Mathf.DegToRad(_currentY), Mathf.DegToRad(_currentX), 0));
		Vector3 position = _targetNode.GlobalPosition + rotation * new Vector3(0, 0, Distance);
		_cameraNode.GlobalTransform = new Transform3D(new Basis(rotation), position);
	}
}
}

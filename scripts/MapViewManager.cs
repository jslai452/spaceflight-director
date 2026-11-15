using Godot;

public partial class MapViewManager : Node
{
	[ExportCategory("Cameras")]
	[Export] public Camera3D MainFlightCamera { get; set; }
	[Export] public Camera3D MapCamera { get; set; }

	[ExportCategory("Orbit Line Integration")]
	[Export] public KeplerOrbitRenderer OrbitRenderer { get; set; }

	[ExportCategory("Transition Settings")]
	[Export] public float ZoomOutDuration { get; set; } = 0.35f;
	[Export] public float TargetZoomFov { get; set; } = 100f;
	private float _originalFov;

	[ExportCategory("State")]
	[Export] public bool IsMapViewActive { get; set; } = false;

	private Tween _currentTween;

	public override void _Ready()
	{
		GD.Print("[MapViewManager] Ready. Initializing flight/map cameras.");
		if (MainFlightCamera != null)
		{
			_originalFov = Mathf.Clamp(MainFlightCamera.Fov, 1f, 179f);
		}

		if (MapCamera != null)
		{
			MapCamera.Current = false;
		}

		if (OrbitRenderer != null)
		{
			OrbitRenderer.Visible = false;
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
		{
			if (keyEvent.Keycode == Key.M)
			{
				GD.Print("[MapViewManager] 'M' key detected. Toggling Map View.");
				ToggleMapView();
			}
		}
	}

	public void ToggleMapView()
	{
		if (_currentTween != null && _currentTween.IsValid())
		{
			_currentTween.Kill();
		}

		if (!IsMapViewActive)
		{
			GD.Print("[MapViewManager] Starting transition to Map View...");
			TransitionToMap();
		}
		else
		{
			GD.Print("[MapViewManager] Starting transition to Flight View...");
			TransitionToFlight();
		}
	}

	private void TransitionToMap()
	{
		_currentTween = CreateTween();
		_currentTween.SetParallel(false);

		if (MainFlightCamera != null)
		{
			_currentTween.TweenProperty(MainFlightCamera, "fov", TargetZoomFov, ZoomOutDuration);
		}

		_currentTween.TweenCallback(Callable.From(() =>
		{
			if (MainFlightCamera != null) MainFlightCamera.Current = false;
			if (MapCamera != null) MapCamera.Current = true;
			if (OrbitRenderer != null) OrbitRenderer.Visible = true;
			if (MainFlightCamera != null) MainFlightCamera.Fov = _originalFov;
			IsMapViewActive = true;
			GD.Print("[MapViewManager] Successfully switched to Map View.");
		}));
	}

	private void TransitionToFlight()
	{
		if (OrbitRenderer != null) OrbitRenderer.Visible = false;
		if (MapCamera != null) MapCamera.Current = false;
		if (MainFlightCamera != null)
		{
			MainFlightCamera.Current = true;
			MainFlightCamera.Fov = TargetZoomFov;
		}

		_currentTween = CreateTween();
		_currentTween.SetParallel(false);

		if (MainFlightCamera != null)
		{
			_currentTween.TweenProperty(MainFlightCamera, "fov", _originalFov, ZoomOutDuration);
		}

		_currentTween.TweenCallback(Callable.From(() =>
		{
			if (MainFlightCamera != null) MainFlightCamera.Fov = _originalFov;
			IsMapViewActive = false;
			GD.Print("[MapViewManager] Successfully switched back to Flight View.");
		}));
	}
}

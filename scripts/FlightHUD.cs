using Godot;
using System;

public partial class FlightHUD : Node
{
	[Export] public Label ThrottleLabel;
	[Export] public Label SeaLevelAltitudeLabel;
	[Export] public Label SpeedLabel;
	[Export] public Label ApoapsisLabel;
	[Export] public Label PeriapsisLabel;
	[Export] public Label SASLabel;
	
	[Export] public Control MissionControlUI;
	[Export] public Button MissionControlButton;
	[Export] public Button BackToFlightButton;
	
	private float _throttle = 0;
	private int _altitudeFromSeaLevelMeters = 0;
	private float _speedMetersPerSecond = 0;
	private float _apoapsis = 0;
	private float _periapsis = 0;
	private bool _sas = false;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (MissionControlButton != null) {
			MissionControlButton.Pressed += OnMissionControlButtonPressed;
		}
		if (BackToFlightButton != null) {
			BackToFlightButton.Pressed += OnBackToFlightButtonPressed;
		}
	}
	
	public void OnBackToFlightButtonPressed() {
		MissionControlUI.Visible = false;
	}
	
	public void OnMissionControlButtonPressed() {
		if (MissionControlUI != null) {
			MissionControlUI.Visible = true;
		}
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public void SetThrottle(float newThrottle) {
		if (newThrottle != null) {
			_throttle = newThrottle;
		}
		UpdateUI();
	}
	
	public void SetAltitude(float newAltitude) {
		if (newAltitude != null) {
			_altitudeFromSeaLevelMeters = (int)Math.Round(newAltitude);
		}
		UpdateUI();
	}
	
	public void SetSpeed(float newSpeed) {
		if (newSpeed != null) {
			_speedMetersPerSecond = newSpeed;
		}
		UpdateUI();
	}
	
	public void UpdateApoapsis(float newApoapsis) {
		if (newApoapsis < 0) {
			_apoapsis = 0;
		} else {
		_apoapsis = newApoapsis;
		}
		UpdateUI();
	}
	public void UpdatePeriapsis(float newPeriapsis) {
		if (newPeriapsis < 0) {
			_periapsis = 0;
		} else {
			_periapsis = newPeriapsis;
		}
		UpdateUI();
	}
	
	public void UpdateSAS(bool newSas) {
		_sas = newSas;
		UpdateUI();
	}
	
	public void UpdateUI() {
		SeaLevelAltitudeLabel.Text = $"Altitude: {_altitudeFromSeaLevelMeters.ToString()} meters";
		ThrottleLabel.Text = $"Throttle: {Math.Round(_throttle).ToString()} %";
		SpeedLabel.Text = $"Speed(Surface): {_speedMetersPerSecond:F2} m/s";
		ApoapsisLabel.Text = $"Apoapsis: {Math.Round(_apoapsis).ToString()} m";
		PeriapsisLabel.Text = $"Periapsis: {Math.Round(_periapsis).ToString()} m";
		SASLabel.Text = $"SAS: {(_sas ? "Enabled" : "Disabled")}";
	}
}

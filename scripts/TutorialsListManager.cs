using Godot;
using System;

public partial class TutorialsListManager : Node
{
	[Export] public Button FlightTutorialButton;
	
	private string _flightTutorialPath = "res://scenes/flight_tutorial.tscn";
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		FlightTutorialButton.Pressed += OnFlightButtonPressed;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
	
	private void OnFlightButtonPressed() {
		SceneLoader.LoadScene(GetTree(), _flightTutorialPath);
	}
}

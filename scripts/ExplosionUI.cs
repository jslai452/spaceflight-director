using Godot;
using System;

public partial class ExplosionUI : Control
{
	private string _mainMenuScenePath = "res://scenes/main_menu_scene.tscn";
	
	[Export] public Button ExitToMainMenuButton;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (ExitToMainMenuButton != null) {
			ExitToMainMenuButton.Pressed += OnExitToMainMenuButtonPressed;
		}
	}
	
	private void OnExitToMainMenuButtonPressed() {
		GetTree().ChangeSceneToFile(_mainMenuScenePath);
	}
}

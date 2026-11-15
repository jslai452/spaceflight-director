using Godot;
using System;

public partial class Credits : Node
{
	[Export] public Button ExitCreditsButton;
	private string _mainMenuPath = "res://scenes/main_menu_scene.tscn";
	
	public override void _Ready() {
		if (ExitCreditsButton != null) {
			ExitCreditsButton.Pressed += OnCreditsButtonPressed;
		}
	}
	
	private void OnCreditsButtonPressed() {
		GetTree().ChangeSceneToFile(_mainMenuPath);
	}
}

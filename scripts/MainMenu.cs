using Godot;
using System;

public partial class MainMenu : Control
{
	private string _firstLevelScenePath = "res://scenes/scenario_select.tscn";
	private string _creditsScenePath = "res://scenes/credits.tscn";
	private string _tutorialsListPath = "res://scenes/tutorials_list.tscn";
	
	[Export] public Button PlayButton;
	[Export] public Button QuitButton;
	[Export] public Button CreditsButton;
	[Export] public Button TutorialsButton;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (PlayButton != null) {
			PlayButton.Pressed += OnPlayButtonPressed;
		}
		
		if (QuitButton != null) {
			QuitButton.Pressed += OnQuitButtonPressed;
		}
		if (CreditsButton != null) {
			CreditsButton.Pressed += OnCreditsButtonPressed;
		}
		if (TutorialsButton != null) {
			TutorialsButton.Pressed += OnTutorialsButtonPressed;
		}
	}
	
	private void OnPlayButtonPressed() {
		GetTree().ChangeSceneToFile(_firstLevelScenePath);
	}
	
	private void OnTutorialsButtonPressed() {
		GetTree().ChangeSceneToFile(_tutorialsListPath);
	}
	
	private void OnCreditsButtonPressed() {
		GetTree().ChangeSceneToFile(_creditsScenePath);
	}
	
	private void OnQuitButtonPressed() {
		GetTree().Quit();
	}
}

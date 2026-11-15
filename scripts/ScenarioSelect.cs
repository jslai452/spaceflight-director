using Godot;
using System;

public partial class ScenarioSelect : Node
{
	[Export] public TextureButton BuildYourOwnRocketButton;
	[Export] public TextureButton NancyRomanSpaceTelescopeMissionButton;
	[Export] public Button BackButton;
	[Export] public TextureButton MissionSelectButton;
	private string _vabScenePath = "res://scenes/vab_scene.tscn";
	private string _nancyRomanSpaceTelescopeMissionPath = "res://scenes/nancy_roman_space_telescope_mission.tscn";
	private string _mainMenuPath = "res://scenes/main_menu_scene.tscn";
	private string _missionSelectPath = "res://scenes/mission_control_ui.tscn";
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (BuildYourOwnRocketButton != null) {
			BuildYourOwnRocketButton.Pressed += GoToVabScene;
		}
		if (NancyRomanSpaceTelescopeMissionButton != null) {
			NancyRomanSpaceTelescopeMissionButton.Pressed += GoToNancyRomanSpaceTelescopeScene;
		}
		if (BackButton != null) {
			BackButton.Pressed += GoToMainMenu;
		}
		if (MissionSelectButton != null) {
			MissionSelectButton.Pressed += GoToMissionControlUIScene;
		}
	}
	
	private void GoToVabScene() {
		if (_vabScenePath == null) {return;}
		SceneLoader.LoadScene(GetTree(), _vabScenePath);
	}
	
	private void GoToNancyRomanSpaceTelescopeScene() {
		if (_nancyRomanSpaceTelescopeMissionPath == null) { return; }
		SceneLoader.LoadScene(GetTree(), _nancyRomanSpaceTelescopeMissionPath);
	}
	
	private void GoToMissionControlUIScene() {
		if (_missionSelectPath == null) {return;}
		SceneLoader.LoadScene(GetTree(), _missionSelectPath);
	}
	private void GoToMainMenu() {
		if (_mainMenuPath == null) {return;}
		GetTree().ChangeSceneToFile(_mainMenuPath);
	}
}

using Godot;
using System;
using Godot.Collections;

public partial class MissionUIManager : Node
{
	[Export] public VBoxContainer vBoxContainer;
	[Export] public Button exitButton;
	private string _scenarioSelectPath = "res://scenes/scenario_select.tscn";
	private string _launchScenePath = "res://scenes/launch_scene.tscn";

	public override void _Ready()
	{
		if (exitButton != null) {
			exitButton.Pressed += OnExitButtonPressed;
		}
		// Fallback to finding it if not assigned in Inspector
		if (vBoxContainer == null)
		{
			vBoxContainer = GetNode<VBoxContainer>("VBoxContainer");
		}

		if (vBoxContainer != null)
		{
			foreach (Node buttonNode in vBoxContainer.GetChildren()) 
			{
				if (buttonNode is Button button) 
				{
					button.Pressed += () => {OnMissionButtonPressed(button); GoToLaunchScene();};
				}
			}
		}
	}
	
	private void OnExitButtonPressed(){
		SceneLoader.LoadScene(GetTree(), _scenarioSelectPath);
	}
	
private void OnMissionButtonPressed(Button button) 
{
	foreach (Node node in button.GetChildren()) 
	{
		if (node.Name == "MissionObjectives") 
		{
			if (node is MissionObjectives missionObjectivesScript) 
			{
				// Use the exported array defined in MissionObjectives.cs
				Array<Objective> missionObjectivesList = missionObjectivesScript.Objectives;
				
				if (missionObjectivesList == null || missionObjectivesList.Count == 0) 
				{
					GD.PrintErr("[MissionUIManager] ERROR: Objectives array is empty!");
					continue;
				} 
				else 
				{
					MissionManager.Instance?.UpdateMissionObjectives(missionObjectivesList);
				}
			}
		} 
		else if (node.Name == "MissionBudget") 
		{
			if (node is MissionBudget missionBudgetScript) 
			{
				long missionBudget = missionBudgetScript.Budget;
				if (missionBudget >= 0) 
				{
					MissionManager.Instance?.UpdateMissionBudget(missionBudget);
				}
			}
		}
	}
	MissionManager.Instance.UpdateMissionStatus(true);
}
	
	private void GoToLaunchScene() {
		if (_launchScenePath == null) {return;}
		SceneLoader.LoadScene(GetTree(), _launchScenePath);
	}
}

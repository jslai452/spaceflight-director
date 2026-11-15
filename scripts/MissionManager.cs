using Godot;
using System;
using Godot.Collections;

public partial class MissionManager : Node
{
	public static MissionManager Instance { get; private set; }

	// 1. Declare a signal that broadcasts the new mission status
	[Signal]
	public delegate void MissionStatusChangedEventHandler(bool inMission);

	[Export] private bool _inMission = false;

	// 2. Wrap it in a public property
	public bool InMission 
	{
		get => _inMission;
		private set 
		{
			if (_inMission != value)
			{
				_inMission = value;
				GD.Print($"[MissionManager] 🚀 MissionStatusChanged -> InMission is now: {_inMission}");
				// 3. Emit the signal whenever the value changes
				EmitSignal(SignalName.MissionStatusChanged, _inMission);
			}
		}
	}
	
	public Array<Objective> missionObjectivesList = new Array<Objective>();
	public long budget = 0;

	public override void _Ready()
	{
		if (Instance != null && Instance != this)
		{
			QueueFree();
		}
		else
		{
			Instance = this;
			GD.Print("[MissionManager] Initialized successfully.");
		}
	}

	public bool GetMissionStatus() 
	{
		return _inMission;
	}

	public void UpdateMissionStatus(bool newMissionStatus) 
	{
		InMission = newMissionStatus;
		GD.Print($"[MissionManager] UpdateMissionStatus called. New status: {newMissionStatus}");
	}
	
	
	public void UpdateMissionObjectives(Array<Objective> newMissionObjectivesList) 
	{
		missionObjectivesList = newMissionObjectivesList;
		GD.Print($"[MissionManager] 🎯 Mission objectives updated. Total objectives: {missionObjectivesList?.Count ?? 0}");
	}
	
	public void UpdateMissionBudget(long newBudget) 
	{
		budget = newBudget;
		GD.Print($"[MissionManager] 💰 Mission budget updated to: {budget}");
	}
}

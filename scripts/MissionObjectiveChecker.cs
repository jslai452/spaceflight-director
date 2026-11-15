using Godot;
using System;
using System.Collections.Generic;

public partial class MissionObjectiveChecker : Node
{
	[ExportCategory("Craft & Gravity References")]
	[Export] public RigidBody3D CraftRigidbody { get; set; }
	[Export] public CelestialBodyGravity RocketGravity { get; set; }
	[Export] public TimeWarpManager WarpManager { get; set; }
	[Export] public TransmitManager TransmitManagerRef { get; set; } // Reference to check transmissions

	[ExportCategory("UI References")]
	[Export] private VBoxContainer _objectivesContainer;

	// Path to your mission complete scene
	[Export] private string _missionCompleteScenePath = "res://scenes/mission_complete.tscn";

	// Dictionary mapping objective function strings to their validation methods
	private Dictionary<string, Func<bool>> _objectiveCheckers;
	private bool _missionCompletedTriggered = false;

	// Startup grace period to prevent instant triggers on spawn
	private float _startupTimer = 0f;
	private const float RequiredGracePeriod = 2.0f; 

	// Track UI labels corresponding to objectives for real-time updates
	private List<(Objective objective, Label label, Func<bool> checker)> _trackedObjectives = new();
	private bool _uiInitialized = false;

	public override void _Ready()
	{
		_objectiveCheckers = new Dictionary<string, Func<bool>>()
		{
			{ "check_low_earth_orbit", CheckForLowEarthOrbit },
			{ "check_high_earth_orbit", CheckForHighEarthOrbit },
			{ "check_stable_orbit", CheckStableOrbit },
			{ "check_outside_atmosphere", CheckOutsideAtmosphere },
			
			// Separate science check functions
			{ "check_transmitted_temperature", CheckTransmittedTemperature },
			{ "check_transmitted_pressure", CheckTransmittedPressure },
			{ "check_transmitted_yeast_sample", CheckTransmittedYeastSample }
		};
	}

	public override void _Process(double delta)
	{
		// Attempt to initialize UI once MissionManager and objectives are available
		if (!_uiInitialized && MissionManager.Instance != null && MissionManager.Instance.missionObjectivesList != null && MissionManager.Instance.missionObjectivesList.Count > 0)
		{
			InitializeObjectivesUI();
		}

		// Only check if we are in a mission and haven't already completed it
		if (_missionCompletedTriggered || MissionManager.Instance == null || !MissionManager.Instance.GetMissionStatus())
		{
			return;
		}

		// Ignore objective checks during the initial spawn window
		if (_startupTimer < RequiredGracePeriod)
		{
			_startupTimer += (float)delta;
			return;
		}

		CheckAllObjectives();
	}

	private void InitializeObjectivesUI()
	{
		GD.Print("[Debug] InitializeObjectivesUI called.");

		if (_objectivesContainer == null)
		{
			GD.PrintErr("[MissionObjectiveChecker] ERROR: Objectives VBoxContainer is NOT assigned in the Inspector!");
			return;
		}

		// Clear any existing children in the container
		foreach (Node child in _objectivesContainer.GetChildren())
		{
			child.QueueFree();
		}

		_trackedObjectives.Clear();

		var objectivesList = MissionManager.Instance?.missionObjectivesList;
		if (objectivesList == null)
		{
			GD.PrintErr("[MissionObjectiveChecker] ERROR: MissionManager.Instance.missionObjectivesList is NULL!");
			return;
		}

		GD.Print($"[Debug] Found {objectivesList.Count} objectives in MissionManager.");

		foreach (var objective in objectivesList)
		{
			if (objective == null)
			{
				GD.Print("[Debug] Skipped a null objective in the list.");
				continue;
			}

			string functionKey = objective.ObjectiveFunction;
			string objName = objective.ObjectiveName;
			GD.Print($"[Debug] Processing objective: '{objName}' with function key: '{functionKey}'");

			if (_objectiveCheckers.TryGetValue(functionKey, out var checkFunction))
			{
				Label objectiveLabel = new Label();
				objectiveLabel.Text = $"[ ] {objName}";
				objectiveLabel.AddThemeColorOverride("font_color", Colors.White);
				_objectivesContainer.AddChild(objectiveLabel);

				_trackedObjectives.Add((objective, objectiveLabel, checkFunction));
				GD.Print($"[Debug] Successfully created and added label for: {objName}");
			}
			else
			{
				GD.PrintErr($"[MissionObjectiveChecker] ERROR: Missing checker function for key: '{functionKey}'. Check spelling/casing in your Objective resource!");
			}
		}

		_uiInitialized = true;
		GD.Print($"[Debug] UI Initialization complete. Total tracked objectives: {_trackedObjectives.Count}");
	}

	private void CheckAllObjectives()
	{
		if (_trackedObjectives.Count == 0) return;

		bool allCompleted = true;

		foreach (var item in _trackedObjectives)
		{
			bool isCompleted = item.checker();

			// Update UI text / style dynamically based on completion status
			if (isCompleted)
			{
				item.label.Text = $"[X] {item.objective.ObjectiveName}";
				item.label.AddThemeColorOverride("font_color", Colors.LightGreen);
			}
			else
			{
				item.label.Text = $"[ ] {item.objective.ObjectiveName}";
				item.label.RemoveThemeColorOverride("font_color");
				allCompleted = false;
			}
		}

		if (allCompleted)
		{
			_missionCompletedTriggered = true;
			TriggerMissionComplete();
		}
	}

	private void TriggerMissionComplete()
	{
		GD.Print("[MissionObjectiveChecker] 🎉 All objectives completed! Transitioning to Mission Complete screen.");
		MissionManager.Instance.UpdateMissionStatus(false);
		SceneLoader.LoadScene(GetTree(), _missionCompleteScenePath);
	}

	// --- Bulletproof Orbital Parameter Calculator (Synced with KeplerOrbitRenderer math) ---
	private bool GetOrbitalParameters(out float periapsisHeight, out float apoapsisHeight, out float eccentricity)
	{
		periapsisHeight = 0f;
		apoapsisHeight = 0f;
		eccentricity = 0f;

		if (CraftRigidbody == null || RocketGravity == null || RocketGravity.PlanetCenter == null) return false;

		float mu = RocketGravity.Mu;
		Node3D realPlanet = RocketGravity.PlanetCenter;

		if (mu <= 0f || realPlanet == null) return false;

		Vector3 r = CraftRigidbody.GlobalPosition - realPlanet.GlobalPosition;

		Vector3 planetVel = Vector3.Zero;
		if (realPlanet is RigidBody3D planetRb)
		{
			planetVel = planetRb.LinearVelocity;
		}

		Vector3 v = Vector3.Zero;
		bool isWarping = WarpManager != null && WarpManager.IsTimeWarping;
		if (isWarping)
		{
			v = WarpManager.GetCurrentOnRailsVelocity() - planetVel;
		}
		else
		{
			v = CraftRigidbody.LinearVelocity - planetVel;
		}

		float rMag = r.Length();
		float vMag = v.Length();

		if (!isWarping && (vMag < 0.05f || rMag < 0.001f || !v.IsFinite() || !r.IsFinite()))
			return false;

		Vector3 h = r.Cross(v);
		if (h.LengthSquared() < 0.0001f) return false;

		Vector3 eVec = v.Cross(h) / mu - r / rMag;
		eccentricity = eVec.Length();

		if (float.IsNaN(eccentricity) || float.IsInfinity(eccentricity) || !eVec.IsFinite())
			return false;

		float energy = (vMag * vMag / 2f) - (mu / rMag);
		if (float.IsNaN(energy) || float.IsInfinity(energy) || energy >= 0f)
			return false; // Must be a bound orbit (elliptic trajectory)

		float a = -mu / (2f * energy);
		if (float.IsNaN(a) || float.IsInfinity(a) || a <= 0f)
			return false;

		if (eccentricity >= 1.0f) return false; // Exclude hyperbolic/escape trajectories

		float periapsisRadius = a * (1f - eccentricity);
		float apoapsisRadius = a * (1f + eccentricity);
		float planetRadius = RocketGravity.PlanetRadius;

		periapsisHeight = periapsisRadius - planetRadius;
		apoapsisHeight = apoapsisRadius - planetRadius;

		return true;
	}

	// --- Individual Orbital Check Functions ---

	public bool CheckOutsideAtmosphere()
	{
		if (CraftRigidbody == null || RocketGravity == null) return false;
		return RocketGravity.IsOutsideAtmosphere(CraftRigidbody.GlobalPosition);
	}

	public bool CheckStableOrbit()
	{
		if (!GetOrbitalParameters(out float periapsisHeight, out float apoapsisHeight, out float eccentricity))
			return false;

		return periapsisHeight > RocketGravity.AtmosphereHeight;
	}

	public bool CheckForLowEarthOrbit()
	{
		if (!GetOrbitalParameters(out float periapsisHeight, out float apoapsisHeight, out float eccentricity))
			return false;

		return periapsisHeight > RocketGravity.AtmosphereHeight && apoapsisHeight <= 200000f;
	}

	public bool CheckForHighEarthOrbit()
	{
		if (!GetOrbitalParameters(out float periapsisHeight, out float apoapsisHeight, out float eccentricity))
			return false;

		return periapsisHeight > RocketGravity.AtmosphereHeight && apoapsisHeight > 200000f;
	}

	// --- Individual Science Check Functions ---

	public bool CheckTransmittedTemperature()
	{
		return HasTransmittedScienceOfType("temperature");
	}

	public bool CheckTransmittedPressure()
	{
		return HasTransmittedScienceOfType("pressure");
	}

	public bool CheckTransmittedYeastSample()
	{
		return HasTransmittedScienceOfType("yeast_sample");
	}

	// Helper to scan TransmitManager's TransmittedData dictionary for a matching instrument type string at Index 0
	private bool HasTransmittedScienceOfType(string targetType)
	{
		if (TransmitManagerRef == null) return false;

		targetType = targetType.Trim().ToLower();

		foreach (var kvp in TransmitManagerRef.TransmittedData)
		{
			var data = kvp.Value;
			if (data != null && data.Count > 0)
			{
				string transmittedType = data[0].Trim().ToLower(); // data[0] is the science type string
				if (transmittedType == targetType)
				{
					return true;
				}
			}
		}

		return false;
	}
}

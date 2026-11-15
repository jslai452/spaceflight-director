using Godot;
using System;
using System.Collections.Generic;

public partial class ReactionWheel : Node
{
	public enum NoseAxis 
	{ 
		PositiveY, 
		NegativeY, 
		PositiveZ, 
		NegativeZ, 
		PositiveX, 
		NegativeX 
	}

	[ExportCategory("Torque Settings")]
	[Export] public float pitchTorque = 100000f;
	[Export] public float yawTorque = 100000f;
	[Export] public float rollTorque = 500000f;
	
	[ExportCategory("References")]
	[Export] public RigidBody3D currentRocketRb;
	[Export] public CelestialBodyGravity rocketGravity;
	[Export] public Button ProgradeButton;
	[Export] public Button RetrogradeButton;
	[Export] public Label ElectricityLabel;

	[ExportCategory("Model Axis Configuration (Per Rocket)")]
	[Export] public NoseAxis RocketNoseDirection = NoseAxis.PositiveY;
	[Export] public bool InvertPitchInput = false;
	[Export] public bool InvertYawInput = false;

	[ExportGroup("Power Consumption Settings")]
	[Export] public float maxPowerDrainPerSecond = 5.0f;

	[ExportGroup("Autopilot (SAS) Settings")]
	[Export] public bool isSasOn = false; // Master SAS toggle
	[Export] public bool isProgradeOn = false;
	[Export] public bool isRetrogradeOn = false;
	[Export] public float autopilotKp = 5.0f; 
	[Export] public float autopilotKd = 1.5f; 
	
	[Export] public FlightHUD flightHud;

	private Vector3 inputVector;
	private bool hasPower = true;

	public override void _Ready()
{
	if (ProgradeButton != null)
	{
		ProgradeButton.Pressed += () =>
		{
			isProgradeOn = !isProgradeOn;
			if (isProgradeOn) 
			{
				isRetrogradeOn = false;
				isSasOn = true; // Auto-engage master SAS when vector is selected
			}
			GD.Print($"[SAS] Prograde mode: {isProgradeOn}");
		};
	}

	if (RetrogradeButton != null)
	{
		RetrogradeButton.Pressed += () =>
		{
			isRetrogradeOn = !isRetrogradeOn;
			if (isRetrogradeOn) 
			{
				isProgradeOn = false;
				isSasOn = true; // Auto-engage master SAS when vector is selected
			}
			GD.Print($"[SAS] Retrograde mode: {isRetrogradeOn}");
		};
	}
}

public override void _Process(double delta)
{
	if (currentRocketRb == null) return;

	// 1. Toggle SAS Master Switch with 'T' key (or custom action)
	if (Input.IsActionJustPressed("toggle_sas"))
	{
		isSasOn = !isSasOn;
		GD.Print($"[SAS] Master SAS State: {isSasOn}");

		if (!isSasOn)
		{
			isProgradeOn = false;
			isRetrogradeOn = false;
		}
	}
	if (flightHud != null) {
		flightHud.UpdateSAS(true);
	}

	// 2. Get manual player input (WASD + QE)
	float pitch = Input.GetAxis("pitch_down", "pitch_up"); 
	float yaw = Input.GetAxis("yaw_left", "yaw_right");    
	float roll = Input.GetAxis("roll_left", "roll_right"); 

	Vector3 manualInput = new Vector3(pitch, yaw, roll);
	manualInput = manualInput.LengthSquared() > 0.05f ? manualInput : Vector3.Zero;

	// 3. Calculate Autopilot Vector based on Priority
	Vector3 autopilotInput = Vector3.Zero;

	if (isProgradeOn || isRetrogradeOn)
	{
		// --- PRIORITY 1: Prograde / Retrograde Vector Lock ---
		Vector3 velocity = currentRocketRb.LinearVelocity;

		if (rocketGravity != null && rocketGravity.PlanetCenter is RigidBody3D planetRb)
		{
			velocity -= planetRb.LinearVelocity;
		}
		
		if (velocity.Length() >= 1.5f)
		{
			Vector3 targetDir = isProgradeOn ? velocity.Normalized() : -velocity.Normalized();
			
			Basis rbBasis = currentRocketRb.GlobalTransform.Basis;
			Vector3 rocketForward = RocketNoseDirection switch
			{
				NoseAxis.PositiveY => rbBasis.Y,
				NoseAxis.NegativeY => -rbBasis.Y,
				NoseAxis.PositiveZ => rbBasis.Z,
				NoseAxis.NegativeZ => -rbBasis.Z,
				NoseAxis.PositiveX => rbBasis.X,
				NoseAxis.NegativeX => -rbBasis.X,
				_ => rbBasis.Y
			};

			Basis invBasis = rbBasis.Inverse();
			Vector3 errorAxis = rocketForward.Cross(targetDir);
			Vector3 localError = invBasis * errorAxis;
			Vector3 localAngularVel = invBasis * currentRocketRb.AngularVelocity;

			float targetPitchInput = (localError.X * autopilotKp) - (localAngularVel.X * autopilotKd);
			float targetYawInput   = (localError.Z * autopilotKp) - (localAngularVel.Z * autopilotKd);
			float targetRollInput  = -(localAngularVel.Y * autopilotKd);

			if (InvertPitchInput) targetPitchInput = -targetPitchInput;
			if (InvertYawInput) targetYawInput = -targetYawInput;

			autopilotInput = new Vector3(
				Mathf.Clamp(targetPitchInput, -1f, 1f),
				Mathf.Clamp(targetYawInput, -1f, 1f),
				Mathf.Clamp(targetRollInput, -1f, 1f)
			);
		}
	}
	else if (isSasOn)
	{
		// --- PRIORITY 2: Basic Stability Mode (Holds current orientation) ---
		Basis rbBasis = currentRocketRb.GlobalTransform.Basis;
		Basis invBasis = rbBasis.Inverse();
		Vector3 localAngularVel = invBasis * currentRocketRb.AngularVelocity;

		float targetPitchInput = -localAngularVel.X * autopilotKd;
		float targetYawInput   = -localAngularVel.Z * autopilotKd;
		float targetRollInput  = -localAngularVel.Y * autopilotKd;

		if (InvertPitchInput) targetPitchInput = -targetPitchInput;
		if (InvertYawInput) targetYawInput = -targetYawInput;

		autopilotInput = new Vector3(
			Mathf.Clamp(targetPitchInput, -1f, 1f),
			Mathf.Clamp(targetYawInput, -1f, 1f),
			Mathf.Clamp(targetRollInput, -1f, 1f)
		);
	}

	// 4. KSP Override: Manual input takes absolute priority while keys are held
	if (manualInput != Vector3.Zero)
	{
		inputVector = manualInput;
	}
	else
	{
		inputVector = autopilotInput;
	}
}
	
	public void ProcessAllElectricityDescendants(double delta)
	{
		if (currentRocketRb == null) return;

		// Only drain power if SAS is on and actively outputting torque/autopilot corrections
		float intensity = isSasOn ? Mathf.Max(Mathf.Abs(inputVector.X), Mathf.Max(Mathf.Abs(inputVector.Y), Mathf.Abs(inputVector.Z))) : 0f;
		float drain = intensity * maxPowerDrainPerSecond * (float)delta;

		List<Battery> allBatteries = GetComponentsInChildren<Battery>(currentRocketRb);
		float totalRemainingElectricity = 0;
		float totalMaxElectricity = 0;
		
		foreach (Battery battery in allBatteries)
		{
			totalRemainingElectricity += battery.GetElectricity();
			totalMaxElectricity += battery.MaxElectricity;
		}

		if (ElectricityLabel != null)
		{
			ElectricityLabel.Text = $"Electricity: {totalRemainingElectricity:F1} / {totalMaxElectricity:F1}";
		}

		if (drain <= 0f)
		{
			hasPower = totalRemainingElectricity > 0f;
			return;
		}

		float remainingDrainNeeded = drain;
		float totalAvailablePower = totalRemainingElectricity;

		if (allBatteries.Count > 0 && totalAvailablePower > 0f)
		{
			float drainPerBattery = remainingDrainNeeded / allBatteries.Count;
			foreach (Battery battery in allBatteries)
			{
				float currentElec = battery.GetElectricity();
				float take = Mathf.Min(currentElec, drainPerBattery);
				battery.DecrementElectricity(take);
				remainingDrainNeeded -= take;
			}

			if (remainingDrainNeeded > 0f)
			{
				foreach (Battery battery in allBatteries)
				{
					float currentElec = battery.GetElectricity();
					if (currentElec > 0f)
					{
						float take = Mathf.Min(currentElec, remainingDrainNeeded);
						battery.DecrementElectricity(take);
						remainingDrainNeeded -= take;
					}
				}
			}
		}

		hasPower = totalAvailablePower > 0f;
		
		if (!hasPower)
		{
			isSasOn = false;
			isProgradeOn = false;
			isRetrogradeOn = false;
			inputVector = Vector3.Zero;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (currentRocketRb == null || currentRocketRb.Freeze) return;

		ProcessAllElectricityDescendants(delta);

		if (!hasPower || !isSasOn && inputVector == Vector3.Zero) return;

		Basis rocketBasis = currentRocketRb.GlobalTransform.Basis;

		List<ReactionWheel> allWheels = GetComponentsInChildren<ReactionWheel>(currentRocketRb);
		float wheelCountMultiplier = allWheels.Count > 0 ? 1.0f / allWheels.Count : 1.0f;

		Vector3 localTorque = new Vector3(
			inputVector.X * pitchTorque * wheelCountMultiplier, 
			inputVector.Z * rollTorque * wheelCountMultiplier,  
			inputVector.Y * yawTorque * wheelCountMultiplier    
		);

		Vector3 globalTorque = rocketBasis * localTorque; 
		currentRocketRb.ApplyTorque(globalTorque);

		float maxAngularSpeed = 3.0f; 
		if (currentRocketRb.AngularVelocity.Length() > maxAngularSpeed)
		{
			currentRocketRb.AngularVelocity = currentRocketRb.AngularVelocity.Normalized() * maxAngularSpeed;
		}
	}

	private List<T> GetComponentsInChildren<T>(Node root) where T : Node
	{
		List<T> results = new List<T>();
		if (root is T match)
		{
			results.Add(match);
		}
		foreach (Node child in root.GetChildren(true))
		{
			results.AddRange(GetComponentsInChildren<T>(child));
		}
		return results;
	}
}

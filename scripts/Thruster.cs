using Godot;
using System;

public partial class Thruster : Node3D
{
	/// <summary>
	/// Multiplier to scale this specific thruster's power relative to others.
	/// </summary>
	[Export] public float ThrustMultiplier = 1f;

	/// <summary>
	/// Toggle whether this individual engine is active.
	/// </summary>
	[Export] public bool IsOn = false;

	/// <summary>
	/// The specific fuel tank this engine draws fuel from.
	/// </summary>
	[Export] public FuelTank AssignedFuelTank;

	// Default values based on Falcon Heavy second stage
	[Export] public float SpecificImpulse = 348f;
	[Export] public float MaxConsumptionRate = 274f;
	[Export] public float ThrottlePercent = 1f; // Assumed 100% throttle for testing
	
	// Unity's AudioSource 3D equivalent in Godot
	[Export] public AudioStreamPlayer EngineAudio;
	[Export] public GpuParticles3D PrimaryPlume { get; set; }
	[Export] public GpuParticles3D SecondaryPlume { get; set; }
	[Export] public float MinSpeedScale { get; set; } = 0.5f;
	[Export] public float MaxSpeedScale { get; set; } = 2.0f;
	[Export] public float MinEmissionRatio { get; set; } = 0.05f;

	/// <summary>
	/// Burns fuel and returns generated thrust. Added 'double delta' parameter to match Godot physics execution frame step.
	/// </summary>
	public float BurnFuel(float throttlePercent, float specificImpulse, float maxConsumptionRate, double delta)
{
	if (!IsOn || AssignedFuelTank == null || AssignedFuelTank.FuelMass <= 0f)
	{
		StopEffects();
		return 0f;
	}

	// 1. Calculate how much fuel is physically removed from the tank this frame
	float requestedBurn = maxConsumptionRate * throttlePercent * (float)delta;
	float burned = AssignedFuelTank.ConsumeFuel(requestedBurn);
	GD.Print($"burned this much {requestedBurn}");
	GD.Print($"assigned fuel tank {AssignedFuelTank}");

	if (burned <= 0f)
	{
		StopEffects();
		return 0f;
	}
	
	UpdateDynamicEffects(throttlePercent);
	PlayEffects();

	// 2. Continuous mass flow rate (kg/s) regardless of delta frame size
	float continuousMassFlowRate = maxConsumptionRate * throttlePercent;
	const float g0 = 9.80665f;
	ThrottlePercent = throttlePercent;

	// 3. Return standard thrust force in Newtons
	return continuousMassFlowRate * (specificImpulse * g0);
}

	private void UpdateDynamicEffects(float throttle)
	{
		if (EngineAudio != null)
		{
			// Pitch increases slightly as the engine works harder (idle rumble to high whine)
			// Note: Godot uses PitchScale instead of pitch
			EngineAudio.PitchScale = Mathf.Lerp(0.8f, 1.2f, throttle);
			
			// Volume increases dramatically with throttle
			// Note: Godot uses VolumeDb. To mimic Unity linear volume scaling, use Mathf.LinearToDb()
			EngineAudio.VolumeDb = Mathf.LinearToDb(Mathf.Lerp(0.1f, 1.0f, throttle));
		}
		bool isThrottling = throttle > 0.01f && IsOn;

		// Primary Plume particle scaling
		if (PrimaryPlume != null)
		{
			PrimaryPlume.Emitting = isThrottling;
			PrimaryPlume.AmountRatio = isThrottling ? Mathf.Max(throttle, MinEmissionRatio) : 0.0f;
			PrimaryPlume.SpeedScale = Mathf.Lerp(MinSpeedScale, MaxSpeedScale, throttle);
		}

		// Secondary Plume particle scaling
		if (SecondaryPlume != null)
		{
			SecondaryPlume.Emitting = isThrottling;
			SecondaryPlume.AmountRatio = isThrottling ? Mathf.Max(throttle, MinEmissionRatio) : 0.0f;
			SecondaryPlume.SpeedScale = Mathf.Lerp(MinSpeedScale, MaxSpeedScale, throttle);
		}
	}

	private void PlayEffects()
	{
		if (EngineAudio != null && !EngineAudio.Playing)
		{
			EngineAudio.Play();
		}
	}

	private void StopEffects()
	{
		if (EngineAudio != null && EngineAudio.Playing)
		{
			EngineAudio.Stop();
		}
		// Turn off particle emitters when engine stops
		if (PrimaryPlume != null)
		{
			PrimaryPlume.Emitting = false;
			PrimaryPlume.AmountRatio = 0.0f;
		}

		if (SecondaryPlume != null)
		{
			SecondaryPlume.Emitting = false;
			SecondaryPlume.AmountRatio = 0.0f;
		}
	}
	/// <summary>
	/// Public helper to force stop audio and particle effects when throttle is zero or engine is disabled.
	/// </summary>
	public void ForceStopEffects()
	{
		StopEffects();
	}
}

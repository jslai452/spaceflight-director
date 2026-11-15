using Godot;
using System;

public partial class CraftExplosion : Node
{
	[Signal]
	public delegate void HighSpeedImpactDetectedEventHandler(float impactSpeed);
	
	[Export] public AudioStreamPlayer ExplosionAudio;
	[Export] 
	public float SpeedThreshold = 5.0f;

	private RigidBody3D _parentBody;
	
	private string _craftExplosionUIPath = "res://scenes/explosion_ui.tscn";

	public override void _Ready()
	{
		_parentBody = GetParent<RigidBody3D>();
		
		if (_parentBody == null)
		{
			GD.PrintErr("CraftExplosion node must be a child of a RigidBody3D!");
			return;
		}

		// Enable contact monitoring so BodyEntered fires
		_parentBody.ContactMonitor = true;
		
		if (_parentBody.MaxContactsReported < 1) 
		{
			_parentBody.MaxContactsReported = 1;
		}
		
		_parentBody.BodyEntered += OnParentBodyEntered;
	}

	private void OnParentBodyEntered(Node body)
	{
		// Get the current speed of the craft using the length of its velocity vector
		float currentSpeed = _parentBody.LinearVelocity.Length();

		if (currentSpeed >= SpeedThreshold)
		{
			// Emit the event signal for explosion handling
			EmitSignal(SignalName.HighSpeedImpactDetected, currentSpeed);
			
			// Optional: Trigger your explosion logic directly here if needed
			Explode();
		}
	}

	private void Explode()
	{
		GD.Print("BOOM! Exploded at high speed!");
		// Add your instantiation code for particles, sound, or QueueFree() here
		ExplosionAudio.Play();
		_parentBody.QueueFree();
		SceneLoader.LoadScene(GetTree(), _craftExplosionUIPath);
	}
}

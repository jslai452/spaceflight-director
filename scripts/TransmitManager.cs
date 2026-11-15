using Godot;
using System;
using System.Collections.Generic;

public partial class TransmitManager : Node
{
	[ExportGroup("Nodes")]
	[Export] public Node3D TransmitterNode; 
	[Export] public Node3D TargetReceiver;  
	[Export] public ScienceInstrumentsManager InstrumentsManager;

	[ExportGroup("UI References")]
	[Export] public Control TransmittingScienceUI;
	[Export] public ProgressBar TransmittingScienceProgressBar; // Make sure this is linked to your ProgressBar node

	[ExportGroup("Transmission Settings")]
	[Export] public float MaxRange = 100000.0f;
	[Export] public float BaseTransmitDuration = 5.0f; 

	public Dictionary<Node, Godot.Collections.Array<string>> TransmittedData = new();

	public bool IsTransmitting { get; private set; } = false;
	public float TransmissionProgress { get; private set; } = 0.0f; 

	private float _currentDurationRequired = 0.0f;
	private float _elapsedTime = 0.0f;
	private Node3D _activeReceiver;

	public override void _Ready()
	{
		if (TransmittingScienceUI != null)
		{
			TransmittingScienceUI.Visible = false;
		}
	}

	public override void _Process(double delta)
	{
		float distance = GetDistanceToTarget(out _activeReceiver);

		if (!IsTransmitting) return;

		if (distance < 0 || distance > MaxRange)
		{
			CancelTransmission("Signal lost! Out of range.");
			return;
		}

		float normalizedDistance = Mathf.Clamp(distance / MaxRange, 0.0f, 1.0f);
		float speedMultiplier = Mathf.Lerp(1.0f, 0.1f, normalizedDistance);

		_elapsedTime += (float)delta * speedMultiplier;
		TransmissionProgress = Mathf.Clamp(_elapsedTime / _currentDurationRequired, 0.0f, 1.0f);

		// Update progress bar UI value (assuming ProgressBar min 0, max 100)
		if (TransmittingScienceProgressBar != null)
		{
			TransmittingScienceProgressBar.Value = TransmissionProgress * 100.0f;
		}

		if (TransmissionProgress >= 1.0f)
		{
			CompleteTransmission();
		}
	}

	public void Transmit()
	{
		if (IsTransmitting)
		{
			GD.Print("Transmission already in progress.");
			return;
		}

		if (InstrumentsManager == null || InstrumentsManager.ActivatedInstruments.Count == 0)
		{
			GD.Print("No science data available to transmit.");
			return;
		}

		float distance = GetDistanceToTarget(out _activeReceiver);

		if (distance < 0 || _activeReceiver == null)
		{
			GD.PrintErr("TransmitterNode is missing, and no CommunicateDish or fallback TargetReceiver was found!");
			return;
		}

		if (distance > MaxRange)
		{
			GD.Print($"Cannot transmit: Closest target ({_activeReceiver.Name}) is too far ({distance:F1}m / {MaxRange:F1}m max).");
			return;
		}

		// Read Index 1 (Megabytes) from each activated instrument's list
		float totalMegabytes = 0f;
		foreach (var kvp in InstrumentsManager.ActivatedInstruments)
		{
			var data = kvp.Value;
			if (data.Count > 1 && float.TryParse(data[1], out float mb))
			{
				totalMegabytes += mb;
			}
			else
			{
				GD.PrintErr($"Invalid or missing megabytes value at Index 1 for instrument.");
			}
		}

		if (totalMegabytes <= 0f) totalMegabytes = 10f; 

		// Start transmission and show progress UI
		IsTransmitting = true;
		_elapsedTime = 0.0f;
		TransmissionProgress = 0.0f;
		_currentDurationRequired = BaseTransmitDuration * (totalMegabytes / 10f); 

		if (TransmittingScienceUI != null)
		{
			TransmittingScienceUI.Visible = true;
		}

		GD.Print($"Starting transmission of {totalMegabytes}MB to '{_activeReceiver.Name}' at distance {distance:F1}m...");
	}

	private void CompleteTransmission()
	{
		IsTransmitting = false;
		TransmissionProgress = 1.0f;

		// Hide the transmitting progress UI when done
		if (TransmittingScienceUI != null)
		{
			TransmittingScienceUI.Visible = false;
		}

		foreach (var kvp in InstrumentsManager.ActivatedInstruments)
		{
			Node instrument = kvp.Key;
			Godot.Collections.Array<string> data = kvp.Value;

			if (!TransmittedData.ContainsKey(instrument))
			{
				TransmittedData.Add(instrument, data.Duplicate());
			}
			else
			{
				foreach (string entry in data)
				{
					if (!TransmittedData[instrument].Contains(entry))
					{
						TransmittedData[instrument].Add(entry);
					}
				}
			}
		}

		InstrumentsManager.ActivatedInstruments.Clear();
		GD.Print($"Transmission complete! Total instruments logged: {TransmittedData.Count}");
	}

	private void CancelTransmission(string reason)
	{
		IsTransmitting = false;
		_elapsedTime = 0.0f;
		TransmissionProgress = 0.0f;
		_activeReceiver = null;

		// Hide UI on cancel
		if (TransmittingScienceUI != null)
		{
			TransmittingScienceUI.Visible = false;
		}

		GD.Print($"Transmission aborted: {reason}");
	}

	private Node3D GetEffectiveReceiver()
	{
		if (TransmitterNode == null) return null;

		var dishes = GetTree().GetNodesInGroup("CommunicateDish");
		Node3D closestDish = null;
		float closestDistanceSq = float.MaxValue;

		foreach (Node node in dishes)
		{
			if (node is Node3D dish3D && dish3D.IsInsideTree())
			{
				float distSq = TransmitterNode.GlobalPosition.DistanceSquaredTo(dish3D.GlobalPosition);
				if (distSq < closestDistanceSq)
				{
					closestDistanceSq = distSq;
					closestDish = dish3D;
				}
			}
		}

		if (closestDish != null)
		{
			float actualDistance = Mathf.Sqrt(closestDistanceSq);
			GD.Print($"Closest CommunicateDish: '{closestDish.Name}' ({actualDistance:F1}m away)");
			return closestDish;
		}

		GD.Print($"No CommunicateDish nodes found. Closest fallback: '{TargetReceiver?.Name ?? "None"}'");
		return TargetReceiver;
	}

	private float GetDistanceToTarget(out Node3D resolvedTarget)
	{
		resolvedTarget = GetEffectiveReceiver();

		if (TransmitterNode == null || resolvedTarget == null)
		{
			return -1.0f;
		}

		return TransmitterNode.GlobalPosition.DistanceTo(resolvedTarget.GlobalPosition);
	}
}

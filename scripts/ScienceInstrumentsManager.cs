using Godot;
using System.Collections.Generic;

public partial class ScienceInstrumentsManager : Node
{
	[Export] public RigidBody3D CurrentRocket;
	[Export] public Control TransmitScienceUI;
	[Export] public Control CollectingScienceDataUI;
	[Export] public Button TransmitScienceButton;
	[Export] public Button DeleteScienceButton;
	[Export] public Control TransmittingScienceUI;
	[Export] public Control TransmittingScienceProgressBar;
	
	[Export] public TransmitManager TransmitManagerScript;
	
	public Dictionary<Node, Godot.Collections.Array<string>> ActivatedInstruments = new();
	
	private List<ScienceInstrument> _scienceInstruments = new();
	private Node _lastActivatedInstrument = null; 

	// Collection state tracking (5 seconds per instrument)
	public bool IsCollecting { get; private set; } = false;
	private float _collectionElapsedTime = 0.0f;
	private float _collectionDuration = 5.0f; 
	private ScienceInstrument _pendingInstrument = null;

	public override void _Ready()
	{
		// Ensure all UI panels start hidden
		if (TransmitScienceUI != null) TransmitScienceUI.Visible = false;
		if (TransmittingScienceUI != null) TransmittingScienceUI.Visible = false;
		if (CollectingScienceDataUI != null) CollectingScienceDataUI.Visible = false;

		if (CurrentRocket != null) 
		{
			_scienceInstruments = GetComponentsInChildren<ScienceInstrument>(CurrentRocket);
			if (_scienceInstruments.Count <= 0) 
			{
				GD.Print("No science instruments found on the rocket.");
				return;
			}
			
			GD.Print($"Successfully bound {_scienceInstruments.Count} science instruments to number keys 1-{_scienceInstruments.Count}.");
		}

		if (TransmitScienceButton != null) {
			TransmitScienceButton.Pressed += OnTransmitScienceButtonPressed;
		}
		
		if (DeleteScienceButton != null) {
			DeleteScienceButton.Pressed += OnDeleteScienceButtonPressed;
		}
	}

	public override void _Process(double delta)
	{
		// Handle the 5-second collection timer
		if (!IsCollecting) return;

		_collectionElapsedTime += (float)delta;

		if (_collectionElapsedTime >= _collectionDuration)
		{
			CompleteCollection();
		}
	}

	private void CompleteCollection()
	{
		IsCollecting = false;
		_collectionElapsedTime = 0.0f;

		// Hide collection UI when done
		if (CollectingScienceDataUI != null)
		{
			CollectingScienceDataUI.Visible = false;
		}

		if (_pendingInstrument != null)
		{
			Godot.Collections.Array<string> data = _pendingInstrument.ExportData;
			if (data != null && data.Count >= 2)
			{
				ActivatedInstruments[_pendingInstrument] = data;
				_lastActivatedInstrument = _pendingInstrument;

				// Show the Transmit / Delete panel UI now that collection is finished
				if (TransmitScienceUI != null)
				{
					TransmitScienceUI.Visible = true;
				}

				GD.Print($"Science data collected for: {_pendingInstrument.Name} | Type: {data[0]} | Size: {data[1]}MB");
			}
			_pendingInstrument = null;
		}
	}
	
	private void OnTransmitScienceButtonPressed() {
		if (TransmitManagerScript != null) {
			TransmitManagerScript.Transmit();
			
			if (TransmitScienceUI != null) {
				TransmitScienceUI.Visible = false;
			}
		}
	}
	
	private void OnDeleteScienceButtonPressed() {
		if (_lastActivatedInstrument != null && ActivatedInstruments.ContainsKey(_lastActivatedInstrument))
		{
			GD.Print($"Deleted science data from instrument: {_lastActivatedInstrument.Name}");
			ActivatedInstruments.Remove(_lastActivatedInstrument);
			_lastActivatedInstrument = null;
		}
		else if (ActivatedInstruments.Count > 0)
		{
			foreach (var key in ActivatedInstruments.Keys)
			{
				_lastActivatedInstrument = key;
				break;
			}
			if (_lastActivatedInstrument != null)
			{
				ActivatedInstruments.Remove(_lastActivatedInstrument);
				_lastActivatedInstrument = null;
			}
		}

		if (ActivatedInstruments.Count == 0 && TransmitScienceUI != null)
		{
			TransmitScienceUI.Visible = false;
		}
	}

	public override void _Input(InputEvent @event)
	{
		// Prevent triggering new instruments while already collecting or busy
		if (IsCollecting) return;

		if (@event is InputEventKey eventKey && eventKey.Pressed && !eventKey.Echo)
		{
			int index = -1;

			if (eventKey.Keycode == Key.Key1) index = 0;
			else if (eventKey.Keycode == Key.Key2) index = 1;
			else if (eventKey.Keycode == Key.Key3) index = 2;
			else if (eventKey.Keycode == Key.Key4) index = 3;
			else if (eventKey.Keycode == Key.Key5) index = 4;
			else if (eventKey.Keycode == Key.Key6) index = 5;
			else if (eventKey.Keycode == Key.Key7) index = 6;
			else if (eventKey.Keycode == Key.Key8) index = 7;
			else if (eventKey.Keycode == Key.Key9) index = 8;

			if (index >= 0 && index < _scienceInstruments.Count)
			{
				var instrument = _scienceInstruments[index];
				if (!ActivatedInstruments.ContainsKey(instrument))
				{
					Godot.Collections.Array<string> data = instrument.ExportData;
					
					if (data == null || data.Count < 2) 
					{
						GD.PrintErr($"Instrument '{instrument.Name}' is missing required ExportData entries (Index 0: Type, Index 1: Megabytes).");
						return;
					}

					// Start the 5-second collection process
					_pendingInstrument = instrument;
					IsCollecting = true;
					_collectionElapsedTime = 0.0f;

					if (CollectingScienceDataUI != null)
					{
						CollectingScienceDataUI.Visible = true;
					}
					
					GD.Print($"Collecting science data for #{index + 1}: {instrument.Name} (5 seconds)...");
				}
			}
		}
	}

	private List<T> GetComponentsInChildren<T>(Node root) where T : Node
	{
		List<T> results = new List<T>();
		if (root is T match)
		{
			results.Add(match);
		}
		foreach (Node child in root.GetChildren(includeInternal: true))
		{
			results.AddRange(GetComponentsInChildren<T>(child));
		}
		return results;
	}
}

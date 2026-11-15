using Godot;
using System.Collections.Generic;

public partial class TutorialOne : Node
{
	[ExportGroup("UI References")]
	[Export] public Label InstructionLabel;
	[Export] public Button OkButton;
	[Export] public AudioStreamPlayer ChimePlayer;

	[ExportGroup("Typewriter Settings")]
	[Export] public float CharactersPerSecond = 45f; // Slightly snappier for flight instructions

	// Define a structure for each tutorial step
	public class TutorialStep
	{
		public string Text;
		public bool PauseGame;
		public List<Key> RequiredKeys = new List<Key>(); // Empty list if it only requires clicking OK
	}

	private List<TutorialStep> _steps = new List<TutorialStep>();
	private int _currentStepIndex = 0;
	private bool _isWaitingForKey = false;

	// Typewriter state variables
	private bool _isTyping = false;
	private string _currentFullText = "";
	private float _typewriterTimer = 0f;
	private float _charInterval = 0.05f;
	private int _displayedCharCount = 0;

	public override void _Ready()
	{
		// Ensure this node still processes inputs/code while paused
		ProcessMode = ProcessModeEnum.Always;

		if (OkButton != null)
		{
			OkButton.ProcessMode = Node.ProcessModeEnum.Always;
			OkButton.Pressed += OnOkButtonPressed;
		}

		_charInterval = 1f / CharactersPerSecond;

		// --- KSP-STYLE FLIGHT TRAINING STEPS ---

		// 1. Intro Briefing (Paused)
		_steps.Add(new TutorialStep
		{
			Text = "Welcome to Flight Training, Cadet!\nBefore lighting any candles, let's verify your cockpit controls.\nClick OK to begin pre-launch checks.",
			PauseGame = true
		});

		// 2. Throttle Fine Control: Left Shift & Left Control
		_steps.Add(new TutorialStep
		{
			Text = "THROTTLE CONTROL:\nHold [Left Shift] to gradually increase thrust, or [Left Ctrl] to throttle down.\nTap [Left Shift] now to test your throttle lever.",
			PauseGame = false,
			RequiredKeys = new List<Key> { Key.Shift }
		});

		_steps.Add(new TutorialStep
		{
			Text = "Good. Now tap [Left Ctrl] to reduce throttle.",
			PauseGame = false,
			RequiredKeys = new List<Key> { Key.Ctrl }
		});

		// 3. Max Throttle & Cut Engine: Z & X
		_steps.Add(new TutorialStep
		{
			Text = "INSTANT THROTTLE:\nIn an emergency, [Z] sets engines to 100% Max Thrust, and [X] cuts throttle to 0%.\nPress [Z] to test full throttle!",
			PauseGame = false,
			RequiredKeys = new List<Key> { Key.Z }
		});

		_steps.Add(new TutorialStep
		{
			Text = "Whoa, easy there! Press [X] immediately to cut the engines before we go anywhere.",
			PauseGame = false,
			RequiredKeys = new List<Key> { Key.X }
		});

		// 4. Pitch & Yaw: W, A, S, D
		_steps.Add(new TutorialStep
		{
			Text = "ATTITUDE CONTROL (PITCH & YAW):\nUse [W] and [S] to pitch the nose down and up.\nUse [A] and [D] to yaw left and right.\nPress any [W/A/S/D] key to test your gimbal and reaction wheels.",
			PauseGame = false,
			RequiredKeys = new List<Key> { Key.W, Key.A, Key.S, Key.D }
		});

		// 5. Roll: Q & E
		_steps.Add(new TutorialStep
		{
			Text = "ROLL CONTROL:\nUse [Q] to roll counter-clockwise and [E] to roll clockwise.\nPress [Q] or [E] to check roll authority.",
			PauseGame = false,
			RequiredKeys = new List<Key> { Key.Q, Key.E }
		});

		// 6. Staging Warning (Paused)
		_steps.Add(new TutorialStep
		{
			Text = "STAGING:\nThe [Spacebar] activates the next stage—igniting engines or firing decouplers.\nCheck your staging sequence carefully so you don't detach your capsule on the pad!\nClick OK when you're ready for liftoff.",
			PauseGame = true
		});

		// 7. Launch! (Spacebar)
		_steps.Add(new TutorialStep
		{
			Text = "All systems nominal. Throttle up with [Z] if needed, then press [Spacebar] to ignite Stage 1 and launch!",
			PauseGame = false,
			RequiredKeys = new List<Key> { Key.Space }
		});

		// 8. Completion (Paused)
		_steps.Add(new TutorialStep
		{
			Text = "Liftoff confirmed! You're flying!\nKeep an eye on your fuel and fly safe, Cadet.\nClick OK to close Flight Training.",
			PauseGame = true
		});

		// Load the first step
		LoadStep(_currentStepIndex);
	}

	public override void _Process(double delta)
	{
		if (_isTyping && InstructionLabel != null)
		{
			_typewriterTimer += (float)delta;
			if (_typewriterTimer >= _charInterval)
			{
				_typewriterTimer = 0f;
				_displayedCharCount++;
				InstructionLabel.VisibleCharacters = _displayedCharCount;

				if (_displayedCharCount >= _currentFullText.Length)
				{
					_isTyping = false;
				}
			}
		}
	}

	private void LoadStep(int index)
	{
		if (index >= _steps.Count)
		{
			if (InstructionLabel != null) InstructionLabel.Text = "";
			GetTree().Paused = false;
			QueueFree();
			return;
		}

		var step = _steps[index];

		_currentFullText = step.Text;
		if (InstructionLabel != null)
		{
			InstructionLabel.Text = _currentFullText;
			InstructionLabel.VisibleCharacters = 0;
		}

		_displayedCharCount = 0;
		_typewriterTimer = 0f;
		_isTyping = true;

		if (step.PauseGame)
		{
			GetTree().Paused = true;
			if (OkButton != null) OkButton.Visible = true;
			_isWaitingForKey = false;
		}
		else
		{
			GetTree().Paused = false;
			if (OkButton != null) OkButton.Visible = false;
			_isWaitingForKey = true;
		}
	}

	private void OnOkButtonPressed()
	{
		if (_isTyping)
		{
			CompleteTypingInstantly();
			return;
		}

		PlayChime();
		_currentStepIndex++;
		LoadStep(_currentStepIndex);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (_isWaitingForKey)
		{
			var step = _steps[_currentStepIndex];

			if (step.RequiredKeys.Count > 0 && @event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
			{
				if (step.RequiredKeys.Contains(keyEvent.Keycode))
				{
					// Note: Removed the early return on _isTyping here so pressing Space/Z
					// actually triggers the launch or throttle action in-game at the same time!
					CompleteTypingInstantly();
					PlayChime();
					_isWaitingForKey = false;
					_currentStepIndex++;
					LoadStep(_currentStepIndex);
				}
			}
		}
		else if (_isTyping && @event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
		{
			CompleteTypingInstantly();
		}
	}

	private void CompleteTypingInstantly()
	{
		_isTyping = false;
		if (InstructionLabel != null)
		{
			InstructionLabel.VisibleCharacters = -1;
		}
	}

	private void PlayChime()
	{
		if (ChimePlayer != null)
		{
			ChimePlayer.Play();
		}
	}
}

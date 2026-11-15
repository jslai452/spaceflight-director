using Godot;

public partial class TimeWarpManager : Node3D
{
	[ExportGroup("Time Warp State")]
	[Export] public int CurrentWarpIndex { get; set; } = 0;
	[Export] public float[] WarpRates { get; set; } = new float[] { 1f, 5f, 10f, 50f, 100f, 1000f };

	public bool IsTimeWarping => CurrentWarpIndex > 0;
	public float CurrentWarpRate => WarpRates[CurrentWarpIndex];

	[ExportCategory("References")]
	[Export] public RigidBody3D CraftRigidbody { get; set; }
	[Export] public CelestialBodyGravity GravityScript { get; set; }

	[ExportGroup("UI Controls")]
	[Export] public Button LeftWarpButton { get; set; }
	[Export] public Button RightWarpButton { get; set; }
	[Export] public Label WarpRateLabel { get; set; }

	// Kepler Orbital Elements
	public float Mu { get; private set; }
	public float A { get; private set; }
	public float E { get; private set; }
	public Vector3 P { get; private set; }
	public Vector3 Q { get; private set; }
	private float _meanAnomalyAtEpoch;
	private float _epochTime;
	private float _meanMotion;
	private Vector3 _lastValidVelocity; // Caches velocity to prevent zeroing out on error

	public override void _Ready()
	{
		if (CraftRigidbody == null)
		{
			CraftRigidbody = GetNodeOrNull<RigidBody3D>("../../") ?? GetNodeOrNull<RigidBody3D>("../");
		}

		if (GravityScript == null && CraftRigidbody != null)
		{
			GravityScript = CraftRigidbody as CelestialBodyGravity;
			if (GravityScript == null)
			{
				GravityScript = CraftRigidbody.GetNodeOrNull<CelestialBodyGravity>("CelestialBodyGravity");
			}
		}

		// Connect UI Buttons
		if (LeftWarpButton != null)
		{
			LeftWarpButton.Pressed += OnLeftWarpPressed;
		}
		if (RightWarpButton != null)
		{
			RightWarpButton.Pressed += OnRightWarpPressed;
		}

		UpdateWarpUI();

		GD.Print($"[TimeWarpManager] Initialized. Rigidbody found: {CraftRigidbody != null}, GravityScript found: {GravityScript != null}");
	}

	public override void _ExitTree()
	{
		if (LeftWarpButton != null)
		{
			LeftWarpButton.Pressed -= OnLeftWarpPressed;
		}
		if (RightWarpButton != null)
		{
			RightWarpButton.Pressed -= OnRightWarpPressed;
		}
	}

	private void OnLeftWarpPressed()
	{
		SetTimeWarp(CurrentWarpIndex - 1);
	}

	private void OnRightWarpPressed()
	{
		SetTimeWarp(CurrentWarpIndex + 1);
	}

	private void UpdateWarpUI()
	{
		if (WarpRateLabel != null)
		{
			WarpRateLabel.Text = $"{CurrentWarpRate}x";
		}

		// Optional: Disable buttons when at minimum or maximum warp
		if (LeftWarpButton != null)
		{
			LeftWarpButton.Disabled = CurrentWarpIndex <= 0;
		}
		if (RightWarpButton != null)
		{
			RightWarpButton.Disabled = CurrentWarpIndex >= WarpRates.Length - 1;
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
		{
			if (keyEvent.Keycode == Key.Period)
			{
				SetTimeWarp(CurrentWarpIndex + 1);
			}
			else if (keyEvent.Keycode == Key.Comma)
			{
				SetTimeWarp(CurrentWarpIndex - 1);
			}
		}
	}

	public override void _Process(double delta)
	{
		float currentTime = (float)(Time.GetTicksMsec() * 0.001f);

		if (IsTimeWarping && GravityScript != null && GravityScript.PlanetCenter != null)
		{
			UpdateOnRailsPosition(currentTime);
		}
	}

	public void SetTimeWarp(int newIndex)
	{
		int clampedIndex = Mathf.Clamp(newIndex, 0, WarpRates.Length - 1);
		float currentTime = (float)(Time.GetTicksMsec() * 0.001f);

		if (clampedIndex == CurrentWarpIndex) return;

		GD.Print($"[TimeWarpManager] Changing warp index from {CurrentWarpIndex} to {clampedIndex} (Rate: {WarpRates[clampedIndex]}x)");

		if (!IsTimeWarping && clampedIndex > 0)
		{
			// Set index first so IsTimeWarping becomes true during validation checks
			CurrentWarpIndex = clampedIndex;
			EnterTimeWarp();

			if (IsTimeWarping)
			{
				GD.Print($"[TimeWarpManager] >>> TIME WARP ACTIVE: {CurrentWarpRate}x <<<");
			}
		}
		else if (IsTimeWarping && clampedIndex == 0)
		{
			ExitTimeWarp();
			CurrentWarpIndex = 0;
			GD.Print("[TimeWarpManager] >>> RETURNED TO 1X PHYSICS <<<");
		}
		else if (IsTimeWarping && clampedIndex > 0)
		{
			// Prevent changing warp rate if somehow inside atmosphere
			if (GravityScript != null && !GravityScript.IsOutsideAtmosphere(CraftRigidbody.GlobalPosition))
			{
				GD.PrintErr("[TimeWarpManager] Cannot change warp: Craft has entered the atmosphere!");
				SetTimeWarp(0);
				return;
			}

			float elapsedTime = currentTime - _epochTime;
			_meanAnomalyAtEpoch += _meanMotion * elapsedTime * CurrentWarpRate;
			_epochTime = currentTime;
			CurrentWarpIndex = clampedIndex;
			GD.Print($"[TimeWarpManager] >>> WARP RATE CHANGED TO: {CurrentWarpRate}x <<<");
		}

		UpdateWarpUI();
	}

	private void EnterTimeWarp()
	{
		if (CraftRigidbody != null && CraftRigidbody.Freeze && CurrentWarpIndex == 0)
		{
			CraftRigidbody.Freeze = false;
		}

		if (GravityScript == null || GravityScript.PlanetCenter == null || CraftRigidbody == null)
		{
			GD.PrintErr("[TimeWarpManager] EnterTimeWarp failed: Missing references.");
			CurrentWarpIndex = 0;
			if (CraftRigidbody != null) CraftRigidbody.Freeze = false;
			return;
		}

		// --- NEW: Block time warp if craft is inside the atmosphere ---
		if (!GravityScript.IsOutsideAtmosphere(CraftRigidbody.GlobalPosition))
		{
			GD.PrintErr("[TimeWarpManager] Cannot enter time warp: Craft is inside the atmosphere.");
			CurrentWarpIndex = 0;
			if (CraftRigidbody != null) CraftRigidbody.Freeze = false;
			return;
		}
		// -------------------------------------------------------------

		Mu = GravityScript.Mu;
		Vector3 r = CraftRigidbody.GlobalPosition - GravityScript.PlanetCenter.GlobalPosition;

		Vector3 planetVel = Vector3.Zero;
		if (GravityScript.PlanetCenter is RigidBody3D planetRb)
		{
			planetVel = planetRb.LinearVelocity;
		}
		Vector3 v = CraftRigidbody.LinearVelocity - planetVel;

		float rMag = r.Length();
		float vMag = v.Length();

		GD.Print($"[TimeWarpManager] Evaluating orbit -> Radius: {rMag:F2}m, Velocity: {vMag:F2}m/s, Mu: {Mu}");

		if (rMag < 0.001f || vMag < 0.5f || Mu <= 0f || !r.IsFinite() || !v.IsFinite())
		{
			GD.PrintErr($"[TimeWarpManager] Cannot warp: Craft is stationary on the ground or invalid (vMag: {vMag:F2}m/s). Launch first!");
			CurrentWarpIndex = 0;
			CraftRigidbody.Freeze = false;
			return;
		}

		float energy = (vMag * vMag / 2f) - (Mu / rMag);

		if (energy >= -0.0001f || float.IsNaN(energy) || float.IsInfinity(energy))
		{
			GD.PrintErr("[TimeWarpManager] Cannot warp: Orbit is parabolic or hyperbolic (Escape trajectory).");
			CurrentWarpIndex = 0;
			CraftRigidbody.Freeze = false;
			return;
		}

		A = -Mu / (2f * energy);
		Vector3 h = r.Cross(v);

		if (h.LengthSquared() < 0.0001f || !h.IsFinite())
		{
			GD.PrintErr("[TimeWarpManager] Cannot warp: Angular momentum is near zero or non-finite.");
			CurrentWarpIndex = 0;
			CraftRigidbody.Freeze = false;
			return;
		}

		Vector3 eVec = v.Cross(h) / Mu - r / rMag;
		E = eVec.Length();

		if (float.IsNaN(E) || float.IsInfinity(E) || E >= 0.999f || !eVec.IsFinite())
		{
			GD.PrintErr($"[TimeWarpManager] Cannot warp: Invalid orbital eccentricity ({E}).");
			CurrentWarpIndex = 0;
			CraftRigidbody.Freeze = false;
			return;
		}

		P = eVec.Normalized();
		Vector3 N = h.Normalized();
		Q = N.Cross(P);

		if (!P.IsFinite() || !Q.IsFinite())
		{
			GD.PrintErr("[TimeWarpManager] Cannot warp: Perifocal coordinate vectors are non-finite.");
			CurrentWarpIndex = 0;
			CraftRigidbody.Freeze = false;
			return;
		}

		float cosE = Mathf.Clamp((A - rMag) / (A * E), -1f, 1f);
		float sinE = r.Dot(Q) / (A * Mathf.Sqrt(Mathf.Max(0.0001f, 1f - E * E)));
		float eccentricAnom = Mathf.Atan2(sinE, cosE);

		_meanAnomalyAtEpoch = eccentricAnom - E * Mathf.Sin(eccentricAnom);
		_epochTime = (float)(Time.GetTicksMsec() * 0.001f);
		_meanMotion = Mathf.Sqrt(Mu / (A * A * A));

		if (float.IsNaN(_meanMotion) || float.IsInfinity(_meanMotion))
		{
			GD.PrintErr("[TimeWarpManager] Cannot warp: Mean motion calculation failed.");
			CurrentWarpIndex = 0;
			CraftRigidbody.Freeze = false;
			return;
		}

		_lastValidVelocity = v; 
		CraftRigidbody.Freeze = true;
		CraftRigidbody.FreezeMode = RigidBody3D.FreezeModeEnum.Kinematic;
		GD.Print("[TimeWarpManager] Physics frozen. Successfully entered Time Warp.");
	}

	private void UpdateOnRailsPosition(float currentTime)
	{
		if (A <= 0f || float.IsNaN(A) || float.IsNaN(E))
		{
			SetTimeWarp(0);
			return;
		}

		if (GravityScript == null || GravityScript.PlanetCenter == null) return;

		float simulatedTimeDelta = (currentTime - _epochTime) * CurrentWarpRate;
		float M = _meanAnomalyAtEpoch + _meanMotion * simulatedTimeDelta;

		float eccentricAnom = SolveKepler(M, E);
		float b = A * Mathf.Sqrt(Mathf.Max(0f, 1f - E * E));
		Vector3 realLocalPoint = A * (Mathf.Cos(eccentricAnom) - E) * P + b * Mathf.Sin(eccentricAnom) * Q;

		Vector3 targetPos = GravityScript.PlanetCenter.GlobalPosition + realLocalPoint;

		// --- NEW: Automatically drop out of warp if trajectory enters the atmosphere ---
		if (!GravityScript.IsOutsideAtmosphere(targetPos))
		{
			GD.Print("[TimeWarpManager] Atmospheric entry detected! Dropping out of time warp to 1x physics.");
			SetTimeWarp(0);
			return;
		}
		// -----------------------------------------------------------------------------

		if (targetPos.IsFinite())
		{
			GlobalPosition = targetPos;
			if (CraftRigidbody != null)
			{
				CraftRigidbody.GlobalPosition = targetPos;
			}

			// Continuously cache valid analytical velocity
			Vector3 currentVel = GetCurrentOnRailsVelocity();
			if (currentVel.IsFinite() && currentVel.LengthSquared() > 0.0001f)
			{
				_lastValidVelocity = currentVel;
			}
		}
		else
		{
			GD.PrintErr("[TimeWarpManager] Aborting warp: Calculated position was non-finite.");
			SetTimeWarp(0);
		}
	}

	private void ExitTimeWarp()
	{
		if (CraftRigidbody == null) return;

		Vector3 currentVel = GetCurrentOnRailsVelocity();

		// Fallback to last cached valid velocity if analytical calculation dips
		if (!currentVel.IsFinite() || currentVel.LengthSquared() < 0.0001f)
		{
			currentVel = _lastValidVelocity;
		}

		Vector3 planetVel = Vector3.Zero;
		if (GravityScript != null && GravityScript.PlanetCenter is RigidBody3D planetRb)
		{
			planetVel = planetRb.LinearVelocity;
		}
		Vector3 absoluteVel = currentVel + planetVel;

		GD.Print($"[TimeWarpManager] Exiting time warp. Restoring physics with velocity: {absoluteVel}");

		CraftRigidbody.Freeze = false;
		CraftRigidbody.LinearVelocity = absoluteVel.IsFinite() ? absoluteVel : planetVel;
	}

	public Vector3 GetCurrentOnRailsVelocity()
	{
		if (A <= 0f || Mu <= 0f || float.IsNaN(E)) return Vector3.Zero;

		float currentTime = (float)(Time.GetTicksMsec() * 0.001f);
		float simulatedTimeDelta = (currentTime - _epochTime) * CurrentWarpRate;
		float M = _meanAnomalyAtEpoch + _meanMotion * simulatedTimeDelta;
		float eccentricAnom = SolveKepler(M, E);

		float rMag = A * (1f - E * Mathf.Cos(eccentricAnom));
		if (rMag <= 0.0001f) return Vector3.Zero;

		float factor = Mathf.Sqrt(Mu * A) / rMag;

		Vector3 vLocal = new Vector3(-Mathf.Sin(eccentricAnom), Mathf.Sqrt(Mathf.Max(0f, 1f - E * E)) * Mathf.Cos(eccentricAnom), 0f) * factor;
		Vector3 vel = (P * vLocal.X + Q * vLocal.Y);
		return vel.IsFinite() ? vel : Vector3.Zero;
	}

	private float SolveKepler(float M, float ecc)
	{
		float eccentricAnom = M;
		for (int i = 0; i < 10; i++)
		{
			float f = eccentricAnom - ecc * Mathf.Sin(eccentricAnom) - M;
			float fPrime = 1f - ecc * Mathf.Cos(eccentricAnom);
			if (Mathf.Abs(fPrime) < 0.00001f) break;
			eccentricAnom -= f / fPrime;
		}
		return eccentricAnom;
	}
}

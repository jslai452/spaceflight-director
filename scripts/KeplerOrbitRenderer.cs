using Godot;

public partial class KeplerOrbitRenderer : MeshInstance3D
{
	[ExportCategory("Craft & Gravity References")]
	[Export] public RigidBody3D CraftRigidbody { get; set; }
	[Export] public CelestialBodyGravity RocketGravity { get; set; }
	[Export] public Node3D MiniPlanetTransform { get; set; }
	[Export] public MeshInstance3D PeriapsisMarker {get; set;}
	[Export] public MeshInstance3D ApoapsisMarker {get; set;} // Will hide on hyperbolic trajectories
	[Export] public MeshInstance3D CraftMarker { get; set; }
	[Export] public TimeWarpManager WarpManager { get; set; } 
	[Export] public FlightHUD flightHud;

	[ExportCategory("Map View Scaling")]
	[Export] public float MapScaleFactor { get; set; } = 10000f;

	[ExportCategory("Line Settings")]
	[Export] public int PositionCount { get; set; } = 120;
	[Export] public float LineWidth { get; set; } = 0.2f;

	private ImmediateMesh _immediateMesh;
	private StandardMaterial3D _lineMaterial;
	private string _lastDebugReason = "";
	private bool _loggedVerticesThisPath = false;

	public override void _Ready()
	{
		_immediateMesh = new ImmediateMesh();
		Mesh = _immediateMesh;

		_lineMaterial = new StandardMaterial3D();
		_lineMaterial.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
		_lineMaterial.AlbedoColor = Colors.White;
		MaterialOverride = _lineMaterial;
	}

	public override void _Process(double delta)
	{
		if (CraftRigidbody == null || RocketGravity == null || MiniPlanetTransform == null)
		{
			LogReason("[KeplerOrbitRenderer] Couldn't make a path because missing references.");
			Visible = false;
			_loggedVerticesThisPath = false;
			return;
		}

		UpdateOrbitPath();
	}

	private void LogReason(string reason)
	{
		if (_lastDebugReason != reason)
		{
			GD.Print(reason);
			_lastDebugReason = reason;
			_loggedVerticesThisPath = false;
		}
	}

	private void UpdateOrbitPath()
	{
		float mu = RocketGravity.Mu;
		Node3D realPlanet = RocketGravity.PlanetCenter;

		if (mu <= 0f || realPlanet == null)
		{
			HideMarkers();
			LogReason($"[KeplerOrbitRenderer] Couldn't make a path because Mu ({mu}) is <= 0 or realPlanet is null.");
			Visible = false;
			return;
		}
		
		// --- 1. Update Craft Position Marker on Map View Safely ---
		if (CraftMarker != null)
		{
			Vector3 realOffset = CraftRigidbody.GlobalPosition - realPlanet.GlobalPosition;
			Vector3 scaledOffset = realOffset / MapScaleFactor;
			Vector3 craftMapPos = MiniPlanetTransform.GlobalPosition + scaledOffset;

			if (craftMapPos.IsFinite())
			{
				CraftMarker.Visible = true;
				CraftMarker.GlobalPosition = craftMapPos;
			}
			else
			{
				CraftMarker.Visible = false;
			}
		}

		Vector3 r = CraftRigidbody.GlobalPosition - realPlanet.GlobalPosition;

		Vector3 planetVel = Vector3.Zero;
		if (realPlanet is RigidBody3D planetRb)
		{
			planetVel = planetRb.LinearVelocity;
		}

		// --- 2. Fetch velocity correctly using the exported WarpManager reference ---
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

		float vMag = v.Length();
		float rMag = r.Length();

		if (!isWarping && (vMag < 0.05f || rMag < 0.001f || !v.IsFinite() || !r.IsFinite()))
		{
			HideMarkers();
			LogReason($"[KeplerOrbitRenderer] Velocity or radius magnitude too small or non-finite.");
			Visible = false;
			return;
		}

		Vector3 h = r.Cross(v);
		if (h.LengthSquared() < 0.0001f)
		{
			HideMarkers();
			LogReason($"[KeplerOrbitRenderer] Angular momentum squared too small.");
			Visible = false;
			return;
		}

		Vector3 eVec = v.Cross(h) / mu - r / rMag;
		float e = eVec.Length();

		if (float.IsNaN(e) || float.IsInfinity(e) || !eVec.IsFinite())
		{
			HideMarkers();
			LogReason($"[KeplerOrbitRenderer] Non-finite eccentricity vector.");
			Visible = false;
			return;
		}

		float energy = (vMag * vMag / 2f) - (mu / rMag);
		if (float.IsNaN(energy) || float.IsInfinity(energy))
		{
			HideMarkers();
			LogReason($"[KeplerOrbitRenderer] Specific orbital energy invalid.");
			Visible = false;
			return;
		}

		float a = -mu / (2f * energy);
		if (float.IsNaN(a) || float.IsInfinity(a))
		{
			Visible = false;
			return;
		}

		Vector3 P = eVec.Normalized();
		Vector3 N = h.Normalized();
		Vector3 Q = N.Cross(P);
		
		bool isHyperbolic = e >= 1.0f;

		// --- Periapsis & Apoapsis Calculations ---
		float planetRadius = RocketGravity != null ? RocketGravity.PlanetRadius : 0f;

		if (!isHyperbolic)
		{
			float periapsisRadius = a * (1f - e);
			float apoapsisRadius = a * (1f + e);
			
			if (flightHud != null)
			{
				flightHud.UpdatePeriapsis(periapsisRadius - planetRadius);
				flightHud.UpdateApoapsis(apoapsisRadius - planetRadius);
			}

			// Periapsis Marker
			if (PeriapsisMarker != null)
			{
				Vector3 periapsisLocal = periapsisRadius * P;
				Vector3 periapsisWorld = MiniPlanetTransform.GlobalPosition + (periapsisLocal / MapScaleFactor);
				PeriapsisMarker.Visible = periapsisWorld.IsFinite();
				if (PeriapsisMarker.Visible) PeriapsisMarker.GlobalPosition = periapsisWorld;
			}

			// Apoapsis Marker
			if (ApoapsisMarker != null)
			{
				Vector3 apoapsisLocal = -apoapsisRadius * P; // Note: For standard ellipses, apoapsis is opposite to P vector direction
				Vector3 apoapsisWorld = MiniPlanetTransform.GlobalPosition + (apoapsisLocal / MapScaleFactor);
				ApoapsisMarker.Visible = apoapsisWorld.IsFinite();
				if (ApoapsisMarker.Visible) ApoapsisMarker.GlobalPosition = apoapsisWorld;
			}
		}
		else
		{
			// Hyperbolic orbit: Periapsis still exists, but no Apoapsis
			float periapsisRadius = Mathf.Abs(a) * (e - 1f);
			
			if (flightHud != null)
			{
				flightHud.UpdatePeriapsis(periapsisRadius - planetRadius);
				flightHud.UpdateApoapsis(float.NaN); // No apoapsis on escape trajectories
			}

			if (PeriapsisMarker != null)
			{
				Vector3 periapsisLocal = periapsisRadius * P;
				Vector3 periapsisWorld = MiniPlanetTransform.GlobalPosition + (periapsisLocal / MapScaleFactor);
				PeriapsisMarker.Visible = periapsisWorld.IsFinite();
				if (PeriapsisMarker.Visible) PeriapsisMarker.GlobalPosition = periapsisWorld;
			}

			if (ApoapsisMarker != null) ApoapsisMarker.Visible = false;
		}

		// === GENERATE PATH (Handles both Elliptic and Hyperbolic curves) ===
		Visible = true;
		_immediateMesh.ClearSurfaces();
		_immediateMesh.SurfaceBegin(Mesh.PrimitiveType.LineStrip);

		if (!isHyperbolic)
		{
			// --- ELLIPTIC ORBIT PATH ---
			float b = a * Mathf.Sqrt(Mathf.Max(0f, 1f - e * e));
			float step = (Mathf.Pi * 2f) / PositionCount;

			for (int i = 0; i < PositionCount; i++)
			{
				float E = i * step;
				Vector3 realLocalPoint = a * (Mathf.Cos(E) - e) * P + b * Mathf.Sin(E) * Q;
				AddValidVertex(realLocalPoint);
			}
		}
		else
		{
			// --- HYPERBOLIC/ESCAPE ORBIT PATH ---
			// For a hyperbola, true anomaly (theta) ranges within (-acos(1/e), acos(1/e)) roughly. 
			// Instead, parameterizing via hyperbolic anomaly H from -H_max to +H_max works cleanly:
			float hMax = 2.0f; // Controls how far out the plotted escape line reaches
			float step = (2.0f * hMax) / PositionCount;
			float b = Mathf.Abs(a) * Mathf.Sqrt(e * e - 1f);

			for (int i = 0; i < PositionCount; i++)
			{
				float H = -hMax + (i * step);
				// Hyperbolic position parametric equations:
				Vector3 realLocalPoint = Mathf.Abs(a) * (e - Mathf.Cosh(H)) * P + b * Mathf.Sinh(H) * Q;
				AddValidVertex(realLocalPoint);
			}
		}

		_immediateMesh.SurfaceEnd();
		LogReason("[KeplerOrbitRenderer] Path calculated successfully.");
		_loggedVerticesThisPath = true;
	}

	private void AddValidVertex(Vector3 realLocalPoint)
	{
		Vector3 scaledLocalPoint = realLocalPoint / MapScaleFactor;
		Vector3 worldPoint = MiniPlanetTransform.GlobalPosition + scaledLocalPoint;
		
		if (worldPoint.IsFinite())
		{
			Vector3 localPoint = GlobalTransform.AffineInverse() * worldPoint;
			if (localPoint.IsFinite())
			{
				_immediateMesh.SurfaceAddVertex(localPoint);
			}
		}
	}

	private void HideMarkers()
	{
		if (PeriapsisMarker != null) PeriapsisMarker.Visible = false;
		if (ApoapsisMarker != null) ApoapsisMarker.Visible = false;
		if (CraftMarker != null) CraftMarker.Visible = false;
	}
}

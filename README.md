Spaceflight Director (v0.2.0.0)



<p align="left">
    <img src="https://img.shields.io/badge/License-Apache%202.0-blue.svg" alt="License: Apache 2.0">

  <img src="https://img.shields.io/badge/Engine-Godot_4.x_--_.NET-blue?logo=godotengine" alt="Godot Engine">

  <img src="https://img.shields.io/badge/version-v0.2.0.0-orange" alt="Version">

  <img src="https://img.shields.io/badge/NASA-Space%20Apps%20Challenge-blueviolet" alt="Space Apps Challenge">

</p>



An immersive, physics-driven orbital mechanics, space science, and flight simulation built in Godot Engine for the **NASA International Space Apps Challenge**.



Take command of the space program: inspect your rocket during assembly, manage electric charge and solar arrays, execute precise orbital maneuvers, survive hypersonic atmospheric re-entry, and conduct real-world scientific magnetosphere experiments around the Earth and Moon.



---



## 🌟 Key Features



### 🌌 Authentic Physics & Orbital Mechanics

**N-Body Gravity & Keplerian Orbits:** Accurate orbital state propagation, patched conics, and a custom real-time trajectory line renderer.
**Tsiolkovsky Rocket Equation:** Grounded in fundamental aerospace mathematics for mass, fuel consumption, and delta-v constraints.
**Custom Time-Warp Engine:** Safely accelerate time up to thousands of times speed to skip long orbital transit phases without breaking physics calculations.


### ☀️ Resource & Electric Charge Management

**Solar Panels & Alignment Math:** Solar panels calculate power generation dynamically based on their angle relative to the Sun using **dot product vectors** ($A \cdot B$).
**Power Drain & Systems:** SAS autopilot and onboard avionics consume electric charge. Facing away from the Sun or flying through planetary shadows drains batteries.


## 🎮 Gameplay & Mission Loop



**Ascent & Insertion:** Launch using custom SAS autopilot modes (**Prograde**, **Retrograde**) to reach stable orbit.
2. **Orbital Operations:** Deploy solar arrays, monitor electric charge, and follow real-time trajectory lines.

4. **Scientific Observation:** Collect telemetry data with the science instruments



### 🕹️ Flight Controls



| Control | Action |

| :--- | :--- |

| **WASD** | Pitch and Yaw rotational control |

| **Q / E** | Roll Left / Roll Right |

| **Spacebar** | Stage Separation |

| **UI Dashboard** | Toggle SAS Autopilot Modes (Prograde / Retrograde) |

| **Time Warp Keys** | Accelerate / Decelerate Simulation Speed |



---



## 🛠️ Tech Stack & Architecture



**Engine:** [Godot Engine 4.x (.NET Edition)](https://godotengine.org/)
**Programming Language:** C# (.NET 8.0)
**Physics Architecture:** Procedural numerical physics integration, vector dot product mechanics, and inverse-cube gravitational/magnetic solvers (100% self-contained for offline performance).
**Target Platform:** Windows Executable (`.exe`) 

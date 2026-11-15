

Spaceflight Director (v0.2.0.0)



<p align="left">
    <img src="https://img.shields.io/badge/License-Apache%202.0-blue.svg" alt="License: Apache 2.0">

  <img src="https://img.shields.io/badge/Engine-Godot_4.x_--_.NET-blue?logo=godotengine" alt="Godot Engine">

  <img src="https://img.shields.io/badge/version-v0.2.0.0-orange" alt="Version">

  <img src="https://img.shields.io/badge/NASA-Space%20Apps%20Challenge-blueviolet" alt="Space Apps Challenge">

</p>



An immersive, physics-driven orbital mechanics, space science, and flight simulation built in Godot Engine for the **NASA Space Apps Challenge 2026**.


Click here to download the pitch video:
https://github.com/jslai452/spaceflight-director/blob/main/SpaceflightPitchVideo.mp4




Take command of the space program: manage electric charge, execute precise orbital maneuvers, and conduct real-world experiments around the Earth.

## 🌟 Key Features



### 🌌 Authentic Physics & Orbital Mechanics

**N-Body Gravity & Keplerian Orbits:** Accurate orbital state propagation, patched conics, and a custom real-time trajectory line renderer.
**Tsiolkovsky Rocket Equation:** Grounded in fundamental aerospace mathematics for ma

s, fuel consumption, and delta-v constraints.
**Custom Time-Warp Engine:** Safely accelerate time up to thousands of times speed to skip long orbital transit phases without breaking physics calculations.


### ☀️ Resource & Electric Charge Management

**Power Drain & Systems:** SAS autopilot and onboard avionics consume electric charge.

## 🎮 Gameplay & Mission Loop



**Ascent & Insertion:** Launch using custom SAS autopilot modes (**Prograde**, **Retrograde**) to reach stable orbit.
2. **Orbital Operations:** Monitor electric charge, and follow real-time trajectory lines.

3. **Scientific Observation:** Collect telemetry data with the science instruments.



### 🕹️ Flight Controls



| Control | Action |

| :--- | :--- |

| **WASD** | Pitch and Yaw rotational control |

| **Q / E** | Roll Left / Roll Right |

| **Spacebar** | Stage Separation |

| **UI Dashboard** | Toggle SAS Autopilot Modes (Prograde / Retrograde) |

| **Time Warp Keys** | Accelerate / Decelerate Simulation Speed |
| **M**| Toggle Map View|


---



## 🛠️ Tech Stack & Architecture



**Engine:** [Godot Engine 4.x (.NET Edition)](https://godotengine.org/)
**Programming Language:** C# (.NET 8.0)
**Physics Architecture:** Procedural numerical physics integration, vector dot product mechanics, and inverse-cube gravitational/magnetic solvers (100% self-contained for offline performance).
**Target Platform:** Windows Executable (`.exe`) 

Credits:

Audio:
Parachute.wav by Benboncan -- https://freesound.org/s/72853/ -- License: Attribution 4.0
Rocket launch by Sanderboah -- https://freesound.org/s/803852/ -- License: Creative Commons 0
Compressed Air-Burst Group-7 by wavecal22 -- https://freesound.org/s/753111/ -- License: Creative Commons 0
countdown.wav by raelwissdorf -- https://freesound.org/s/99973/ -- License: Attribution 4.0
"Dreamy Flashback" Kevin MacLeod (incompetech.com)
Licensed under Creative Commons: By Attribution 4.0 License
http://creativecommons.org/licenses/by/4.0/
Success Jingle by JustInvoke -- https://freesound.org/s/446111/ -- License: Attribution 4.0
Astronomical Explosion by SamsterBirdies -- https://freesound.org/s/760509/ -- License: Creative Commons 0
"Vibing Over Venus" Kevin MacLeod (incompetech.com)
Licensed under Creative Commons: By Attribution 4.0 License
http://creativecommons.org/licenses/by/4.0/

Enviroment:
Space 3D Skybox Generator by wwwtyro: Used to generate the background space environment/skybox for our game. Available at https://tools.wwwtyro.net/space-3d/index.html#animationSpeed=1&fov=80&nebulae=true&pointStars=true&resolution=1024&seed=2nzmm1apqfm0&stars=true&sun=false

NASA Resources:
NASA Blue Marble Earth Texture: Used as the texture for our 3D Earth Model, available at: https://science.nasa.gov/earth/earth-observatory/collections/blue-marble/
50-Foot C-Band Radar Dish at Kennedy Space Center (NASA ID: KSC-06pd0031): Used as a visual asset, sourced from Nasa Image and Video Library
Roman Space Telescope Illustration: Used as a visual asset, available at: https://assets.science.nasa.gov/dynamicimage/assets/science/missions/rst/spacecraft-illustrations/Roman_Space_Telescope_Animation2_Still.jpg
70 meter dish (NASA 3D Resources): Used as a 3D asset in our interactive scene. Model courtesy of NASA/JPL-Caltech, available at: https://science.nasa.gov/3d-resources/70-meter-dish/
CubeSat – 1 RU Generic (NASA 3D Resources): Used as a 3D asset in our interactive scene. Model courtesy of NASA/JPL-Caltech, available at: https://science.nasa.gov/3d-resources/cubesat-1-ru-generic/
Mobile Launcher (NASA 3D Resources): Used as a 3D asset in our interactive scene. Model courtesy of NASA/JPL-Caltech, available at: https://science.nasa.gov/3d-resources/mobile-launcher/
Nancy Grace Roman Space Telescope 3D Model (A) (NASA 3D Resources): Used as a 3D asset in our interactive scene. Model courtesy of NASA/JPL-Caltech, available at: https://science.nasa.gov/3d-resources/nancy-grace-roman-space-telescope-a/
Vehicle Assembly Building (VAB) (NASA 3D Resources): Used as a 3D asset in our interactive scene. Model courtesy of NASA/JPL-Caltech, available at: https://science.nasa.gov/3d-resources/vehicle-assembly-building-vab/

Sketchfab Models:
"SpaceX Falcon Heavy V4" (https://skfb.ly/oMRLy) by Clarence365 is licensed under Creative Commons Attribution (http://creativecommons.org/licenses/by/4.0/).
"Space Launch System (SLS)" (https://skfb.ly/ot97t) by Clarence365 is licensed under Creative Commons Attribution (http://creativecommons.org/licenses/by/4.0/).

Particle Packs:
Particle Pack by Kenney: Used for particle effects in the scene. Available at https://godotengine.org/asset-library/asset/783

# PortfolioGame

A third-person multiplayer shooter built as a portfolio project using a custom multiplayer solution.

This project focuses on implementing core multiplayer systems from scratch, including physics synchronization, interpolation, client-side prediction.


## Features

- Third-person player controller
- Character animations
- Weapons: shooting, reloading, damage handling
- Drivable vehicle


## Why this project

This repository is intended as a code portfolio project.  
The main focus is on multiplayer architecture and gameplay synchronization rather than content production.

It demonstrates:
- custom networking approach
- synchronization of dynamic physics objects
- handling of latency-related issues
- scalable gameplay systems organization


## What to look at

If you are reviewing this project as a portfolio sample, the most relevant parts are:

- `Scripts/Multiplayer` — core networking logic
- `Scripts/Multiplayer/Predictable.cs` — synchronization, reconcilation and interpolation logic
- `Scripts/Player` — player controller, animation handling and state management


## How to run

1. Clone this repository
2. Open the project in Unity
3. Build the client and dedicated server using the toolbar [Build] menu
4. Download and run the lobby server from this repository: [https://github.com/reNER0/MultiplayerLobbyServer]
5. Configure `appsettings.json`
6. Start the lobby server
7. Launch the game client and connect

Alternatively, you can review the project as a code sample without running it.

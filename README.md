# DCCPanelController (MAUI)

A cross-platform **model railroad control panel layout designer and controller** built with **.NET MAUI**.

Design schematic-style panels (turnouts, lights, routes, sensors/occupancy, etc.), then operate your layout from the same UI. The app is designed to connect to **JMRI** and/or **WiThrottle** servers and can display **occupancy** when sourced from JMRI. :contentReference[oaicite:6]{index=6}

## Status

This project is actively used and works end-to-end (profiles → panels → swipe between panels → operate accessories). Platform coverage is best on iPad/iPhone/Mac today; Android and Windows may need UI refinement and broader testing. :contentReference[oaicite:7]{index=7}

## Features

- **Panel Designer**
  - Create and edit control panels using a schematic/grid approach
  - Palette-style placement of controls (turnouts, lights, sensors, blocks, routes, etc.)
- **Operator Mode**
  - Swipe between panels and operate your layout in real time :contentReference[oaicite:8]{index=8}
- **Connectivity**
  - Connect to **JMRI** and/or **WiThrottle**
  - Show **occupancy / blocks in use** when using JMRI as the source :contentReference[oaicite:9]{index=9}
- **Profiles**
  - Multiple profiles, each with one or more panels :contentReference[oaicite:10]{index=10}
- **Simulator / Testing Views**
  - Includes simulator/testing UI for development (see project views in the solution) :contentReference[oaicite:11]{index=11}

## Supported Platforms

Target frameworks include: **Android**, **iOS**, **Mac Catalyst**, and **Windows**. :contentReference[oaicite:12]{index=12}

> Note: Actual runtime support depends on having the platform toolchains installed (Xcode for iOS/Mac, Android SDK for Android, Visual Studio on Windows for Windows).

## Getting Started (Developers)

### Prerequisites

- **.NET SDK** with MAUI workloads installed
- Platform toolchains as needed:
  - macOS: Xcode (for iOS/Mac Catalyst)
  - Android: Android SDK / emulator
  - Windows: Visual Studio + Windows tooling

### Build & Run

From the repo root:

```bash
dotnet restore
dotnet build DCCPanelController.sln

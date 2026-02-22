# RIM Jetty 1 & Susan Kruger — AR Experience

An immersive augmented reality (AR) tablet application built in Unity, creating an interactive, educational journey through a crucial period of South African history. The project honours the legacy of those who passed through the Jetty 1 and Susan Kruger sites during the struggle against apartheid.

Through tablet AR experiences, visitors can discover stories, visuals, and archival material from the apartheid era at historically significant locations on Robben Island.

## Project Info

| Field | Detail |
|---|---|
| **Organisation** | UWC Immersive Zone (UWC Innovation Hub) |
| **Platform** | Android Tablets (ARCore) |
| **Engine** | Unity (C#) |
| **AR SDKs** | ARCore, ARKit, Vuforia, EasyAR |
| **3D Tools** | Blender, 3ds Max |
| **Language** | C# |
| **License** | See `LICENSE.txt` |

## Repository Structure

```
├── .github/
│   └── ISSUE_TEMPLATE/           # Issue templates for project management
│       ├── 2d-assets-task.md
│       ├── 3d-assests-task.md
│       ├── audio-task.md
│       ├── bug-task.md
│       ├── doc-task.md
│       ├── pre-prod-task.md
│       ├── unity-development-task.md
│       ├── video-task.md
│       └── xr-test-case.md
│
├── RIMARApp/                     # Unity Project Root
│   ├── Assets/
│   │   ├── Editor/               # Editor scripts & Vuforia config
│   │   │   ├── Migration/        # AddVuforiaEnginePackage.cs
│   │   │   ├── SimpleAABFixer.cs
│   │   │   ├── UnityGooglePlayAABMaster.cs
│   │   │   └── Vuforia/          # Image targets, model targets, testers
│   │   │
│   │   ├── Images/               # Reference images, QR codes, materials
│   │   │   ├── QRCodes/          # 30 QR code markers (1–30)
│   │   │   │   └── ColorQRCodes/ # Colour-coded corner QR markers (R/G/B/Y)
│   │   │   └── LogoIcons/        # UI icons (checkpoint, help, inventory, etc.)
│   │   │
│   │   ├── Map Data/             # Spatial map data (DevStudio, DevChillZone)
│   │   │
│   │   ├── Materials/            # Shared materials
│   │   │
│   │   ├── MobileARTemplateAssets/   # AR template scripts & UI
│   │   │   ├── Scripts/          # ARFeatheredPlaneMeshVisualizerCompanion, ARTemplateMenuManager, GoalManager
│   │   │   └── UI/Scripts/       # CutoutMaskUI
│   │   │
│   │   ├── Models/               # 3D models (marquette, environment assets)
│   │   │
│   │   ├── Plugins/              # Android plugins
│   │   │
│   │   ├── Prefabs/              # Reusable prefab assets
│   │   │
│   │   ├── RIMJetty1Ex/          # Jetty 1 experience-specific assets
│   │   │   ├── Script/           # ElapsedTimer, SceneManager, UISceneLoader
│   │   │   └── Scripts/          # AR feature managers, image/object tracking,
│   │   │                         # billboard, card augmentation, diagnostics
│   │   │
│   │   ├── Resources/            # Runtime-loaded resources
│   │   │
│   │   ├── Samples/              # XR Interaction Toolkit samples (v3.1.1 & v3.4.0)
│   │   │
│   │   ├── Scenes/
│   │   │   ├── MainMenu.unity                 # Main menu scene
│   │   │   ├── ARTemplate_R01.unity           # AR template base scene
│   │   │   ├── Jetty1Zones/                   # Jetty 1 chapter scenes
│   │   │   │   ├── Ch02_J4-Cell.unity
│   │   │   │   ├── Ch03_J1-Marquette.unity
│   │   │   │   └── Ch05_J7-WallOfLetters.unity
│   │   │   ├── SusanKrugerZones/              # Susan Kruger chapter scenes
│   │   │   │   ├── Ch07_SK2-VisitorsDeck.unity
│   │   │   │   └── Ch08_SK3-CaptainsCabin.unity
│   │   │   ├── Templates/                     # Interaction template scenes
│   │   │   │   ├── 1. ARTemplate.unity
│   │   │   │   ├── 2. SingleTouchInteractions.unity
│   │   │   │   └── 3. GestureSwipeInteractions.unity
│   │   │   └── (test scenes: MarquetteTest, PlaneGridTest, VuforiaTest, etc.)
│   │   │
│   │   ├── Scripts/              # Core application scripts
│   │   │   ├── ARPlacementManager.cs
│   │   │   ├── ARPopupHandler.cs
│   │   │   ├── CountdownTimer.cs
│   │   │   ├── GestureManager.cs
│   │   │   ├── GridSpawner.cs
│   │   │   ├── ImageTrackingController.cs
│   │   │   ├── MarkerTap.cs
│   │   │   ├── ProgressTracker.cs
│   │   │   ├── QRCodeSpawner.cs
│   │   │   └── SceneLoader.cs
│   │   │
│   │   ├── Settings/             # Project settings, render pipeline config
│   │   │
│   │   ├── StreamingAssets/      # Vuforia databases & streaming data
│   │   │
│   │   ├── TextMesh Pro/         # TextMeshPro assets
│   │   │
│   │   ├── Videos/               # Video content for AR overlays
│   │   │
│   │   ├── XR/                   # XR configuration
│   │   │
│   │   ├── XRI/                  # XR Interaction Toolkit config
│   │   │
│   │   └── _Recovery/            # Recovery scene backups
│   │
│   ├── Keystore/                 # Android signing keystore
│   ├── Packages/                 # Unity package manifest
│   └── ProjectSettings/          # Unity project settings
│
├── .gitignore
├── .gitattributes
└── LICENSE.txt
```

## Key Scripts

### Core (`Assets/Scripts/`)
| Script | Purpose |
|---|---|
| `ARPlacementManager.cs` | Manages AR object placement on detected surfaces |
| `ARPopupHandler.cs` | Handles information popups triggered by AR interactions |
| `CountdownTimer.cs` | Timer for timed experience segments |
| `GestureManager.cs` | Touch/swipe gesture handling |
| `GridSpawner.cs` | Spawns content on a grid layout over detected planes |
| `ImageTrackingController.cs` | Controls AR image tracking behaviour |
| `MarkerTap.cs` | Handles tap interactions on AR markers |
| `ProgressTracker.cs` | Tracks visitor progress through chapters |
| `QRCodeSpawner.cs` | Spawns AR content at QR code marker positions |
| `SceneLoader.cs` | Scene transition and loading management |

### Jetty 1 Experience (`Assets/RIMJetty1Ex/Scripts/`)
| Script | Purpose |
|---|---|
| `ARFeatureManager.cs` | Manages AR feature availability |
| `ARModeUI.cs` | AR mode UI state management |
| `ImageTrackingHandler/Manager.cs` | Image target tracking pipeline |
| `ObjectTrackingHandler/Manager.cs` | 3D object recognition tracking |
| `ObjectRecognitionSetup/UI.cs` | Object recognition configuration & UI |
| `Billboard.cs` | Always-face-camera behaviour for AR labels |
| `CardAugmentationContent.cs` | Card-based AR content overlay |
| `ObjectAugmentationManager.cs` | Manages augmented content attached to objects |

### Editor (`Assets/Editor/`)
| Script | Purpose |
|---|---|
| `AddVuforiaEnginePackage.cs` | Auto-adds Vuforia package on migration |
| `SimpleAABFixer.cs` | Fixes Android App Bundle build issues |
| `UnityGooglePlayAABMaster.cs` | Google Play AAB publishing helper |

## Scenes & Chapters

The experience is divided into chapters across two historical sites:

**Jetty 1:**
- Ch02 — J4 Cell
- Ch03 — J1 Marquette
- Ch05 — J7 Wall of Letters

**Susan Kruger:**
- Ch07 — SK2 Visitor's Deck
- Ch08 — SK3 Captain's Cabin

## AR Tracking Methods

The project implements multiple AR tracking approaches:
- **QR Code markers** — 30 numbered QR codes + 4 colour-coded corner markers for spatial anchoring
- **Image tracking** — Reference image library for recognising printed images/signage
- **Object tracking** — 3D object recognition (Vuforia model targets)
- **Plane detection** — Surface detection for placing virtual content on real surfaces
- **Spatial maps** — Pre-scanned environment maps for persistent AR placement

## Getting Started

### Prerequisites
- Unity (check `ProjectSettings/ProjectVersion.txt` for exact version)
- Android Build Support module
- ARCore XR Plugin
- XR Interaction Toolkit
- Vuforia Engine (license required)

### Setup
1. Clone the repository
2. Open `RIMARApp/` as a Unity project
3. Import any missing packages via the Package Manager
4. Configure your Vuforia license key in `RIMARApp/Assets/Resources/VuforiaConfiguration.asset`
5. Build to an Android tablet with ARCore support

## Team

Created by **UWC Immersive Zone** (UWC Innovation Hub)

## License

See [LICENSE.txt](LICENSE.txt) for details.

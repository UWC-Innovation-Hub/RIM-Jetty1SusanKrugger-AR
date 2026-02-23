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

<!-- AUTO-STRUCTURE-START -->
```
├── .gitattributes/
    ├── ISSUE_TEMPLATE/
    ├── workflows/
├── .gitignore/
├── LICENSE.txt/
├── README.md/
├── RIMARApp/
    ├── Assets/
        ├── Editor/
            ├── Migration/
            ├── Vuforia/
        ├── Images/
            ├── LogoIcons/
            ├── Materials/
            ├── QRCodes/
        ├── Map Data/
        ├── Materials/
        ├── MobileARTemplateAssets/
            ├── AffordanceThemes/
            ├── Materials/
            ├── Prefabs/
            ├── Scripts/
            ├── Shaders/
            ├── Tutorial/
            ├── UI/
        ├── Models/
            ├── Marquette/
        ├── Plugins/
            ├── Android/
        ├── Prefabs/
            ├── QRCodes/
        ├── RIMJetty1Ex/
            ├── 3DContent/
            ├── Images/
            ├── Reference Objects/
            ├── Script/
            ├── Scripts/
        ├── Resources/
            ├── VuforiaModels/
        ├── Samples/
            ├── XR Interaction Toolkit/
        ├── Scenes/
            ├── Jetty1Zones/
            ├── SusanKrugerZones/
            ├── Templates/
        ├── Scripts/
        ├── Settings/
            ├── Build Profiles/
            ├── Project Configuration/
        ├── StreamingAssets/
            ├── EasyARSamples/
            ├── Vuforia/
        ├── TextMesh Pro/
            ├── Documentation/
            ├── Fonts/
            ├── Resources/
            ├── Shaders/
            ├── Sprites/
        ├── Videos/
        ├── XR/
            ├── Loaders/
            ├── Resources/
            ├── Settings/
            ├── UserSimulationSettings/
        ├── XRI/
            ├── Settings/
    ├── Keystore/
    ├── Packages/
    ├── ProjectSettings/
            ├── com.unity.learn.iet-framework/
            ├── com.unity.template-authoring/
            ├── com.unity.testtools.codecoverage/
```
<!-- AUTO-STRUCTURE-END -->

## Key Scripts

<!-- AUTO-SCRIPTS-START -->
| Script | Path |
|---|---|
| `AddVuforiaEnginePackage.cs` | `Assets/Editor/Migration/AddVuforiaEnginePackage.cs` |
| `SimpleAABFixer.cs` | `Assets/Editor/SimpleAABFixer.cs` |
| `UnityGooglePlayAABMaster.cs` | `Assets/Editor/UnityGooglePlayAABMaster.cs` |
| `ElapsedTtimer.cs` | `Assets/RIMJetty1Ex/Script/ElapsedTtimer.cs` |
| `SceneManager.cs` | `Assets/RIMJetty1Ex/Script/SceneManager.cs` |
| `UISceneLoader.cs` | `Assets/RIMJetty1Ex/Script/UISceneLoader.cs` |
| `ARFeatureManager.cs` | `Assets/RIMJetty1Ex/Scripts/ARFeatureManager.cs` |
| `ARModeUI.cs` | `Assets/RIMJetty1Ex/Scripts/ARModeUI.cs` |
| `AndroidTrackingAlternative.cs` | `Assets/RIMJetty1Ex/Scripts/AndroidTrackingAlternative.cs` |
| `Billboard.cs` | `Assets/RIMJetty1Ex/Scripts/Billboard.cs` |
| `CardAugmentationContent.cs` | `Assets/RIMJetty1Ex/Scripts/CardAugmentationContent.cs` |
| `ImageTrackingHandler.cs` | `Assets/RIMJetty1Ex/Scripts/ImageTrackingHandler.cs` |
| `ImageTrackingManager.cs` | `Assets/RIMJetty1Ex/Scripts/ImageTrackingManager.cs` |
| `InputSystemARFeatureManager.cs` | `Assets/RIMJetty1Ex/Scripts/InputSystemARFeatureManager.cs` |
| `InputSystemObjectRecognitionSetup.cs` | `Assets/RIMJetty1Ex/Scripts/InputSystemObjectRecognitionSetup.cs` |
| `MeshReferenceObjectCreator.cs` | `Assets/RIMJetty1Ex/Scripts/MeshReferenceObjectCreator.cs` |
| `ObjectAugmentationManager.cs` | `Assets/RIMJetty1Ex/Scripts/ObjectAugmentationManager.cs` |
| `ObjectRecognitionSetup.cs` | `Assets/RIMJetty1Ex/Scripts/ObjectRecognitionSetup.cs` |
| `ObjectRecognitionUI.cs` | `Assets/RIMJetty1Ex/Scripts/ObjectRecognitionUI.cs` |
| `ObjectTrackingDiagnostic.cs` | `Assets/RIMJetty1Ex/Scripts/ObjectTrackingDiagnostic.cs` |
| `ObjectTrackingHandler.cs` | `Assets/RIMJetty1Ex/Scripts/ObjectTrackingHandler.cs` |
| `ObjectTrackingManager.cs` | `Assets/RIMJetty1Ex/Scripts/ObjectTrackingManager.cs` |
| `ObjectTrackingSetupHelper.cs` | `Assets/RIMJetty1Ex/Scripts/ObjectTrackingSetupHelper.cs` |
| `QuickBuildFix.cs` | `Assets/RIMJetty1Ex/Scripts/QuickBuildFix.cs` |
| `ReferenceObjectHelper.cs` | `Assets/RIMJetty1Ex/Scripts/ReferenceObjectHelper.cs` |
| `SimpleObjectRecognition.cs` | `Assets/RIMJetty1Ex/Scripts/SimpleObjectRecognition.cs` |
| `ARPlacementManager.cs` | `Assets/Scripts/ARPlacementManager.cs` |
| `ARPopupHandler.cs` | `Assets/Scripts/ARPopupHandler.cs` |
| `CountdownTimer.cs` | `Assets/Scripts/CountdownTimer.cs` |
| `GestureManager.cs` | `Assets/Scripts/GestureManager.cs` |
| `GridSpawner.cs` | `Assets/Scripts/GridSpawner.cs` |
| `ImageTrackingController.cs` | `Assets/Scripts/ImageTrackingController.cs` |
| `MarkerTap.cs` | `Assets/Scripts/MarkerTap.cs` |
| `ProgressTracker.cs` | `Assets/Scripts/ProgressTracker.cs` |
| `QRCodeSpawner.cs` | `Assets/Scripts/QRCodeSpawner.cs` |
| `SceneLoader.cs` | `Assets/Scripts/SceneLoader.cs` |
| `SceneTemplate_RotateCube.cs` | `Assets/Settings/Project Configuration/SceneTemplate_RotateCube.cs` |
<!-- AUTO-SCRIPTS-END -->

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

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
        ├── Assets/
            ├── Fonts/
            ├── LocationPin/
        ├── Audio/
        ├── Editor/
            ├── Migration/
            ├── Vuforia/
        ├── Images/
            ├── Locations/
            ├── LogoIcons/
            ├── Materials/
            ├── QRCodes/
        ├── Map Data/
        ├── Markers/
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
            ├── Export/
            ├── InfoPanels/
            ├── QRCodes/
        ├── RIMJetty1Ex/
            ├── 3DContent/
            ├── Images/
            ├── Reference Objects/
            ├── Script/
            ├── Scripts/
        ├── Resources/
            ├── Prefabs/
            ├── VuforiaModels/
        ├── Samples/
            ├── XR Interaction Toolkit/
        ├── Scenes/
            ├── AR_Cell_InteractionV1/
            ├── Grid_AR-Interaction/
            ├── Jetty1Zones/
            ├── SusanKrugerZones/
            ├── Templates/
        ├── Scripts/
            ├── Marquette/
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
        ├── Textures/
        ├── Videos/
            ├── Locations/
        ├── XR/
            ├── Loaders/
            ├── Resources/
            ├── Settings/
            ├── Temp/
            ├── UserSimulationSettings/
        ├── XRI/
            ├── Settings/
    ├── Keystore/
    ├── Packages/
        ├── com.bezi.sidekick/
            ├── Editor/
    ├── ProjectSettings/
            ├── com.unity.learn.iet-framework/
            ├── com.unity.probuilder/
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
| `ARContentAnimator.cs` | `Assets/Scripts/ARContentAnimator.cs` |
| `ARDebugManager.cs` | `Assets/Scripts/ARDebugManager.cs` |
| `ARFoundationImageTracker.cs` | `Assets/Scripts/ARFoundationImageTracker.cs` |
| `ARGridManager.cs` | `Assets/Scripts/ARGridManager.cs` |
| `ARImageTrackerTest.cs` | `Assets/Scripts/ARImageTrackerTest.cs` |
| `ARPlacementManager.cs` | `Assets/Scripts/ARPlacementManager.cs` |
| `ARScenePrefabExporter.cs` | `Assets/Scripts/ARScenePrefabExporter.cs` |
| `ARTouchHandler.cs` | `Assets/Scripts/ARTouchHandler.cs` |
| `ARTouchInteraction.cs` | `Assets/Scripts/ARTouchInteraction.cs` |
| `CellSpawner.cs` | `Assets/Scripts/CellSpawner.cs` |
| `CleanupSpawnedObjects.cs` | `Assets/Scripts/CleanupSpawnedObjects.cs` |
| `ClickDiagnostic.cs` | `Assets/Scripts/ClickDiagnostic.cs` |
| `ClickableCube.cs` | `Assets/Scripts/ClickableCube.cs` |
| `ClosePanel.cs` | `Assets/Scripts/ClosePanel.cs` |
| `FingerprintInteraction.cs` | `Assets/Scripts/FingerprintInteraction.cs` |
| `GridClickSetup.cs` | `Assets/Scripts/GridClickSetup.cs` |
| `GridCoordinateSystem.cs` | `Assets/Scripts/GridCoordinateSystem.cs` |
| `GridDuplicationFix.cs` | `Assets/Scripts/GridDuplicationFix.cs` |
| `GridMeshGenerator.cs` | `Assets/Scripts/GridMeshGenerator.cs` |
| `ImageSpawnManager.cs` | `Assets/Scripts/ImageSpawnManager.cs` |
| `ImageTrackingHandler.cs` | `Assets/Scripts/ImageTrackingHandler.cs` |
| `InfoPanelController.cs` | `Assets/Scripts/InfoPanelController.cs` |
| `LabelManager.cs` | `Assets/Scripts/LabelManager.cs` |
| `MarkerInteraction.cs` | `Assets/Scripts/MarkerInteraction.cs` |
| `BillboardCamera.cs` | `Assets/Scripts/Marquette/BillboardCamera.cs` |
| `GridSpawner.cs` | `Assets/Scripts/Marquette/GridSpawner.cs` |
| `ARLocationMarker.cs` | `Assets/Scripts/Marquette/Location Info Panels/ARLocationMarker.cs` |
| `BillboardUI.cs` | `Assets/Scripts/Marquette/Location Info Panels/BillboardUI.cs` |
| `DistanceScaler.cs` | `Assets/Scripts/Marquette/Location Info Panels/DistanceScaler.cs` |
| `InfoPanelSpawner.cs` | `Assets/Scripts/Marquette/Location Info Panels/InfoPanelSpawner.cs` |
| `InfoPanelUI.cs` | `Assets/Scripts/Marquette/Location Info Panels/InfoPanelUI.cs` |
| `LocationData.cs` | `Assets/Scripts/Marquette/Location Info Panels/LocationData.cs` |
| `LocationDatabase.cs` | `Assets/Scripts/Marquette/Location Info Panels/LocationDatabase.cs` |
| `CountdownTimer.cs` | `Assets/Scripts/Marquette/MarquetteUI/CountdownTimer.cs` |
| `GalleryManager.cs` | `Assets/Scripts/Marquette/MarquetteUI/GalleryManager.cs` |
| `GameManager.cs` | `Assets/Scripts/Marquette/MarquetteUI/GameManager.cs` |
| `ProgressTracker.cs` | `Assets/Scripts/Marquette/MarquetteUI/ProgressTracker.cs` |
| `UIFlowManager.cs` | `Assets/Scripts/Marquette/MarquetteUI/UIFlowManager.cs` |
| `ScreenshotManager.cs` | `Assets/Scripts/Marquette/ScreenshotFeature/ScreenshotManager.cs` |
| `ScreenshotPreview.cs` | `Assets/Scripts/Marquette/ScreenshotFeature/ScreenshotPreview.cs` |
| `ProximityReveal.cs` | `Assets/Scripts/ProximityReveal.cs` |
| `QRCodeAnchor.cs` | `Assets/Scripts/QRCodeAnchor.cs` |
| `QRCodeSpawner.cs` | `Assets/Scripts/QRCodeSpawner.cs` |
| `QRSceneSpawner.cs` | `Assets/Scripts/QRSceneSpawner.cs` |
| `SceneLayoutHelper.cs` | `Assets/Scripts/SceneLayoutHelper.cs` |
| `SimpleARUI.cs` | `Assets/Scripts/SimpleARUI.cs` |
| `VideoManager.cs` | `Assets/Scripts/VideoManager.cs` |
| `WorldAnchor.cs` | `Assets/Scripts/WorldAnchor.cs` |
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

========================================
CLICKABLE AR CUBES - SETUP GUIDE
========================================

WHAT WAS CREATED:
-----------------
✓ ClickableCube.cs - Makes cubes clickable and spawn objects
✓ ARTouchHandler.cs - Handles AR touch input
✓ GridClickSetup.cs - Manager to setup all cubes at once
✓ ExampleSpawnObject.prefab - Sample object that spawns

YOUR GRID STRUCTURE:
-------------------
GridRoot (25 cubes)
  ├── A1, A2, A3, A4, A5
  ├── B1, B2, B3, B4, B5
  ├── C1, C2, C3, C4, C5
  ├── D1, D2, D3, D4, D5
  └── E1, E2, E3, E4, E5

QUICK SETUP (3 STEPS):
---------------------

STEP 1: Setup Grid Manager
   1. Select "GridClickManager" GameObject in scene
   2. In Inspector, find "Grid Click Setup" component
   3. Drag "GridRoot" from Hierarchy into "Grid Root" field
   4. Drag "ExampleSpawnObject" prefab into "Default Spawn Object" field
   5. Right-click on component → "Setup All Cubes"
      → This adds ClickableCube to all 25 cubes!

STEP 2: Setup AR Touch Handler
   1. Select "ARTouchHandler" GameObject
   2. In Inspector, find "AR Touch Handler" component
   3. Drag "Main Camera" (under XR Origin) into "Ar Camera" field
   4. Leave other settings as default

STEP 3: Test!
   - In Unity Editor: Click cubes with mouse
   - On AR Device: Tap cubes with finger
   - Objects appear above clicked cubes!

DETAILED CONFIGURATION:
----------------------

GridClickManager Settings:
   - Default Spawn Object: Prefab to spawn on all cubes
   - Assign To All Cubes: Auto-assign spawn object to all cubes
   - Grid Root: Reference to your GridRoot GameObject
   - Normal Color: Cube color when idle (white)
   - Highlight Color: Cube color on hover (yellow)
   - Selected Color: Cube color after spawning (green)
   - Spawn Offset: Position offset from cube (0, 0.05, 0 = 5cm above)
   - Spawn Scale: Size multiplier for spawned object
   - Spawn Only Once: If checked, cube spawns only once

ARTouchHandler Settings:
   - Ar Camera: Your AR camera (Main Camera)
   - Interactable Layers: Which layers can be clicked (default: all)
   - Max Raycast Distance: How far touch can reach (10m)
   - Show Debug Rays: Visual debug lines (useful for testing)

Individual Cube Settings:
   Each cube can have different spawn objects!
   1. Select any cube (e.g., A1)
   2. Find "Clickable Cube" component
   3. Change "Object To Spawn" to a different prefab
   4. Adjust colors, spawn offset, etc.

FEATURES:
---------

✓ Click Detection
   - Works with AR touch on mobile
   - Works with mouse in Unity Editor
   - Visual feedback (color changes)

✓ Object Spawning
   - Spawn custom prefabs
   - Auto-spawn default sphere if no prefab assigned
   - Adjustable spawn position and scale
   - Each cube remembers if it spawned

✓ Visual Feedback
   - Normal: White (idle)
   - Highlight: Yellow (hover/targeted)
   - Selected: Green (after spawning)

✓ Customization Per Cube
   - Different spawn objects per cube
   - Different colors per cube
   - Different spawn behaviors

USAGE EXAMPLES:
--------------

Example 1: Different Objects Per Row
   - Row A cubes → Spawn spheres
   - Row B cubes → Spawn cubes
   - Row C cubes → Spawn cylinders
   - Row D cubes → Spawn custom models
   - Row E cubes → Spawn UI elements

Example 2: One-Time Spawn
   - Spawn Only Once: ✓ Checked
   - User taps once → object appears
   - Tapping again does nothing
   - Good for placing permanent objects

Example 3: Toggle Spawn
   - Spawn Only Once: ✓ Checked
   - Destroy On Next Click: ✓ Checked
   - First tap → object appears
   - Second tap → object disappears
   - Good for showing/hiding info

TESTING:
--------

In Unity Editor (without AR):
   1. Enter Play Mode
   2. Click cubes with mouse
   3. Objects should spawn above cubes
   4. Cubes turn green after spawning

On AR Device:
   1. Build to Android/iOS
   2. Scan your image target to show grid
   3. Tap cubes on screen
   4. Objects appear in AR space!

TROUBLESHOOTING:
---------------

Cubes don't respond to clicks:
   → Check ARTouchHandler is in scene
   → Verify AR Camera is assigned
   → Ensure cubes have BoxCollider
   → Run "Setup All Cubes" again

Nothing spawns when clicked:
   → Check cube has ClickableCube component
   → Verify Object To Spawn is assigned
   → Check spawn offset isn't too far away
   → Look in Scene view, not just Game view

Objects spawn in wrong position:
   → Adjust Spawn Offset in ClickableCube
   → Default is (0, 0.05, 0) = 5cm above cube
   → Try (0, 0.1, 0) for 10cm above

Mouse clicks don't work in Editor:
   → Make sure you're clicking the cube directly
   → Camera must be able to see the cube
   → Check Show Debug Rays to see raycast

Touch doesn't work on device:
   → Ensure ARTouchHandler has AR Camera assigned
   → Check camera is from XR Origin
   → Verify grid is visible in AR

ADVANCED CUSTOMIZATION:
----------------------

Create Custom Spawn Objects:
   1. Create your 3D model or GameObject
   2. Add any components (animations, scripts, etc.)
   3. Drag to Prefabs folder to make prefab
   4. Assign to cube's "Object To Spawn"

Spawn with Animation:
   1. Add Animator to spawn prefab
   2. Create animation (pop-in, fade, etc.)
   3. Object plays animation when spawned

Spawn Interactive Objects:
   1. Add scripts to spawn prefab
   2. Can make spawned objects clickable too
   3. Can add physics (Rigidbody)
   4. Can add audio, particles, etc.

Change Spawn Behavior:
   Edit ClickableCube.cs to:
   - Spawn multiple objects
   - Spawn in random positions
   - Spawn with random rotations
   - Spawn different objects randomly

SCRIPT API REFERENCE:
--------------------

ClickableCube Methods:
   - OnCubeClicked() - Manually trigger spawn
   - ResetCube() - Remove spawned object, reset state
   - SetObjectToSpawn(GameObject obj) - Change spawn prefab
   - GetSpawnedObject() - Get reference to spawned object
   - HasSpawned() - Check if cube has spawned

GridClickSetup Methods:
   - SetupAllCubes() - Add ClickableCube to all cubes
   - RemoveAllClickableCubes() - Remove from all cubes
   - ResetAllCubes() - Reset all cubes to initial state
   - ResetAllCubesRuntime() - Reset during gameplay

Example Code:
```csharp
// Get specific cube
ClickableCube cube = GameObject.Find("A1").GetComponent<ClickableCube>();

// Manually trigger spawn
cube.OnCubeClicked();

// Change what it spawns
cube.SetObjectToSpawn(myCustomPrefab);

// Reset cube
cube.ResetCube();

// Reset all cubes
GridClickSetup manager = FindObjectOfType<GridClickSetup>();
manager.ResetAllCubesRuntime();
```

PERFORMANCE TIPS:
----------------
- Use simple spawn prefabs for better performance
- Limit spawned object complexity
- Consider object pooling for many spawns
- Destroy objects when not needed
- Use LOD (Level of Detail) on spawn objects

NEXT STEPS:
-----------
1. ✓ Run "Setup All Cubes" in GridClickManager
2. ✓ Assign AR Camera to ARTouchHandler
3. ✓ Test in Unity Editor with mouse
4. ✓ Create custom spawn prefabs
5. ✓ Assign different prefabs to different cubes
6. ✓ Build and test on AR device

Your clickable AR grid is ready!
========================================

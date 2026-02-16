========================================
AR GRID WITH INTERACTIVE VIDEO BUTTONS
Quick Setup Guide
========================================

WHAT WAS CREATED:
-----------------
✓ CubeInteractiveButton.cs - Manages buttons on cubes
✓ VideoPlayerManager.cs - Video playback with controls
✓ GridActivationController.cs - QR code activation
✓ ARGridSetupHelper.cs - Automated setup tool

SETUP STEPS:
-----------

1. ORGANIZE CUBES
   - Create empty GameObject: "GridParent" at (0,0,0)
   - Drag all 25 cubes as children of GridParent

2. RUN AUTOMATED SETUP
   Menu: Tools → AR Grid → Setup Complete System
   
   This creates:
   - Video player UI with controls
   - Interactive buttons on all cubes
   - Grid activation system

3. CONFIGURE AR TRACKING
   Select: ARGridSystemManager
   - Tracked Image Manager: XR Origin → AR Tracked Image Manager
   - Grid Parent: GridParent GameObject
   - Activate On Any QR Code: ✓
   - Show Buttons On Activation: ✓

4. SETUP VIDEO PLAYER REFERENCES
   Select: VideoPlayerManager
   Assign these from VideoPlayerCanvas hierarchy:
   - Video Display: VideoPanel/VideoDisplay (RawImage)
   - Video Panel: VideoPanel
   - Play Pause Button: ControlsPanel/PlayPauseButton
   - Close Button: ControlsPanel/CloseButton
   - Progress Slider: ControlsPanel/ProgressSlider
   - Play Pause Button Text: PlayPauseButton/ButtonText
   - Time Text: ControlsPanel/TimeText

5. ADD QR CODE
   - Create: XR → Reference Image Library
   - Add your QR code image + physical size
   - Assign to: XR Origin → AR Tracked Image Manager

6. ADD VIDEOS
   - Place in: Assets/StreamingAssets/Videos/
   - Each cube can have different video path
   - Edit in: Cube → CubeInteractiveButton → Video Clip Path

TESTING:
--------
WITHOUT DEVICE:
   - Play Mode
   - Select ARGridSystemManager
   - Inspector: Manual Activate Grid button
   - Click cube buttons to test videos

ON DEVICE:
   - Build to Android/iOS
   - Print QR code at exact physical size
   - Scan QR code
   - Grid appears with buttons
   - Tap buttons to play videos

VIDEO PLAYER CONTROLS:
---------------------
- Play/Pause button
- Progress slider (scrubbing)
- Time display (current / total)
- Close button

CUSTOMIZATION:
-------------
Change button appearance:
   Cube → CubeInteractiveButton:
   - Normal Color
   - Highlight Color
   - Hover Scale
   - Video Clip Path

Change button size:
   Cube → ButtonCanvas → RectTransform Size Delta

Change grid position:
   ARGridSystemManager → GridActivationController:
   - Grid Offset (x, y, z meters)
   - Attach Grid To QR Code
   - Keep Grid At QR Position

SUPPORTED VIDEO FORMATS:
-----------------------
.mp4 (H.264) - Best compatibility
.mov - iOS/macOS
.webm - Android

VIDEO SOURCES:
-------------
Local: "Videos/MyVideo.mp4"
Web: "https://example.com/video.mp4"

TROUBLESHOOTING:
---------------
Buttons don't appear:
   → Run Tools → AR Grid → Setup Cube Buttons
   → Check EventSystem exists in scene
   → Verify ButtonCanvas assigned

Videos don't play:
   → Check video file path
   → Verify VideoPlayerManager references
   → Test with local .mp4 file first

Grid doesn't activate:
   → Assign AR Tracked Image Manager
   → Check QR in Reference Image Library
   → Test Manual Activate Grid first
   → Good lighting when scanning

SCRIPTS API:
-----------
CubeInteractiveButton:
   - ShowButton()
   - HideButton()
   - SetVideoPath(string path)

VideoPlayerManager:
   - PlayVideo(string path, bool autoPlay)
   - TogglePlayPause()
   - CloseVideo()
   - SetVolume(float 0-1)
   - SetLooping(bool loop)

GridActivationController:
   - ManualActivateGrid()
   - ManualDeactivateGrid()
   - ToggleGrid()

========================================
For detailed guide, see:
Pages/AR QR Code App Guide
========================================

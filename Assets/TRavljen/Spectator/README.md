
# Spectator

Before you start, here is some information about the package.

Spectator is a light package for a flexible In-Game camera that allows player free movement around the scene (collision included).
This is an out-of-the-box solution that requires no coding and simple drag & drop of a prefab into the scene will do the trick. The code is well documented and easy to customize. Camera behavior can be adjusted without any coding with the settings available on the components. Advance use might require coding, such as changing the follow target to the main component in game run-time (shown in demo scene).

All examples below are tested or even demonstrated in scenes provided!

Good use cases:
- Free spectator mode
- Follow player spectator mode (rotating around players)
- God mode (free mode without collision) & in-game debugging

Can also be used for (advance use):
- Top-down player camera (use demonstrated in package scene)
- 3rd person camera (where player follows camera rotation)

Simple demo scenes included, but no extra 3D models or other assets!


Package features:
- No setup required, just drag & drop
- No coding is required for out of the box use
- Minimal coding is required for advance use
- Long list of adjustable configurations
- In-Game Toggle Keys (enable, follow, collision)
- Well documented code
- Light solution


General configuration:
- Spectator Enabled (on/off)
- Follow Enabled (on/off)
- Speed
- Center Distance (distance at which the camera will be placed when center action is pressed)
- Rotation Required Trigger (if rotating camera requires additional action to hold down)
- Rotation Speed
- Camera Smooth
- Zoom Sensitivity + Invert Zoom
- Collision Enabled (on/off)
- Restrict Position + Allowed Area (clamps camera to set bounds)
- Lock cursor (locking cursor when spectator is enabled)
- Selecton Enabled (on/off)
- Double Click Enabled (on/off)
- Max Select Distance (raycast max distance)
- Boost Enabled (on/off)
- Boost Speed

Follow configuration:
- Initial Distance (initial distance from follow object)
- Min Distance (min zoom)
- Max Distance (max zoom)
- Target Offset (offset from the transform position)
- Always Look At Target (enables looking at target directly and not with controls, but this is not suitable for all situations, specially the TOP-DOWN view)
- Lock Rotation X
- Invert Rotation X
- Min X Angle
- Max X Angle
- Use Start Rotation X
- Start Rotation X
- Lock Rotation Y
- Invert Rotation Y
- Min Y Angle
- Max Y Angle
- Use Start Rotation Y
- Start Rotation Y

## Setup Instructions

Here are steps to setup the package:

- Import the package
- Drag and drop the desired prefab into the scene (based on the input you use, either new or old input system)
- Optionally connect the camera you wish to control with this component, if none is set `Camera.main` will be used

Then it depends on what you wish to achieve with this component.

### 1. Follow & Free spectator
This is enabled by default on the prefab so only adjusting configurations for customisation is needed, otherwise works out of the box.

### 2. Free spectator
If the player is to be a free moving spectator, then you only need to adjusting configuration to your liking - disable collision, set rotation and movement speed, etc.

To disable follow features simply set `FollowCamera.FollowEnabled` on `false`.

### 3. Follow spectator
To enable only following spectator, remove the value on action/key for toggling the spectator follow and set the following target in code with function `SpectatorPlayer.SetFollowTarget`.
This will disable any other movement but follow. Here you can set max and min values for X and Y rotation axes as well as disable movement on specific axis.

### 4. Top down spectator
Example shown in the top down demo scene. Primary configurations that need to be set on `FollowCamera` component:
- Lock Rotation X (enable)
- Use Start Rotation X (enable)
- Start Rotation X = Set desired rotation (80 degrees for example), rotation will be locked on this value

This enables player to rotate around Y axes (around the following object), but prevents him from looking up or down.
In the demo scene I've shown how you can also use this configuration to control the movement of the unit, for this you also need to set flag `Required Rotation Trigger` on `SpectatorPlayer` component to `true` and this will required mouse right click (by default) for camera rotation so that the mouse can move freely and navigate the unit.
# OpenWorld2D_Unity
Terrain Generation in Unity using 3D aspects for a top down 2D world. 

![Alt_Text](https://media.giphy.com/media/gfIcgbNeFFQWa1BOLU/giphy.gif)

![Alt_Text](https://media.giphy.com/media/f3kAOgAv7jfBqgYnui/giphy.gif)

# Features

Player controls
- WASD/ Arrow keys for movement
- Shift to sprint
- Left click in a direction to roll
- Space to Jump

Observer Pattern
- this pattern allows for decoupling of Player and World classes, can be used throughout the project. 

Scriptable Tile / TerrainTile
- checks neighboring tiles upon construction of sprite tilemap and replaces tiles with correct configuration per mask. 
- allows for procedural tile placement since we do not have to manually draw the correct sprite, instead just provide height data from perlin noise. 

Plants placed using Perlin Noise

# Tilemap Engine:

Adjustable Perlin Noise Constants
- MAX_HEIGHT
- HEIGHT_DELTA
- CHUNK_WIDTH
- CHUNK_HEIGHT
- HEIGHT_DIFF
- H_NOISE_FREQUENCY
- Leads to variety of images shown below/ variety of terrains

Physics Terrain Collisions
- createMergedCollider() merges 3D box colliders in the x and y directions to optimise physics coverage of entire terrain
-  createRidgedCollider() will create a convex mesh collider for ridges that are sloped .
- collider_types holds information over various collider types to account for different sprite tiles to get accurate physics behavior. createMergedCollider() will account for different collider_types/ dimensions in order to determine if it can merge neighboring tiles. 



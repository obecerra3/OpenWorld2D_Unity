# Terrain2D_Unity
Terrain Generation in Unity using 3D aspects for a top down 2D world. 

![Alt_Text](https://giphy.com/gifs/f3kAOgAv7jfBqgYnui)

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

# Tilemap Engine:

Adjustable Perlin Noise Constants
- MAX_HEIGHT
- HEIGHT_DELTA
- CHUNK_WIDTH
- CHUNK_HEIGHT
- MAP_UPDATE_DISTANCE
- HEIGHT_DIFF
- H_NOISE_FREQUENCY
- Leads to variety of images shown below/ variety of terrains

Physics Terrain Collisions
- createMergedCollider() merges 3D box colliders in the x and y directions to optimise physics coverage of entire terrain
- (in Progress) createRidgedCollider() will create a convex mesh collider for ridges to avoid strange behavior where the player
 can walk on tiles that appear to be sloped. 
- collider_types holds information over various collider types to account for different sprite tiles to get accurate physics behavior. createMergedCollider() will account for different collider_types/ dimensions in order to determine if it can merge neighboring tiles. 
 
Data arrays:
- int[,,] map holds info. over whether an (x,y,z) contains any terrain with (0 or 1)
- float[,] height_map holds info. over the perlin noise generated height 
- int[,,] mask_map holds info. over the sprite tile mask type (masks are found by querying neighboring tiles). 
  -> this is very useful for removing illegal values that your Scriptable Terrain Tile does not account for. For example I remove these values in loadGraphics() 



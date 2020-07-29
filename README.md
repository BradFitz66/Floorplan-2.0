# BEING REWORKED
Due to how awful my code is in this and how much of this code is tailored towards how I'm developing my game rather than fitting a general purpose role, I've decided I'm going to recreate completely it under a new name. This won't get any new updates and I'll update this once the new tool is ready.

# Floorplan
 Based off of the Unity asset by Alexis Morin.


This is an extensiive modification to Alexis Morin's floorplan unity plugin. So much so that I believe the code base to be atleast 90% changed if not 100%.

Bewarned, however, a lot of this code was written by a complete idiot (me) and while I'm developing a game so a lot of features may be based around said game and may not be fit for general purpose.


What are the changes I've made, you say? Well:

1. Completely overhauled controls. Now it's a custom inspector with it's own UI where you can choose which tile you want to draw, it's materials (supports objects with multiple materials) and which tool you want to draw with (currently only a rectangle and filled rectangle tool. The filled rectangle tool only works with floor tiles and the normal rectangle tool only works with walls)

2. Added a script that combines all the tiles created by floorplan into one big mesh. I found that, even with static batching, performance was significantly reduced especially when post-processing was used. This script just goes through all tiles and combines their meshes. It has a filter for any objects with a rigidbody (to stop stuff like doors that you want to have dynamic hinges on from being merged)

3. Tileset changes. Before, a tileset only had 1 tile per type (Floor, Wall, Arch, Window). Now there's three categories and it's been changed so each type is an array of game objects so you can have (technically) an infinite amount of tiles per type.

4. Overlap prevention. Before a tile is placed it checks that area using CheckSphere to make sure there isn't another tile. If there is, it won't place a tile there. (this will only check for objects created by floorplan). Floor tiles however will replace any floor tile at that position.

5. Multi material support. As aforementioned, I implemented support to choose materials for objects that can have multiple materials on the tile. For this to work, the first child of the tile prefab should be the object that you want to change material for. It only supports up to 4 materials but this can easily be changed (limited due to space each material row takes up)


## Roadmap
1. Optimize mesh collider for merged tiles. Currently, when tiles are merged, there's a tonne of redundant triangles that can reduced down to as little as two triangles. I haven't seen any performance problems due to the current mesh, but that may be different for each person. If anyone has any ideas on how I could achieve this, please create an issue. I'm pretty dumb so please link to any specific algorithm I could use.

2. Better code. This code is very ugly. Performance doesn't suffer from it I believe, but readability and coherence does.

3. Better undo/redo functionality. Currently it supports the undo/redo of walls you place.

4. Delete tool. There's currently no way to directly delete tiles without selecting them in the editor and manually deleting them.

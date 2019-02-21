# Unity-Dungeon-Generator
Unity Dungeon Generator is a Procedural Generation for dungeon creation. It will result a bunch of data for your needs.

## Library
Unity Dungeon Generator using Delaunay library from https://github.com/jceipek/Unity-delaunay, it is MIT License. In this project, the Delaunay already included.

## Feature
There is 2 Algorithm in this package
1. Spreads Rooms automatically (for creating rooms) x Delaunay Triangulation (for connection each room)

![alt text](https://github.com/damarindra/Unity-Dungeon-Generator/blob/master/delaunay.gif "Delaunay")

2. Binary Space Partition (for creating rooms) x Delaunay Triangulation (for connection each room)

![alt text](https://github.com/damarindra/Unity-Dungeon-Generator/blob/master/bsp.gif "Binary Space Partition")

### Which one is better?
IMO, the first one is the best. It will create natural looking dungeon.

## How to use it?
### Spreads Rooms automatically x Delaunay Triangulation
1. Create Empty GameObject, add **Dungeon Rooms Generator** Component and **Delaunay Dungeon Generator**.
2. Setup the value in inspector, for starter, this is my recommendation.

![alt text](https://github.com/damarindra/Unity-Dungeon-Generator/blob/master/delaunay.jpg "Delaunay")

3. Click the Complete Actions, it will automatically create the data for you dungeon.

### Binary Space Partition x Delaunay Triangulation
1. Create Empty GameObject, add **BSP Dungeon Generation** Component.
2. Setup the value in inspector, for starter, this is my recommendation. (Value option for Room Counts until Spread Distance is ignored, it only used when using spreads rooms)

![alt text](https://github.com/damarindra/Unity-Dungeon-Generator/blob/master/bsp.jpg "Binary Space Partition")

3. Click the Generate, it will automatically create the data for you dungeon.

### Then?
You can implements the dungeon using **rooms** and **corridors** variable from DungeonGenerator.cs.  ***Spreads Rooms automatically x Delaunay Triangulation*** will not using **rooms**, but mainRooms variable from DelaunayDungeonGenerator.cs. You can see TilemapBSPDriver.cs for how to implements it.

# License
It is MIT License. But if you create a game with this script, shoutout on your social account really appreciate!!

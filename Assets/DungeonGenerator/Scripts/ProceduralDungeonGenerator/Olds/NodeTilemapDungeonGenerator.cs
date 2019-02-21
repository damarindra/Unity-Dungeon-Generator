using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DI.DungeonGenerator {
	[System.Serializable]
	[CreateAssetMenu(fileName = "DungeonNodes", menuName = "DI/Dungeon/Tilemap Node")]
	public class NodeTilemapDungeonGenerator : NodeGraphGenerator
	{
		public NodeTilemap[] nodes;
		public Tilemap[] rooms;
		//for determining nodes room using index from rooms variable
		public int[] roomsIndexNodes;

		public override void StartGenerationSequence()
		{
			base.StartGenerationSequence();
			nodes = new NodeTilemap[nodeCount];
			roomsIndexNodes = new int[nodeCount];
			for(int i = 0; i < nodes.Length; i++)
			{
				nodes[i] = new NodeTilemap(base.nodes[i]);
				roomsIndexNodes[i] = Random.Range(0, rooms.Length);
			}
		}
	}

	public class NodeTilemap : Node
	{
		public Tilemap room;
		public NodeTilemap(Node copy)
		{
			this.id = copy.id;
			this.nodeConnections = copy.nodeConnections;
			this.parent = copy.parent;
			this.rectPosition = copy.rectPosition;
		}
	}
}

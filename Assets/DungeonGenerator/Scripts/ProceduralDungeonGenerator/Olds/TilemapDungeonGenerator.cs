using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DI.DungeonGenerator{
	public class TilemapDungeonGenerator : MonoBehaviour {
		[SerializeField] NodeTilemapDungeonGenerator node;
		[SerializeField] Tilemap tilemap;
		[SerializeField] Tile tile;

		List<DummyTilemap> dummyRooms = new List<DummyTilemap>();
		
		void Start(){
			DrawTiles(tilemap, node.rooms[0], Vector3Int.zero);
		}
		
		public void StartGenerationSequence(){
			if(tilemap == null || node == null || tile == null)
				return;

			for(int i = 0; i < node.nodes.Length; i++){
				 
			}
		}

		//private Vector3Int FindNextPosition(Vector3Int parentCenter, int index, Vector3Int moveDirection)
		//{
		//	int iter = 0;
		//	while (true)
		//	{
		//		if (tilemap.GetTile(parentCenter + moveDirection * iter) == null)
		//		{
		//			if (index == 0)
		//				dummyRooms.Add(new DummyTilemap(parentCenter, node.rooms[index]));
		//			else {

		//			}
		//			break;
		//		}
		//		iter++;
		//	}
		//}

		private void DrawTiles(Tilemap targetTilemap, Tilemap copy, Vector3Int offset){
			for(int x = 0; x < copy.size.x; x++){
				for(int y = 0; y < copy.size.y; y++){
					targetTilemap.SetTile(new Vector3Int(x,y,0) + offset, tile);
				}
			}
		}

		private class DummyTilemap
		{
			public Vector3Int origin;
			public Vector3Int size;
			public Vector3Int center { get { return origin + new Vector3Int(size.x / 2, size.y / 2, size.z); } }
			public List<Vector3Int> tileCoordinateUsed = new List<Vector3Int>();

			public DummyTilemap(Vector3Int origin, Tilemap tilemap) {
				this.origin = origin;
				this.size = tilemap.size;
				Vector3Int offset = origin - tilemap.origin;
				for (int x = 0; x < tilemap.size.x; x++)
				{
					for (int y = 0; y < tilemap.size.y; y++)
					{
						if (tilemap.GetTile(new Vector3Int(x, y, 0)) != null) {
							Vector3Int result = tilemap.origin + offset;
							result.x += x;
							result.y += y;
							tileCoordinateUsed.Add(result);
						}
					}
				}
			}
		}
	}
}

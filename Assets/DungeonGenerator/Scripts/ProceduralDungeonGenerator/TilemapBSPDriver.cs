using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DI.DungeonGenerator
{
	[RequireComponent(typeof(BSPDungeonGeneration))]
	public class TilemapBSPDriver : MonoBehaviour {
		[SerializeField] Tile tile;
		[SerializeField] Tilemap tilemap;
		BSPDungeonGeneration bspdungeon;

		private void Awake()
		{
			bspdungeon = GetComponent<BSPDungeonGeneration>();
			bspdungeon.StartRoomGeneration();
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.B))
			{
				ApplyTile();
			}
		}

		public void ApplyTile()
		{
			if (bspdungeon == null)
				bspdungeon = GetComponent<BSPDungeonGeneration>();
			foreach(Room r in bspdungeon.rooms)
			{
				int _x = 0;
				while(_x < Mathf.Abs(r.rect.width))
				{
					int _y = 0;
					while(_y < Mathf.Abs(r.rect.height))
					{
						Vector3Int v = new Vector3Int((int)(r.rect.x + _x * Mathf.Sign(r.rect.width) + (Mathf.Sign(r.rect.width) == -1 ? (-1) : 0)), (int)(r.rect.y + _y * Mathf.Sign(r.rect.height) + (Mathf.Sign(r.rect.height) == -1 ? (-1) : 0)), 0);
						tilemap.SetTile(v, tile);
						_y++;
					}
					_x++;
				}
			}
			foreach(Corridor c in bspdungeon.corridors)
			{
				foreach(Rect r in c.ways)
				{
					int _x = 0;
					while (_x < Mathf.Abs(r.width))
					{
						int _y = 0;
						while (_y < Mathf.Abs(r.height))
						{
							Vector3Int v = new Vector3Int((int)(r.x + _x * Mathf.Sign(r.width) + (Mathf.Sign(r.width) == -1 ? (-1) : 0)), (int)(r.y + _y * Mathf.Sign(r.height) + (Mathf.Sign(r.height) == -1 ? (-1) : 0)), 0);
							tilemap.SetTile(v, tile);
							_y++;
						}
						_x++;
					}
				}
			}
		}

		public void RemoveTiles()
		{
			BoundsInt bounds = tilemap.cellBounds;
			print(bounds);
			for (int x = bounds.x; x < bounds.size.x; x++) {
				for(int y = bounds.y; y < bounds.size.y; y++)
				{
					tilemap.SetTile(new Vector3Int(x, y, 0), null);
				}
			}
		}
	}
}

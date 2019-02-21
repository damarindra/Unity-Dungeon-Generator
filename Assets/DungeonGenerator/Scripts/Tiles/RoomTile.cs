using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


[System.Serializable]
[CreateAssetMenu(fileName = "New Room Tile", menuName = "Tiles/Room Tile")]
public class RoomTile : Tile {

	public Sprite m_Preview;
	public TilingRoom m_Floor;
	public TilingRoom m_WallLeft;
	public TilingRoom m_WallTop;
	public TilingRoom m_WallRight;
	public TilingRoom m_WallBottom;
	public TilingRoom m_WallTopLeft;
	public TilingRoom m_WallTopRight;
	public TilingRoom m_WallBottomLeft;
	public TilingRoom m_WallBottomRight;
	public TileBase m_Door;

	TileNeighbors m_Neighbors = new TileNeighbors();

	[System.Serializable]
	public class TilingRoom{
		public GameObject m_GameObject;
		public TileBase m_Tile;
		public enum TilingType {
			GameObject, Tile
		}
		public TilingType m_TilingType = TilingType.GameObject;
	}
	
	public class TileNeighbors
	{
		TileBase[] neighbors = new TileBase[8];
		public enum Direction {
			TopLeft, Top, TopRight, Left, Right, BottomLeft, Bottom, BottomRight
		}

		//TileBase array will started from bottom left, moving right. Index 3 will be middle left, Index 5 will be top left.
		public void CollectNeighbors(ITilemap tilemap, Vector3Int position)
		{
			int index = 0;
			for (int y = -1; y < 2; y++)
			{
				for (int x = -1; x < 2; x++)
				{
					if (x == 0 && y == 0)
						continue;
					neighbors[index] = tilemap.GetTile(position + new Vector3Int(x, y, 0));
					index++;
				}
			}
			
		}

		public TileBase GetNeighbors(Direction dir)
		{
			switch (dir)
			{
				case Direction.BottomLeft:
					return neighbors[0];
				case Direction.Bottom:
					return neighbors[1];
				case Direction.BottomRight:
					return neighbors[2];
				case Direction.Left:
					return neighbors[3];
				case Direction.Right:
					return neighbors[4];
				case Direction.TopLeft:
					return neighbors[5];
				case Direction.Top:
					return neighbors[6];
				default:return neighbors[7];
			}
		}
	}

	public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
	{
		base.GetTileData(position, tilemap, ref tileData);
		m_Neighbors.CollectNeighbors(tilemap, position);

		TilingRoom result = GetThisTile(tilemap, position, m_Neighbors);
		if (result.m_TilingType == TilingRoom.TilingType.GameObject)
			tileData.gameObject = result.m_GameObject;
		else 
			result.m_Tile.GetTileData(position, tilemap, ref tileData);
		tileData.sprite = m_Preview;
	}

	
	public TilingRoom GetThisTile(ITilemap tilemap, Vector3Int position, TileNeighbors neighbors)
	{
		//DOOR
		if (neighbors.GetNeighbors(TileNeighbors.Direction.Left) == m_Door || neighbors.GetNeighbors(TileNeighbors.Direction.Right) == m_Door || neighbors.GetNeighbors(TileNeighbors.Direction.Top) == m_Door || neighbors.GetNeighbors(TileNeighbors.Direction.Bottom) == m_Door)
			return m_Floor;
		else if (neighbors.GetNeighbors(TileNeighbors.Direction.Left) == null)
			return m_WallLeft;
		else if (neighbors.GetNeighbors(TileNeighbors.Direction.Right) == null)
			return m_WallRight;
		else if (neighbors.GetNeighbors(TileNeighbors.Direction.Top) == null)
			return m_WallTop;
		else if (neighbors.GetNeighbors(TileNeighbors.Direction.Bottom) == null)
			return m_WallBottom;
		else if (neighbors.GetNeighbors(TileNeighbors.Direction.TopLeft) == null)
			return m_WallTopLeft;
		else if (neighbors.GetNeighbors(TileNeighbors.Direction.TopRight) == null)
			return m_WallTopRight;
		else if (neighbors.GetNeighbors(TileNeighbors.Direction.BottomLeft) == null)
			return m_WallBottomLeft;
		else if (neighbors.GetNeighbors(TileNeighbors.Direction.BottomRight) == null)
			return m_WallBottomRight;

		return m_Floor;
	}

	public override bool StartUp(Vector3Int location, ITilemap tilemap, GameObject instantiateedGameObject)
	{
		if (instantiateedGameObject != null)
		{
			Vector3 loc = location;
			Tilemap t = tilemap.GetComponent<Tilemap>();
			switch (t.orientation)
			{
				case Tilemap.Orientation.XZ:
					loc.y = location.z;
					loc.z = location.y;
					break;
				case Tilemap.Orientation.YX:
					loc.x = location.y;
					loc.y = location.x;
					break;
				case Tilemap.Orientation.YZ:
					loc.x = location.y;
					loc.y = location.z;
					loc.z = location.x;
					break;
				case Tilemap.Orientation.ZX:
					loc.x = location.z;
					loc.y = location.x;
					break;
				case Tilemap.Orientation.ZY:
					loc.x = location.z;
					loc.z = location.x;
					break;
			}
			instantiateedGameObject.transform.position = loc + tilemap.GetComponent<Tilemap>().tileAnchor;
		}

		return true;
	}

	public override void RefreshTile(Vector3Int location, ITilemap tileMap)
	{
		for (int y = -1; y <= 1; y++)
		{
			for (int x = -1; x <= 1; x++)
			{
				base.RefreshTile(location + new Vector3Int(x, y, 0), tileMap);
			}
		}
	}
}

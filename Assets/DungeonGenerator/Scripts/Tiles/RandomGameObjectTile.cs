using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
[CreateAssetMenu(fileName = "New GameObject Random Tile", menuName = "Tiles/GameObject Random Tile")]
public class RandomGameObjectTile : Tile
{
	public Sprite spr;
	public GameObject[] m_gameObject;

	public override void GetTileData(Vector3Int location, ITilemap tileMap, ref TileData tileData)
	{
		base.GetTileData(location, tileMap, ref tileData);
		if ((m_gameObject != null) && (m_gameObject.Length > 0))
		{
			long hash = location.x;
			hash = (hash + 0xabcd1234) + (hash << 15);
			hash = (hash + 0x0987efab) ^ (hash >> 11);
			hash ^= location.y;
			hash = (hash + 0x46ac12fd) + (hash << 7);
			hash = (hash + 0xbe9730af) ^ (hash << 11);
			UnityEngine.Random.InitState((int)hash);
			tileData.sprite = spr;
			tileData.gameObject = m_gameObject[(int)(m_gameObject.Length * UnityEngine.Random.value)];
		}
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

#if UNITY_EDITOR
[CustomEditor(typeof(RandomGameObjectTile))]
public class RandomGameObjectTileEditor : Editor
{
	private RandomGameObjectTile tile { get { return (target as RandomGameObjectTile); } }

	public override void OnInspectorGUI()
	{
		EditorGUI.BeginChangeCheck();
		tile.spr = (Sprite)EditorGUILayout.ObjectField("Sprite", tile.spr, typeof(Sprite), false, null);

		int count = EditorGUILayout.DelayedIntField("Number of GameObject", tile.m_gameObject != null ? tile.m_gameObject.Length : 0);
		if (count < 0)
			count = 0;
		if (tile.m_gameObject == null || tile.m_gameObject.Length != count)
		{
			Array.Resize<GameObject>(ref tile.m_gameObject, count);
		}

		if (count == 0)
			return;

		EditorGUILayout.LabelField("Place random GameObject.");
		EditorGUILayout.Space();

		for (int i = 0; i < count; i++)
		{
			tile.m_gameObject[i] = (GameObject)EditorGUILayout.ObjectField("GameObject " + (i + 1), tile.m_gameObject[i], typeof(GameObject), false, null);
		}
		if (EditorGUI.EndChangeCheck())
			EditorUtility.SetDirty(tile);
	}
}

#endif
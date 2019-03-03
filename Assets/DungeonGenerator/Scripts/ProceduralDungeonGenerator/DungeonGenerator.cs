using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DI.DungeonGenerator
{
	public class DungeonGenerator : MonoBehaviour {
		
		public virtual void StartRoomGeneration() { }
		[HideInInspector] public List<Room> rooms = new List<Room>();
		//[HideInInspector] public List<Corridor> corridors = new List<Corridor>();
		[SerializeField] protected int corridorWidth = 3;
		
		#region UNITY_EDITOR
#if UNITY_EDITOR
		[HideInInspector]public bool drawGizmos = true;
		protected virtual void OnDrawGizmosSelected()
		{
			if (!drawGizmos)
				return;
			if(rooms == null)
			{
				print(rooms);
				return;
			}
			for (int i = 0; i < rooms.Count; i++)
			{

				Vector2 bottomLeft = (rooms)[i].rect.position;
				Vector2 topRight = (rooms)[i].rect.position + (rooms)[i].rect.size;
				Vector2 topLeft = bottomLeft + Vector2.up * (rooms)[i].rect.height;
				Vector2 bottomRight = bottomLeft + Vector2.right * (rooms)[i].rect.width;

				Gizmos.DrawLine(bottomLeft, bottomRight);
				Gizmos.DrawLine(bottomLeft, topLeft);
				Gizmos.DrawLine(topRight, topLeft);
				Gizmos.DrawLine(topRight, bottomRight);

				UnityEditor.Handles.Label(topLeft + Vector2.right, rooms[i].id.ToString());

			}
			//foreach (Corridor r in corridors)
			//{
			//	foreach (Rect cr in r.ways)
			//		Gizmos.DrawCube(cr.center, cr.size);
			//}
		}

		public void CreateColliderVisual()
		{
			int i = 0;
			foreach (Room r in (rooms))
			{
				GameObject g = new GameObject(i.ToString());
				var b = g.AddComponent<BoxCollider>();
				b.size = r.rect.size;
				b.center = r.rect.center;
				g.transform.parent = transform;
				i++;
			}
		}
		public void DeleteAllColliderVisual()
		{
			for (int i = transform.childCount; i > 0; i--)
			{
				DestroyImmediate(transform.GetChild(i - 1).gameObject);
			}
		}
#endif
		#endregion
	}

	[System.Serializable]
	public class MinMaxInt
	{
		[SerializeField] int min;
		[SerializeField] int max;

		public MinMaxInt(int min, int max)
		{
			this.min = min;
			this.max = max;
		}
		public int Random()
		{
			return UnityEngine.Random.Range(min, max + 1);
		}
	}

	[System.Serializable]
	public class MinMaxFloat
	{
		[SerializeField] float min;
		[SerializeField] float max;
		public MinMaxFloat(float min, float max)
		{
			this.min = min;
			this.max = max;
		}

		public float Random()
		{
			return UnityEngine.Random.Range(min, max);
		}
	}
}

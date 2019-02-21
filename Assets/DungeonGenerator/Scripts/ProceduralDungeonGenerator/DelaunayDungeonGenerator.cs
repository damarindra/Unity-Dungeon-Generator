using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Delaunay;
using Delaunay.Geo;

namespace DI.DungeonGenerator
{
	//this implementation need https://github.com/jceipek/Unity-delaunay. It is MIT license, project already imported.
	public class DelaunayDungeonGenerator : MonoBehaviour {

		public DungeonRoomsGenerator dg { get { if (m_dg == null) m_dg = GetComponent<DungeonRoomsGenerator>(); return m_dg; } }
		private DungeonRoomsGenerator m_dg;

		[SerializeField, Range(0f, 1f)] private float mainRoomSize = 0.5f;
		[SerializeField, Range(0f, .1f)] private float keepConnection = 0.05f;

		[HideInInspector]public List<Room> mainRooms = new List<Room>();

		//Delaunay
		Delaunay.Voronoi v;
		private List<LineSegment> m_spanningTree;
		private List<LineSegment> m_delaunayTriangulation;
#if DEBUG_MODE
		public bool drawTriangulation = true;
		public bool drawSpanningTree = true;
#endif

		public void StartTriangulation()
		{
			if (dg == null)
				return;
			mainRooms.Clear();
			float mean = dg.rooms.Average(o => o.rect.size.sqrMagnitude);
			var roomFromTheBiggest = dg.rooms.OrderBy(o => o.rect.size.sqrMagnitude).ToList();
			roomFromTheBiggest.Reverse();
			foreach(Room r in roomFromTheBiggest)
			{
				if(r.rect.size.sqrMagnitude >= mean)
				{
					mainRooms.Add(r);
				}
				if (mainRooms.Count >= dg.rooms.Count * mainRoomSize)
					break;
			}

			List<Vector2> m_points = mainRooms.Select(o => o.rect.center).ToList();
			List<uint> colors = new List<uint>();
			foreach (Vector2 v in m_points)
				colors.Add(0);

			v = new Voronoi(m_points, colors, new Rect(0, 0, -999999, 999999));

			m_delaunayTriangulation = v.DelaunayTriangulation();
			m_spanningTree = v.SpanningTree(KruskalType.MINIMUM);

			List<LineSegment> trisLeft = new List<LineSegment>();
			foreach(LineSegment d in m_delaunayTriangulation)
			{
				bool safeToAdd = true;
				foreach(LineSegment s in m_spanningTree)
				{
					if ((d.p0 == s.p0 && d.p1 == s.p1) || (d.p0 == s.p1 && d.p1 == s.p0)) 
					{
						safeToAdd = false;
						break;
					}
				}
				if(safeToAdd)
					trisLeft.Add(d);
			}

			trisLeft = trisLeft.OrderBy(o => (Vector2.SqrMagnitude((Vector2)(o.p0 - o.p1)))).ToList();
			for(int i = 0; i < (int)(trisLeft.Count * keepConnection); i++)
			{
				m_spanningTree.Add(trisLeft[i]);
			}
			
			dg.roomConnection.Clear();
			foreach(LineSegment l in m_spanningTree)
			{
				if (dg.roomConnection.ContainsKey(l))
					continue;

				Room r1 = null, r2 = null;
				foreach(Room r in mainRooms)
				{
					if (r.rect.center == l.p0)
						r1 = r;
					else if (r.rect.center == l.p1)
						r2 = r;
				}

				if(r1 == null || r2 == null)
				{
					Debug.Log("Dude, something doesn't right! Room is not detected in triangulation! Check here");
				}
				else 
				{
					dg.roomConnection.Add(l, new List<Room>(2) { r1, r2 });
				}
			}
		}

		public void DoGeneration()
		{
			dg.StartRoomGeneration();
			StartTriangulation();
			dg.StartCorridorsGeneration(mainRooms, dg.corridors);
		}

#if UNITY_EDITOR
		private void OnDrawGizmosSelected()
		{
#if DEBUG_MODE
			Gizmos.color = Color.magenta;
			if (m_delaunayTriangulation != null && drawTriangulation)
			{
				for (int i = 0; i < m_delaunayTriangulation.Count; i++)
				{
					Vector2 left = (Vector2)m_delaunayTriangulation[i].p0;
					Vector2 right = (Vector2)m_delaunayTriangulation[i].p1;
					Gizmos.DrawLine((Vector3)left, (Vector3)right);
				}
			}
			if (m_spanningTree != null && drawSpanningTree)
			{
				Gizmos.color = Color.green;
				for (int i = 0; i < m_spanningTree.Count; i++)
				{
					LineSegment seg = m_spanningTree[i];
					Vector2 left = (Vector2)seg.p0;
					Vector2 right = (Vector2)seg.p1;
					Gizmos.DrawLine((Vector3)left, (Vector3)right);
				}
			}
			Gizmos.color = Color.magenta;
			for (int i = 0; i < mainRooms.Count; i++)
			{
				Vector2 bottomLeft = mainRooms[i].rect.position;
				Vector2 topRight = mainRooms[i].rect.position + mainRooms[i].rect.size;
				Vector2 topLeft = bottomLeft + Vector2.up * mainRooms[i].rect.height;
				Vector2 bottomRight = bottomLeft + Vector2.right * mainRooms[i].rect.width;

				Gizmos.DrawLine(bottomLeft, bottomRight);
				Gizmos.DrawLine(bottomLeft, topLeft);
				Gizmos.DrawLine(topRight, topLeft);
				Gizmos.DrawLine(topRight, bottomRight);

				UnityEditor.Handles.Label(topLeft + Vector2.right, mainRooms[i].id.ToString());
			}
#endif
		}
#endif
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Delaunay;
using Delaunay.Geo;
using System.Linq;

namespace DI.DungeonGenerator
{
	/// <summary>
	/// Binary Space Partition Algorithm uses for room creation. The connection still using Delaunay Triangulation
	/// </summary>
	public class BSPDungeonGeneration : DungeonRoomsGenerator {
		[SerializeField] private Vector2Int boardSize = new Vector2Int(30, 30);
		[SerializeField] private int partitionCount = 10;
		[SerializeField] private MinMaxFloat sliceRange = new MinMaxFloat(0.3f, 0.5f);
		[SerializeField, Tooltip("After creating all partition, will resize all side with the marginRoom. This will decreasing the size of the room")] private int marginRoom = 2;
		[SerializeField] private MinMaxFloat roomScaleSize = new MinMaxFloat(0.4f, 0.85f);

		//Delaunay
		Delaunay.Voronoi v;
		private List<LineSegment> m_spanningTree;
		private List<LineSegment> m_delaunayTriangulation;
		[SerializeField, Range(0f, .3f)] private float keepConnection = 0.15f;

		public Room SliceRoom(Room origin)
		{
			Room result = new Room(new Vector2Int((int)origin.rect.position.x, (int)origin.rect.position.y), new Vector2Int((int)origin.rect.size.x, (int)origin.rect.size.y));
			if (origin.rect.width > origin.rect.height)
			{
				result.rect.width = Mathf.RoundToInt(sliceRange.Random() * origin.rect.width);
				origin.rect.width -= result.rect.width;
				result.rect.x = origin.rect.position.x + origin.rect.width;
			}
			else
			{
				result.rect.height = Mathf.RoundToInt(sliceRange.Random() * origin.rect.height);
				origin.rect.height -= result.rect.height;
				result.rect.y = origin.rect.position.y + origin.rect.height;
			}
			return result;
		}

		public override void StartRoomGeneration()
		{
			StartPartitioning();
			RoomGeneration();
			StartTriangulation();
			StartCorridorsGeneration(rooms);
			//DoorGeneration();
			//StartCorridorsCreation(0);
		}

		void StartPartitioning()
		{
			rooms.Clear();
			rooms.Add(new Room(new Vector2Int(), boardSize));

			int i = 0;
			while (i < partitionCount - 1)
			{
				//Slice from the biggest
				int bigRoom = 0;
				for (int r = 1; r < rooms.Count; r++)
				{
					if (rooms[bigRoom].rect.size.sqrMagnitude < rooms[r].rect.size.sqrMagnitude)
						bigRoom = r;
				}
				Room ro = SliceRoom(rooms[bigRoom]);
				ro.id = i;
				rooms.Add(ro);
				i++;
			}
		}

		void RoomGeneration()
		{
			foreach(Room r in rooms)
			{
				//Add Margin
				r.rect.x += marginRoom;
				r.rect.y += marginRoom;
				r.rect.width -= (marginRoom*2);
				r.rect.height -= (marginRoom*2);

				Rect _rect = r.rect;

				_rect.width  = Mathf.RoundToInt(_rect.width * roomScaleSize.Random());
				_rect.height = Mathf.RoundToInt(_rect.height * roomScaleSize.Random());
				_rect.x += Mathf.RoundToInt((r.rect.width - _rect.width) * Random.Range(0f, 1f));
				_rect.y += Mathf.RoundToInt((r.rect.height - _rect.height) * Random.Range(0f, 1f));
				r.rect = _rect;
			}
		}

		public void StartTriangulation()
		{
			List<Vector2> m_points = rooms.Select(o => o.rect.center).ToList();
			List<uint> colors = new List<uint>();
			foreach (Vector2 v in m_points)
				colors.Add(0);

			v = new Voronoi(m_points, colors, new Rect(0, 0, -999999, 999999));

			m_delaunayTriangulation = v.DelaunayTriangulation();
			m_spanningTree = v.SpanningTree(KruskalType.MINIMUM);

			List<LineSegment> trisLeft = new List<LineSegment>();
			foreach (LineSegment d in m_delaunayTriangulation)
			{
				bool safeToAdd = true;
				foreach (LineSegment s in m_spanningTree)
				{
					if ((d.p0 == s.p0 && d.p1 == s.p1) || (d.p0 == s.p1 && d.p1 == s.p0))
					{
						safeToAdd = false;
						break;
					}
				}
				if (safeToAdd)
					trisLeft.Add(d);
			}

			trisLeft = trisLeft.OrderBy(o => (Vector2.SqrMagnitude((Vector2)(o.p0 - o.p1)))).ToList();
			for (int i = 0; i < (int)(trisLeft.Count * keepConnection); i++)
			{
				m_spanningTree.Add(trisLeft[i]);
			}

			roomConnection.Clear();
			foreach (LineSegment l in m_spanningTree)
			{
				if (roomConnection.ContainsKey(l))
					continue;

				Room r1 = null, r2 = null;
				foreach (Room r in rooms)
				{
					if (r.rect.center == l.p0)
						r1 = r;
					else if (r.rect.center == l.p1)
						r2 = r;
				}

				if (r1 == null || r2 == null)
				{
					Debug.Log("Dude, something doesn't right! Room is not detected in triangulation! Check here");
				}
				else
				{
					roomConnection.Add(l, new List<Room>(2) { r1, r2 });
				}
			}
		}
	}

}

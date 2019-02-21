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
			StartCorridorsGeneration(rooms, corridors);
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

		#region OBSOLETE
		[System.Obsolete("Use StartTriangulation instead")] private MinMaxInt branchCount = new MinMaxInt(1, 4);
		//key will be index form rooms, value will be list of index connected rooms
		[System.Obsolete("Use StartTriangulation instead")] protected Dictionary<int, List<int>> roomCorridorDict = new Dictionary<int, List<int>>();
		[System.Obsolete("Use StartTriangulation instead")] private List<Door> doors = new List<Door>();


		[System.Obsolete("Use StartTriangulation instead")]
		void DoorGeneration()
		{
			doors.Clear();
			foreach (Room r in rooms)
			{
				Door d = new Door();
				d.rect = r.rect;
				d.corridorWidth = corridorWidth;
				doors.Add(d);
			}
		}

		[System.Obsolete("Use StartTriangulation instead")]
		void StartCorridorsCreation(int fromIndex)
		{
			int closesIndex = fromIndex;

			roomCorridorDict.Clear();
			corridors.Clear();
			for(int key = 0; key < rooms.Count; key++)
			{
				int _branchCount = branchCount.Random();
				List<int> b;
				roomCorridorDict.TryGetValue(key, out b);
				if (b == null)
					b = new List<int>();
				while (_branchCount > 0)
				{
					float closesDistance = Mathf.Infinity;
					closesIndex = -1;

					bool safe = true;
					for(int i = 0; i < rooms.Count; i++)
					{
						List<int> _bb;
						roomCorridorDict.TryGetValue(i, out _bb);
						if (rooms[i] == rooms[key] || b.Contains(i))
							continue;
						if(_bb != null) {
							if (_bb.Contains(key))
								continue;
						}
						float _distance = (rooms[i].rect.center - rooms[key].rect.center).sqrMagnitude;
						if (_distance < closesDistance) {
							closesDistance = _distance;
							closesIndex = i;
						}
					}

					if (closesIndex != -1) {
						Corridor c = CreateCorridor(key, closesIndex);
						safe = true;
						//checking if this corridor not colliding with other room.
						foreach (Room _r in rooms) {
							foreach (Rect cr in c.ways)	{
								if (cr.Overlaps(_r.rect)) {
									safe = false;
									goto endloop;
								}
							}
						}
						endloop:
						if (safe)
						{
							corridors.Add(c);
							b.Add(closesIndex);
							//This will verify if the key is new
							if (b.Count == 1)
								roomCorridorDict.Add(key, b);
							else
								roomCorridorDict[key] = b;
						}
					}
					
					_branchCount--;
				}
			}
		}

		[System.Obsolete("Use StartTriangulation instead")]
		Corridor CreateCorridor(int from, int to)
		{
			//when creating corridor, check the intersection between all room. So it will be safe to say the corrider is good
			Corridor c = new Corridor();
			Vector2 dir = (rooms[to].rect.center - rooms[from].rect.center).normalized;
			float angleUp = Vector2.Angle(dir, Vector2.up);
			float angleRight = Vector2.Angle(dir, Vector2.right);

			//is Up
			if (angleUp <= 45)
			{
				c.startCorridor = doors[from].topDoorPos;
				c.endCorridor = doors[to].bottomDoorPos;

				//Create half way from start
				c.ways.Add(new Rect(c.startCorridor, new Vector2(corridorWidth, Mathf.CeilToInt((c.endCorridor.y - c.startCorridor.y) / 2f))));
				//create half way from middle to end
				c.ways.Add(new Rect(new Vector2(c.endCorridor.x, c.startCorridor.y + c.ways[0].height - 1), new Vector2(corridorWidth, (c.endCorridor.y - c.startCorridor.y - c.ways[0].height + 1))));
				//create connection for each corridor
				float _dir = c.endCorridor.x - c.startCorridor.x;
				if (_dir != 0)
				{
					//connect to right
					if (_dir > 0)
						c.ways.Add(new Rect(new Vector2(c.startCorridor.x, c.startCorridor.y + c.ways[0].height - 1), new Vector2(_dir + Mathf.Ceil(corridorWidth / 2f), corridorWidth)));
					else
						c.ways.Add(new Rect(new Vector2(c.endCorridor.x + Mathf.Floor(corridorWidth / 2f), c.startCorridor.y + c.ways[0].height - 1), new Vector2(Mathf.Abs(_dir) + Mathf.Ceil(corridorWidth / 2f), corridorWidth)));
				}
			}
			//is Down
			else if (angleUp >= 135)
			{
				c.startCorridor = doors[from].bottomDoorPos;
				c.endCorridor = doors[to].topDoorPos;

				//Create half way from bottom (end first in this case)
				c.ways.Add(new Rect(c.endCorridor, new Vector2(corridorWidth, Mathf.CeilToInt((c.startCorridor.y - c.endCorridor.y) / 2f))));
				//create half way from middle to top (start in this case)
				c.ways.Add(new Rect(new Vector2(c.startCorridor.x, c.endCorridor.y + c.ways[0].height - 1), new Vector2(corridorWidth, (c.startCorridor.y - c.endCorridor.y - c.ways[0].height + 1))));
				//create connection for each corridor
				float _dir = c.startCorridor.x - c.endCorridor.x;
				if (_dir != 0)
				{
					//connect to right
					if (_dir > 0)
						c.ways.Add(new Rect(new Vector2(c.endCorridor.x, c.endCorridor.y + c.ways[0].height - 1), new Vector2(_dir + Mathf.Ceil(corridorWidth / 2f), corridorWidth)));
					else
						c.ways.Add(new Rect(new Vector2(c.startCorridor.x + Mathf.Floor(corridorWidth / 2f), c.endCorridor.y + c.ways[0].height - 1), new Vector2(Mathf.Abs(_dir) + Mathf.Ceil(corridorWidth / 2f), corridorWidth)));
				}
			}
			else if (angleRight <= 45)
			{
				c.startCorridor = doors[from].rightDoorPos;
				c.endCorridor = doors[to].leftDoorPos;

				//Create half way from start
				c.ways.Add(new Rect(c.startCorridor, new Vector2(Mathf.CeilToInt((c.endCorridor.x - c.startCorridor.x) / 2f), corridorWidth)));
				//create half way from middle to end
				c.ways.Add(new Rect(new Vector2(c.startCorridor.x + c.ways[0].width - 1, c.endCorridor.y), new Vector2((c.endCorridor.x - c.startCorridor.x - c.ways[0].width + 1), corridorWidth)));
				//create connection for each corridor
				float _dir = c.endCorridor.y - c.startCorridor.y;
				if (_dir != 0)
				{
					//connect to right
					if (_dir > 0)
						c.ways.Add(new Rect(new Vector2(c.startCorridor.x + c.ways[0].width - 1, c.startCorridor.y), new Vector2(corridorWidth, _dir + Mathf.Ceil(corridorWidth / 2f))));
					else
						c.ways.Add(new Rect(new Vector2(c.startCorridor.x + c.ways[0].width - 1, c.endCorridor.y + Mathf.Floor(corridorWidth / 2f)), new Vector2(corridorWidth, Mathf.Abs(_dir) + Mathf.Ceil(corridorWidth / 2f))));
				}
			}
			else if (angleRight >= 135)
			{
				c.startCorridor = doors[from].leftDoorPos;
				c.endCorridor = doors[to].rightDoorPos;

				//Create half way from bottom (end first in this case)
				c.ways.Add(new Rect(c.endCorridor, new Vector2(Mathf.CeilToInt((c.startCorridor.x - c.endCorridor.x) / 2f), corridorWidth)));
				//create half way from middle to top (start in this case)
				c.ways.Add(new Rect(new Vector2(c.endCorridor.x + c.ways[0].width - 1, c.startCorridor.y), new Vector2((c.startCorridor.x - c.endCorridor.x - c.ways[0].width + 1), corridorWidth)));
				//create connection for each corridor
				float _dir = c.startCorridor.y - c.endCorridor.y;
				if (_dir != 0)
				{
					//connect to right
					if (_dir > 0)
						c.ways.Add(new Rect(new Vector2(c.endCorridor.x + c.ways[0].width - 1, c.endCorridor.y), new Vector2(corridorWidth, _dir + Mathf.Ceil(corridorWidth / 2f))));
					else
						c.ways.Add(new Rect(new Vector2(c.endCorridor.x + c.ways[0].width - 1, c.startCorridor.y + Mathf.Floor(corridorWidth / 2f)), new Vector2(corridorWidth, Mathf.Abs(_dir) + Mathf.Ceil(corridorWidth / 2f))));
				}
			}

			return c;
		}
		#endregion
	}

	public class Door {

		public Rect rect;
		public int corridorWidth;

		#region GetDoorPosition
		//0 left, 1 top, 2 right, 3 bottom
		public Vector2Int[] doorsPosition = new Vector2Int[4] { new Vector2Int(-1, -1), new Vector2Int(-1, -1), new Vector2Int(-1, -1), new Vector2Int(-1, -1) };
		public Vector2Int bottomDoorPos
		{
			get
			{
				if (doorsPosition[3] == new Vector2Int(-1, -1))
					doorsPosition[3] = new Vector2Int(Random.Range(1, (int)rect.width - (corridorWidth + 1)), 0);
				return doorsPosition[3] + new Vector2Int((int)rect.position.x, (int)rect.position.y);
			}
		}
		public Vector2Int leftDoorPos
		{
			get
			{
				if (doorsPosition[0] == new Vector2Int(-1, -1))
					doorsPosition[0] = new Vector2Int(0, Random.Range(1, (int)rect.height - (corridorWidth + 1)));
				return doorsPosition[0] + new Vector2Int((int)rect.position.x, (int)rect.position.y);
			}
		}
		public Vector2Int rightDoorPos
		{
			get
			{
				if (doorsPosition[2] == new Vector2Int(-1, -1))
					doorsPosition[2] = new Vector2Int((int)rect.width, Random.Range(1, (int)rect.height - (corridorWidth + 1)));
				return doorsPosition[2] + new Vector2Int((int)rect.position.x, (int)rect.position.y);
			}
		}
		public Vector2Int topDoorPos
		{
			get
			{
				if (doorsPosition[1] == new Vector2Int(-1, -1))
					doorsPosition[1] = new Vector2Int(Random.Range(1, (int)rect.width - (corridorWidth + 1)), (int)rect.height);
				return doorsPosition[1] + new Vector2Int((int)rect.position.x, (int)rect.position.y);
			}
		}
		#endregion

	}
}

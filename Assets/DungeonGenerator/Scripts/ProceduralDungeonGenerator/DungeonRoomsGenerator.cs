using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Delaunay.Geo;

namespace DI.DungeonGenerator
{
	public class DungeonRoomsGenerator : DungeonGenerator {
		[SerializeField] private int roomCount = 10;
		[SerializeField] private MinMaxInt roomSize = new MinMaxInt(6, 12);
		[SerializeField] private MinMaxInt spreadDistance = new MinMaxInt(1, 10);
		[HideInInspector]public List<RoomConnectionLib> roomPossibleConnections = new List<RoomConnectionLib>();
		[HideInInspector]public Dictionary<LineSegment, List<Room>> roomConnection = new Dictionary<LineSegment, List<Room>>();

#if DEBUG_MODE

		public bool drawDoorFinder = true;
		public bool drawPossibleCorridor = true;
		public bool drawPossibleLCorridor = true;
#endif

		public override void StartRoomGeneration()
		{
			base.StartRoomGeneration();
			RoomsGenerator();
			SpreadAndRePositioningAllRooms();
		}

		protected void RoomsGenerator()
		{
			rooms.Clear();
			for(int i = 0; i < roomCount; i++)
			{
				Vector2Int size = new Vector2Int(roomSize.Random(), roomSize.Random());
				Vector2 position = Random.insideUnitCircle * spreadDistance.Random();
				position -= new Vector2(size.x / 2f, size.y / 2f);
				Room r = new Room(new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y)), size);
				r.id = i;
				rooms.Add(r);
			}

			//RearrangeFromTheCenter
			rooms = rooms.OrderBy(o => o.rect.center.sqrMagnitude).ToList();
		}

		public bool SpreadAndRePositioningAllRooms()
		{
			bool safe = true;
			for(int i = 0; i < rooms.Count; i++)
			{
				Vector2 dir = Vector2.zero;
				for(int j = 0; j < i; j++)
				{
					if (rooms[i].rect.Overlaps(rooms[j].rect))
					{
						Vector2 dirV = rooms[i].rect.center - rooms[j].rect.center;
						//First step to find the direction
						if (Mathf.Abs(dirV.x) > Mathf.Abs(dirV.y))
						{
							if (Random.Range(0f, 1f) <= Mathf.Abs(dirV.y))
							{
								dirV.x = 0;
								dirV.y = Mathf.Sign(dirV.y);
							}
							else {
								dirV.x = Mathf.Sign(dirV.x);
								dirV.y = 0;
							}
						}
						else
						{
							if (Random.Range(0f, 1f) <= Mathf.Abs(dirV.x))
							{
								dirV.x = Mathf.Sign(dirV.x);
								dirV.y = 0;
							}
							else
							{
								dirV.x = 0;
								dirV.y = Mathf.Sign(dirV.y);
							}
						}
						//prevent infinite toggle movement, just let it go dude!!
						if (Vector2.Angle(dirV, dir) > 135 || Vector2.Angle(dirV, rooms[i].rect.center.normalized * -1) < 45)
							dirV *= -1;

						dir = dirV;
						Vector2 moveAmount = Vector2.zero;
						if(dir.x != 0)
							moveAmount.x = (rooms[j].rect.center.x + (dir.x * rooms[j].rect.width / 2f)) - (rooms[i].rect.center.x - (dir.x * rooms[i].rect.width / 2f));
						else
							moveAmount.y = (rooms[j].rect.center.y + (dir.y * rooms[j].rect.height / 2f)) - (rooms[i].rect.center.y - (dir.y * rooms[i].rect.height / 2f));
						rooms[i].rect.position += moveAmount;

						j = -1;
						safe = false;
					}
				}
			}
			return safe;
		}


		public void StartCorridorsGeneration(List<Room> _rooms, List<Corridor> _corridors)
		{
			StartGenerateStraightConnectionLib(_rooms);
			StraightCorridorGeneration();

			StartGenerateLConnectionLib(_rooms, _corridors);
			LCorridorGeneration();
		}

		public void StartGenerateStraightConnectionLib(List<Room> _rooms)
		{
			corridors.Clear();
			roomPossibleConnections.Clear();
			foreach (LineSegment l in roomConnection.Keys)
			{
				List<Room> connectedRoom = roomConnection[l];

				RoomConnectionLib rcl = new RoomConnectionLib(connectedRoom[0], connectedRoom[1]);
				rcl.connections = new List<RoomConnectionLib.Line>();

				int xPos = 0;
				int yPos = 0;
				//Check vertical first, will shifting x
				xPos = 0;
				while (xPos < connectedRoom[0].rect.width)
				{
					Vector2 intersection1, intersection2;
					Vector2 p1, p2;
					p1 = connectedRoom[0].rect.position + new Vector2(xPos, yPos) + Vector2.one * 0.1f;
					p2 = new Vector2(connectedRoom[0].rect.x, connectedRoom[1].rect.y) + new Vector2(xPos, 0) + Vector2.one * 0.1f;
					if (connectedRoom[1].Intersect(p1, p2, out intersection2))
					{
						if (connectedRoom[0].Intersect(p1, p2, out intersection1))
							rcl.connections.Add(new RoomConnectionLib.Line(intersection1, intersection2));
						rcl.corridorDir = (RoomConnectionLib.CorridorDir)1;

						//will check if any room between connected room
						foreach (Room r in _rooms)
						{
							if (r == connectedRoom[0] || r == connectedRoom[1])
								continue;

							if ((Mathf.Max(connectedRoom[0].position.y, connectedRoom[1].position.y + connectedRoom[1].size.y) >= r.position.y + r.size.y)
								&& (Mathf.Min(connectedRoom[0].position.y + connectedRoom[0].size.y, connectedRoom[1].position.y) <= r.position.y))
							{
								Vector2 tempIntersect;
								if (r.Intersect(p1, p2, out tempIntersect))
								{
									Debug.Log("Dude, that was a room between connected room, now I will delete that connection for you <3");
									rcl.connections.RemoveAt(rcl.connections.Count - 1);
									break;
								}
							}
						}
					}

					xPos += 1;
				}

				//if no intersection when vertical checking, check with horizontal
				if (rcl.connections.Count == 0)
				{
					xPos = 0;
					yPos = 0;
					//Check horizontal, will shifting y
					while (yPos < connectedRoom[0].rect.height)
					{
						Vector2 intersection1, intersection2;
						Vector2 p1, p2;
						p1 = connectedRoom[0].rect.position + new Vector2(xPos, yPos) + Vector2.one * 0.1f;
						p2 = new Vector2(connectedRoom[1].rect.x, connectedRoom[0].rect.y) + new Vector2(0, yPos) + Vector2.one * 0.1f;
						if (connectedRoom[1].Intersect(p1, p2, out intersection2))
						{
							if (connectedRoom[0].Intersect(p1, p2, out intersection1))
								rcl.connections.Add(new RoomConnectionLib.Line(intersection1, intersection2));
							rcl.corridorDir = (RoomConnectionLib.CorridorDir)2;

							//will check if any room between connected room
							foreach (Room r in _rooms)
							{
								if (r == connectedRoom[0] || r == connectedRoom[1])
									continue;

								if ((Mathf.Max(connectedRoom[0].position.x, connectedRoom[1].position.x + connectedRoom[1].size.x) >= r.position.x + r.size.x)
									&& (Mathf.Min(connectedRoom[0].position.x + connectedRoom[0].size.x, connectedRoom[1].position.x) <= r.position.x))
								{
									Vector2 tempIntersect;
									if (r.Intersect(p1, p2, out tempIntersect))
									{
										Debug.Log("Dude, that was a room between connected room, now I will delete that connection for you <3");
										rcl.connections.RemoveAt(rcl.connections.Count - 1);
										break;
									}
								}
							}
						}

						yPos += 1;
					}
				}

				//Remove first and last possible connection, why?? we don't want generate corridor at the edge / corner
				if (rcl.connections.Count > 0)
				{
					rcl.connections.RemoveAt(0);
					if (rcl.connections.Count > 0)
					{
						rcl.connections.RemoveAt(rcl.connections.Count - 1);
					}
					//Also we need adjust the possible connection with the width of the corridor.
					//the formula is remove list from behind until (corridorSize - 1)
					int possibleSize = rcl.connections.Count;
					for (int i = 0; i < corridorWidth - 1; i++)
					{
						if (rcl.connections.Count > 0)
						{
							rcl.connections.RemoveAt(rcl.connections.Count - 1);
						}
						else
						{
							Debug.LogWarning("Connection between Room : " + connectedRoom[0].id + " and Room : " + connectedRoom[1].id + " are imposible!! Corridor size is bigger than possible size between room : " + possibleSize);
							break;
						}
					}
				}
				roomPossibleConnections.Add(rcl);
			}
		}

		public void StartGenerateLConnectionLib(List<Room> _rooms, List<Corridor> _corridors)
		{
			for (int i = 0; i < roomPossibleConnections.Count; i++)
			{
				RoomConnectionLib rcl = roomPossibleConnections[i];
				StartGenerateLConnectionBetweenRoom(rcl.r1, rcl.r2, _rooms, _corridors, ref rcl);
			}
		}

		void StartGenerateLConnectionBetweenRoom(Room r1, Room r2, List<Room> _rooms, List<Corridor> _corridors, ref RoomConnectionLib rcl)
		{
			//Condition if L Connection is the only options!!
			if (rcl.connections.Count == 0)
			{
				//yuhu L fucking shit algorithm so fuck shit I hate you so much!!!@ fuck offf!!!!!!!!!!!!!!!!! BEGONE!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
				Vector2 r1Direction = Vector2.zero;
				Vector2 r2Direction = Vector2.zero;
				Vector2 lR1 = Vector2.zero;
				Vector2 lR2 = Vector2.zero;
				if (r1.rect.x < r2.rect.x)
				{
					r1Direction.x = 1;
					r2Direction.x = -1;
					lR1.x = Mathf.Abs(r2.rect.x + r2.rect.width - r1.rect.x);
					lR2.x = Mathf.Abs(r2.rect.x - r1.rect.x);
				}
				else
				{
					if (r1.rect.x == r2.rect.x)
						Debug.LogWarning("Still no implementation for this shit! Open this warning to know what happen");

					r1Direction.x = -1;
					r2Direction.x = 1;
					lR1.x = Mathf.Abs(r1.rect.x - r2.rect.x);
					lR2.x = Mathf.Abs(r1.rect.x + r1.rect.width - r2.rect.x);
				}

				if (r1.rect.y < r2.rect.y)
				{
					r1Direction.y = 1;
					r2Direction.y = -1;
					lR1.y = Mathf.Abs(r2.rect.y + r2.rect.height - r1.rect.y);
					lR2.y = Mathf.Abs(r2.rect.y - r1.rect.y);
				}
				else
				{
					if (r1.rect.y == r2.rect.y)
						Debug.LogWarning("Still no implementation for this shit! Open this warning to know what happen");

					r1Direction.y = -1;
					r2Direction.y = 1;
					lR1.y = Mathf.Abs(r1.rect.y - r2.rect.y);
					lR2.y = Mathf.Abs(r1.rect.y + r1.rect.height - r2.rect.y);
				}

				CreateLConnectionLib(r1, r2, lR1.x * r1Direction.x, lR2.y * r2Direction.y, _rooms, _corridors, ref rcl);
				CreateLConnectionLib(r2, r1, lR2.x * r2Direction.x, lR1.y * r1Direction.y, _rooms, _corridors, ref rcl);
			}
		}

		void CreateLConnectionLib(Room r1, Room r2, float width, float height, List<Room> _rooms, List<Corridor> _corridors, ref RoomConnectionLib rcl)
		{
			int startIndex = rcl.lConnections.Count;
			List<int> sizeEachColumn = new List<int>();
			//loop vertical (height from room 1)
			for (int y = 0; y < r1.rect.height; y++)
			{
				Vector2 start = new Vector2(r1.rect.x, y + r1.rect.y + 0.1f);
				if (width > 0)
					start.x = r1.rect.x + r1.rect.width - 0.1f;
				Vector2 end = new Vector2(r1.rect.x + width + 0.5f * Mathf.Sign(width), y + r1.rect.y + 0.1f);
				RoomConnectionLib.Line l1;
				if (IsLineCollideWithRoomsOrCorridor(start, end, out l1, _rooms, _corridors, r1))
				{
					//Make it imposible to do corridor
					if (l1.Length < corridorWidth)
						l1.v2 = l1.v1;
					//if length less than 0.3f, just let it go
					if (l1.Length < 0.3f)
						continue;
				}
				else l1 = new RoomConnectionLib.Line(start, end);

				int w = 0;
				//loop horizontal (width from room 2)
				//find connection
				for (int x = 0; x < r2.rect.width; x++)
				{
					start.x = r2.rect.x + x + 0.1f;
					start.y = r2.rect.y + 0.1f;
					if (height > 0)
						start.y = r2.rect.y + r2.rect.height - 0.1f;
					end.x = start.x;
					end.y = start.y + height + 0.5f * Mathf.Sign(height);
					RoomConnectionLib.Line l2;
					if (IsLineCollideWithRoomsOrCorridor(start, end, out l2, _rooms, _corridors, r2))
					{
						//Make it imposible to do corridor
						if (l2.Length < corridorWidth)
							l2.v2 = l2.v1;
					}
					else l2 = new RoomConnectionLib.Line(start, end);


					//if 2 line intersection, then this will become corridor
					Vector2 itsc;
					if (LineSegmentsIntersection.Math2d.LineSegmentsIntersection(l2.v1, l2.v2, l1.v1, l1.v2, out itsc))
					{
						//ignore first connection, it will result good connection
						if (w != 0)
						{
							RoomConnectionLib.LConnection lCon = new RoomConnectionLib.LConnection();
							l1.v1.x = Mathf.Round(l1.v1.x);
							l1.v1.y = Mathf.Round(l1.v1.y);
							l2.v1.x = Mathf.Round(l2.v1.x);
							l2.v1.y = Mathf.Round(l2.v1.y);
							itsc.x = Mathf.Round(itsc.x);
							itsc.y = Mathf.Round(itsc.y);
							lCon.l1 = new RoomConnectionLib.Line(l1.v1, itsc);
							lCon.l2 = new RoomConnectionLib.Line(l2.v1, itsc);
							rcl.lConnections.Add(lCon);
						}
						w++;
					}
				}
				//remove the last possible connection(width)
				for (int i = 0; i < corridorWidth + 1; i++)
				{
					if (rcl.lConnections.Count == 0 || w <= 0)
						break;
					rcl.lConnections.RemoveAt(rcl.lConnections.Count - 1);
					w--;
				}
				if (w != 0)
					sizeEachColumn.Add(w - 1);
			}

			//remove the last possible connection(height)
			for (int i = 0; i < corridorWidth + 1; i++)
			{
				if (sizeEachColumn.Count - 1 - i < 0)
					break;
				int del = sizeEachColumn[sizeEachColumn.Count - 1 - i];
				for (int j = 0; j < del; j++)
				{
					if (rcl.lConnections.Count == 0)
						break;
					rcl.lConnections.RemoveAt(rcl.lConnections.Count - 1);
				}
			}

			//remove the first row, this will be resulting good connection
			if (sizeEachColumn.Count > 0)
			{
				for (int i = 0; i < sizeEachColumn[0]; i++)
				{
					if (rcl.lConnections.Count <= startIndex)
						break;
					rcl.lConnections.RemoveAt(startIndex);
				}
			}
		}

		public void StraightCorridorGeneration()
		{
			int i = 0;
			foreach (RoomConnectionLib rcl in roomPossibleConnections)
			{
				Corridor corridor = new Corridor();
				//Get one of the door
				if (rcl.connections.Count != 0)
				{
					int choosenIndex = Random.Range(0, rcl.connections.Count);
					RoomConnectionLib.Line con = rcl.connections[choosenIndex];
					corridor.doorPosition[0] = new Vector2Int(Mathf.RoundToInt(con.v1.x), Mathf.RoundToInt(con.v1.y));
					corridor.doorPosition[1] = new Vector2Int(Mathf.RoundToInt(con.v2.x), Mathf.RoundToInt(con.v2.y));

					if (corridor.doorPosition[0] != corridor.doorPosition[1])
					{

						//Create way corridor
						if (rcl.corridorDir == RoomConnectionLib.CorridorDir.Vertical)
							corridor.ways.Add(new Rect(corridor.doorPosition[0], new Vector2(corridorWidth, corridor.doorPosition[1].y - corridor.doorPosition[0].y)));
						else if (rcl.corridorDir == RoomConnectionLib.CorridorDir.Horizontal)
							corridor.ways.Add(new Rect(corridor.doorPosition[0], new Vector2(corridor.doorPosition[1].x - corridor.doorPosition[0].x, corridorWidth)));
					}
				}
				if (corridors.Count != roomPossibleConnections.Count)
					corridors.Add(corridor);
				else
					corridors[i] = corridor;
				i++;
			}
		}

		public void LCorridorGeneration()
		{
			int i = 0;
			foreach (RoomConnectionLib rcl in roomPossibleConnections)
			{
				Corridor corridor = new Corridor();
				//Create the letter L corridor
				rcl.corridorDir = RoomConnectionLib.CorridorDir.L;
				if (rcl.lConnections.Count != 0)
				{
					int choosenIndex = Random.Range(0, rcl.lConnections.Count);
					RoomConnectionLib.LConnection lcon = rcl.lConnections[choosenIndex];
					corridor.doorPosition[0] = new Vector2Int(Mathf.RoundToInt(lcon.l1.v1.x), Mathf.RoundToInt(lcon.l1.v1.y));
					corridor.doorPosition[1] = new Vector2Int(Mathf.RoundToInt(lcon.l2.v1.x), Mathf.RoundToInt(lcon.l2.v1.y));
					corridor.ways.Add(new Rect(corridor.doorPosition[0], (lcon.l1.v1.x == lcon.l1.v2.x ? new Vector2(corridorWidth, lcon.l1.v2.y - lcon.l1.v1.y) : new Vector2(lcon.l1.v2.x - lcon.l1.v1.x, corridorWidth))));
					corridor.ways.Add(new Rect(corridor.doorPosition[1], (lcon.l2.v1.x == lcon.l2.v2.x ? new Vector2(corridorWidth, lcon.l2.v2.y - lcon.l2.v1.y) : new Vector2(lcon.l2.v2.x - lcon.l2.v1.x, corridorWidth))));


					//Fix the minor bug
					// |__
					// will result colliding within corridor
					if (lcon.l1.v1.y > lcon.l1.v2.y && lcon.l2.v1.x > lcon.l2.v2.x)
					{
						Rect r = corridor.ways[0];
						r.height += corridorWidth;
						corridor.ways[0] = r;
					}
					else if (lcon.l2.v1.y > lcon.l2.v2.y && lcon.l1.v1.x > lcon.l1.v2.x)
					{
						Rect r = corridor.ways[1];
						r.height += corridorWidth;
						corridor.ways[1] = r;
					}
					//  __
					// |
					// will result no connection
					if (lcon.l1.v1.x < lcon.l1.v2.x && lcon.l2.v1.y < lcon.l2.v2.y)
					{
						Rect r = corridor.ways[0];
						r.width += corridorWidth;
						corridor.ways[0] = r;
					}
					else if (lcon.l2.v1.x < lcon.l2.v2.x && lcon.l1.v1.y < lcon.l1.v2.y)
					{
						Rect r = corridor.ways[1];
						r.width += corridorWidth;
						corridor.ways[1] = r;
					}

					if (corridors.Count != roomPossibleConnections.Count)
						corridors.Add(corridor);
					else
						corridors[i] = corridor;
				}
				i++;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="line">Resulted line after intersection</param>
		/// <param name="_rooms"></param>
		/// <param name="_corridors"></param>
		/// <param name="exclude"></param>
		/// <param name="alsoCheckCorridoor"></param>
		/// <returns></returns>
		bool IsLineCollideWithRoomsOrCorridor(Vector2 start, Vector2 end, out RoomConnectionLib.Line line, List<Room> _rooms, List<Corridor> _corridors, Room exclude = null, bool alsoCheckCorridoor = false)
		{
			line = new RoomConnectionLib.Line(start, end);
			foreach (Room r in _rooms)
			{
				if (r == exclude)
					continue;
				Vector2 p;
				if (r.Intersect(start, end, out p))
				{
					line.v2 = p;
					return true;
				}
			}
			if (alsoCheckCorridoor)
			{
				foreach (Corridor c in _corridors)
				{
					Vector2 p;
					if (c.Intersect(start, end, out p))
					{
						line.v2 = p;
						return true;
					}
				}
			}
			return false;
		}

#if UNITY_EDITOR
#if DEBUG_MODE
		protected override void OnDrawGizmosSelected()
		{
			base.OnDrawGizmosSelected();
			foreach (Corridor c in corridors)
			{
				if (c.ways.Count == 0)
				{
					Gizmos.color = Color.yellow;
					Vector2 bottomLeft = c.doorPosition[0];
					Vector2 topRight = c.doorPosition[0] + Vector2.right;
					Vector2 topLeft = c.doorPosition[0] + Vector2.up;
					Vector2 bottomRight = c.doorPosition[0] + Vector2.one;

					Gizmos.DrawLine(bottomLeft, bottomRight);
					Gizmos.DrawLine(bottomLeft, topLeft);
					Gizmos.DrawLine(topRight, topLeft);
					Gizmos.DrawLine(topRight, bottomRight);

					Gizmos.color = Color.blue;
					bottomLeft = c.doorPosition[1];
					topRight = c.doorPosition[1] + Vector2.right;
					topLeft = c.doorPosition[1] + Vector2.up;
					bottomRight = c.doorPosition[1] + Vector2.one;

					Gizmos.DrawLine(bottomLeft, bottomRight);
					Gizmos.DrawLine(bottomLeft, topLeft);
					Gizmos.DrawLine(topRight, topLeft);
					Gizmos.DrawLine(topRight, bottomRight);
				}
			}
			Gizmos.color = Color.red;
			foreach (LineSegment l in roomConnection.Keys)
			{
				if (!drawDoorFinder)
					break;
				List<Room> connectedRoom = roomConnection[l];
				List<Vector2> possiblePosition = new List<Vector2>();
				int xPos = 0;
				int yPos = 0;
				while (xPos < connectedRoom[0].rect.width)
				{
					Vector2 p1, p2;
					p1 = connectedRoom[0].rect.position + new Vector2(xPos, yPos) + Vector2.one * 0.5f;
					p2 = new Vector2(connectedRoom[0].rect.x, connectedRoom[1].rect.y) + new Vector2(xPos, 0) + Vector2.one * 0.5f;
					Gizmos.DrawLine(p1, p2);
					xPos += 1;
				}


				xPos = 0;
				yPos = 0;
				//Check horizontal, will shifting y
				while (yPos < connectedRoom[0].rect.height)
				{
					Vector2 p1, p2;
					p1 = connectedRoom[0].rect.position + new Vector2(xPos, yPos) + Vector2.one * 0.5f;
					p2 = new Vector2(connectedRoom[1].rect.x, connectedRoom[0].rect.y) + new Vector2(0, yPos) + Vector2.one * 0.5f;
					Gizmos.DrawLine(p1, p2);
					yPos += 1;
				}


			}
			foreach (Corridor r in corridors)
			{
				Gizmos.color = Color.red;
				foreach (Rect cr in r.ways)
				{
					Vector2 bottomLeft = cr.position;
					Vector2 bottomRight = cr.position + Vector2.right * cr.width;
					Vector2 topLeft = cr.position + Vector2.up * cr.height;
					Vector2 topRight = cr.position + cr.size;

					Gizmos.DrawLine(bottomLeft, bottomRight);
					Gizmos.DrawLine(bottomLeft, topLeft);
					Gizmos.DrawLine(topRight, topLeft);
					Gizmos.DrawLine(topRight, bottomRight);
				}
				Gizmos.color = Color.cyan;
				Gizmos.DrawLine(r.doorPosition[0] - new Vector2(0.5f, -0.5f), r.doorPosition[0] + Vector2.one * 0.5f);
				Gizmos.DrawLine((Vector2)r.doorPosition[0], r.doorPosition[0] + Vector2.up);

				Gizmos.color = Color.green;
				Gizmos.DrawLine(r.doorPosition[1] - new Vector2(0.5f, -0.5f), r.doorPosition[1] + Vector2.one * 0.5f);
				Gizmos.DrawLine((Vector2)r.doorPosition[1], r.doorPosition[1] + Vector2.up);
			}

			Gizmos.color = Color.yellow;
			if (drawPossibleCorridor)
			{
				foreach (RoomConnectionLib rcl in roomPossibleConnections)
				{
					foreach (RoomConnectionLib.Line c in rcl.connections)
						Gizmos.DrawLine(c.v1, c.v2);
				}
			}
			if (drawPossibleLCorridor)
			{
				foreach (RoomConnectionLib rcl in roomPossibleConnections)
				{
					if (rcl.lConnections == null)
						continue;
					foreach (RoomConnectionLib.LConnection c in rcl.lConnections)
					{
						Gizmos.DrawLine(c.l1.v1, c.l1.v2);
						Gizmos.DrawLine(c.l2.v1, c.l2.v2);
					}
				}

			}
		}

#endif

#endif

	}

	[System.Serializable]
	public class RoomConnectionLib
	{
		public Room r1, r2;
		//0 : no direction, 1 : vertical direction, 2 : horizontal direction
		public CorridorDir corridorDir = 0;
		public enum CorridorDir
		{
			None = 0, Vertical = 1, Horizontal = 2, L = 3
		}
		public List<Line> connections = new List<Line>();
		public List<LConnection> lConnections = new List<LConnection>();

		public RoomConnectionLib(Room r1, Room r2)
		{
			this.r1 = r1;
			this.r2 = r2;
		}

		public class Line
		{
			public Vector2 v1, v2;
			public Line(Vector2 v1, Vector2 v2)
			{
				this.v1 = v1;
				this.v2 = v2;
			}
			public float Length
			{
				get { return (v2 - v1).magnitude; }
			}
		}

		public class LConnection
		{
			public Line l1, l2;
		}
	}
}

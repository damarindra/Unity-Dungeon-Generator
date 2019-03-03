using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LineSegmentsIntersection;

namespace DI.DungeonGenerator
{
	[System.Serializable]
	public class Room
	{
		public int id;
		public Rect rect;
		public List<Door> doorPositions = new List<Door>();

		public Room() { }
		public Room(Vector2Int position, Vector2Int size)
		{
			rect.size = size;
			rect.position = position;
		}

		public Vector2 size
		{
			get { return rect.size; }
		}

		public Vector2 position
		{
			get { return rect.position; }
		}

		public bool Overlap(Rect r)
		{
			return rect.Overlaps(r);
		}
		
		[System.Obsolete("Use Rect.Intersect()")]
		public bool Intersect(Vector2 p1, Vector2 p2, out List<Vector2> intersection)
		{
			intersection = new List<Vector2>();
			Vector2 r;
			if(Math2d.LineSegmentsIntersection(p1, p2, rect.position, rect.position + Vector2.up * rect.height, out r))
				intersection.Add(r);
			if (Math2d.LineSegmentsIntersection(p1, p2, rect.position, rect.position + Vector2.right * rect.width, out r))
				intersection.Add(r);
			if (Math2d.LineSegmentsIntersection(p1, p2, rect.position + rect.size, rect.position + Vector2.up * rect.height, out r))
				intersection.Add(r);
			if (Math2d.LineSegmentsIntersection(p1, p2, rect.position + rect.size, rect.position + Vector2.right * rect.width, out r))
				intersection.Add(r);

			return intersection.Count != 0;
		}
	}

	[System.Serializable]
	public class Door
	{
		public Vector2Int position
		{
			get
			{
				return new Vector2Int((int)doorRect.position.x, (int)doorRect.position.y);
			}
			set
			{
				doorRect.position = value;
			}
		}
		public Vector2Int size
		{
			get { return new Vector2Int((int)doorRect.size.x, (int)doorRect.size.y); }
			set { doorRect.size = value; }
		}
		private Rect doorRect;
		public Rect corridor;
		//0 : no direction, 1 : vertical direction, 2 : horizontal direction
		public CorridorDir corridorDir = 0;
	}

	public static class RectExtension
	{
		public static bool Intersect(this Rect _r, Vector2 p1, Vector2 p2, out Vector2 point)
		{
			bool result = false;
			point = new Vector2();
			if (Math2d.LineSegmentsIntersection(p1, p2, _r.position, _r.position + Vector2.up * _r.height, out point))
			{
				result = true;
				p2 = point;
			}
			if (Math2d.LineSegmentsIntersection(p1, p2, _r.position, _r.position + Vector2.right * _r.width, out point))
			{
				result = true;
				p2 = point;
			}
			if (Math2d.LineSegmentsIntersection(p1, p2, _r.position + Vector2.up * _r.height, _r.position + _r.size, out point))
			{
				result = true;
				p2 = point;
			}
			if (Math2d.LineSegmentsIntersection(p1, p2, _r.position + Vector2.right * _r.width, _r.position + _r.size, out point))
			{
				result = true;
				p2 = point;
			}
			point = p2;
			return result;
		}
	}

	/*
	[System.Serializable]
	public class Corridor
	{
		public Vector2Int startCorridor { get { return doorPosition[0]; } set { doorPosition[0] = value; } }
		public Vector2Int endCorridor { get { return doorPosition[1]; } set { doorPosition[1] = value; } }
		public Vector2Int[] doorPosition = new Vector2Int[2] { new Vector2Int(-1, -1), new Vector2Int(-1, -1) };
		public List<Rect> ways = new List<Rect>();

		public bool Intersect(Vector2 p1, Vector2 p2, out Vector2 point)
		{
			bool result = false;
			point = new Vector2();
			foreach(Rect rect in ways)
			{
				Vector2 p = new Vector2(Mathf.Infinity, Mathf.Infinity);
				if (Math2d.LineSegmentsIntersection(p1, p2, rect.position, rect.position + Vector2.up * rect.height, out point))
				{
					result = true;
					if (p.sqrMagnitude > point.sqrMagnitude)
						p = point;
				}
				if (Math2d.LineSegmentsIntersection(p1, p2, rect.position, rect.position + Vector2.right * rect.width, out point))
				{
					result = true;
					if (p.sqrMagnitude > point.sqrMagnitude)
						p = point;
				}
				if (Math2d.LineSegmentsIntersection(p1, p2, rect.position + rect.size, rect.position + Vector2.up * rect.height, out point))
				{
					result = true;
					if (p.sqrMagnitude > point.sqrMagnitude)
						p = point;
				}
				if (Math2d.LineSegmentsIntersection(p1, p2, rect.position + rect.size, rect.position + Vector2.right * rect.width, out point))
				{
					result = true;
					if (p.sqrMagnitude > point.sqrMagnitude)
						p = point;
				}
				point = p;
			}
			return result;
		}
	}*/

}
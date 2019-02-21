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
		public bool Intersect(Vector2 p1, Vector2 p2, out Vector2 point)
		{
			point = new Vector2();
			if (Math2d.LineSegmentsIntersection(p1, p2, rect.position, rect.position + Vector2.up * rect.height, out point))
				return true;
			else if (Math2d.LineSegmentsIntersection(p1, p2, rect.position, rect.position + Vector2.right * rect.width, out point))
				return true;
			else if (Math2d.LineSegmentsIntersection(p1, p2, rect.position + rect.size, rect.position + Vector2.up * rect.height, out point))
				return true;
			else if (Math2d.LineSegmentsIntersection(p1, p2, rect.position + rect.size, rect.position + Vector2.right * rect.width, out point))
				return true;

			return false;
		}
	}

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
	}

}
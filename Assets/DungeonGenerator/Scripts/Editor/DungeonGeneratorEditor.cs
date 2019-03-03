using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DI.DungeonGenerator {
	[CustomEditor(typeof(BSPDungeonGeneration), true)]
	public class BSPDungeonGeneratorEditor : DungeonGeneratorEditor
	{
		BSPDungeonGeneration drg { get { return (BSPDungeonGeneration)target; } }
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			if (GUILayout.Button("Triangulate Delaunay"))
			{
				drg.StartTriangulation();
			}
			if (GUILayout.Button("Generate Straight Corridor Lib"))
			{
				drg.StartGenerateStraightConnectionLib(drg.rooms);
			}
			if (GUILayout.Button("Generate Straight Corridor"))
			{
				drg.StraightCorridorGeneration();
			}
			if (GUILayout.Button("Generate L Corridor Lib"))
			{
				drg.StartGenerateLConnectionLib(drg.rooms);
			}
			if (GUILayout.Button("Generate L Corridor"))
			{
				drg.LCorridorGeneration();
			}
		}
	}

	[CustomEditor(typeof(DungeonGenerator), true)]
	public class DungeonGeneratorEditor : Editor
	{
		DungeonGenerator dg { get { return (DungeonGenerator)target; } }

		public override void OnInspectorGUI()
		{
			if (GUILayout.Button("Generate"))
			{
				dg.StartRoomGeneration();
			}
			if (GUILayout.Button("Toggle Gizmos"))
			{
				dg.drawGizmos = !dg.drawGizmos;
			}
			if(GUILayout.Button("Test Math"))
			{
				Vector2 start1 = new Vector2(-11, -8), end1 = new Vector2(-4, -8);
				Vector2 pout;
				Debug.Log(LineSegmentsIntersection(start1, end1, start1 + new Vector2(0.1f, -0.1f), start1 + Vector2.up, out pout));
			}
			/*
			if (GUILayout.Button("Visualize With Collider"))
			{
				dg.CreateColliderVisual();
			}
			if (GUILayout.Button("Remove All Collider Visualization"))
			{
				dg.DeleteAllColliderVisual();
			}*/
			base.OnInspectorGUI();
		}

		public bool LineSegmentsIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector3 p4, out Vector2 intersection)
		{
			intersection = Vector2.zero;

			var d = (p2.x - p1.x) * (p4.y - p3.y) - (p2.y - p1.y) * (p4.x - p3.x);

			if (d == 0.0f)
			{
				return false;
			}

			var u = ((p3.x - p1.x) * (p4.y - p3.y) - (p3.y - p1.y) * (p4.x - p3.x)) / d;
			var v = ((p3.x - p1.x) * (p2.y - p1.y) - (p3.y - p1.y) * (p2.x - p1.x)) / d;

			if (u < 0.0f || u > 1.0f || v < 0.0f || v > 1.0f)
			{
				return false;
			}

			intersection.x = p1.x + u * (p2.x - p1.x);
			intersection.y = p1.y + u * (p2.y - p1.y);

			return true;
		}

	}

	[CustomEditor(typeof(DungeonRoomsGenerator), true)]
	public class DungeonRoomGeneratorEditor : DungeonGeneratorEditor
	{
		DungeonRoomsGenerator drg { get { return (DungeonRoomsGenerator)target; } }
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
		}
	}

	[CustomEditor(typeof(TilemapBSPDriver))]
	public class TilemapBSPDriverEditor : Editor
	{
		TilemapBSPDriver t { get { return (TilemapBSPDriver)target; } }

		public override void OnInspectorGUI()
		{
			if(GUILayout.Button("Apply Tile"))
			{
				t.ApplyTile();
			}
			if(GUILayout.Button("Remove All Tile"))
			{
				t.RemoveTiles();
			}
			base.OnInspectorGUI();
		}
	}

	[CustomEditor(typeof(DelaunayDungeonGenerator))]
	public class DelaunayDungeonGeneratorEditor : Editor
	{
		DelaunayDungeonGenerator ddg { get { return (DelaunayDungeonGenerator)target; } }

		public override void OnInspectorGUI()
		{
			if (GUILayout.Button("Complete Actions"))
			{
				ddg.DoGeneration();
			}
			if (GUILayout.Button("Triangulate Delaunay"))
			{
				ddg.StartTriangulation();
			}
			if (GUILayout.Button("Create Corridors"))
			{
				ddg.dg.StartCorridorsGeneration(ddg.mainRooms);
			}
			if (GUILayout.Button("Generate Straight Corridor Lib"))
			{
				ddg.dg.StartGenerateStraightConnectionLib(ddg.mainRooms);
			}
			if (GUILayout.Button("Generate Straight Corridor"))
			{
				ddg.dg.StraightCorridorGeneration();
			}
			if (GUILayout.Button("Generate L Corridor Lib"))
			{
				ddg.dg.StartGenerateLConnectionLib(ddg.mainRooms);
			}
			if (GUILayout.Button("Generate L Corridor"))
			{
				ddg.dg.LCorridorGeneration();
			}
			base.OnInspectorGUI();
		}
	}
}

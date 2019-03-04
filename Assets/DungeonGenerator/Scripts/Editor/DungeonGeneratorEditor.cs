using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

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
			if(GUILayout.Button("Toggle Debug Mode"))
			{
				string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
				List<string> allDefines = definesString.Split(';').ToList();
				if (allDefines.Contains("DEBUG_MODE"))
				{
					allDefines.Remove("DEBUG_MODE");
				}
				else
				{
					allDefines.Add("DEBUG_MODE");
				}
				PlayerSettings.SetScriptingDefineSymbolsForGroup(
					EditorUserBuildSettings.selectedBuildTargetGroup,
					string.Join(";", allDefines.ToArray()));
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DI.DungeonGenerator
{
	public class NodeGraphDungeonWindow : EditorWindow {

		private GUIStyle nodeStyle;
		
		private Vector2 offset;

		private INodeGraph ngp {
			get {
				if (Selection.activeObject != null) {
					if(Selection.activeObject.GetType().GetInterface("INodeGraph") == typeof(INodeGraph))
						return Selection.activeObject as INodeGraph;
				}
				return null;
			}
		}
		public static NodeGraphDungeonWindow Instance
		{
			get { return GetWindow<NodeGraphDungeonWindow>(); }
		}
		public static Rect GetSize
		{
			get { return Instance.position; }
		}

		[MenuItem("Window/Node Graph Dungeon Editor")]
		private static void OpenWindow()
		{
			NodeGraphDungeonWindow window = GetWindow<NodeGraphDungeonWindow>();
			window.titleContent = new GUIContent("Node Graph Dungeon Editor");
		}
		
		private void OnEnable()
		{
			nodeStyle = new GUIStyle();
			nodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
			nodeStyle.border = new RectOffset(12, 12, 12, 12);
		}

		private void OnGUI()
		{
			DrawGrid(20, 0.2f, Color.gray);
			DrawGrid(100, 0.4f, Color.gray);
			//float toolbarHeight = EditorStyles.toolbar.CalcHeight(new GUIContent(""), 50);
			
			DrawNodes();

			EditorGUILayout.LabelField("", EditorStyles.toolbar);
			ProcessEvents(Event.current);
			if (GUI.changed)
				Repaint();
		}

		private void DrawNodes()
		{
			if (ngp == null)
				return;
			Debug.Log((NodeTilemap)ngp.GetNodes()[0]);
			foreach(Node n in ngp.GetNodes())
			{
				GUI.Box(new Rect(n.rectPosition.position + offset, n.rectPosition.size), n.id.ToString(), nodeStyle);
				n.DrawConnection(ngp.GetNodes(), offset);
			}
		}

		private void ProcessEvents(Event e)
		{
			switch (e.type)
			{
				case EventType.MouseDrag:
					if (e.button == 2)
					{
						offset += e.delta;
						GUI.changed = true;
					}
					break;
			}
		}

		private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
		{
			int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
			int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

			Handles.BeginGUI();
			Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

			Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);

			for (int i = 0; i < widthDivs; i++)
			{
				Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
			}

			for (int j = 0; j < heightDivs; j++)
			{
				Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width, gridSpacing * j, 0f) + newOffset);
			}

			Handles.color = Color.white;
			Handles.EndGUI();
		}
	}
}

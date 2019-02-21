using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DI.DungeonGenerator {
	[CustomEditor(typeof(NodeGraphGenerator), true)]
	public class NodeGraphGeneratorEditor : Editor
	{
		NodeGraphGenerator ngp { get { return (NodeGraphGenerator)target; } }

		public override void OnInspectorGUI()
		{
			if (GUILayout.Button("Generate Nodes"))
			{
				ngp.StartGenerationSequence();
				EditorUtility.SetDirty(ngp);
				if(NodeGraphDungeonWindow.Instance != null)
					NodeGraphDungeonWindow.Instance.Repaint();
			}
			base.OnInspectorGUI();
		}
	}
}

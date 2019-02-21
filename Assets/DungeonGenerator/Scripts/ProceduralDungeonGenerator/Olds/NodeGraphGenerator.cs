using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DI.DungeonGenerator
{
	public interface INodeGraph
	{
		Node[] GetNodes();
	}

	[System.Serializable]
	[CreateAssetMenu(fileName = "DungeonNodes", menuName = "DI/Dungeon/Node")]
	public class NodeGraphGenerator : ScriptableObject, INodeGraph {

		[SerializeField] protected int nodeCount = 10;
		[SerializeField] int maxBranchCount = 3;
		[SerializeField] bool randomConnectEndBranch = false;
		[Range(0.0f, 1.0f), SerializeField] float chanceConnectEndBranch = 0.5f;
		public Node[] nodes;
		
		public virtual void StartGenerationSequence()
		{
			GenerateNodes();
			Rect rect = new Rect(0, 0, 60, 60);
			nodes[0].CalculatePosition(ref rect, new Vector2Int(120, 80), nodes);
			AddRandomizeEndBranchConnection();
		}

		public void GenerateNodes()
		{
			nodes = new Node[nodeCount];
			for (int i = 0; i < nodeCount; i++)
				nodes[i] = new Node(i);

			int currentIdx = 0;
			int indexUsed = 0;
			while (indexUsed < nodeCount -1)
			{
				int branch = Random.Range(1, maxBranchCount + 1);
				if (branch + indexUsed >= nodeCount)
					branch = nodeCount - indexUsed - 1;
				int tempBranch = branch;
				while (branch > 0) {
					nodes[currentIdx].AddConnection(indexUsed + branch);
					nodes[indexUsed + branch].parent = indexUsed;
					branch--;
				}
				indexUsed += tempBranch;
				currentIdx++;
			}
		}

		public void AddRandomizeEndBranchConnection()
		{
			if (randomConnectEndBranch)
			{
				Node candidate = null;
				for (int i = 0; i < nodeCount; i++)
				{
					Node n = nodes[i];
					if (n.nodeConnections.Count == 0)
					{
						if (candidate == null)
						{
							if (Random.Range(0.0f, 1.0f) <= chanceConnectEndBranch)
								candidate = n;
						}
						else
						{
							candidate.AddConnection(i);
							candidate = null;
						}
					}
				}
			}
		}

		public Node[] GetNodes()
		{
			return nodes;
		}
	}


	[System.Serializable]
	public class Node
	{
		public int id;
		public int parent;
		public List<int> nodeConnections = new List<int>();

		public Node(){}
		public Node(int id)
		{
			this.id = id;
		}

		public bool AddConnection(int idx)
		{
			if (nodeConnections.IndexOf(idx) != -1)
				return false;
			nodeConnections.Add(idx);
			return true;
		}

#if UNITY_EDITOR
		public Rect rectPosition;
		public void CalculatePosition(ref Rect rect, Vector2Int spacing, Node[] nodes)
		{
			////Draw from the most far
			rect.x += spacing.x;
			foreach (int n in nodeConnections)
			{
				try
				{
					nodes[n].CalculatePosition(ref rect, spacing, nodes);
				}
				catch (System.Exception e)
				{
					Debug.Log(n);
				}
			}
			if (nodeConnections.Count == 0)
				rect.y += spacing.y;
			rect.x -= spacing.x;
			rectPosition = rect;
			if (nodeConnections.Count > 0)
				rectPosition.y = nodes[nodeConnections[0]].rectPosition.y + (nodes[nodeConnections[nodeConnections.Count - 1]].rectPosition.y - nodes[nodeConnections[0]].rectPosition.y) / 2;
		}
		public void DrawConnection(Node[] nodes, Vector2 offset)
		{
			foreach (int i in nodeConnections)
			{
				Node n = nodes[i];
				Handles.DrawBezier(rectPosition.position + offset + new Vector2(rectPosition.width, rectPosition.height / 2),
						n.rectPosition.position + offset + new Vector2(0, n.rectPosition.height / 2),
						rectPosition.center + offset - Vector2.left * 50f,
						n.rectPosition.center + offset + Vector2.left * 50f,
						Color.white,
						null,
						2f
				);
			}
		}
#endif
	}
}

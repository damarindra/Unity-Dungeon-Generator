using Delaunay.LR;
using Delaunay.Utils;
using System.Collections.Generic;

namespace Delaunay
{
	public enum VertexOrSite
	{
		VERTEX,
		SITE
	}
	
	sealed class EdgeReorderer: Utils.IDisposable
	{
		private List<Edge> _edges;
		private List<Side> _edgeOrientations;
		public List<Edge> edges {
			get { return _edges;}
		}
		public List<Side> edgeOrientations {
			get{ return _edgeOrientations;}
		}
		
		public EdgeReorderer (List<Edge> origEdges, VertexOrSite criterion)
		{
			_edges = new List<Edge> ();
			_edgeOrientations = new List<Side> ();
			if (origEdges.Count > 0) {
				_edges = ReorderEdges (origEdges, criterion);
			}
		}
		
		public void Dispose ()
		{
			_edges = null;
			_edgeOrientations = null;
		}

		private List<Edge> ReorderEdges (List<Edge> origEdges, VertexOrSite criterion)
		{
			int i;
			int n = origEdges.Count;
			Edge edge;
			// we're going to reorder the edges in order of traversal
			bool[] done = new bool[n];
			int nDone = 0;
			for (int j=0; j<n; j++) {
				done [j] = false;
			}
			List<Edge> newEdges = new List<Edge> (); // TODO: Switch to Deque if performance is a concern
			
			i = 0;
			edge = origEdges [i];
			newEdges.Add (edge);
			_edgeOrientations.Add (Side.LEFT);
			ICoord firstPoint = (criterion == VertexOrSite.VERTEX) ? (ICoord)edge.leftVertex : (ICoord)edge.leftSite;
			ICoord lastPoint = (criterion == VertexOrSite.VERTEX) ? (ICoord)edge.rightVertex : (ICoord)edge.rightSite;
			
			if (firstPoint == Vertex.VERTEX_AT_INFINITY || lastPoint == Vertex.VERTEX_AT_INFINITY) {
				return new List<Edge> ();
			}
			
			done [i] = true;
			++nDone;
			
			while (nDone < n) {
				for (i = 1; i < n; ++i) {
					if (done [i]) {
						continue;
					}
					edge = origEdges [i];
					ICoord leftPoint = (criterion == VertexOrSite.VERTEX) ? (ICoord)edge.leftVertex : (ICoord)edge.leftSite;
					ICoord rightPoint = (criterion == VertexOrSite.VERTEX) ? (ICoord)edge.rightVertex : (ICoord)edge.rightSite;
					if (leftPoint == Vertex.VERTEX_AT_INFINITY || rightPoint == Vertex.VERTEX_AT_INFINITY) {
						return new List<Edge> ();
					}
					if (leftPoint == lastPoint) {
						lastPoint = rightPoint;
						_edgeOrientations.Add (Side.LEFT);
						newEdges.Add (edge);
						done [i] = true;
					} else if (rightPoint == firstPoint) {
						firstPoint = leftPoint;
						_edgeOrientations.Insert (0, Side.LEFT); // TODO: Change datastructure if this is slow
						newEdges.Insert (0, edge);
						done [i] = true;
					} else if (leftPoint == firstPoint) {
						firstPoint = rightPoint;
						_edgeOrientations.Insert (0, Side.RIGHT);
						newEdges.Insert (0, edge);
						done [i] = true;
					} else if (rightPoint == lastPoint) {
						lastPoint = leftPoint;
						_edgeOrientations.Add (Side.RIGHT);
						newEdges.Add (edge);
						done [i] = true;
					}
					if (done [i]) {
						++nDone;
					}
				}
			}
			
			return newEdges;
		}

	}
}
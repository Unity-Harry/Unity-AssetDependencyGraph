using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Core.Routing;
using Microsoft.Msagl.Layout.Layered;

public class GraphViewLayouter
{
    // https://www.microsoft.com/en-us/research/project/microsoft-automatic-graph-layout/#code-samples
    public void Adjust(UnityEditor.Experimental.GraphView.GraphView graphView)
    {
        var elements = graphView.graphElements.ToList();
        var nodes = elements.Where(x => x is UnityEditor.Experimental.GraphView.Node)
            .Cast<UnityEditor.Experimental.GraphView.Node>();
        var edges = elements.Where(x => x is UnityEditor.Experimental.GraphView.Edge)
            .Cast<UnityEditor.Experimental.GraphView.Edge>();

        GeometryGraph graph = new GeometryGraph();

        Dictionary<UnityEditor.Experimental.GraphView.Node, Node> geomNodes =
            new Dictionary<UnityEditor.Experimental.GraphView.Node, Node>();

        foreach (var node in nodes) {
            var nodeRect = node.GetPosition();
            Node geometryNode = new Node(CurveFactory.CreateRectangle(nodeRect.width, nodeRect.height, new Point()), node);
            graph.Nodes.Add(geometryNode);
            geomNodes.Add(node, geometryNode);
        }

        foreach (var e in edges) {
            // Exact edge end positions could be found with e.output.GetGlobalCenter()
            Edge geometryEdge = new Edge(geomNodes[e.output.node], geomNodes[e.input.node]);
            graph.Edges.Add(geometryEdge);
        }

        // settings found to look good (enough separation, not too much, clean layout) by experimentation
        var settings = new SugiyamaLayoutSettings {
            Transformation = PlaneTransformation.Rotation(Math.PI / 2),
            EdgeRoutingSettings = {EdgeRoutingMode = EdgeRoutingMode.StraightLine},
            // AspectRatio = 9.0 / 16.0, // allows adjusting aspect
            LayerSeparation = 200,
            GridSizeByX = 300,
            GridSizeByY = 300,
            SnapToGridByY = SnapToGridByY.Bottom
        };
        var layout = new LayeredLayout(graph, settings);
        layout.Run();

        foreach (var n in graph.Nodes) {
            var unityNode = (UnityEditor.Experimental.GraphView.Node) n.UserData;
            unityNode.SetPosition(new Rect((float) n.Center.X, (float) n.Center.Y, 0, 0));
        }
    }
}
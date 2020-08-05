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
    public /*async*/ void Adjust(UnityEditor.Experimental.GraphView.GraphView graphView) {
        // await Task.Delay(30);

        var elements = graphView.graphElements.ToList();
        var nodes = elements.Where(x => x is UnityEditor.Experimental.GraphView.Node)
            .Cast<UnityEditor.Experimental.GraphView.Node>();
        var edges = elements.Where(x => x is UnityEditor.Experimental.GraphView.Edge)
            .Cast<UnityEditor.Experimental.GraphView.Edge>();

        GeometryGraph graph = new GeometryGraph();

        Dictionary<UnityEditor.Experimental.GraphView.Node, Node> geomNodes =
            new Dictionary<UnityEditor.Experimental.GraphView.Node, Node>();

        foreach (var n in nodes) {
            var r = n.GetPosition();
            // Debug.Log(r.position + ", " + r.width + " * " + r.height);

            Node geometryNode = new Node(CurveFactory.CreateRectangle(r.width, r.height, new Point()), n);
            graph.Nodes.Add(geometryNode);
            geomNodes.Add(n, geometryNode);
        }

        foreach (var e in edges) {
            Edge geometryEdge = new Edge(geomNodes[e.output.node], geomNodes[e.input.node]);
            graph.Edges.Add(geometryEdge);
            // Debug.Log(e.output.GetGlobalCenter() + " --- " + e.input.GetGlobalCenter());
        }

        var settings = new SugiyamaLayoutSettings {
            Transformation = PlaneTransformation.Rotation(Math.PI / 2),
            EdgeRoutingSettings = {EdgeRoutingMode = EdgeRoutingMode.StraightLine},
            // AspectRatio = 9.0 / 16.0,
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
            // Debug.Log(n.Center.X + ", " + n.Center.Y);
        }
    }
}
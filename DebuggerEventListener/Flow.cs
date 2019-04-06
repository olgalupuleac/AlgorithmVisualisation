using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using EnvDTE;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using Color = Microsoft.Msagl.Drawing.Color;
using Process = EnvDTE.Process;
using StackFrame = EnvDTE.StackFrame;

namespace DebuggerEventListener
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Shell;

    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("116f0099-7b0a-4faa-a0e8-d2e992e04959")]
    public class Flow : ToolWindowPane
    {
        private EnvDTE.DTE applicationObject;
        private EnvDTE.DebuggerEvents debugEvents;
        private EnvDTE.Debugger debugger;
        private System.Windows.Forms.Form form;
        private Graph graph;
        private List<Edge> edges;

        /// <summary>
        /// Initializes a new instance of the <see cref="Flow"/> class.
        /// </summary>
        public Flow() : base(null)
        {
            this.Caption = "Flow";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = new FlowControl();
        }

        protected override void Initialize()
        {
            applicationObject = (DTE) Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(DTE));
            // Place the following code in the event handler  
            debugEvents = applicationObject.Events.DebuggerEvents;
            debugEvents.OnContextChanged +=
                UpdateGraph;
            debugger = applicationObject.Debugger;
        }


        private string NodeLabel(Node graphNode)
        {
            string d = debugger.GetExpression($"d[{graphNode.Id}]").Value;
            string h = debugger.GetExpression($"head[{graphNode.Id}]").Value;
            return $"{graphNode.Id}, d={d}, h={h}";
        }

        private Tuple<int, int> CapacityAndFlow(int edgeId)
        {
            int cap = Int32.Parse(debugger.GetExpression($"edges[{edgeId}].cap").Value);
            int flow = Int32.Parse(debugger.GetExpression($"edges[{edgeId}].flow").Value);
            return new Tuple<int, int>(cap, flow);
        }

        private string EdgeLabel(Tuple<int, int> capacityAndFlow)
        {
            return $"{capacityAndFlow.Item1},{capacityAndFlow.Item2}";
        }

        private void SetEdgeStyle(Edge edge, Tuple<int, int> cf)
        {

            if (cf.Item2 == 0)
            {
                edge.Attr.ArrowheadAtTarget = ArrowStyle.None;
                edge.Attr.ArrowheadAtSource = ArrowStyle.None;
            }
            if (cf.Item2 < 0)
            {
                edge.Attr.ArrowheadAtSource = ArrowStyle.Normal;
                edge.Attr.ArrowheadAtTarget = ArrowStyle.None;
            }
            if (cf.Item2 > 0)
            {
                edge.Attr.ArrowheadAtSource = ArrowStyle.None;
                edge.Attr.ArrowheadAtTarget = ArrowStyle.Normal;
            }

            if (Math.Abs(cf.Item1) == Math.Abs(cf.Item2))
            {
                edge.Attr.LineWidth *= 3;
            }
        }

        private void RenderGraph()
        {
            graph = new Microsoft.Msagl.Drawing.Graph("dinic");
            edges = new List<Edge>();
            //create the graph content 
            Expression numberOfEdgesExpr = debugger.GetExpression("edges.size()");
            int numberOfEdges = Int32.Parse(numberOfEdgesExpr.Value);
            for (int i = 0; i < numberOfEdges; i += 2)
            {
                string from = debugger.GetExpression($"edges[{i}].from").Value;
                string to = debugger.GetExpression($"edges[{i}].to").Value;
                Tuple<int, int> cf = CapacityAndFlow(i);
                Edge edge = graph.AddEdge(from, EdgeLabel(cf), to);
                edge.Label.FontSize = 6;
                edges.Add(edge);
                SetEdgeStyle(edge, cf);
            }

            string s = debugger.GetExpression("s").Value;
            string t = debugger.GetExpression("t").Value;
            Node sNode = graph.FindNode(s);
            Node tNode = graph.FindNode(t);
            if (sNode != null && tNode != null)
            {
                sNode.Attr.FillColor = Color.Yellow;
                tNode.Attr.FillColor = Color.Green;
            }

            foreach (var graphNode in graph.Nodes)
            {
                graphNode.Label.Text = NodeLabel(graphNode);
            }
        }


        private void ClearGraph()
        {
            foreach (Node node in graph.Nodes)
            {
                node.Attr.Color = Color.Black;
                node.Label.FontColor = Color.Black;
            }

            foreach (Edge edge in edges)
            {
                edge.Label.FontColor = Color.Black;
                if (edge.Attr.Color == Color.Aquamarine)
                {
                    edge.Attr.Color = Color.Black;
                }
            }
        }

        private void HighlightTmpEdge(string edgeId)
        {
            Expression idExpr = debugger.GetExpression(edgeId);
            Debug.WriteLine($"{edgeId}={idExpr.Value}");
            if (idExpr.IsValidValue)
            {
                int id = Int32.Parse(idExpr.Value) / 2;
                if (id >= 0 && id < edges.Count && edges[id].Attr.Color != Color.Red)
                {
                    edges[id].Attr.Color = Color.Aquamarine;
                }
            }
        }


        private void HighlightDfsEdges(Thread newthread)
        {
            for (int i = 2; i < newthread.StackFrames.Count; i++)
            {
                StackFrame sf = newthread.StackFrames.Item(i);
                if (sf.FunctionName.Equals("dfs")
                    && newthread.StackFrames.Item(i - 1).FunctionName.Equals("dfs"))
                {
                    foreach (Expression item in sf.Locals)
                    {
                        Debug.WriteLine($"{item.Name}={item.Value}");
                        if (item.Name.Equals("id"))
                        {
                            Debug.WriteLine("Should be red");
                            int id = Int32.Parse(item.Value) / 2;
                            edges[id].Attr.Color = Color.Red;
                            //string from = sf.Arguments.Item(1).Value;
                            /*if (edges[id].Source.Equals(from))
                            {
                                edges[id].Attr.ArrowheadAtTarget = ArrowStyle.Normal;
                            }
                            else
                            {
                                edges[id].Attr.ArrowheadAtSource = ArrowStyle.Normal;
                            }*/
                        }
                    }
                }
            }
        }

        private void HighlightLastChanges()
        {
            for (int i = 0; i < edges.Count; i++)
            {
                //Debug.WriteLine($"{edges[i].LabelText} ?= {cap},{flow}");
                Tuple<int, int> cf = CapacityAndFlow(2 * i);
                string label = EdgeLabel(cf);
                if (edges[i].LabelText.Equals(label))
                {
                    continue;
                }

                edges[i].Label.Text = label;
                edges[i].Label.FontColor = Color.Red;
                SetEdgeStyle(edges[i], cf);
                //edge.Attr.ArrowheadAtTarget = ArrowStyle.None;
            }

            foreach (Node node in graph.Nodes)
            {
                string label = NodeLabel(node);
                if (node.Label.Text.Equals(label))
                {
                    continue;
                }

                node.Label.Text = label;
                node.Label.FontColor = Color.Red;
            }
        }


        private void RenderDfs(Thread newthread, StackFrame newstackframe)
        {
            if (graph == null)
            {
                RenderGraph();
            }

            ClearGraph();
            HighlightTmpEdge("id");
            HighlightDfsEdges(newthread);
            HighlightLastChanges();
            string v = newstackframe.Arguments.Item(1).Value;
            graph.FindNode(v).Attr.Color = Color.Red;
        }


        private void RenderBfs()
        {
            if (graph == null)
            {
                RenderGraph();
            }

            ClearGraph();
            Expression vExpr = debugger.GetExpression("v");
            if (vExpr.IsValidValue)
            {
                Node v = graph.FindNode(vExpr.Value);
                if (v != null)
                {
                    v.Attr.Color = Color.Red;
                }
            }

            HighlightTmpEdge("x");
            HighlightLastChanges();
        }

        private void UpdateGraph(Process newprocess, Program newprogram, Thread newthread, StackFrame newstackframe)
        {
            if (newstackframe == null)
            {
                return;
            }

            switch (newstackframe.FunctionName)
            {
                case "dfs":
                    RenderDfs(newthread, newstackframe);
                    break;
                case "bfs":
                    RenderBfs();
                    break;
                default:
                    RenderGraph();
                    break;
            }

            if (form == null)
            {
                form = new System.Windows.Forms.Form();
                form.Size = new Size(600, 600);
            }

            //form.
            form.SuspendLayout();
            form.Controls.Clear();
            form.Controls.Add(new GViewer {Graph = graph, Dock = System.Windows.Forms.DockStyle.Fill});
            form.ResumeLayout();
            form.Show();
        }
    }
}
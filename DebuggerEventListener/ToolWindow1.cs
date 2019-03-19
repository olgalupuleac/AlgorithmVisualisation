using System.Windows.Forms;
using System.Windows.Input;
using EnvDTE;
using Microsoft.Msagl.Drawing;

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
    [Guid("55b901ce-8004-411d-a7c0-4d507c182ba4")]
    public class ToolWindow1 : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToolWindow1"/> class.
        /// </summary>
        ///
        private EnvDTE.DebuggerEvents debugEvents;

        private EnvDTE.Debugger debugger;

        //private 
        public ToolWindow1() : base(null)
        {
            this.Caption = "ToolWindow1";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = new ToolWindow1Control();
        }

        protected override void Initialize()
        {
            EnvDTE.DTE applicationObject = (DTE) Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(DTE));
            // Place the following code in the event handler  
            debugEvents = applicationObject.Events.DebuggerEvents;
            debugEvents.OnContextChanged +=
                ContextHandler;
            StartEvents(applicationObject);
            debugger = applicationObject.Debugger;
        }

        // Place the following Event handler code  
        public void StartEvents(DTE dte)
        {
            ((ToolWindow1Control) this.Content).listBox.Items.Add("Start");
        }

        public void ContextHandler(EnvDTE.Process newProc,
            EnvDTE.Program newProg, EnvDTE.Thread newThread, EnvDTE.StackFrame newStkFrame)
        {
            if (newStkFrame != null)
            {
                Main(newStkFrame);
            }
            else
            {
                ((ToolWindow1Control) this.Content).listBox.Items.Add("No stackframe");
            }
        }

        public void Main(EnvDTE.StackFrame newStkFrame)
        {
            //create a form 
            System.Windows.Forms.Form form = new System.Windows.Forms.Form();
            //create a viewer object 
            Microsoft.Msagl.GraphViewerGdi.GViewer viewer = new Microsoft.Msagl.GraphViewerGdi.GViewer();
            //create a graph object 
            Microsoft.Msagl.Drawing.Graph graph = new Microsoft.Msagl.Drawing.Graph("graph");
            //create the graph content 
            graph.Directed = false;
            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    String expression = $"graph[{i}][{j}]";
                    EnvDTE.Expression expr = debugger.GetExpression(expression, true);
                    if (expr.IsValidValue && expr.Value.Equals("true"))
                    {
                        EnvDTE.Expression isParent = debugger.GetExpression($"p[{j}] == {i}", true);
                        ((ToolWindow1Control) this.Content).listBox.Items.Add($"{isParent.Name}={isParent.Value}");
                        if (isParent.IsValidValue && isParent.Value.Equals("true"))
                        {
                            graph.AddEdge($"{i}", $"{j}").Attr.Color = Color.Red;
                        }
                        else
                        {
                            graph.AddEdge($"{i}", $"{j}");
                        }
                    }
                }
            }

           if (newStkFrame.FunctionName.Equals("dfs"))
            {
                EnvDTE.Expression arg = newStkFrame.Arguments.Item(1);
                graph.FindNode(arg.Value).Attr.FillColor = Microsoft.Msagl.Drawing.Color.Magenta;
            }
            /*graph.AddEdge("A", "B");
            graph.AddEdge("B", "C");
            graph.AddEdge("A", "C").Attr.Color = Microsoft.Msagl.Drawing.Color.Green;
            graph.FindNode("A").Attr.FillColor = Microsoft.Msagl.Drawing.Color.Magenta;
            graph.FindNode("B").Attr.FillColor = Microsoft.Msagl.Drawing.Color.MistyRose;
            Microsoft.Msagl.Drawing.Node c = graph.FindNode("C");
            c.Attr.FillColor = Microsoft.Msagl.Drawing.Color.Pale.Green;
            c.Attr.Shape = Microsoft.Msagl.Drawing.Shape.Diamond;*/
            //bind the graph to the viewer 
            viewer.Graph = graph;
            //associate the viewer with the form 
            form.SuspendLayout();
            viewer.Dock = System.Windows.Forms.DockStyle.Fill;
            //((ToolWindow1Control)this.Content)
            form.Controls.Add(viewer);
            //((ToolWindow1Control) this.Content).control = viewer;
            form.ResumeLayout();
            //show the form 
            form.ShowDialog();
            //form.Refresh();
        }
    }
}
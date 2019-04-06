using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using EnvDTE;
using Microsoft.Msagl.GraphViewerGdi;
using Color = Microsoft.Msagl.Drawing.Color;
using Size = System.Drawing.Size;

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
        private EnvDTE.DTE applicationObject;
        private EnvDTE.DebuggerEvents debugEvents;
        private EnvDTE.Debugger debugger;
        private System.Windows.Forms.Form form;

        /// <summary>
        /// Initializes a new instance of the <see cref="Dfs"/> class.
        /// </summary>

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
            applicationObject = (DTE) Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(DTE));
            // Place the following code in the event handler  
            debugEvents = applicationObject.Events.DebuggerEvents;
            debugEvents.OnContextChanged +=
                UpdateGraph;
            debugger = applicationObject.Debugger;
        }

        private void UpdateGraph(Process newprocess, Program newprogram, Thread newthread, StackFrame newstackframe)
        {
            if (newstackframe == null)
            {
                return;
            }

            Microsoft.Msagl.Drawing.Graph graph = new Microsoft.Msagl.Drawing.Graph("graph");
            //create the graph content 
            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    String expression = $"graph[{i}][{j}]";
                    EnvDTE.Expression expr = debugger.GetExpression(expression, true);
                    if (expr.IsValidValue && expr.Value.Equals("true"))
                    {
                        EnvDTE.Expression isParent = debugger.GetExpression($"p[{j}] == {i}", true);
                        //((ToolWindow1Control)this.Content).listBox.Items.Add($"{isParent.Name}={isParent.Value}");
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

            if (newstackframe.FunctionName.Equals("dfs"))
            {
                EnvDTE.Expression arg = newstackframe.Arguments.Item(1);
                graph.FindNode(arg.Value).Attr.FillColor = Microsoft.Msagl.Drawing.Color.Magenta;
            }

            //GViewer viewer =;
            //DfsControl.viewer = viewer;

            ((ToolWindow1Control)this.Content).MyWindowsFormsHost = new WindowsFormsHost() { Child = new GViewer { Graph = graph, Dock = System.Windows.Forms.DockStyle.Fill } };
            if (form == null)
            {
                form = new System.Windows.Forms.Form();
                form.Size = new Size(600, 600);
                form.SuspendLayout();
                form.Controls.Add(new GViewer { Graph = graph, Dock = System.Windows.Forms.DockStyle.Fill });
                form.ResumeLayout();
                form.Show();
            }
            else
            {
                form.SuspendLayout();
                form.Controls.Clear();
                form.Controls.Add(new GViewer { Graph = graph, Dock = System.Windows.Forms.DockStyle.Fill });
                form.ResumeLayout();
            }
            //System.Windows.Forms.Form form = new System.Windows.Forms.Form();
            //form.SuspendLayout();
            //
            //
            //form.Show();
            //ToolWindow1Control.MyWindowsFormsHost = new WindowsFormsHost() {Child = viewer};
        }
    }
}
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Shapes;
using EnvDTE;

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
    [Guid("6534dd68-bda0-41f9-a9e8-bbcbe6017c10")]
    public class Graph : ToolWindowPane
    {
        private EnvDTE.DebuggerEvents debugEvents;
        private EnvDTE.Debugger debugger;
        private Dictionary<int, Ellipse> nodes = new Dictionary<int, Ellipse>();
        private Dictionary<int, Line> edges = new Dictionary<int, Line>();
        /// <summary>
        /// Initializes a new instance of the <see cref="Graph"/> class.
        /// </summary>
        public Graph() : base(null)
        {
            this.Caption = "Graph";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = new GraphControl();
        }

        protected override void Initialize()
        {
            EnvDTE.DTE applicationObject = (DTE)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(DTE));
            // Place the following code in the event handler  
            debugEvents = applicationObject.Events.DebuggerEvents;
            debugEvents.OnContextChanged +=
                ContextHandler;
            debugger = applicationObject.Debugger;
            nodes.Add(0, ((GraphControl)this.Content).A);
            nodes.Add(1, ((GraphControl)this.Content).B);
            nodes.Add(2, ((GraphControl)this.Content).C);
            nodes.Add(3, ((GraphControl)this.Content).D);

            //hash = i ^ 2 + j ^ 2

            edges.Add(1, ((GraphControl)this.Content).AB);
            edges.Add(4, ((GraphControl)this.Content).AC);
            edges.Add(9, ((GraphControl)this.Content).AD);
            edges.Add(5, ((GraphControl)this.Content).BC);
            edges.Add(10, ((GraphControl)this.Content).BD);
            edges.Add(13, ((GraphControl)this.Content).CD);
        }

        public void ContextHandler(EnvDTE.Process newProc,
            EnvDTE.Program newProg, EnvDTE.Thread newThread, EnvDTE.StackFrame newStkFrame)
        {
            if (newStkFrame.FunctionName != "dfs")
            {
                return;
            }
            //{
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        String expression = $"graph[{i}][{j}]";
                        EnvDTE.Expression expr = debugger.GetExpression(expression, true);
                        if (expr.IsValidValue && expr.Value.Equals("true"))
                        {
                            EnvDTE.Expression isParent = debugger.GetExpression($"p[{i}]", true);
                            if (isParent.IsValidValue && isParent.Value.Equals($"{j}"))
                            {
                        
                                // .Fill = new SolidColorBrush(System.Windows.Media.Colors.Red);
                            }

                        }

                    }
                }

            //}
        }

    }
}

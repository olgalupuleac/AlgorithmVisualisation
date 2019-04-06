using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows.Data;
using System.Windows.Forms.Integration;
using System.Xml;
using Microsoft.Msagl.GraphViewerGdi;

namespace DebuggerEventListener
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for ToolWindow1Control.
    /// </summary>
    public partial class ToolWindow1Control : UserControl, INotifyPropertyChanged
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToolWindow1Control"/> class.
        /// </summary>
        public ToolWindow1Control()
        {
            this.InitializeComponent();
            this.DataContext = this;

        }

        public static GViewer viewer { get; set; } = new GViewer();

        //public static WindowsFormsHost MyWindowsFormsHost;
        public static GViewer viwer;

        static ToolWindow1Control()
        {
            viwer = new GViewer();
            
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /*static DfsControl()
        {
            viewer = new GViewer();
        }*/

        private static WindowsFormsHost windows = new WindowsFormsHost() { Child = viewer };

        public WindowsFormsHost MyWindowsFormsHost
        {
            get
            {
                Debug.WriteLine(Thread.CurrentThread.ManagedThreadId + " Getting value");
                /*System.Windows.Forms.Form form = new System.Windows.Forms.Form();
                //form.SuspendLayout();
                form.Controls.Add(new GViewer { Graph = (windows.Child as GViewer).Graph, Dock = System.Windows.Forms.DockStyle.Fill });
                //form.ResumeLayout();
                form.Show();*/
                return windows;
            }

            set
            {
                windows = value;
                Debug.WriteLine("Changing value");

                NotifyPropertyChanged("MyWindowsFormsHost");

                /*System.Windows.Forms.Form form = new System.Windows.Forms.Form();
                form.Controls.Add((windows.Child as GViewer));
                form.Show();*/
            }
        }
        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                Debug.WriteLine("Notify property changed");
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }

        }
    }

    public class DebugDataBindingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            System.Windows.Forms.Form form = new System.Windows.Forms.Form();
            form.Controls.Add(((WindowsFormsHost)value).Child as GViewer);
            form.Show();
            //Debugger.Break();
            return value;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            //Debugger.Break();
            return value;
        }
    }
}
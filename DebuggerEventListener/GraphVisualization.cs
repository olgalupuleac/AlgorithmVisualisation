using System;
using System.Collections.Generic;
using EnvDTE;
using Microsoft.Msagl.Drawing;

namespace DebuggerEventListener
{
    internal class ColorHighlighter
    {
        public int Priority { get; set; }
        public Color Color { get; set; }
    }

    internal class EdgeWrapper
    {
        public Edge Edge { get; set; }

        public int ColorPriority { get; set; }

        public void Clear(int priority)
        {
            Edge.Label.FontColor = Color.Black;
            if (priority < ColorPriority) return;
            Edge.Attr.Color = Color.Black;
            ColorPriority = 0;
        }

        public void Refresh(ColorHighlighter color, string label)
        {
            if (color != null && ColorPriority < color.Priority)
            {
                Edge.Attr.Color = color.Color;
                ColorPriority = color.Priority;
            }

            if (label != null && !Edge.LabelText.Equals(label))
            {
                Edge.Label.Text = label;
                Edge.Label.FontColor = Color.Red;
            }
        }
    }


    internal class NodeWrapper
    {
        public Node Node { get; set; }

        public int ColorPriority { get; set; }

        public int FillColorPriority { get; set; }

        public void Clear(int colorPriority, int fillColorPriority)
        {
            Node.Label.FontColor = Color.Black;
            if (ColorPriority < colorPriority)
            {
                Node.Attr.Color = Color.Black;
            }

            if (FillColorPriority < fillColorPriority)
            {
                Node.Attr.FillColor = Color.Black;
            }
        }

        public void Refresh(ColorHighlighter color, string label, ColorHighlighter fillColor)
        {
            if (color != null && ColorPriority < color.Priority)
            {
                Node.Attr.Color = color.Color;
                ColorPriority = color.Priority;
            }
            if (fillColor != null && FillColorPriority < fillColor.Priority)
            {
                Node.Attr.FillColor = fillColor.Color;
                FillColorPriority = fillColor.Priority;
            }
            if (label != null && !Node.Label.Text.Equals(label))
            {
                Node.Label.Text = label;
                Node.Label.FontColor = Color.Red;
            }
        }
    }


    public class GraphDescription
    {
       public List<string> NodeIds { get; set; }
       public List<string> EdgesIds { get; set; }

       public List<Func<string, string>> NodeLabels;
    }
}
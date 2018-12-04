﻿//BSD, 2014-present, WinterDev
//----------------------------------------------------------------------------
// Anti-Grain Geometry - Version 2.4
// Copyright (C) 2002-2005 Maxim Shemanarev (http://www.antigrain.com)
//
// C# Port port by: Lars Brubaker
//                  larsbrubaker@gmail.com
// Copyright (C) 2007
//
// Permission to copy, use, modify, sell and distribute this software 
// is granted provided this copyright notice appears in all copies. 
// This software is provided "as is" without express or implied
// warranty, and with no claim as to its suitability for any purpose.
//
//----------------------------------------------------------------------------
// Contact: mcseem@antigrain.com
//          mcseemagg@yahoo.com
//          http://www.antigrain.com
//----------------------------------------------------------------------------
//
// Rounded rectangle vertex generator
//
//----------------------------------------------------------------------------
using PixelFarm.Drawing;
namespace PixelFarm.CpuBlit.VertexProcessing
{

    public class SimpleRect
    {
        RectD bounds;
        public SimpleRect()
        {
        }
        public SimpleRect(double left, double bottom, double right, double top)
        {
            bounds = new RectD(left, bottom, right, top);
            if (left > right)
            {
                bounds.Left = right;
                bounds.Right = left;
            }

            if (bottom > top)
            {
                bounds.Bottom = top;
                bounds.Top = bottom;
            }
        }
        public void SetRect(double left, double bottom, double right, double top)
        {
            bounds = new RectD(left, bottom, right, top);
            if (left > right) { bounds.Left = right; bounds.Right = left; }
            if (bottom > top) { bounds.Bottom = top; bounds.Top = bottom; }
        }
        public void SetRectFromLTWH(double left, double top, double width, double height)
        {
            SetRect(left, top + height, left + width, top);
        }
        public void Offset(double dx, double dy)
        {
            bounds.Offset(dx, dy);
        }
        public double Height
        {
            get { return bounds.Height; }
        }
        public double Width
        {
            get { return bounds.Width; }
        }
        public VertexStore MakeVxs(VertexStore output)
        {
            using (VectorToolBox.Borrow(output, out PathWriter pw))
            {
                MakeVxs(pw);
            }

            return output;
        }
        public void MakeVxs(PathWriter pathWriter)
        {
            pathWriter.MoveTo(bounds.Left, bounds.Bottom);
            pathWriter.LineTo(bounds.Right, bounds.Bottom);
            pathWriter.LineTo(bounds.Right, bounds.Top);
            pathWriter.LineTo(bounds.Left, bounds.Top);
            pathWriter.CloseFigure();
        }
    }
}


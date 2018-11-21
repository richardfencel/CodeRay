using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

// Copyright Richard Fencel Software (c) 2014

namespace CodeRayWpfLib
{
    // This is used to superimpose a red or purple rectangle on a window in the client app (e.g. when we select a view in the 
    // tree and want to show where that view is in the app). For an explanation as to how Adorners work, see:
    // http://msdn.microsoft.com/en-us/library/ms746703(v=vs.110).aspx
    public class RectangleAdorner : Adorner
    {
        private Color _color;

         // Be sure to call the base class constructor. 
        public RectangleAdorner(UIElement adornedElement, Color color) : base(adornedElement)
        {
            _color = color;
        }

        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            Size sz = new Size(double.PositiveInfinity, double.PositiveInfinity);
            AdornedElement.Measure(sz);
            
            Rect adornedElementRect = new Rect(this.AdornedElement.RenderSize);
          
            Pen renderPen = new Pen(new SolidColorBrush(_color), 2);
            drawingContext.DrawRectangle(null, renderPen, adornedElementRect);
        }
    }
}

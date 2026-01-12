using System.Windows;
using System.Windows.Media;

namespace ImageHandle.ROI
{
    public class ROIDrawingVisual : DrawingVisual
    {
        public void Draw(Point topLeft, Point bottomRight, Brush brushes = null)
        {
            Pen pen = new Pen(brushes ?? Brushes.Red, 2);
            Point bottomLeft = new Point(topLeft.X, bottomRight.Y);
            Point topRight = new Point(bottomRight.X, topLeft.Y);
            using (DrawingContext dc = this.RenderOpen())
            {
                dc.DrawRectangle(Brushes.Transparent, pen, new Rect(topLeft, bottomRight));
                dc.DrawEllipse(brushes ?? Brushes.Red, pen, topLeft, 5, 5);
                dc.DrawEllipse(brushes ?? Brushes.Red, pen, topRight, 5, 5);
                dc.DrawEllipse(brushes ?? Brushes.Red, pen, bottomRight, 5, 5);
                dc.DrawEllipse(brushes ?? Brushes.Red, pen, bottomLeft, 5, 5);
            }
        }
    }
}
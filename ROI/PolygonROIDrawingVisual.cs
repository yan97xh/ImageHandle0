using System.Windows;
using System.Windows.Media;

namespace ImageHandle.ROI
{
    internal class PolygonROIDrawingVisual : DrawingVisual
    {
        /// <summary>
        /// 绘制多边形边框
        /// </summary>
        public void Draw(List<Point> points, Brush brushes = null)
        {
            Pen _strokePen = new Pen(brushes ?? Brushes.Red, 2.0);
            using (DrawingContext dc = RenderOpen())
            {
                if (points.Count == 0)
                {
                    return;
                }
                if (points.Count == 1)
                {
                    dc.DrawEllipse(brushes ?? Brushes.Red, _strokePen, points[0], 5, 5);
                    return;
                }

                // 创建多边形边框
                StreamGeometry geometry = new StreamGeometry();

                using (StreamGeometryContext ctx = geometry.Open())
                {
                    // 移动到第一个点
                    ctx.BeginFigure(points[0], false, points.Count >= 3); // 只有3个以上点才闭合

                    // 绘制所有线段
                    for (int i = 1; i < points.Count; i++)
                    {
                        ctx.LineTo(points[i], true, false);
                    }
                }

                // 绘制边框
                dc.DrawGeometry(null, _strokePen, geometry);

                // 绘制顶点
                foreach (Point point in points)
                {
                    dc.DrawEllipse(brushes ?? Brushes.Red, _strokePen, point, 5, 5);
                }
            }
        }
    }
}
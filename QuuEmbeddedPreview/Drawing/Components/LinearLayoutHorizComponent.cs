using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuuEmbeddedPreview.Drawing.Components
{
    class LinearLayoutHorizComponent : LinearLayoutComponent
    {
        protected override T GetMajorAxis<T>(T width, T height)
        {
            return width;
        }

        /*public override void Measure(int parentWidth, int parentHeight, out int targetWidth, out int targetHeight)
        {
            MeasureAndCalculate(GetMajorAxis(parentWidth, parentHeight), out int remainingHeight, out int expandableHeight, out int[] heights);
            targetHeight = parentWidth;
            targetWidth = GetMajorAxis(parentWidth, parentHeight) - remainingHeight;
        }*/

        protected override void LayoutChild(DrawingComponent child, int majorOffset, int majorSize)
        {
            child.Layout(X + majorOffset, Y, majorSize, Height);
        }

        protected override int MeasureChild(DrawingComponent child)
        {
            child.Measure(Width, Height, out int targetWidth, out int targetHeight);
            return targetWidth;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuuEmbeddedPreview.Drawing.Components
{
    abstract class LinearLayoutComponent : DrawingComponent
    {
        private Dictionary<DrawingComponent, LinearLayoutSettings> childSettings = new Dictionary<DrawingComponent, LinearLayoutSettings>();

        protected void MeasureAndCalculate(int majorSize, out int remainingHeight, out int expandableHeight, out int[] heights)
        {
            remainingHeight = majorSize;
            expandableHeight = 0;
            heights = new int[children.Count];
            for (int i = 0; i < children.Count; i++)
            {
                //Get child and its settings
                DrawingComponent c = children[i];
                LinearLayoutSettings setup = GetChildSettings(c);

                //Apply fixed-size objects
                if (setup.AutoSize)
                {
                    //Measure
                    int targetMajor = MeasureChild(c);

                    //Apply to height and subtract from auto-sized height
                    heights[i] = Math.Min(targetMajor, remainingHeight);
                    remainingHeight -= heights[i];

                    //Add to expandable height if we will expand this later
                    if (setup.AllowGrow)
                        expandableHeight += heights[i];
                }
                else
                {
                    //Calculate size in pixels
                    heights[i] = (int)(majorSize * setup.SizePercent);

                    //Subtract from remaining height for auto-sized objects
                    remainingHeight -= heights[i];
                }
            }
        }

        protected override void LayoutChildren()
        {
            //First, measure all children
            MeasureAndCalculate(GetMajorAxis(Width, Height), out int remainingHeight, out int expandableHeight, out int[] heights);

            //Expand expandable children
            if (expandableHeight > 0 && remainingHeight > 0)
            {
                //Calculate ratio
                float ratio = (float)(remainingHeight + expandableHeight) / expandableHeight;

                //Apply to all heights where applicable
                for (int i = 0; i < children.Count; i++)
                {
                    DrawingComponent c = children[i];
                    LinearLayoutSettings setup = GetChildSettings(c);
                    if (setup.AllowGrow && setup.AutoSize)
                        heights[i] = (int)(heights[i] * ratio);
                }
            }

            //Apply heights to children
            int offset = 0;
            for (int i = 0; i < children.Count; i++)
            {
                LayoutChild(children[i], offset, heights[i]);
                offset += heights[i];
            }
        }

        public void AddChild(DrawingComponent child, LinearLayoutSettings settings)
        {
            AddChild(child);
            childSettings.Add(child, settings);
        }

        private LinearLayoutSettings GetChildSettings(DrawingComponent c)
        {
            LinearLayoutSettings setup;
            if (!childSettings.TryGetValue(c, out setup))
                setup = new LinearLayoutSettings
                {
                    AutoSize = true,
                    AllowGrow = false,
                    SizePercent = 0
                };
            return setup;
        }

        protected abstract T GetMajorAxis<T>(T width, T height);
        protected abstract int MeasureChild(DrawingComponent child); // where major is the primary axis
        protected abstract void LayoutChild(DrawingComponent child, int majorOffset, int majorSize);

        public class LinearLayoutSettings
        {
            public bool AutoSize { get; set; }
            public bool AllowGrow { get; set; } // only applies to autosize
            public float SizePercent { get; set; }
        }
    }
}

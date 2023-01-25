using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuuEmbeddedPreview.Drawing
{
    class DrawingComponent
    {
        private bool layoutStale = true; //TODO: Use this
        protected List<DrawingComponent> children = new List<DrawingComponent>();

        private bool matchingMarginAspect = false;
        private float marginTop = 0;
        private float marginLeft = 0;
        private float marginBottom = 0;
        private float marginRight = 0;

        public int X { get; private set; }
        public int Y { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public bool MatchingMarginAspect
        {
            get => matchingMarginAspect;
            set
            {
                matchingMarginAspect = value;
                layoutStale = true;
            }
        }
        public float MarginTop
        {
            get => marginTop;
            set
            {
                marginTop = value;
                layoutStale = true;
            }
        }
        public float MarginLeft
        {
            get => marginLeft;
            set
            {
                marginLeft = value;
                layoutStale = true;
            }
        }
        public float MarginBottom
        {
            get => marginBottom;
            set
            {
                marginBottom = value;
                layoutStale = true;
            }
        }
        public float MarginRight
        {
            get => marginRight;
            set
            {
                marginRight = value;
                layoutStale = true;
            }
        }
        public bool Visible { get; set; } = true;
        public Color BackgroundColor { get; set; } = new Color(0, 0, 0, 0);

        public void SetMargins(float margins, bool matchingAspect = true)
        {
            MatchingMarginAspect = matchingAspect;
            MarginTop = margins;
            MarginBottom = margins;
            MarginLeft = margins;
            MarginRight = margins;
        }

        public void AddChild(DrawingComponent child)
        {
            //Do some sanity checks
            if (child == this)
                throw new Exception("Can't add self as a child!");
            if (children.Contains(child))
                throw new Exception("Can't add child that has already been added!");

            //Add
            children.Add(child);
            layoutStale = true;
        }

        /// <summary>
        /// Applies a new layout and also applies it to it's children. Will not be ran while drawing.
        /// </summary>
        public void Layout(int x, int y, int width, int height)
        {
            //If matching margin aspect ratios are requested, calculate the margins based on a square image
            int marginWidth = width;
            int marginHeight = height;
            if (matchingMarginAspect)
            {
                marginWidth = Math.Min(marginWidth, marginHeight);
                marginHeight = marginWidth;
            }

            //Calculate margins
            int marginLeftPx = (int)(marginWidth * marginLeft);
            int marginTopPx = (int)(marginHeight * marginTop);
            int marginRightPx = (int)(marginWidth * marginRight);
            int marginBottomPx = (int)(marginHeight * marginBottom);

            //Apply margins
            X = x + marginLeftPx;
            Y = y + marginTopPx;
            Width = width - marginLeftPx - marginRightPx;
            Height = height - marginTopPx - marginBottomPx;

            //Apply
            LayoutSelf();
            LayoutChildren();
        }

        protected virtual void LayoutSelf()
        {

        }

        protected virtual void LayoutChildren()
        {
            //Apply basic to children. You'll probably want to override this.
            foreach (var c in children)
                c.Layout(X, Y, Width, Height);
        }

        /// <summary>
        /// Determine the size this view WANTS.
        /// </summary>
        /// <param name="parentWidth"></param>
        /// <param name="parentHeight"></param>
        /// <param name="targetWidth"></param>
        /// <param name="targetHeight"></param>
        public virtual void Measure(int parentWidth, int parentHeight, out int targetWidth, out int targetHeight)
        {
            targetWidth = parentWidth;
            targetHeight = parentHeight;
        }

        /// <summary>
        /// Called just before we begin drawing
        /// </summary>
        public virtual void DrawPre()
        {
            //Apply to children
            foreach (var c in children)
                c.DrawPre();
        }

        /// <summary>
        /// Called to render this
        /// </summary>
        public void Draw()
        {
            //Skip if invisible
            if (!Visible)
                return;

            //Draw background
            if (BackgroundColor.a != 0)
                Raylib.DrawRectangle(X, Y, Width, Height, BackgroundColor);

            //DEBUG
            //Raylib.DrawRectangleLines(X, Y, Width, Height, Color.RED);

            //Draw self
            DrawSelf();

            //Draw children to children
            foreach (var c in children)
                c.Draw();
        }

        protected virtual void DrawSelf()
        {

        }

        /// <summary>
        /// Called just after we finish drawing
        /// </summary>
        public virtual void DrawPost()
        {
            //Apply to children
            foreach (var c in children)
                c.DrawPost();
        }
    }
}

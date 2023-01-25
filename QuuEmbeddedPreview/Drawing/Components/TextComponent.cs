using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace QuuEmbeddedPreview.Drawing.Components
{
    class TextComponent : DrawingComponent
    {
        public TextComponent(RayMemoryAsset fontData, Color color, string text = "")
        {
            this.fontData = fontData;
            Color = color;
            Text = text;
        }

        private readonly RayMemoryAsset fontData;

        public string Text
        {
            get => text;
            set
            {
                text = value;
                positioningStale = true;
            }
        }
        public Color Color { get; set; }
        public HorizAlignment AlignmentHorizontal { get; set; } = HorizAlignment.Center;
        public VertAlignment AlignmentVertical { get; set; } = VertAlignment.Middle;

        private Font? font;
        private bool fontStale;

        private bool positioningStale;
        private int offsetX;
        private int offsetY;
        private string adjustedText;
        private string text;

        protected override void LayoutSelf()
        {
            fontStale = true;
        }

        public override void Measure(int parentWidth, int parentHeight, out int targetWidth, out int targetHeight)
        {
            targetWidth = 1;
            targetHeight = parentHeight;
        }

        public override void DrawPre()
        {
            if (fontStale)
            {
                //Dispose of old font if needed
                if (font != null)
                {
                    Raylib.UnloadFont(font.Value);
                    font = null;
                }

                //Load new font
                font = fontData.LoadAsFont(Height);

                //Reset flags
                fontStale = false;
                positioningStale = true;
            }

            base.DrawPre();
        }

        protected override void DrawSelf()
        {
            if (font != null && text != null && text.Length > 0)
                RenderText();
        }

        private void RenderText()
        {
            //Check if we need to update text positioning
            if (positioningStale)
            {
                //Measure text to see if it'll fit
                adjustedText = Text;
                if (Raylib.MeasureTextEx(font.Value, adjustedText, Height, 0).X > Width)
                {
                    //Jank, but truncate as much as possible from the text
                    int len = Text.Length;
                    while (len > 0 && Raylib.MeasureTextEx(font.Value, adjustedText.Substring(0, len).TrimEnd() + "...", Height, 0).X > Width)
                        len--;

                    //Determine final text
                    if (len == 0)
                        return;
                    adjustedText = adjustedText.Substring(0, len).TrimEnd() + "...";
                }

                //Measure the text so that we can center it
                Vector2 textSize = Raylib.MeasureTextEx(font.Value, adjustedText, Height, 0);

                //Determine horizontal centering
                offsetX = 0;
                switch (AlignmentHorizontal)
                {
                    case HorizAlignment.Center:
                        offsetX = (int)((Width - textSize.X) / 2);
                        break;
                    case HorizAlignment.Right:
                        offsetX = (int)(Width - textSize.X);
                        break;
                }

                //Determine vertical centering
                offsetY = 0;
                switch (AlignmentVertical)
                {
                    case VertAlignment.Middle:
                        offsetY = (int)((Height - textSize.Y) / 2);
                        break;
                    case VertAlignment.Bottom:
                        offsetY = (int)(Height - textSize.Y);
                        break;
                }

                //Reset flag
                positioningStale = false;
            }

            //Draw
            Raylib.DrawTextEx(font.Value, adjustedText, new Vector2(X + offsetX, Y + offsetY), Height, 0, Color);
        }

        public enum HorizAlignment
        {
            Left,
            Center,
            Right
        }

        public enum VertAlignment
        {
            Top,
            Middle,
            Bottom
        }
    }
}

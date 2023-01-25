using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuuEmbeddedPreview.Drawing.Components
{
    class ImageComponent : DrawingComponent
    {
        public ImageComponent(Image? image = null)
        {
            Image = image;
            textureStale = true;
        }

        public Image? Image
        {
            get => image;
            set
            {
                image = value;
                textureStale = true;
            }
        }

        private Image? image;
        private Texture2D? texture;
        private bool textureStale;

        private int imageWidth;
        private int imageHeight;
        private int imageOffsetX;
        private int imageOffsetY;

        private void CalculateDimensions(float containerWidth, float containerHeight, out int imageOffsetX, out int imageOffsetY, out int imageWidth, out int imageHeight)
        {
            if (Image == null)
            {
                //Go to defaults
                imageOffsetX = 0;
                imageOffsetY = 0;
                imageWidth = Width;
                imageHeight = Height;
            }
            else
            {
                //Determine aspect ratio and how to make it fit
                float bestRatio = Math.Min(containerWidth / Image.Value.width, containerHeight / Image.Value.height);
                imageWidth = (int)(Image.Value.width * bestRatio);
                imageHeight = (int)(Image.Value.height * bestRatio);

                //Determine image offset to center it
                imageOffsetX = (Width - imageWidth) / 2;
                imageOffsetY = (Height - imageHeight) / 2;
            }
        }

        protected override void LayoutSelf()
        {
            //Calculate
            CalculateDimensions(Width, Height, out imageOffsetX, out imageOffsetY, out imageWidth, out imageHeight);

            //Mark that the layout has changed
            textureStale = true;
        }

        public override void Measure(int parentWidth, int parentHeight, out int targetWidth, out int targetHeight)
        {
            //Calculate
            CalculateDimensions(parentWidth, parentHeight, out int imageOffsetX, out int imageOffsetY, out targetWidth, out targetHeight);
        }

        public override void DrawPre()
        {
            //Refresh as needed
            if (textureStale)
            {
                //Destroy old texture if any
                if (texture != null)
                {
                    Raylib.UnloadTexture(texture.Value);
                    texture = null;
                }

                //Resize image to the desired resolution and convert it to a texture
                if (Image != null && Width > 0 && Height > 0)
                {
                    //Resize image and push to texture
                    Image resized = Raylib.ImageCopy(Image.Value);
                    Raylib.ImageResize(ref resized, imageWidth, imageHeight);
                    texture = Raylib.LoadTextureFromImage(resized);
                    Raylib.UnloadImage(resized);
                }

                textureStale = false;
            }

            base.DrawPre();
        }

        protected override void DrawSelf()
        {
            //Draw image
            if (texture != null)
                Raylib.DrawTexture(texture.Value, X + imageOffsetX, Y + imageOffsetY, Color.WHITE);
        }

        protected override void LayoutChildren()
        {
            //Apply basic to children. You'll probably want to override this.
            foreach (var c in children)
                c.Layout(X + imageOffsetX, Y + imageOffsetY, imageWidth, imageHeight);
        }
    }
}

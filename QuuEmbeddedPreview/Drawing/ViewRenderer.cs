using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuuEmbeddedPreview.Drawing
{
    class ViewRenderer : IDisposable
    {
        public ViewRenderer(string title, int width, int height)
        {
            this.width = width;
            this.height = height;
            Raylib.InitWindow(width, height, title);
            Raylib.SetTargetFPS(15);
        }

        private readonly int width;
        private readonly int height;

        private IDrawingView view;
        private Queue<Action<ViewRenderer>> queue = new Queue<Action<ViewRenderer>>();
        private object mutex = new object();

        public Color Background { get; set; } = Color.BLACK;

        public void ChangeRoot(IDrawingView view)
        {
            this.view = view;
            view.RootComponent.Layout(0, 0, width, height);
        }

        public void Dispose()
        {
            Raylib.CloseWindow();
        }

        public void QueueCommand(Action<ViewRenderer> cmd)
        {
            lock (mutex)
                queue.Enqueue(cmd);
        }

        public void Work()
        {
            while (!Raylib.WindowShouldClose())
            {
                //Apply any commands
                lock (mutex)
                {
                    while (queue.TryDequeue(out Action<ViewRenderer> cmd))
                        cmd(this);
                }

                //Do rendering
                if (view != null)
                {
                    //Process
                    view.ProcessFrame();

                    //Render
                    view.RootComponent.DrawPre();
                    Raylib.BeginDrawing();
                    Raylib.ClearBackground(Background);
                    view.RootComponent.Draw();
                    Raylib.EndDrawing();
                    view.RootComponent.DrawPost();
                }
            }
        }
    }
}

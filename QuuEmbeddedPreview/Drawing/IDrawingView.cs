using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuuEmbeddedPreview.Drawing
{
    interface IDrawingView
    {
        DrawingComponent RootComponent { get; }
        void ProcessFrame();
    }
}

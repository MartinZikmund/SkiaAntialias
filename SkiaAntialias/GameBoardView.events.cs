using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkiaAntialias
{
    public partial class GameBoardView
    {
        public event GameBoardEventHandler<SKSurface> Initialized;
        public event GameBoardEventHandler<EventArgs> Update;

        public
#if __IOS__
            new
#endif
            event GameBoardEventHandler<SKSurface> Draw;
    }
}

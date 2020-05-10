using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkiaAntialias
{
    public delegate void GameBoardEventHandler<TEventArgs>(GameBoardView gameBoard, TEventArgs args);
}

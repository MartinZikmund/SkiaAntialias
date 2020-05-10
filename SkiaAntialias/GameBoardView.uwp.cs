#if WINDOWS_UWP
using SkiaSharp;
using System;
using SKNativePaintGLSurfaceEventArgs = SkiaSharp.Views.UWP.SKPaintGLSurfaceEventArgs;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace SkiaAntialias
{
    public partial class GameBoardView
    {
        public GameBoardView()
        {
            PreInitialize();
        }

        partial void PlatformInitalize(object sender, SKNativePaintGLSurfaceEventArgs e)
        {
            EnableRenderLoop = false;
            DrawInBackground = false;
        }

        partial void InvalidateSurface() => Invalidate();

        async partial void RunOnUiThread(Action action)
        {
            var dispatcher = CoreApplication.MainView?.CoreWindow?.Dispatcher;

            if (dispatcher == null)
            {
                throw new InvalidOperationException("Unable to find main thread.");
            }
            await dispatcher.RunAsync(CoreDispatcherPriority.High, () => action());
        }

        partial void BeforeUpdate()
        {
        }

        public SKPoint GetScaledCoord(double x, double y) =>
            new SKPoint((float)(x * ContentsScale), (float)(y * ContentsScale));

        public float ScaleFactor => (float)ContentsScale;
    }
}
#endif
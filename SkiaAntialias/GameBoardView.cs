﻿using System;

using SkiaSharp;
using System.Threading;
using SKNativeView = SkiaSharp.Views.UWP.SKSwapChainPanel;
using SKNativePaintGLSurfaceEventArgs = SkiaSharp.Views.UWP.SKPaintGLSurfaceEventArgs;
using AlchemyX.Skia.Infrastructure;

namespace SkiaAntialias
{
    public partial class GameBoardView : SKNativeView
    {
        private readonly object _renderLock = new object();

        private int _uiThreadId;
        private RenderLoop _renderLoop;
        private bool _shouldRender = false;

        public bool IsGLView { get; private set; }

        public bool IsDirty { get; set; }

        public SKSize ScaledSize => new SKSize(CanvasSize.Width / ScaleFactor, CanvasSize.Height / ScaleFactor);

        /// <summary>
        /// Hooks up to the first rendering to initialize.
        /// </summary>
        private void PreInitialize()
        {
            _uiThreadId = Thread.CurrentThread.ManagedThreadId;

            this.PaintSurface += OnNativeControlInitialized;
#if !__WASM__
            IsGLView = true;
#else
            IsGLView = false;
#endif
        }

        /// <summary>
        /// Runs when the underlying control is done initializing (first rendering goes through).
        /// </summary>
        /// <param name="sender">Native control.</param>
        /// <param name="e">Event args.</param>
        private void OnNativeControlInitialized(object sender, SKNativePaintGLSurfaceEventArgs e)
        {
            this.PaintSurface -= OnNativeControlInitialized;
            Initialize(sender, e);
        }

        /// <summary>
        /// Performs custom initialization and starts game time.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void Initialize(object sender, SKNativePaintGLSurfaceEventArgs e)
        {
            SetupInput();
            PlatformInitalize(sender, e);
            if (Thread.CurrentThread.ManagedThreadId == _uiThreadId)
            {
                this.PaintSurface += OnRendering;
                InitializeGameLoop(() => OnUpdate(), e, false);
            }
            else
            {
                this.PaintSurface += OnRenderingLock;
                RunOnUiThread(() =>
                {
                    InitializeGameLoop(() => OnUpdateLock(), e, true);
                });
            }
        }

        private void SetupInput()
        {
        }

        private void InitializeGameLoop(Action onUpdate, SKNativePaintGLSurfaceEventArgs e, bool isMultiThreaded)
        {
            // Run update loop and register for rendering events.
            onUpdate();

            _renderLoop = new RenderLoop();
            _renderLoop.Start(onUpdate);
            Initialized?.Invoke(this, e.Surface);
        }

        /// <summary>
        /// Platform-specific initialization steps.
        /// </summary>
        partial void PlatformInitalize(object sender, SKNativePaintGLSurfaceEventArgs e);

        /// <summary>
        /// Invalidates render surface.
        /// </summary>
        partial void InvalidateSurface();

        /// <summary>
        /// Runs an action on the UI thread
        /// </summary>
        /// <param name="action"></param>
        partial void RunOnUiThread(Action action);

        partial void BeforeUpdate();

        private void OnUpdate()
        {
            BeforeUpdate();

            // Note that run loop should be run on Main Thread.
            Update?.Invoke(this, EventArgs.Empty);

            if (IsDirty)
            {
                _shouldRender = true;
                InvalidateSurface();
            }
        }

        private void OnUpdateLock()
        {
            lock (_renderLock)
            {
                OnUpdate();
            }
        }

        private void OnRendering(object sender, SKNativePaintGLSurfaceEventArgs e)
        {
            _shouldRender = false;
            var surface = e.Surface;
            var canvas = surface.Canvas;

            var worldMatrix = SKMatrix.MakeIdentity();
            // Apply DPI scaling.
            SKMatrix.Concat(ref worldMatrix, worldMatrix, SKMatrix.MakeScale(ScaleFactor, ScaleFactor));

            canvas.ResetMatrix();
            canvas.SetMatrix(worldMatrix);

            Draw?.Invoke(this, e.Surface);
        }

        private void OnRenderingLock(object sender, SKNativePaintGLSurfaceEventArgs e)
        {
            lock (_renderLock)
            {
                OnRendering(sender, e);
            }
        }

    }
}
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Overlay.NET.Common;
using Overlay.NET.Directx;
using Process.NET.Windows;

namespace AvadaKedavraHelper
{
    public class Overlay : DirectXOverlayPlugin
    {
        private readonly TickEngine _tickEngine = new TickEngine();
        public OverlaySetting setting {  get; set; }
        private int _framerate = 1000/30;
        private int _displayFps;
        private int _font;
        private int _fpsFont;
        private int _i;
        private int _interiorBrush;
        private int _redBrush;
        private int _redOpacityBrush;
        private float _rotation;
        private Stopwatch _watch;

        public override void Initialize(IWindow targetWindow)
        {
            // Set target window by calling the base method
            base.Initialize(targetWindow);

            var type = GetType();


            OverlayWindow = new DirectXOverlayWindow(targetWindow.Handle, false);
            _watch = Stopwatch.StartNew();

            _redBrush = OverlayWindow.Graphics.CreateBrush(0x7FFF0000);
            _redOpacityBrush = OverlayWindow.Graphics.CreateBrush(Color.FromArgb(80, 255, 0, 0));
            _interiorBrush = OverlayWindow.Graphics.CreateBrush(0x7FFFFF00);

            _fpsFont = OverlayWindow.Graphics.CreateFont("Arial", 25, true);

            _rotation = 0.0f;
            _displayFps = 0;
            _i = 0;
            // Set up update interval and register events for the tick engine.

            _tickEngine.PreTick += OnPreTick;
            _tickEngine.Tick += OnTick;
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!OverlayWindow.IsVisible)
            {
                return;
            }

            OverlayWindow.Update();
            InternalRender();
        }

        private void OnPreTick(object sender, EventArgs e)
        {
            var targetWindowIsActivated = TargetWindow.IsActivated;
            if (!targetWindowIsActivated && OverlayWindow.IsVisible)
            {
                _watch.Stop();
                ClearScreen();
                OverlayWindow.Hide();
            }
            else if (targetWindowIsActivated && !OverlayWindow.IsVisible)
            {
                OverlayWindow.Show();
            }
        }

        // ReSharper disable once RedundantOverriddenMember
        public override void Enable()
        {
            _tickEngine.Interval = _framerate.Milliseconds();
            _tickEngine.IsTicking = true;
            base.Enable();
        }

        // ReSharper disable once RedundantOverriddenMember
        public override void Disable()
        {
            _tickEngine.IsTicking = false;
            base.Disable();
        }

        public override void Update() => _tickEngine.Pulse();

        protected void InternalRender()
        {
            if (!_watch.IsRunning)
            {
                _watch.Start();
            }

            OverlayWindow.Graphics.BeginScene();
            OverlayWindow.Graphics.ClearScene();

            if (setting.ToggleDrawUI)
            {
                //Draw Here
                if (setting.PlayerX != -1)
                {
                    OverlayWindow.Graphics.DrawRectangle(setting.PlayerX, setting.PlayerY, setting.PlayerWidth, setting.PlayerHeight, 2, _redBrush);
                    OverlayWindow.Graphics.DrawLine(setting.PlayerX + setting.PlayerWidth / 2, setting.PlayerY + setting.PlayerHeight - 8, 
                                                    Cursor.Position.X, Cursor.Position.Y, 2, _redBrush);
                }


                _rotation += 0.03f; //related to speed

                if (_rotation > 50.0f) //size of the swastika
                {
                    _rotation = -50.0f;
                }

                if (_watch.ElapsedMilliseconds > 1000)
                {
                    _i = _displayFps;
                    _displayFps = 0;
                    _watch.Restart();
                }

                else
                {
                    _displayFps++;
                }

                OverlayWindow.Graphics.DrawText("FPS: " + _i, _fpsFont, _redBrush, 1820, 10, false);
            }

            OverlayWindow.Graphics.EndScene();
        }

        public override void Dispose()
        {
            OverlayWindow.Dispose();
            base.Dispose();
        }

        private void ClearScreen()
        {
            OverlayWindow.Graphics.BeginScene();
            OverlayWindow.Graphics.ClearScene();
            OverlayWindow.Graphics.EndScene();
        }
    }
}
using Process.NET;
using Process.NET.Memory;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using Yolov5Net.Scorer;
using Yolov5Net.Scorer.Models;
using System.Windows.Forms;
using System.Drawing.Imaging;
using Microsoft.ML.OnnxRuntime;
using Gma.System.MouseKeyHook;

namespace AvadaKedavraHelper
{
    class Program
    {
        const String PROCCESS_NAME = "harrypotter";
        static Overlay overlay = new Overlay();
        static OverlaySetting setting = new OverlaySetting();

        static void Main(string[] args)
        {



            //Register hotkey
            HotKeyManager.RegisterHotKey(Keys.F2, KeyModifiers.NoRepeat);
            HotKeyManager.HotKeyPressed += new EventHandler<HotKeyEventArgs>(HotKeyManager_HotKeyPressed);

            //Hook button
            //Hook.GlobalEvents().MouseDown += MouseButton_Down;

            ProcessSharp processSharp;
            Console.WriteLine("Waiting for the process 'harrypotter.exe' to start.....");
            System.Diagnostics.Process process = null;

            while (process == null)
            {
                process = System.Diagnostics.Process.GetProcessesByName(PROCCESS_NAME).FirstOrDefault();
                Thread.Sleep(500);
            }
            Console.WriteLine("Process 'harrypotter.exe' is found, start to inject.....");
            processSharp = new ProcessSharp(process, MemoryType.Remote);
            overlay.Initialize(processSharp.WindowFactory.MainWindow);
            overlay.Enable();

            using var scorer = new YoloScorer<HarryPotterModel>("model/harrypotter.onnx");

            while (true)
            {
               if (setting.ToggleDrawUI)
                {
                    Image image = ScreenCapture();
                    //Start Inference               
                    List<YoloPrediction> predictions = scorer.Predict(image);
                    setting.PlayerX = -1;
                    foreach (var prediction in predictions) // iterate predictions to draw results
                    {
                        setting.PlayerX = (int)Math.Round(prediction.Rectangle.X);
                        setting.PlayerY = (int)Math.Round(prediction.Rectangle.Y);
                        setting.PlayerWidth = (int)Math.Round(prediction.Rectangle.Width);
                        setting.PlayerHeight = (int)Math.Round(prediction.Rectangle.Height);

                        //Check result
                        /*
                        double score = Math.Round(prediction.Score, 2);

                        using var graphics = Graphics.FromImage(image);
                        graphics.DrawRectangles(new Pen(prediction.Label.Color, 1),
                            new[] { prediction.Rectangle });

                        var (x, y) = (prediction.Rectangle.X - 3, prediction.Rectangle.Y - 23);

                        graphics.DrawString($"{prediction.Label.Name} ({score})",
                            new Font("Arial", 16, GraphicsUnit.Pixel), new SolidBrush(prediction.Label.Color),
                            new PointF(x, y));
                        */

                    }
                    
                }
                overlay.setting = setting;
                overlay.Update();
            }
        }


        static void HotKeyManager_HotKeyPressed(object sender, HotKeyEventArgs e)
        {
            setting.ToggleDrawUI = !setting.ToggleDrawUI;
        }

        private static async void MouseButton_Down(object sender, MouseEventArgs e)
        {
            Console.WriteLine(e.Button.ToString());
        }

        private static Bitmap ScreenCapture()
        {
            Bitmap bm = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format24bppRgb);
            Graphics g = Graphics.FromImage(bm);
            g.CopyFromScreen(0, 0, 0, 0, bm.Size);
            return bm;
        }
    }
}


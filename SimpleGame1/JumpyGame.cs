using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using SimpleGame1Trainer;

namespace SimpleGame1
{
    public class JumpyGameLogic : GameLogic
    {
        private readonly Random _randomEngine = new Random(DateTime.Now.Second);
        private const int ProbabilitySeed = 60;
        private bool _jumpedThisFrame = false;

        public Tuple<double, double> NetWantsToJump = new Tuple<double, double>(0.0, 0.0);

        public Trainer JumpyTrainer = new Trainer();
        public FixedSizedQueue<JumpyReflexData> LastReflexes = new FixedSizedQueue<JumpyReflexData>() { Limit = 5 };

        public override void PrepareGame(GameEngine engine)
        {
            JumpyTrainer.Init();
            engine.AddObject(new Jumpy());
        }

        public override void FrameChores(GameEngine engine)
        {
            engine.GameObjects.RemoveAll(item => item.Location.X < 0);
            if (_randomEngine.Next(1, ProbabilitySeed) < 2 && !engine.GameObjects.Any(item => item.Location.X > 300))
                engine.AddObject(new Obstacle());

            NetWantsToJump = JumpyTrainer.Forward(new JumpyReflexData {Features = GetFeatures(engine)});
            if(NetWantsToJump.Item1 > .5)
                (engine.GameObjects.First(item=>item.GetType()==typeof(Jumpy)) as Jumpy)?.Jump();
        }

        public override bool CheckGameLogic(GameEngine engine)
        {
            var jumpy = engine.GameObjects.FirstOrDefault(item => item.GetType() == typeof(Jumpy));
            var gameEnds = engine.GameObjects.Where(item => item.GetType() == typeof(Obstacle)).Any(obstacle => Collide(jumpy, obstacle));

            LastReflexes.Enqueue(new JumpyReflexData { Features = GetFeatures(engine), Jump = _jumpedThisFrame });
            _jumpedThisFrame = false;
            //JumpyTrainer.Train(LastReflexes.ToList(), !gameEnds);
            JumpyConfig.Log = GetArrayString(GetFeatures(engine)) + "   " + (engine.GameObjects.Count - 1) + "\r\n" + JumpyConfig.Log;
            return gameEnds;
        }

        private string GetArrayString(double[] getFeatures)
        {
            return string.Join(",", getFeatures);
        }

        private int iteration = 0;
        private double max0 = 0;
        public override void DrawBackground(Graphics graphicsEngine)
        {
            try
            {
                graphicsEngine.FillRectangle(JumpyConfig.BackgroundColor, 0, 0, JumpyConfig.GameHeight,
                    JumpyConfig.GameWidth);
                graphicsEngine.DrawLine(JumpyConfig.LineColor, 0, JumpyConfig.BaseLine, JumpyConfig.GameWidth,
                    JumpyConfig.BaseLine);
                graphicsEngine.DrawString(NetWantsToJump.Item1 + " , " + NetWantsToJump.Item2,
                    new Font(new FontFamily("Arial"), 10), NetWantsToJump.Item1 > 0.5 ? Brushes.Red : Brushes.Black, 10,
                    10);
                graphicsEngine.DrawString(max0.ToString(), new Font(new FontFamily("Arial"), 10), Brushes.Black, 10,
                    30);

                if (iteration++>300)
                    if (NetWantsToJump.Item1 > max0) max0 = NetWantsToJump.Item1;
            }
            catch { }
        }

        public override void KeyPressed(Keys key)
        {
            if (key == Keys.Space)
                _jumpedThisFrame = true;
        }

        private bool Collide(GameObject a, GameObject b)
        {
            return !Rectangle.Intersect(a.ObjectRectangle, b.ObjectRectangle).IsEmpty;
        }

        private double[] GetFeatures(GameEngine engine)
        {
            var resp = new double[10];
            var objects = engine.GameObjects.Where(item => item.Location.X < 300).ToList();
            foreach (var gameObject in objects)
            {
                resp[gameObject.Location.X/30] = 1.0;
            }
            return resp;
        }
    }


    public static class JumpyConfig
    {
        public static int GameWidth => 600;
        public static int GameHeight => 600;
        public static int BaseLine => 250;
        public static int JumpyLocationX => 30;
        public static int JumpyWidth => 30;
        public static int JumpyHeight => 10;
        public static int JumpHeight => 20;
        public static int JumpDistance => 150;
        public static Brush JumpyColor => Brushes.Orange;
        public static Brush ObstacleColor => Brushes.BlueViolet;
        public static int ObstacleWidth => 30;
        public static int ObstacleHeight => 10;
        public static int ObstacleDefaultLocationX => 500;
        public static int ObstacleStep => 5;
        public static Brush BackgroundColor => Brushes.White;
        public static Pen LineColor => Pens.LightGray;
        public static string Log { get; set; }
    }

    public class Obstacle : GameObject
    {
        public Obstacle()
        {
            BaseColor = JumpyConfig.ObstacleColor;
            Location = new Point(JumpyConfig.ObstacleDefaultLocationX, JumpyConfig.BaseLine - JumpyConfig.ObstacleHeight);
            ObjectSize = new Size(JumpyConfig.ObstacleWidth, JumpyConfig.ObstacleHeight);
        }

        public override void FramePassed()
        {
            Location = new Point(Location.X - JumpyConfig.ObstacleStep, Location.Y);
            base.FramePassed();
        }
    }

    public class Jumpy : GameObject
    {
        private int _currentFrameOnJump = 0;
        private bool _onJump = false;

        public Jumpy()
        {
            BaseColor = JumpyConfig.JumpyColor;
            Location = new Point(JumpyConfig.JumpyLocationX, JumpyConfig.BaseLine - JumpyConfig.JumpyHeight);
            ObjectSize = new Size(JumpyConfig.JumpyWidth, JumpyConfig.JumpyHeight);
        }

        public override void FramePassed()
        {
            _currentFrameOnJump += JumpyConfig.ObstacleStep;
            if (_onJump && _currentFrameOnJump >= JumpyConfig.JumpDistance)
                Fall();

            base.FramePassed();
        }

        public override void OnKeyPressed(Keys key)
        {
            if (key == Keys.Space)
                Jump();
            base.OnKeyPressed(key);
        }

        public void Jump()
        {
            if (_onJump) return;
            _onJump = true;
            _currentFrameOnJump = 0;
            Location = new Point(Location.X, Location.Y - JumpyConfig.JumpHeight);
        }

        public void Fall()
        {
            if (!_onJump) return;
            _onJump = false;
            Location = new Point(Location.X, Location.Y + JumpyConfig.JumpHeight);
        }
    }

    public class FixedSizedQueue<T> : ConcurrentQueue<T>
    {
        public int Limit { get; set; }
        public new void Enqueue(T obj)
        {
            base.Enqueue(obj);
            T overflow;
            while (Count > Limit && TryDequeue(out overflow)) ;
        }
    }
}

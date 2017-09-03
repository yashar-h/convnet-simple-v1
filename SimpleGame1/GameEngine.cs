using System;
using System.Collections.Generic;
using System.Drawing;
using System.Timers;
using System.Windows.Forms;
using Timer = System.Timers.Timer;

namespace SimpleGame1
{
    public class GameEngine
    {
        public Timer FrameTimer { get; set; }
        public int FrameRateMs { get; set; }
        public Graphics GraphicsEngine { get; set; }
        public GameLogic Logic { get; set; }
        public List<GameObject> GameObjects { get; set; }

        protected delegate void FramePassed();
        protected event FramePassed OnFramePassed;

        protected delegate void DrawAll(Graphics graphicsEngine);
        protected event DrawAll OnDrawAll;

        protected delegate void OnKeyPress(Keys key);
        protected event OnKeyPress KeyPressed;

        public GameEngine(Graphics graphicsEngine, int frameRateMs, Action<KeyEventHandler> attachKeyboardEventHandler)
        {
            GraphicsEngine = graphicsEngine;
            Logic = new JumpyGameLogic();
            KeyPressed += Logic.KeyPressed;
            KeyPressed += KeyPressedCore;
            GameObjects = new List<GameObject>();
            Logic.PrepareGame(this);

            attachKeyboardEventHandler(KeyboardHandler);
            FrameRateMs = frameRateMs;
            FrameTimer = new Timer(frameRateMs);
            FrameTimer.Elapsed += FrameTimerOnElapsed;

            OnFramePassed += LogicChores;
        }

        public KeyEventHandler GetKeyboardHandler => KeyboardHandler;

        public void KeyboardHandler(object sender, KeyEventArgs keyEventArgs)
        {
            KeyPressed?.Invoke(keyEventArgs.KeyCode);
        }

        private void FrameTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            OnFramePassed?.Invoke();
            DrawBackgroundAndObjects(GraphicsEngine);
        }

        public void StartGame()
        {
            FrameTimer.Start();
        }

        public void StopGame()
        {
            FrameTimer.Stop();
        }

        public void ResetGame()
        {
            GameObjects.RemoveAll(item => item.GetType() == typeof (Obstacle));
            FrameTimer.Start();
        }

        private void LogicChores()
        {
            if (Logic.CheckGameLogic(this))
            {
                //StopGame();
                ResetGame();
            }
            Logic.FrameChores(this);
        }

        public void DrawBackgroundAndObjects(Graphics graphicsEngine)
        {
            Logic.DrawBackground(graphicsEngine);
            OnDrawAll?.Invoke(GraphicsEngine);
        }
        
        public void AddObject(GameObject obj)
        {
            GameObjects.Add(obj);
            OnFramePassed += obj.FramePassed;
            OnDrawAll += obj.Draw;
            KeyPressed += obj.OnKeyPressed;
        }

        public void KeyPressedCore(Keys key)
        {
            if (key == Keys.Escape)
            {
                if (FrameTimer.Enabled)
                {
                    FrameTimer.Stop();
                }
                else
                {
                    FrameTimer.Start();
                }
            }

            //if (key == Keys.Enter)
            //{
            //    ResetGame();
            //}
        }
    }

    public abstract class GameLogic
    {
        public virtual void PrepareGame(GameEngine engine)
        {
        }

        public virtual void FrameChores(GameEngine engine)
        {
        }

        public virtual bool CheckGameLogic(GameEngine engine)
        {
            return true;
        }

        public virtual void DrawBackground(Graphics graphicsEngine)
        {
        }

        public virtual void KeyPressed(Keys key)
        {
        }
    }

    public abstract class GameObject
    {
        public Point Location { get; set; }
        public Brush BaseColor { get; set; }
        public Size ObjectSize { get; set; }

        public Rectangle ObjectRectangle => new Rectangle(Location, ObjectSize);

        public virtual void FramePassed()
        {

        }

        public virtual void Draw(Graphics graphicsEngine)
        {
            try
            {
                graphicsEngine.FillRectangle(BaseColor, new Rectangle(Location, ObjectSize));
            }
            catch { }
        }

        public virtual void OnKeyPressed(Keys key)
        {

        }
    }
}

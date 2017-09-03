using System;
using System.Windows.Forms;

namespace SimpleGame1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private GameEngine _engine;
        private void Form1_Load(object sender, EventArgs e)
        {
            _engine = new GameEngine(panel1.CreateGraphics(), 20, handler => KeyDown += handler);
            _engine.StartGame();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            textBox1.Text = JumpyConfig.Log;
        }
    }

    
}

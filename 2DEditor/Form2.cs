using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _2DEditor
{
    public partial class Form2 : Form
    {
        Graphics pbGraph;
        Graphics imgGraph;
        Image fonImg;
        List<float> rad;
        List<float> angl;
        Point Center;
        Point[] Figure;
        int GLangl = 0;
        float curAngl = 0;
        float curRad = 1;
        int xCoord = 0, yCoord = 0;
        bool grab = false;
        public Form2(List<float> _rad, List<float> _angl)
        {
            InitializeComponent();
            pictureBox1.BackColor = Color.White;
            rad = _rad; angl = _angl;
            if (rad.Count == 0) this.Close();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            trackBar1.Enabled = true;
            trackBar1.Value = 50;  
            fonImg = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            pbGraph = pictureBox1.CreateGraphics();
            imgGraph = Graphics.FromImage(fonImg);
            Center = new Point(pictureBox1.Width / 2, pictureBox1.Height / 2);
            Figure = new Point[rad.Count];            
            for(int i = 0; i < Figure.Length; ++i)
            {
                curAngl = angl[i] + GLangl; if (curAngl >= 360) curAngl -= 360;
                xCoord = Center.X + Convert.ToInt32(((curAngl < 180) ? Math.Cos((curAngl) / 180 * Math.PI)
                    : -Math.Cos((curAngl - 180) / 180 * Math.PI)) * rad[i]);
                yCoord = Center.Y + Convert.ToInt32(((curAngl < 180) ? Math.Sin((curAngl) / 180 * Math.PI)
                    : -Math.Sin((curAngl - 180) / 180 * Math.PI)) * rad[i]);
                Figure[i] = new Point(xCoord, yCoord);
            }
            timer1.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Draw();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GLangl += 3;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            GLangl -= 3;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            grab = true;
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            grab = false;
        }

        private void Form2_SizeChanged(object sender, EventArgs e)
        {
            fonImg = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            pbGraph = pictureBox1.CreateGraphics();
            imgGraph = Graphics.FromImage(fonImg);
        }

        void Draw()
        {
            if (grab) { Center = new Point(Cursor.Position.X - this.Location.X - 20, Cursor.Position.Y - this.Location.Y - 42); }
            imgGraph.FillRectangle(Brushes.White, 0, 0, pictureBox1.Width, pictureBox1.Height);
            for (int i = 0; i < Figure.Length; ++i)
            {
                curAngl = angl[i] + GLangl; if (curAngl >= 360) curAngl -= 360;
                curRad = rad[i] * trackBar1.Value / 50;
                xCoord = Center.X + Convert.ToInt32(((curAngl < 180) ? Math.Cos((curAngl) / 180 * Math.PI)
                    : -Math.Cos((curAngl - 180) / 180 * Math.PI)) * curRad);
                yCoord = Center.Y + Convert.ToInt32(((curAngl < 180) ? Math.Sin((curAngl) / 180 * Math.PI)
                    : -Math.Sin((curAngl - 180) / 180 * Math.PI)) * curRad);
                Figure[i] = new Point(xCoord, yCoord);
            }
            imgGraph.FillPolygon(Brushes.Black, Figure);
            imgGraph.FillRectangle(Brushes.White, Center.X, Center.Y, 1, 1);
            pbGraph.DrawImage(fonImg, 0, 0); 
        }
    }
}

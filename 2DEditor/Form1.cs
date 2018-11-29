using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _2DEditor
{
    public partial class Form1 : Form
    {
        Graphics pbGraph; //рисуем на pb
        Graphics pbReal;
        Image bgImg; //фоновая картинка
        Image ImgLoad; //подргужаемый фон
        List<Point> Log; //все добавленные и отменённые точки
        List<Point> ActualPoints; //все активные точки
        Point MainPoint; //центр вращения
        Point startRect, startField, endField; //выделение обласи для удаления
        List<Point> DelPoint; //удаляемые точки
        List<float> rad = new List<float>();
        List<float> angl = new List<float>();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            bgImg = new Bitmap(pictureBox1.Width,pictureBox1.Height);
            Log = new List<Point>();
            ActualPoints = new List<Point>();
            pbGraph = Graphics.FromImage(bgImg);    
            pbReal = pictureBox1.CreateGraphics();
            pictureBox1.BackColor = Color.White;
            MainPoint = new Point();
        }

        private void button1_Click(object sender, EventArgs e)
        { 
            if(ActualPoints.Count!=0 && MainPoint!=new Point())
            {
                SaveFileDialog sd = new SaveFileDialog();
                sd.Filter = "Text files(*.txt)|*.txt|All files(*.*)|*.*";
                if (sd.ShowDialog() == DialogResult.OK)
                {
                    angl = new List<float>();
                    rad = new List<float>();
                    for (int i = 0; i < ActualPoints.Count; ++i)
                    {
                        int first = MainPoint.X - ActualPoints[i].X;
                        int second = MainPoint.Y - ActualPoints[i].Y;
                        rad.Add(Convert.ToSingle(Math.Sqrt(Math.Pow((MainPoint.X - ActualPoints[i].X), 2) +
                            Math.Pow((MainPoint.Y - ActualPoints[i].Y), 2))));
                        if (ActualPoints[i].Y <= MainPoint.Y) angl.Add(Convert.ToSingle(Math.Acos((ActualPoints[i].X - MainPoint.X) / rad[i]) / Math.PI * 180));
                        else angl.Add(Convert.ToSingle(360 - Math.Acos((ActualPoints[i].X - MainPoint.X) / rad[i]) / Math.PI * 180));
                    }
                    using (StreamWriter sr = new StreamWriter(sd.FileName))
                    {
                        sr.WriteLine(MainPoint.X.ToString() + " " + MainPoint.Y.ToString());
                        for (int i = 0; i < rad.Count; ++i)
                        {
                            sr.WriteLine(rad[i].ToString() + " " + angl[i].ToString());
                        }                        
                    }
                }                
            }
            else { MessageBox.Show("Empty points or center point"); }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog od = new OpenFileDialog();
            if( od.ShowDialog()== DialogResult.OK)
            {
                ImgLoad = new Bitmap(od.FileName);
                ReDraw();
            }
        }
        
        bool hasNearPoints(Point check)
        {
            for(int i = 0; i < ActualPoints.Count; ++i)            
                if (ActualPoints[i].X >= check.X - 3 && ActualPoints[i].X <= check.X + 3
                    && ActualPoints[i].Y >= check.Y - 3 && ActualPoints[i].Y <= check.Y + 3) return true;            
            return false;
        }
        Point GetPoint(Point check)
        {
            for (int i = 0; i < ActualPoints.Count; ++i)            
                if (ActualPoints[i].X >= check.X - 3 && ActualPoints[i].X <= check.X + 3
                    && ActualPoints[i].Y >= check.Y - 3 && ActualPoints[i].Y <= check.Y + 3) return ActualPoints[i];             
            return new Point();
        }
        
        void ReDraw()
        {
            pbGraph.FillRectangle(Brushes.White, 0, 0, pictureBox1.Width, pictureBox1.Height);
            if (ImgLoad != null) pbGraph.DrawImage(ImgLoad, 0, 0);
            if (ImgLoad!=null) pbGraph.DrawImage(ImgLoad, 0, 0);
            for (int i = 0; i < ActualPoints.Count; ++i)
            {
                pbGraph.FillRectangle(Brushes.Red, ActualPoints[i].X - 3, ActualPoints[i].Y - 3, 7, 7);
                pbGraph.FillRectangle(Brushes.Black, ActualPoints[i].X - 2, ActualPoints[i].Y - 2, 5, 5);
                pbGraph.FillRectangle(Brushes.Red, ActualPoints[i].X - 1, ActualPoints[i].Y - 1, 3, 3);
                pbGraph.FillRectangle(Brushes.White, ActualPoints[i].X, ActualPoints[i].Y, 1, 1);
            }
            if(MainPoint!= new Point())
            {
                pbGraph.FillRectangle(Brushes.Green, MainPoint.X - 4, MainPoint.Y - 4, 9, 9);
                pbGraph.FillRectangle(Brushes.Green, MainPoint.X - 3, MainPoint.Y - 3, 7, 7);
                pbGraph.FillRectangle(Brushes.Black, MainPoint.X - 2, MainPoint.Y - 2, 5, 5);
                pbGraph.FillRectangle(Brushes.Green, MainPoint.X - 1, MainPoint.Y - 1, 3, 3);
                pbGraph.FillRectangle(Brushes.White, MainPoint.X, MainPoint.Y, 1, 1);
            }
            pbReal.DrawImage(bgImg, 0, 0);
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && !hasNearPoints(e.Location))
            {
                if(checkBox1.Checked==true)
                {
                    MainPoint = new Point(e.X, e.Y);
                    ReDraw();
                }
                else
                {
                    Point tmp = new Point(e.X, e.Y);
                    Log.Add(tmp);
                    ActualPoints.Add(tmp);
                    ReDraw();
                }
            }
            else
            {
                if( e.Button == MouseButtons.Right && hasNearPoints(e.Location))
                {
                    Point tmp = GetPoint(e.Location);
                    Log.Add(tmp); 
                    ActualPoints.Remove(tmp);
                    ReDraw();
                }
            }
        }

        /// <summary>
        /// Раздел отвечающий за выделение точек правой кнопкой и их удаление
        /// </summary>
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                timer1.Enabled = true;
                startRect = e.Location;
                DelPoint = new List<Point>();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            pbGraph.FillRectangle(Brushes.White, 0, 0, pictureBox1.Width, pictureBox1.Height);
            if (ImgLoad != null) pbGraph.DrawImage(ImgLoad, 0, 0);
            for (int i = 0; i < ActualPoints.Count; ++i)
            {
                pbGraph.FillRectangle(Brushes.Red, ActualPoints[i].X - 3, ActualPoints[i].Y - 3, 7, 7);
                pbGraph.FillRectangle(Brushes.Black, ActualPoints[i].X - 2, ActualPoints[i].Y - 2, 5, 5);
                pbGraph.FillRectangle(Brushes.Red, ActualPoints[i].X - 1, ActualPoints[i].Y - 1, 3, 3);
                pbGraph.FillRectangle(Brushes.White, ActualPoints[i].X, ActualPoints[i].Y, 1, 1);
            }
            if (MainPoint != new Point())
            {
                pbGraph.FillRectangle(Brushes.Green, MainPoint.X - 4, MainPoint.Y - 4, 9, 9);
                pbGraph.FillRectangle(Brushes.Green, MainPoint.X - 3, MainPoint.Y - 3, 7, 7);
                pbGraph.FillRectangle(Brushes.Black, MainPoint.X - 2, MainPoint.Y - 2, 5, 5);
                pbGraph.FillRectangle(Brushes.Green, MainPoint.X - 1, MainPoint.Y - 1, 3, 3);
                pbGraph.FillRectangle(Brushes.White, MainPoint.X, MainPoint.Y, 1, 1);
            }
            int xLoc = (Cursor.Position.X - this.Location.X - 20) - startRect.X ;
            int yLoc = (Cursor.Position.Y - this.Location.Y - 57) - startRect.Y ;
            if (xLoc > 0)
                if (yLoc > 0) { pbGraph.DrawRectangle(Pens.Black, startRect.X, startRect.Y, xLoc, yLoc); startField = startRect;
                    endField = new Point(Cursor.Position.X - this.Location.X - 20, Cursor.Position.Y - this.Location.Y - 57);
                }
                else { pbGraph.DrawRectangle(Pens.Black, startRect.X, (Cursor.Position.Y - this.Location.Y - 57), xLoc, -yLoc);
                    startField = new Point(startRect.X, (Cursor.Position.Y - this.Location.Y - 57));
                    endField = new Point(Cursor.Position.X - this.Location.X - 20,startRect.Y);
                }
            else if (yLoc > 0) { pbGraph.DrawRectangle(Pens.Black, (Cursor.Position.X - this.Location.X - 20), startRect.Y, -xLoc, yLoc);
                startField = new Point(Cursor.Position.X - this.Location.X - 20, startRect.Y);
                endField = new Point(startRect.X, (Cursor.Position.Y - this.Location.Y - 57));
            }
            else { pbGraph.DrawRectangle(Pens.Black, (Cursor.Position.X - this.Location.X - 20),
                (Cursor.Position.Y - this.Location.Y - 57), -xLoc, -yLoc);
                startField = new Point(Cursor.Position.X - this.Location.X - 20, Cursor.Position.Y - this.Location.Y - 57);
                endField = startRect;
            }
            pbReal.DrawImage(bgImg, 0, 0);
        }
                
        List<Point> InRect()
        {
            List<Point> res = new List<Point>();
            for (int i = 0; i < ActualPoints.Count; ++i)
                if (ActualPoints[i].X >= startField.X && ActualPoints[i].X <= endField.X
                    && ActualPoints[i].Y >= startField.Y && ActualPoints[i].Y <= endField.Y) { res.Add(ActualPoints[i]); }
            return res;
        }

        void RefreshPointInRect(List<Point> lst)
        {
            DelPoint = lst;
            for (int i = 0; i < lst.Count; ++i)
            {
                pbGraph.FillRectangle(Brushes.Blue, lst[i].X - 3, lst[i].Y - 3, 7, 7);
                pbGraph.FillRectangle(Brushes.Black, lst[i].X - 2, lst[i].Y - 2, 5, 5);
                pbGraph.FillRectangle(Brushes.Blue, lst[i].X - 1, lst[i].Y - 1, 3, 3);
                pbGraph.FillRectangle(Brushes.White, lst[i].X, lst[i].Y, 1, 1);
            }
            pbReal.DrawImage(bgImg, 0, 0);
        }
                
        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            try
            {
                bgImg = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                pbGraph = Graphics.FromImage(bgImg);
                pbReal = pictureBox1.CreateGraphics();
                pictureBox1.BackColor = Color.White;
                ReDraw();
            }
            catch { }
        }
        //???
        private void rollbackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Log.Count > 1)
            {
                Point tmp = Log.Last();
                int k = 0;
                for (int i = Log.Count - 1; i > 0; --i)
                    if (Log[i] == tmp) k++;
                if (k % 2 == 0) { ActualPoints.Add(tmp); Log.RemoveAt(Log.Count - 1); }
                else { ActualPoints.Remove(tmp); Log.RemoveAt(Log.Count - 1); }
            }
            else if (Log.Count != 0) { Log = new List<Point>(); ActualPoints = new List<Point>(); }
            else { Log.Clear(); ActualPoints.Clear(); }
            ReDraw();
        }

        private void clearAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Log = new List<Point>();
            ActualPoints = new List<Point>();
            ReDraw();
        }

        private void centerPointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            checkBox1.Checked = !checkBox1.Checked;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (angl.Count != 0)
            {
                Form2 fr = new Form2(rad, angl);
                fr.Show();
            }
        }

        private void deleteSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < DelPoint.Count; ++i)
            {
                ActualPoints.Remove(DelPoint[i]);
            }
            DelPoint = new List<Point>();
            ReDraw();
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                timer1.Enabled = false;
                ReDraw();
                RefreshPointInRect(InRect());
            }
        }
    }
}

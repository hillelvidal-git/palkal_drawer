using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;

namespace NewPalkalDrawer
{
    public partial class RotateLevelForm : Form
    {
        public double angleToRotate;
        Editor acEd;

        public RotateLevelForm(Editor ed)
        {
            this.acEd = ed;
            InitializeComponent();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            this.button2.Visible = (this.textBox2.Text == "אני מסכים לגמרי");
            this.groupBox1.Enabled = (this.textBox2.Text == "אני מסכים לגמרי");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //שמור את בחירת המשתמש
            this.angleToRotate = Convert.ToDouble(this.numericUpDown1.Value);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //המשתמש בוחר קו ע"י שתי נקודות בשרטוט

            try
            {
                this.Hide(); //End form interaction
                acEd.WriteMessage("\nשרטט קו שיהיה קו הרוחב של השרטוט לאחר הסיבוב!");
                Point3d[] pts = DrawBL.AcadTools.GetUserLine();
                double RadAngle2D = GetRotaion(pts);
                acEd.WriteMessage("\n.......\nRotation Angle: " + RadAngle2D);
                this.numericUpDown1.Value = (decimal)RadAngle2D;
            }
            catch (Exception ee)
            {
                MessageBox.Show("הפעולה בוטלה\n" + ee.Message);
            }
            finally
            {
                this.Show();
            }
        }

        private double GetRotaion(Point3d[] pts)
        {
            double dx = pts[1].X - pts[0].X;
            double dy = pts[1].Y - pts[0].Y;

            acEd.WriteMessage("\ndX = " + dx);
            acEd.WriteMessage("\ndY = " + dy);

            if (dx == 0)
            {
                acEd.WriteMessage("\ndX is 0!");
                if (dy > 0) return (Math.PI / 2);
                else return (0 - Math.PI / 2);
            }

            if (dy == 0)
            {
                acEd.WriteMessage("\ndY is 0!");
                if (dx < 0) return 0;
                else return Math.PI;
            }

            int quarter;
            if (dx > 0)
                if (dy > 0) quarter = 1;
                else quarter = 4;
            else
                if (dy > 0) quarter = 2;
                else quarter = 3;

            acEd.WriteMessage("\nQuarter: " + quarter);

            double absAngle = Math.Atan(Math.Abs(dy) / Math.Abs(dx));
            acEd.WriteMessage("\nAbs Angle: " + absAngle);

            switch (quarter)
            {
                case 1:
                    return 0 - absAngle;
                case 2:
                    return absAngle;
                case 3:
                    return 0 - absAngle;
                case 4:
                    return absAngle;
            }

            return 0;
        }
    }
}

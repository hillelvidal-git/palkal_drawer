using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;

namespace NewPalkalDrawer
{
    public partial class ImportPointsForm : Form
    {
        List<string[]> points = new List<string[]>();
        Editor acEd;

        public ImportPointsForm(Editor ed)
        {
            this.acEd = ed;
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.lblFilePath.Text = this.openFileDialog1.FileName;
                this.lblResult.Text = TryParseFile(this.openFileDialog1.FileName);
            }
        }

        private string TryParseFile(string p)
        {
            using (StreamReader sr = new StreamReader(p))
            {
                while (!sr.EndOfStream)
                {
                    try
                    {
                        TryParseLine(sr.ReadLine());
                    }
                    catch { }
                }
            }

            return points.Count.ToString() + " Points in File.";
        }

        private void TryParseLine(string line)
        {
            string[] words = new string[4];

            int s = 0;
            int e = line.IndexOf(","); //fisrt comma
            words[0] = line.Substring(s, e);

            s = e + 1;
            e = line.IndexOf(",", s); //second comma
            words[1] = line.Substring(s, e - s);

            s = e + 1;
            e = line.IndexOf(",", s); //third comma
            words[2] = line.Substring(s, e - s);

            s = e + 1;
            words[3] = line.Substring(s);

            this.points.Add(words);
            this.listBox1.Items.Add(words[0] + ":  " + words[1] + ", " + words[2] + ", " + words[3]);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DrawPoints();
        }

        private void DrawPoints()
        {
            using (Transaction ta = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.TransactionManager.StartTransaction())
            {
                BlockTable bt = ta.GetObject(Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database.BlockTableId, OpenMode.ForWrite) as BlockTable;
                BlockTableRecord ms = ta.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                double[] pos;

                foreach (string[] pointData in this.points)
                {
                    try
                    {
                        pos = new double[] { Convert.ToDouble(pointData[1]), Convert.ToDouble(pointData[2]), Convert.ToDouble(pointData[3]) };

                        //Draw point
                        DBPoint newPoint = new DBPoint(new Autodesk.AutoCAD.Geometry.Point3d(pos));
                        ms.AppendEntity(newPoint);
                        ta.AddNewlyCreatedDBObject(newPoint, true);

                        //Draw text
                        DBText newText = new DBText();
                        newText.Position = new Autodesk.AutoCAD.Geometry.Point3d(pos);
                        newText.TextString = pointData[0];
                        ms.AppendEntity(newText);
                        ta.AddNewlyCreatedDBObject(newText, true);

                    }
                    catch { }
                }

                //אשר את כל העסק
                ta.Commit();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace NewPalkalDrawer
{
    public partial class FishPoints : Form
    {
        DrawBL.BL myBL;
        List<object[]> srv, orbs, gen;

        public FishPoints(DrawBL.BL bl)
        {
            InitializeComponent();
            myBL = bl;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            srv = myBL.acTools.SelectBlocks("SRV_Point");
            label1.Text = "SRV: " + srv.Count;
            orbs = myBL.acTools.SelectBlocks("ORBS_Point");
            label2.Text = "ORBS: " + orbs.Count;
            gen = myBL.acTools.SelectBlocks("GEN_Point");
            label3.Text = "GEN: " + gen.Count;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int[] up = new int[4];

            Point3d pos;
            int levelId = Convert.ToInt32(textBox1.Text);
            int fieldId = Convert.ToInt32(textBox2.Text);
            int classId;
            string num;
            BedekSurveyWebService.ArrayOfAnyType values;
            DateTime time = DateTime.Now;

            using (Transaction ta = myBL.acTools.StartTransaction())
            {
                classId = 0; //SRV
                foreach (object[] pt_srv in this.srv)
                {
                    ObjectId acId = (ObjectId)pt_srv[0];
                    BlockReference br = ta.GetObject(acId, OpenMode.ForRead) as BlockReference;
                    pos = br.Position;
                    num = pt_srv[1].ToString();
                    num = num.Substring(num.IndexOf("_") + 1);
                    values = new BedekSurveyWebService.ArrayOfAnyType{
                        levelId, fieldId, classId,
                        num,"",time,
                        pos.X, pos.Y, pos.Z};
                    if (UploadPt(values)) up[0]++;
                }

                fieldId = -1;
                foreach (object[] pt_orbs in this.orbs)
                {
                    ObjectId acId = (ObjectId)pt_orbs[0];
                    BlockReference br = ta.GetObject(acId, OpenMode.ForRead) as BlockReference;
                    pos = br.Position;
                    num = pt_orbs[1].ToString();
                    if (pt_orbs[2].ToString().StartsWith("BS"))
                        classId = 1;
                    else classId = 2;
                    
                    values = new BedekSurveyWebService.ArrayOfAnyType{
                        levelId, fieldId, classId,
                        num,"",time,
                        pos.X, pos.Y, pos.Z};
                    if (UploadPt(values)) up[classId]++;
                }

                foreach (object[] pt_gen in this.gen)
                {
                    ObjectId acId = (ObjectId)pt_gen[0];
                    BlockReference br = ta.GetObject(acId, OpenMode.ForRead) as BlockReference;
                    pos = br.Position;
                    num = pt_gen[1].ToString();
                    classId = 3;

                    values = new BedekSurveyWebService.ArrayOfAnyType{
                        levelId, fieldId, classId,
                        num,"",time,
                        pos.X, pos.Y, pos.Z};
                    if (UploadPt(values)) up[3]++;
                }
            }

            label6.Text = "SRV: " + up[0];
            label5.Text = "BS: " + up[1];
            label4.Text = "OR: " + up[2];
            label8.Text = "GEN: " + up[3];
        }

        private bool UploadPt(BedekSurveyWebService.ArrayOfAnyType values)
        {
            int webId; string msg;
            try
            {
                BedekSurveyWebService.SurveyServiceSoapClient client = new BedekSurveyWebService.SurveyServiceSoapClient();
                return client.InsertNewMeasurements(values, out webId, out msg);
            }
            catch (Exception e)
            {
                msg = e.Message;
                webId = -1;
                return false;
            }

        }
    }
}

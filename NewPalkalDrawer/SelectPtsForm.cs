using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.ApplicationServices;

namespace NewPalkalDrawer
{
    public partial class SelectPtsForm : Form
    {
        private ObjectId[] LastSelSetIds = new ObjectId[0];
        DrawBL.BL myBL;
        public SelectPtsForm(DrawBL.BL bl)
        {
            this.myBL = bl;
            InitializeComponent();
            LoadProjectDetails();
        }

        private void LoadProjectDetails()
        {
            try
            {
                this.cblDetails.Items.Clear();
                object[][] details = myBL.GetEngDetails();

                this.cblDetails.Items.Add("0. פלטה", true);
                foreach (object[] detail in details)
                {
                    this.cblDetails.Items.Add(detail[0].ToString() + ". " + detail[1].ToString().Trim(), true);
                }
            }
            catch
            {

            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            chkCyan.Enabled = radioButton1.Checked;
            chkPhaseAll.Enabled = rbDatabasePoints.Checked;
            chkPhase0.Enabled = rbDatabasePoints.Checked;
            chkPhase1.Enabled = rbDatabasePoints.Checked;
            chkPhase2.Enabled = rbDatabasePoints.Checked;
            chkPhase3.Enabled = rbDatabasePoints.Checked;
            chkPhase4.Enabled = rbDatabasePoints.Checked;
            chkPhase5.Enabled = rbDatabasePoints.Checked;
            groupBox2.Enabled = rbDatabasePoints.Checked;

            checkBox2_CheckedChanged(new object(), new EventArgs());
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            chkPhase0.Checked = (chkPhaseAll.Checked);
            chkPhase0.Enabled = !chkPhaseAll.Checked;
            chkPhase1.Checked = (chkPhaseAll.Checked);
            chkPhase1.Enabled = !chkPhaseAll.Checked;
            chkPhase2.Checked = (chkPhaseAll.Checked);
            chkPhase2.Enabled = !chkPhaseAll.Checked;
            chkPhase3.Checked = (chkPhaseAll.Checked);
            chkPhase3.Enabled = !chkPhaseAll.Checked;
            chkPhase4.Checked = (chkPhaseAll.Checked);
            chkPhase4.Enabled = !chkPhaseAll.Checked;
            chkPhase5.Checked = (chkPhaseAll.Checked);
            chkPhase5.Enabled = !chkPhaseAll.Checked;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //הסתר את החלון על מנת לאפשר בחירה
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;

            //אם המשתמש בחר בכך יש להשאיר את הבחירה הקודמת ולאפשר הוספה
            ObjectId[] PreviousSelSetIds;
            if (chkAddSelection.Checked)
            {
                using (Transaction tr = acDoc.TransactionManager.StartTransaction())
                {
                    foreach (ObjectId id in LastSelSetIds)
                    {
                        Entity ent = (Entity)tr.GetObject(id, OpenMode.ForRead);
                        // ... and highlight the entity
                        ent.Highlight();
                    }
                    tr.Commit();
                }
            }

            //יצירת סט של קריטריונים לבחירה
            TypedValue[] acTypValAr;

            if (rbDatabasePoints.Checked)
            {
                //נקודות במסד הנתונים
                List<string> phases = new List<string>();

                for (int i = 0; i <= 5; i++)
                {
                    phases.Add("None");
                    try
                    {
                        CheckBox ctrl = this.groupBox1.Controls["chkPhase" + i.ToString()] as CheckBox;
                        if (ctrl.Checked)
                            phases[i] = "Points_" + ctrl.Text;
                    }
                    catch { }
                }

                int valsNum = 10;
                acTypValAr = new TypedValue[valsNum];

                acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "INSERT"), 0);
                acTypValAr.SetValue(new TypedValue((int)DxfCode.BlockName, DrawBL.AcadTools.StrengthrningPointBlockName), 1);

                acTypValAr.SetValue(new TypedValue((int)DxfCode.Operator, "<or"), 2);
                acTypValAr.SetValue(new TypedValue((int)DxfCode.LayerName, phases[0]), 3);
                acTypValAr.SetValue(new TypedValue((int)DxfCode.LayerName, phases[1]), 4);
                acTypValAr.SetValue(new TypedValue((int)DxfCode.LayerName, phases[2]), 5);
                acTypValAr.SetValue(new TypedValue((int)DxfCode.LayerName, phases[3]), 6);
                acTypValAr.SetValue(new TypedValue((int)DxfCode.LayerName, phases[4]), 7);
                acTypValAr.SetValue(new TypedValue((int)DxfCode.LayerName, phases[5]), 8);
                acTypValAr.SetValue(new TypedValue((int)DxfCode.Operator, "or>"), 9);

            }
            else
            {
                //נקודות שבתכנון
                if (!chkCyan.Checked)
                {
                    //אל תבחר נקודות בצבע תכלת
                    int valsNum = 4;
                    acTypValAr = new TypedValue[valsNum];

                    acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "INSERT"), 0);
                    acTypValAr.SetValue(new TypedValue((int)DxfCode.BlockName, DrawBL.AcadTools.PlanningPointBlockName), 1);
                    acTypValAr.SetValue(new TypedValue((int)DxfCode.Operator, "!="), 2);
                    acTypValAr.SetValue(new TypedValue((int)DxfCode.Color, 4), 3);
                }
                else
                {
                    //בחר נקודות בתכנון מכל הצבעים
                    int valsNum = 2;
                    acTypValAr = new TypedValue[valsNum];

                    acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "INSERT"), 0);
                    acTypValAr.SetValue(new TypedValue((int)DxfCode.BlockName, DrawBL.AcadTools.PlanningPointBlockName), 1);
                }
            }

            // Assign the filter criteria to a SelectionFilter object
            SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);

            // Get the current document and database
            Database acCurDb = acDoc.Database;

            ObjectId[] NewSelSetIds;

            this.Hide();

            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Request for object to be selected in the drawing area
                PromptSelectionResult acSSPrompt = null;
                if (this.chkPolygon.Checked)
                {
                    Autodesk.AutoCAD.Geometry.Point3dCollection polygon = new Autodesk.AutoCAD.Geometry.Point3dCollection();
                    PromptEntityResult per;
                    PromptEntityOptions peo = new PromptEntityOptions("\nPick a Polyline. [Attention: Polyline cannot cross itself!]");
                    try
                    {
                        per = acDoc.Editor.GetEntity(peo);
                        if (per.Status == PromptStatus.OK)
                        {
                            Polyline pline = acTrans.GetObject(per.ObjectId, OpenMode.ForRead) as Polyline;
                            for (int i = 0; i < pline.NumberOfVertices; i++)
                                polygon.Add(pline.GetPoint3dAt(i));
                        }
                        else
                        {
                            acDoc.Editor.WriteMessage("\nPolygon Selection Error");
                        }
                    }
                    catch (System.Exception ep)
                    {
                        acDoc.Editor.WriteMessage("\nPolygon Error: " + ep.Message);
                    }

                    acSSPrompt = acDoc.Editor.SelectWindowPolygon(polygon, acSelFtr);
                }
                else //Select By Window or Picking...
                    acSSPrompt = acDoc.Editor.GetSelection(acSelFtr);

                // If the prompt status is OK, objects were selected
                if (acSSPrompt.Status == PromptStatus.OK)
                {
                    NewSelSetIds = FilterDetails(acSSPrompt.Value.GetObjectIds(), acTrans);
                }
                else
                {
                    NewSelSetIds = new ObjectId[0];
                    acDoc.Editor.WriteMessage("\nSelection Error! - No Point has been counted.");
                }

                if (chkAddSelection.Checked)
                {
                    //שמור את הסט הקודם לפני ששומרים את הנוכחי
                    PreviousSelSetIds = LastSelSetIds;
                    LastSelSetIds = MergeIdsArrays(PreviousSelSetIds, NewSelSetIds);
                }
                else
                {
                    LastSelSetIds = NewSelSetIds;
                }


                acTrans.Commit();
                // Dispose of the transaction
            }

            this.textBox1.Text = LastSelSetIds.Length.ToString() + "  נק'";
            this.Show();
        }

        private ObjectId[] FilterDetails(ObjectId[] ids, Transaction acTrans)
        {
            if (!this.rbDatabasePoints.Checked)
                return ids;

            List<int> details = GetCheckedDetails();
            List<ObjectId> filtered = new List<ObjectId>();
            BlockReference br;
            AttributeReference ar;
            int Detail;

            foreach (ObjectId id in ids)
            {
                try
                {
                    br = acTrans.GetObject(id, OpenMode.ForRead) as BlockReference;
                    ar = acTrans.GetObject(br.AttributeCollection[2], OpenMode.ForRead) as AttributeReference;
                    Detail = Convert.ToInt32(ar.TextString);
                    if (details.Contains(Detail))
                        filtered.Add(id);
                }
                catch
                {
                    continue;
                }
            }

            return filtered.ToArray();
        }

        private List<int> GetCheckedDetails()
        {
            List<int> details = new List<int>();
            string det;
            foreach (object itm in this.cblDetails.CheckedItems)
            {
                try
                {
                    det = itm.ToString();
                    det = det.Substring(0, det.IndexOf("."));
                    details.Add(Convert.ToInt32(det));
                }
                catch { }
            }
            return details;
        }

        private ObjectId[] MergeIdsArrays(ObjectId[] oldArray, ObjectId[] newArray)
        {
            List<ObjectId> IdsList = new List<ObjectId>();
            IdsList.AddRange(oldArray);
            foreach (ObjectId id in newArray)
                if (!IdsList.Contains(id))
                    IdsList.Add(id);

            return IdsList.ToArray();
        }

        private void SelectPtsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //סמן את הנקודות שנבחרו גם לאחר סגירת החלון
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            acDoc.Editor.SetImpliedSelection(LastSelSetIds);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < cblDetails.Items.Count; i++)
                cblDetails.SetItemChecked(i, true);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < cblDetails.Items.Count; i++)
                cblDetails.SetItemChecked(i, false);
        }
    }
}
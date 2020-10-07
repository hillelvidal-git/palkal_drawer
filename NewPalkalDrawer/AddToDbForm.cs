using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using System.IO;

namespace NewPalkalDrawer
{
    public partial class AddToDbForm : Form
    {
        DrawBL.BL myBL;
        int myLevelId, myProjectId;
        public bool Connected;

        bool _distoMode;
        bool DistoMode
        {
            get
            {
                return _distoMode;
            }
            set
            {
                this._distoMode = value;
                chkActivePts.Visible = chkPlanningPts.Visible = chkNewsOnly.Visible = value;
                label6.Visible = cmbDetails.Visible = !value; //בחירת פרט לא נצרכת בשליחה לדיסטומט

                if (value)
                {
                    this.Text = "שליחת נקודות לדיסטומט";
                    this.btnAddToDb.Text = "שלח לסימון בשטח";
                    this.chkActivePts.Checked = true;
                    btnPickPoints.ImageList = this.btnAddToDb.ImageList = this.imgsDisto;
                    lblProject.ForeColor = Color.Lime;
                    lblLevel.ForeColor = Color.Lime;
                    lblPtNum.ForeColor = Color.Lime;
                }
                else
                {
                    this.Text = "הוספת נקודות למסד הנתונים";
                    this.btnAddToDb.Text = "הוסף למסד הנתונים";
                    this.chkPlanningPts.Checked = true;
                    btnPickPoints.ImageList = this.btnAddToDb.ImageList = this.imgsDB;
                    lblProject.ForeColor = Color.FromKnownColor(KnownColor.Highlight);
                    lblLevel.ForeColor = Color.FromKnownColor(KnownColor.Highlight);
                    lblPtNum.ForeColor = Color.FromKnownColor(KnownColor.Highlight);
                }

            }
        }

        public AddToDbForm(DrawBL.BL bl, bool disto)
        {
            myBL = bl;
            InitializeComponent();
            this.DistoMode = disto;

            string projectName, levelName;
            int blocId; DateTime time; string msg;

            //סדרה של בדיקות שדרושות במצב שצריך לשלוח נקודות לשרת
            //במצב שליחה לדיסטומט הבדיקות אינן מעכבות
            if (!myBL.CheckServerConnection(out msg))
            {
                log("\nCannot Connect to SERVER!");
                if (!DistoMode) { this.Connected = false; return; }
            }
            log("\nServer Conncetion OK");

            if (!myBL.GetDrawingCertificate(out this.myLevelId, out blocId, out time, out msg))
            {
                log("\nCannot read dwg level ID!");
                if (!DistoMode) { this.Connected = false; return; }
            }
            log("\nLevelID: " + myLevelId);

            if (!myBL.GetProjectByLevel(this.myLevelId, out this.myProjectId, out projectName))
            {
                log("\nCannot Get project ID");
                if (!DistoMode) { this.Connected = false; return; }
            }
            log("\nProjectID: " + myProjectId);

            if (!myBL.GetLevelName(this.myLevelId, out levelName))
            {
                log("\nCannot Get level Name");
                if (!DistoMode) { this.Connected = false; return; }
            }

            this.Connected = true;
            try
            {
                this.lblProject.Text = projectName.Trim();
                this.lblLevel.Text = levelName.Trim();
            }
            catch
            {
                this.lblProject.Text = "Project";
                this.lblLevel.Text = "Level";
            }

            //טעינת רשימת הגושים והפרטים בפרויקט
            //במצב יצוא לדיסטומט זה לא מעכב
            LoadBlocs();
            LoadDetails();
        }

        private void LoadDetails()
        {
            try
            {
                object[][] details = myBL.GetEngDetails();
                this.cmbDetails.Items.Clear();
                foreach (object[] detail in details)
                {
                    this.cmbDetails.Items.Add(detail[0].ToString() + ". " + detail[1].ToString().Trim());
                }
            }
            catch
            {
            }
        }

        private void btnPickPoints_Click(object sender, EventArgs e)
        {
            treeView1.Nodes.Clear();
            string blockName;
            if (chkActivePts.Checked)
                blockName = DrawBL.AcadTools.StrengthrningPointBlockName;
            else
                blockName = DrawBL.AcadTools.PlanningPointBlockName;

            List<object[]> pts = myBL.acTools.SelectBlocks(blockName);
            BuildTree(pts);
        }

        private void BuildTree(List<object[]> pts)
        {
            string[] name;
            int points = 0;

            TreeNode FieldNode, LineNode, PointNode;
            foreach (object[] pt in pts)
            {
                if (!ParseName(pt[1].ToString(), out name))
                {
                    if (!DistoMode)
                        continue; //במצב שליחה למסד ניתן להכליל רק נקודות עם שם ברור
                    else
                    {
                        //במצב שליחה לסימון ניתן להכליל גם נקודות עם שם לא ברור
                        try
                        {
                            name = new string[] { "Other", pt[1].ToString(), "1" };
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }

                FieldNode = treeView1.Nodes[name[0]];
                if (FieldNode == null)
                {
                    FieldNode = treeView1.Nodes.Add(name[0], "Field " + name[0]);
                }

                LineNode = FieldNode.Nodes[name[1]];
                if (LineNode == null)
                {
                    LineNode = FieldNode.Nodes.Add(name[1], "Line " + name[1]);
                }

                PointNode = LineNode.Nodes[name[2]];
                if (PointNode == null)
                {
                    PointNode = LineNode.Nodes.Add(name[2], name[2]);
                    PointNode.Tag = pt[0]; //position of the point
                    points++;
                }
            }

            treeView1.Sort();

            lblPtNum.Text = points.ToString();
        }

        private bool ParseName(string p, out string[] parts)
        {
            //מחלק את שם הנקודה לשלושה חלקים - שדה, צלע, מספר

            int e = 0;
            int s = 0;
            parts = new string[3];
            p += "_";

            for (int i = 0; i < p.Length; i++)
            {
                if (p[i] == '_')
                {
                    parts[e] = p.Substring(s, i - s);

                    //תיקון: הוספת אפס אם צריך
                    if (e > 0)
                        if (parts[e].Length == 1) parts[e] = "0" + parts[e];

                    e++;
                    if (e == 3) return true;
                    s = i + 1;
                }
            }

            return false;
        }

        private void btnAddToDb_Click(object sender, EventArgs e)
        {
            //בדיקות הדרושות רק במצב שליחה למסד
            if (!DistoMode)
            {
                if ((this.cmbDetails.SelectedItem == null) || (this.cmbBlocs.SelectedItem == null))
                {
                    MessageBox.Show("לא נבחרו פרט וגוש תקינים");
                    return;
                }
            }

            if (lblPtNum.Text == "0")
            {
                MessageBox.Show("לא נבחרו נקודות");
                return;
            }


            if (this.DistoMode)
                SendToStakeout();
            else
                SendToDatabase();

        }

        private void SendToStakeout()
        {
            string blocName;

            try
            {
                blocName = cmbBlocs.SelectedItem.ToString();
                blocName = blocName.Substring(blocName.IndexOf(".") + 1).Trim();
            }
            catch
            {
                blocName = "AnyBloc";
            }

            SendToDistomat(lblProject.Text, lblLevel.Text, blocName);
            this.Close();
        }

        List<string> distoPage, distoPoints;
        private void dl(string line)
        {
            this.distoPage.Add(line);
        }

        private void dp(string line)
        {
            this.distoPoints.Add(line);
        }

        private void InitializePages()
        {
            this.distoPoints = new List<string>();
            this.distoPage = new List<string>();
        }

        internal void SendToDistomat(string projectName, string levelName, string blocName)
        {
            List<string> errors = new List<string>();
            string distoFileName;

            InitializePages(); // הכן את קבצי היומן
            int lineTotal;
            int Exported = 0;

            string fieldName, lineName, ptShortName;
            int ptNum, ptDbId;

            string msg;
            bool isPlata;
            double[] position; double rot;

            string now = DateTime.Now.ToString();

            distoFileName = "[" + projectName + "][" + levelName + "][" + blocName + "]";

            dl("פרויקט: " + projectName + "  מפלס: " + levelName + "  גוש: " + blocName);
            dl("תאריך הפקה: " + now);
            dl("======================================");
            dl("");

            // Start a transaction
            using (Transaction ta = this.myBL.acTools.StartTransaction())
            {
                foreach (TreeNode fieldNode in treeView1.Nodes)
                {
                    fieldName = fieldNode.Name;
                    dl("");
                    dl("");
                    dl("שדה " + fieldName);
                    dl("------------------------");
                    distoFileName += "(" + fieldName + ")";

                    foreach (TreeNode lineNode in fieldNode.Nodes)
                    {
                        lineName = lineNode.Name;
                        lineTotal = 0;

                        foreach (TreeNode ptNode in lineNode.Nodes)
                        {
                            try
                            {
                                //קבלת פרטי הנקודה

                                if (chkActivePts.Checked)
                                {
                                    //נסה לקבל את פרטי נקודה פעילה
                                    if (!ParseActivePoint(ta, ptNode.Name, (ObjectId)(ptNode.Tag), out ptNum, out isPlata, out position, out ptDbId, out msg))
                                        continue;
                                }
                                else
                                {
                                    //נסה לקבל את פרטי נקודת תכנון
                                    if (!ParsePlanningPoint(ta, ptNode.Name, (ObjectId)(ptNode.Tag), out ptNum, out isPlata, out position, out rot, out ptDbId, out msg))
                                        continue;
                                }


                                if (ptNum < 10) //הוספת אפס למספר חד ספרתי
                                    ptShortName = fieldName + "_" + lineName + "_0" + ptNum.ToString();
                                else
                                    ptShortName = fieldName + "_" + lineName + "_" + ptNum.ToString();

                                if (isPlata) ptShortName += "  [Plata!]";

                                //הוסף את פרטי הנקודה לקובץ הדיסטומט
                                dp(
                                    ptDbId.ToString() +
                                    "," +
                                    ptShortName +
                                    "," +
                                    position[0].ToString() +
                                    "," +
                                    position[1].ToString() +
                                    "," +
                                    position[2].ToString() +
                                    "," +
                                    "False" + //הנקודה עדיין לא סומנה
                                    ","
                                    );

                                lineTotal++; //קדם את מונה הנקודות בצלע
                                Exported++; //קדם את מונה הנקודות הכללי
                            }
                            catch (System.Exception ept)
                            {
                                //הנקודה לא נוספה בהצלחה
                                log("\nPoint Not Added: " + ptNode.Name + " In Line " + lineName + " In Field " + fieldName + " ---> " + ept.Message);
                                continue; //עבור לנקודה הבאה
                            }

                        } //עבור כל נקודה 

                        dl("");
                        dl("צלע " + lineName + "     (" + lineTotal.ToString() + " נק')");
                        dl("..................................................................................................................");

                    } //עבור כל צלע

                } //עבור כל שדה

                ta.Commit();
            } //Transaction

            dl("");
            dl("----- סוף הקובץ. סה\"כ: " + Exported.ToString() + " נקודות לזריקה -----");
            WriteDistoFile(distoFileName);
        }

        private void WriteDistoFile(string suggestedName)
        {
            string distoFolder = myBL.GetStakeoutFolder();
            if (!Directory.Exists(distoFolder))
                Directory.CreateDirectory(distoFolder);

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "בחר את מיקום קובץ הזריקה";
            sfd.InitialDirectory = distoFolder;
            sfd.FileName = suggestedName + ".TXT";

            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            using (StreamWriter sw = new StreamWriter(sfd.FileName))
            {
                sw.WriteLine("Bedek ExportPoints - From PlakalDrawer");
                sw.WriteLine("Date: " + DateTime.Now.ToString());
                sw.WriteLine("");

                distoPoints.Sort();
                foreach (string line in distoPoints)
                    sw.WriteLine(line);
            }

            string logFile = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\DistoLog.TXT";
            using (StreamWriter sw = new StreamWriter(logFile))
            {
                sw.WriteLine("סימון נקודות תיקון. שם קובץ הנקודות:");
                sw.WriteLine(sfd.FileName.Substring(sfd.FileName.LastIndexOf("\\") + 1));

                foreach (string line in distoPage)
                    sw.WriteLine(line);
            }

            if (MessageBox.Show("דף יומן לסימון הנקודות נשמר על שולחן העבודה.\nהאם תרצה גם לשלוח אותו להדפסה?", "הדפסת דף סימון", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, MessageBoxOptions.RtlReading)
                == System.Windows.Forms.DialogResult.Yes)
                myBL.PrintTxtFile(logFile);
        }

        private void SendToDatabase()
        {
            int blocId, fieldId, lineId, ptNum;
            int regularDetailId, ptDetail;
            bool isPlata;
            double[] position;
            double rotation_Rad;
            string ptMsg; int ptNewId, ptDbId;
            string msg;

            if (!GetBlocId(out blocId, out msg))
            { MessageBox.Show("לא ניתן לקבל את מספר הבלוק" + "\n" + msg, "הפעולה בוטלה"); return; }

            if (!GetRegularDetail(out regularDetailId))
            { MessageBox.Show("לא ניתן לקבל את מספר הפרט", "הפעולה בוטלה"); return; }
            log("\nRegular Detail: " + regularDetailId);

            bool allOk = true;
            int uploaded = 0;
            log("\n\nUploading new points...");

            using (Transaction ta = myBL.acTools.StartTransaction())
            {
                foreach (TreeNode fieldNode in treeView1.Nodes)
                {
                    log(1, "Filed: " + fieldNode.Name);
                    if (!GetFieldId(fieldNode.Name, blocId, out fieldId, out msg))
                    { log(1, "[Error] " + msg); fieldNode.ForeColor = Color.Red; allOk = false; continue; }
                    foreach (TreeNode lineNode in fieldNode.Nodes)
                    {
                        log(2, "Line: " + lineNode.Name);
                        if (!GetLineId(lineNode.Name, fieldId, out lineId, out msg))
                        { log(2, "[Error] " + msg); lineNode.ForeColor = Color.Red; allOk = false; continue; }
                        foreach (TreeNode pointNode in lineNode.Nodes)
                        {
                            log(3, "Point: " + pointNode.Name);
                            if (!ParsePlanningPoint(ta, pointNode.Name, (ObjectId)(pointNode.Tag), out ptNum, out isPlata, out position, out rotation_Rad, out ptDbId, out msg))
                            { log(3, "[Error] " + msg); pointNode.ForeColor = Color.Red; allOk = false; continue; }

                            if (isPlata)
                            { log(3, "[Plata!]"); ptDetail = DrawBL.AcadTools.PlataDetailID; }
                            else
                            { log(3, "[Regular!]"); ptDetail = regularDetailId; }

                            log(3, "Uploading point");
                            if (AddPointToDb(ptNum, lineId, position, rotation_Rad, ptDetail, 0, 0, out ptMsg, out ptNewId))
                            {
                                uploaded++;
                                log(4, "[OK] New Id: " + ptNewId);
                                HidePlanningPoint((ObjectId)(pointNode.Tag), ta);
                                pointNode.ForeColor = Color.Green;
                            }
                            else
                            {
                                log(4, "[Error] " + ptMsg);
                            }
                        }
                    }
                }
                ta.Commit();
            }
            //שמור את השינויים בשרטוט
            myBL.SaveDrawing();

            if (allOk) MessageBox.Show("כל " + uploaded + " הנקודות נוספו בהצלחה למסד הנתונים" + "\nהפעל עדכון שרטוט על מנת לראות את הנקודות החדשות", "איזה כיף :-)");

            //בכל מקרה סיים את תצוגת הטופס
            this.Close();
        }

        private void log(int p, string msg)
        {
            for (int i = 0; i < p; i++)
                msg = "    " + msg;

            log("\n" + msg);
        }

        private void log(string p)
        {
            Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(p);
        }

        private bool GetRegularDetail(out int regularDetailId)
        {
            regularDetailId = -1; //זה ישתנה מיד

            try
            {
                string det1 = cmbDetails.SelectedItem.ToString();
                det1 = det1.Substring(0, det1.IndexOf("."));
                regularDetailId = Convert.ToInt32(det1);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void HidePlanningPoint(ObjectId objectId, Transaction ta)
        {
            try
            {
                BlockReference br = ta.GetObject(objectId, OpenMode.ForWrite) as BlockReference;
                br.Layer = DrawBL.AcadTools.OldPlanningPointsLayer;
                br.ColorIndex = 256;
            }
            catch { }
        }

        private bool AddPointToDb
            (int ptNum, int lineId, double[] position, double rotation_Rad, int ptDetail, int statusId, int problem, out string msg, out int newId)
        {
            return myBL.UploadNewPoint(ptNum, lineId, position, rotation_Rad, ptDetail, statusId, problem, out msg, out newId);
        }

        private bool ParsePlanningPoint(Transaction ta, string ptName, ObjectId objectId, out int ptNum, out bool isPlata, out double[] position, out double rotation_Rad, out int DbId, out string msg)
        {
            DbId = -1;
            try
            {
                BlockReference br = ta.GetObject(objectId, OpenMode.ForRead) as BlockReference;
                position = new double[] { br.Position.X, br.Position.Y, br.Position.Z };
                rotation_Rad = br.Rotation;

                string ptSuffix;

                int p = -1;
                for (int i = 0; i < ptName.Length; i++)
                {
                    if (!char.IsDigit(ptName, i)) { p = i; break; }
                }

                if (p == -1) //name contains only digits
                {
                    ptNum = Convert.ToInt32(ptName);
                    isPlata = false;
                }
                else //name contains letters
                {
                    ptNum = Convert.ToInt32(ptName.Substring(0, p));
                    ptSuffix = ptName.Substring(p);
                    isPlata = ((ptSuffix == "Plata") || (ptSuffix == "PLATA") || (ptSuffix == "XXX"));
                }
                msg = "OK";
                return true;
            }
            catch (Exception e)
            {
                position = new double[0]; ptNum = -1; isPlata = false;
                rotation_Rad = 0;
                msg = e.Message;
                return false;
            }
        }

        private bool ParseActivePoint(Transaction ta, string ptName, ObjectId objectId, out int ptNum, out bool isPlata, out double[] position, out int DbId, out string msg)
        {
            try
            {
                BlockReference br = ta.GetObject(objectId, OpenMode.ForRead) as BlockReference;
                AttributeReference arDBID = ta.GetObject(br.AttributeCollection[3], OpenMode.ForRead) as AttributeReference;
                AttributeReference arDETAIL = ta.GetObject(br.AttributeCollection[2], OpenMode.ForRead) as AttributeReference;

                position = new double[] { br.Position.X, br.Position.Y, br.Position.Z };
                DbId = Convert.ToInt32(arDBID.TextString);
                int detail = Convert.ToInt32(arDETAIL.TextString);
                isPlata = (detail == DrawBL.AcadTools.PlataDetailID);
                ptNum = Convert.ToInt32(ptName);

                msg = "OK";
                return true;
            }
            catch (Exception e)
            {
                position = new double[0]; ptNum = -1; isPlata = false;
                msg = e.Message;
                DbId = -1;
                return false;
            }
        }


        private bool GetLineId(string name, int fieldId, out int lineId, out string msg)
        {
            try
            {
                int intName = Convert.ToInt32(name);
                return myBL.GetLineId(intName, fieldId, out lineId, out msg);
            }
            catch (Exception e)
            {
                lineId = -1;
                msg = e.Message;
                return false;
            }
        }

        private bool GetFieldId(string name, int blocId, out int fieldId, out string msg)
        {
            try
            {
                return myBL.GetFieldId(name, blocId, out fieldId, out msg);
            }
            catch (Exception e)
            {
                msg = e.Message;
                fieldId = -1;
                return false;
            }
        }

        private bool GetBlocId(out int blocId, out string msg)
        {
            try
            {
                string bloc = cmbBlocs.SelectedItem.ToString();
                bloc = bloc.Substring(0, bloc.IndexOf("."));
                blocId = Convert.ToInt32(bloc);
                msg = "OK";
                return true;
            }
            catch (Exception e)
            {
                blocId = -1;
                msg = e.Message;
                return false;
            }
        }

        private void LoadBlocs()
        {
            object[][] blocs;
            if (!myBL.GetLevelBlocs(this.myLevelId, out blocs)) return;

            cmbBlocs.Items.Clear();
            foreach (object[] bloc in blocs)
                cmbBlocs.Items.Add(bloc[0].ToString() + ". " + ((string)bloc[1]).Trim());
        }

        private void chkPlanningPts_CheckedChanged(object sender, EventArgs e)
        {
            treeView1.Nodes.Clear();
            lblPtNum.Text = "0";
        }
    }

    public class MyList
    {
        public string Name;
        public int Id;

        public MyList()
        {
            this.Name = "";
            this.Id = 0;
        }

        public MyList(int id, string name)
        {
            this.Name = name;
            this.Id = id;
        }

        public int ItemData
        {
            get
            {
                return this.Id;
            }
            set
            {
                this.Id = value;
            }
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace NewPalkalDrawer
{
    public partial class InsertPointsForm : Form
    {
        DrawBL.BL myBL;
        DrawBL.AcadTools acTools;

        Point3d[] DirectionLine = new Point3d[2];
        double LineAngle;
        private bool actionCanceled = false;
        static double PlataRibWidth = 0.078; //רוחב צלע שממנו ומטה מבצעים פלטה
        string[] DwgPointNames; //כל הנקודות שבשרטוט, על מנת להציע מספרי צלע

        public InsertPointsForm(DrawBL.BL bl)
        {
            this.myBL = bl;
            this.acTools = this.myBL.acTools;
            InitializeComponent();

            DwgPointNames = GetDwgPointNames();
            LoadUserLastInput();
        }

        private void LoadUserLastInput()
        {
            try
            {
                //load last input
                this.txtFPF.Text = InsertionSettings.Default.FirstPointField;
                this.nudFPL.Value = (int)InsertionSettings.Default.FirstPointLine + 1;
                this.nudFPB.Value = (int)InsertionSettings.Default.FirstPointBore;
                this.nudFirstGap.Value = InsertionSettings.Default.FirstGap;
                this.nudRegularGap.Value = InsertionSettings.Default.RegularGap;
                this.nudLBP.Value = InsertionSettings.Default.PointsNumber;
                this.checkBox2.Checked = InsertionSettings.Default.bMaxPointNum;
            }
            catch { }
        }

        private void InsertPointsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //save input
            if (this.DialogResult == DialogResult.OK)
            {
                InsertionSettings.Default.FirstPointField = this.txtFPF.Text.Trim();
                if (this.actionCanceled)
                    InsertionSettings.Default.FirstPointLine = (int)this.nudFPL.Value - 1;
                else
                    InsertionSettings.Default.FirstPointLine = (int)this.nudFPL.Value;
                InsertionSettings.Default.FirstPointBore = (int)this.nudFPB.Value;
                InsertionSettings.Default.FirstGap = (int)this.nudFirstGap.Value;
                InsertionSettings.Default.RegularGap = (int)this.nudRegularGap.Value;
                InsertionSettings.Default.PointsNumber = (int)this.nudLBP.Value;
                InsertionSettings.Default.bMaxPointNum = this.checkBox2.Checked;
                InsertionSettings.Default.Save();
            }
        }

        private void btnDraw_Click(object sender, EventArgs e)
        {
            this.Hide(); //End form interaction

            try
            {
                this.DirectionLine = DrawBL.AcadTools.GetUserLine();
            }
            catch
            {
                MessageBox.Show("הפעולה בוטלה");
                this.actionCanceled = true;
            }

            //הפעולה בוטלה - הפסק
            if (this.actionCanceled) return;

            this.LineAngle = GetAngle(DirectionLine[0], DirectionLine[1]);

            try
            {
                using (Transaction ta = this.acTools.StartTransaction())
                {
                    double firstGap = Convert.ToDouble(this.nudFirstGap.Value) / 100;
                    double Step = Convert.ToDouble(this.nudRegularGap.Value) / 100;
                    List<NamedPoint> ThePoints = CalculatePoints(ta, DirectionLine[0], DirectionLine[1], firstGap, Step);
                    DrawPoints(ThePoints, ta);

                    ta.Commit();
                }
            }
            catch (System.Exception drEx)
            {
                Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog(drEx.Message);
            }

        }

        private List<NamedPoint> CalculatePoints(Transaction ta, Point3d startPt, Point3d endPt, double firstGap, double step)
        {
            double givenDist, currentDist, currentAngle, userAngle, lineAngle;
            double gap;
            Point3d currentPoint;
            Point3d lastPoint = startPt;
            int color = 5;
            double maxAngDev = 0.0005; //אפשרות הסטייה מהזווית הראשונית
            List<NamedPoint> ThePoints = new List<NamedPoint>();

            string fieldName = this.txtFPF.Text.Trim();
            int rib = (int)this.nudFPL.Value;
            int ptNum = (int)this.nudFPB.Value - 1;
            string ptText, strPt, strLine;

            //מציאת המרחק אותו יש לתקן
            givenDist = GetDist2d(startPt, endPt);

            //מציאת המגמה הכללית של הצלע
            userAngle = GetAngle(startPt, endPt);
            currentAngle = userAngle;

            currentDist = 0;

            //לולאת חישוב הנקודות
            //======================
            do
            {
                ptNum++;
                strPt = ptNum.ToString();
                if (strPt.Length == 1) strPt = "0" + strPt;
                strLine = rib.ToString();
                if (strLine.Length == 1) strLine = "0" + strLine;

                ptText = fieldName + "_" + strLine + "_" + strPt;
                gap = step;
                if (ptNum == 1)
                    gap = firstGap;

                //חישוב המיקום הראשוני - על פי מגמת הצלע העדכנית
                //====================================================
                currentPoint = new Point3d(
                    lastPoint.X + gap * Math.Cos(currentAngle),
                    lastPoint.Y + gap * Math.Sin(currentAngle),
                    0);

                //תיקון המיקום
                //================
                if (!chkSimple.Checked)
                    currentPoint = CenterPoint(currentPoint, ref color, currentAngle, ta);

                //הוספת הנקודה לרשימה
                //=======================
                ThePoints.Add(new NamedPoint(currentPoint, ptText, color));

                //עדכון מגמת הצלע - לשימוש בחישוב ראשוני של הנקודה הבאה
                //=============================================================
                currentAngle = GetAngle(lastPoint, currentPoint);

                //בדיקת זוית הצלע מהתחלתה למניעת עיוותים
                //===========================================
                lineAngle = GetAngle(startPt, currentPoint);
                if ((lineAngle / userAngle) > maxAngDev)
                    currentAngle = userAngle;

                //שמירת הנקודה לשימוש עתידי
                //=============================
                lastPoint = new Point3d(currentPoint.X, currentPoint.Y, currentPoint.Z);

                //חישוב המרחק המצטבר מהנקודה הראשונה לצורך תנאי העצירה
                //===========================================================
                currentDist = GetDist2d(currentPoint, startPt);

            } while ((!this.checkBox2.Checked && currentDist <= givenDist) || (this.checkBox2.Checked && ptNum < (int)this.nudLBP.Value));

            return ThePoints;
        }

        struct NamedPoint
        {
            public Point3d pt;
            public string name;
            public int color;
            public NamedPoint(Point3d _pt, string _name, int _color)
            {
                this.pt = _pt;
                this.name = _name;
                this.color = _color;
            }
        }

        private Point3d CenterPoint(Point3d pt, ref int color, double lineAngle, Transaction ta)
        {
            double maxDist = 0.25; //הרוחב המקסימלי של צלע, למעלה מכך כנראה שחסרים נתונים
            Polyline rightPoly, leftPoly;
            Point3d closestRightPt, closestLeftPt;

            FindClosestBoxes(pt, lineAngle, maxDist, out rightPoly, out leftPoly, out closestRightPt, out closestLeftPt, ta);

            if ((rightPoly != null) && (leftPoly != null))
            {
                //יש תיבות משני הצדדים
                pt = GetMidPoint(closestRightPt, closestLeftPt);
                double ribWidth = GetDist2d(closestRightPt, closestLeftPt);

                if (ribWidth <= PlataRibWidth)
                {
                    //צלע מוצרת
                    color = 1;
                    //acEd.WriteMessage("\nPlata: " + ribWidth.ToString());
                }
                else if (ribWidth >= 20)
                {
                    color = 6;
                }
                else
                    //קביעת הצבע לנורמלי
                    color = 5;

                //טיפול במקרים בעייתיים כגון שנקודת האמצע יוצאת רחוקה מדי!
                //TODO...
            }
            else if ((rightPoly == null) && (leftPoly == null))
            {
                //אין תיבה משום צד
                //אין צורך לבצע תיקון, אלא להשתמש במיקום הראשוני שחושב על פי מגמת הצלע
                color = 4;
                //לא מתבצע שינוי של הנקודה

            }
            else if (leftPoly == null)
            {
                //אין תיבה מצד שמאל אלא רק מימין
                pt = PushPointFrom(closestRightPt, pt);
                color = 3;

            }
            else if (rightPoly == null)
            {
                //אין תיבה מימין אלא רק משמאל
                pt = PushPointFrom(closestLeftPt, pt);
                color = 3;

            }

            return pt;
        }

        private Point3d PushPointFrom(Point3d closestPt, Point3d pt)
        {
            //יישור הנקודה במרחק של 5 ס"מ מהנקודה הקרובה ביותר על הפולי הקרוב

            //המרחק לנקודה הקרובה
            double dist = GetDist2d(pt, closestPt);

            //ההפרש בין המרחק לבין הרצוי - 5 ס"מ
            double gap = dist - 0.05;

            //הזוית מהנקודה הקרובה
            double a = GetAngle(closestPt, pt);
            double dx = gap * Math.Sin(a - Math.PI);
            double dy = gap * Math.Cos(a - Math.PI);
            //tAcDoc.Editor.WriteMessage("\nalpha= " + a.ToString("0.000") + " ,  dx= " + dx.ToString("0.000") + " ,  dy=" + dy.ToString("0.000"));
            return new Point3d(
                pt.X + dx,
                pt.Y + dy,
                0);
        }

        private Point3d GetMidPoint(Point3d pt1, Point3d pt2)
        {
            return new Point3d((pt1.X + pt2.X) / 2, (pt1.Y + pt2.Y) / 2, (pt1.Z + pt2.Z) / 2);
        }

        double finderRange = 0.25;
        private void FindClosestBoxes(Point3d pt, double lineAngle, double maxDist, out Polyline rightPoly, out Polyline leftPoly, out Point3d rightPt, out Point3d leftPt, Transaction acTa)
        {
            rightPoly = null;
            leftPoly = null;
            rightPt = new Point3d();
            leftPt = new Point3d();
            Editor acEd = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;

            //הכנת הקוים לחיפוש תיבות משני הצדדים
            double normalAngle = lineAngle + Math.PI / 2;

            Point3dCollection rightFinder = new Point3dCollection(
                new Point3d[]
                {
                new Point3d(pt.X,pt.Y,0),
                new Point3d(pt.X+finderRange*Math.Cos(normalAngle),pt.Y+finderRange*Math.Sin(normalAngle), 0),
                }
                );

            Point3dCollection leftFinder = new Point3dCollection(
                new Point3d[]
                {
                new Point3d(pt.X,pt.Y,0),
                new Point3d(pt.X-finderRange*Math.Cos(normalAngle),pt.Y-finderRange*Math.Sin(normalAngle), 0),
                }
                );

            //הכנת סט הקריטריונים
            TypedValue[] acTypValAr = new TypedValue[5];
            acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "lwpolyline"), 0);
            acTypValAr.SetValue(new TypedValue((int)DxfCode.Operator, "<or"), 1);
            acTypValAr.SetValue(new TypedValue((int)DxfCode.LayerName, "Bedek_Boxes_Inferior"), 2);
            acTypValAr.SetValue(new TypedValue((int)DxfCode.LayerName, "Bedek_Boxes_Superior"), 3);
            acTypValAr.SetValue(new TypedValue((int)DxfCode.Operator, "or>"), 4);
            SelectionFilter sf = new SelectionFilter(acTypValAr);
            Polyline ent;

            PromptSelectionResult psr1 = acEd.SelectFence(rightFinder, sf);
            PromptSelectionResult psr2 = acEd.SelectFence(leftFinder, sf);
            double nearestRight, nearestLeft;
            double dist;


            if (psr1.Status == PromptStatus.OK)
            {
                try
                {

                    ////First Polyline
                    //בחירת האובייקטים מצד ראשון
                    nearestRight = 5;
                    //acEd.WriteMessage("\nFirst Direction: " + psr1.Value.Count.ToString() + "  Polys detected");

                    //חיפוש הפולי הקרוב ביותר מצד זה
                    foreach (ObjectId id in psr1.Value.GetObjectIds())
                    {
                        ent = acTa.GetObject(id, OpenMode.ForRead) as Polyline;
                        if (ent.NumberOfVertices < 20) continue;

                        //בדיקה האם זה הקרוב ביותר עד כה
                        rightPt = ent.GetClosestPointTo(pt, false);
                        dist = GetDist2d(pt, rightPt);
                        //acEd.WriteMessage("\n    dist: " + dist.ToString());
                        if (dist < nearestRight)
                        {
                            rightPoly = ent;
                            nearestRight = dist;
                            //acEd.WriteMessage("  >>> Nearest: " + nearest.ToString());
                        }
                    }
                }
                catch
                {
                    //acEd.WriteMessage("\nERROR finidng RIGHT polyline: " + e1.Message);
                }
            }

            if (psr2.Status == PromptStatus.OK)
            {
                try
                {
                    ////Second Polyline
                    //בחירת האובייקטים מצד שני
                    nearestLeft = 5;
                    //acEd.WriteMessage("\nSecond Direction: " + psr2.Value.Count.ToString() + "  Polys detected");

                    //חיפוש הפולי הקרוב ביותר מצד זה
                    foreach (ObjectId id in psr2.Value.GetObjectIds())
                    {
                        ent = acTa.GetObject(id, OpenMode.ForRead) as Polyline;
                        if (ent.NumberOfVertices < 20) continue;

                        //בדיקה האם זה הקרוב ביותר עד כה
                        leftPt = ent.GetClosestPointTo(pt, false);
                        dist = GetDist2d(pt, leftPt);
                        //acEd.WriteMessage("\n    dist: " + dist.ToString());
                        if (dist < nearestLeft)
                        {
                            leftPoly = ent;
                            nearestLeft = dist;
                            //acEd.WriteMessage("  >>> Nearest: " + nearest.ToString());
                        }
                    }
                }
                catch
                {
                    //acEd.WriteMessage("\nERROR finidng LEFT polyline: " + e2.Message);
                }
            }

        }

        private void DrawPoints(List<NamedPoint> points, Transaction ta)
        {
            int color;
            Point3d currentPoint;
            string name;
            List<AttributeDefinition> blkAtts = this.acTools.PlanningPointAttributes;
            BlockTableRecord ms = ta.GetObject(this.acTools.acBlockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

            foreach (NamedPoint pt in points)
            {
                //טען את ערכי הנקודה הנוכחית
                currentPoint = pt.pt;
                name = pt.name;
                color = pt.color;

                if (color == 1) name += "PLATA";
                string LayerName = DrawBL.AcadTools.GetPointStatusLayer(-1);

                //צור הפניית בלוק
                BlockReference infoBlk = new BlockReference(currentPoint, this.acTools.acBlockTable[DrawBL.AcadTools.PlanningPointBlockName]);

                //קבע את מאפייני הנקודה
                infoBlk.SetDatabaseDefaults();
                try
                {
                    infoBlk.Layer = LayerName;
                }
                catch
                {
                    infoBlk.Layer = "0";
                }
                infoBlk.ColorIndex = color;

                //הוסף את הנקודה לשרטוט
                ObjectId blkRefID = ms.AppendEntity(infoBlk);
                ta.AddNewlyCreatedDBObject(infoBlk, true);

                //סובב את הבלוק על מנת שהטקסט יהיה קריא
                double rotation = LineAngle - Math.PI / 2;
                if (rotation > Math.PI / 2) rotation += Math.PI;
                infoBlk.Rotation = rotation;


                //הוסף לנקודה את שמה
                AttributeReference ar = new AttributeReference();
                ar.SetAttributeFromBlock(blkAtts[0], infoBlk.BlockTransform);
                ar.Position = blkAtts[0].Position.TransformBy(infoBlk.BlockTransform);
                ar.Tag = blkAtts[0].Tag;
                ar.TextString = name;
                ar.AdjustAlignment(this.acTools.acDatabse);

                infoBlk.AttributeCollection.AppendAttribute(ar);
                ta.AddNewlyCreatedDBObject(ar, true);
            }
        }

        public double GetDist2d(Point2d pt1, Point2d pt2)
        {
            return Math.Sqrt(Math.Pow((pt2.Y - pt1.Y), 2) + Math.Pow((pt2.X - pt1.X), 2));
        }

        private double GetDist2d(Point3d pt1, Point3d pt2)
        {
            return Math.Sqrt(Math.Pow((pt2.Y - pt1.Y), 2) + Math.Pow((pt2.X - pt1.X), 2));
        }


        private double GetAngle(Point3d pt1, Point3d pt2)
        {
            //הזוית המוחזרת נמדדת מהיכוון החיובי של ציר האיקס ונגד כיוון השעון
            double a;
            double dx = pt2.X - pt1.X;
            double dy = pt2.Y - pt1.Y;
            //Ignore Z position

            if (dy != 0)
            {
                if (dx == 0)
                {
                    a = Math.Sign(dy) * Math.PI / 2;
                }
                else
                {
                    double tan_a = dy / dx;
                    a = Math.Atan(tan_a);

                    if (dx < 0)
                        a += Math.PI;
                }
            }
            else //dy=0, so we can't calculate tangens
            {
                a = (1 - Math.Sign(dx)) * Math.PI / 2;
            }

            if (a < 0) a += 2 * Math.PI;
            return a;
        }

        private double[] Get3dPoint(string message)
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database myDB = doc.Database;

            try
            {
                using (Transaction ta1 = myDB.TransactionManager.StartTransaction())
                {
                    this.Hide();
                    PromptResult pResult = ed.GetPoint(message);
                    this.Show();
                    if (pResult.Status == PromptStatus.OK)
                    {
                        string Pres = pResult.ToString();
                        string xyzResult = Pres;
                        xyzResult = xyzResult.Remove(0, 8);
                        xyzResult = xyzResult.Remove(xyzResult.Length - 4);
                        xyzResult += ",";

                        int[] comma = new int[4];
                        string[] element = new string[4];
                        int length;
                        comma[0] = -1;

                        for (int i = 1; i < 3; i++)
                        {
                            comma[i] = xyzResult.IndexOf(",", comma[i - 1] + 1);
                            length = comma[i] - comma[i - 1] - 1;
                            element[i] = xyzResult.Substring(comma[i - 1] + 1, length);
                        }

                        double X = Convert.ToDouble(element[1]);
                        double Y = Convert.ToDouble(element[2]);
                        double Z = Convert.ToDouble(element[3]);
                        return new double[] { X, Y, Z };
                    }
                }
                return null;
            }
            catch
            {
                throw new Exception("Point error");
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            this.nudLBP.Enabled = this.checkBox2.Checked;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            RefreshLine();
        }

        private void txtFPF_TextChanged(object sender, EventArgs e)
        {
            RefreshLine();
        }

        private void RefreshLine()
        {
            try
            {
                this.nudFPL.Value = GetNextLine(this.txtFPF.Text);
            }
            catch (Exception e1)
            {
                Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nError: " + e1.Message);
            }
        }

        private int GetNextLine(string fieldName)
        {
            int maxLine = 0;
            int lineEnd;
            int lineSt = fieldName.Length + 1;
            string strLine;
            int line;

            foreach (string pt in this.DwgPointNames)
            {
                try
                {
                    if (pt.StartsWith(fieldName + '_'))
                    {
                        lineEnd = pt.IndexOf('_', lineSt);
                        strLine = pt.Substring(lineSt, lineEnd - lineSt);
                        //acDoc.Editor.WriteMessage("\nLine: " + strLine + "     << " + ar.TextString);
                        line = Convert.ToInt32(strLine);
                        if (line > maxLine) maxLine = line;
                    }
                }
                catch { }
            }
            //this.tAcDoc.Editor.WriteMessage("\n===>> Field: " + fieldName + " >> MAX: " + maxLine.ToString());
            return maxLine + 1;
        }

        private string[] GetDwgPointNames()
        {
            BlockReference br;
            AttributeReference ar;
            List<string> names = new List<string>();
            string blockName = DrawBL.AcadTools.PlanningPointBlockName;

            //יצירת סט של קריטריונים לבחירה
            TypedValue[] acTypValAr;

            //בחר נקודות בתכנון מכל הצבעים
            acTypValAr = new TypedValue[2];
            acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "INSERT"), 0);
            acTypValAr.SetValue(new TypedValue((int)DxfCode.BlockName, blockName), 1);

            // Assign the filter criteria to a SelectionFilter object
            SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);

            // Get the current document and database
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = this.acTools.acDatabse;

            ObjectId[] NewSelSetIds;

            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                PromptSelectionResult acSSPrompt = null;
                acSSPrompt = acDoc.Editor.SelectAll(acSelFtr);

                // If the prompt status is OK, objects were selected
                if (acSSPrompt.Status == PromptStatus.OK)
                {
                    NewSelSetIds = acSSPrompt.Value.GetObjectIds();
                }
                else
                {
                    NewSelSetIds = new ObjectId[0];
                    acDoc.Editor.WriteMessage("\nError While Selecting Points.");
                    return new string[0];
                }

                foreach (ObjectId id in NewSelSetIds)
                {
                    try
                    {
                        br = acTrans.GetObject(id, OpenMode.ForRead) as BlockReference;
                        ar = acTrans.GetObject(br.AttributeCollection[0], OpenMode.ForRead) as AttributeReference;
                        names.Add(ar.TextString);
                    }
                    catch { }
                }

                acTrans.Commit();
                // Dispose of the transaction
            }
            return names.ToArray();
        }

    }
}
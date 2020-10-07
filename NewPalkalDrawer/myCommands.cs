using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;


// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(NewPalkalDrawer.MyCommands))]

namespace NewPalkalDrawer
{

    // This class is instantiated by AutoCAD for each document when
    // a command is called by the user the first time in the context
    // of a given document. In other words, non static data in this class
    // is implicitly per-document!
    public class MyCommands
    {
        // The CommandMethod attribute can be applied to any public  member 
        // function of any public class.
        // The function should take no arguments and return nothing.
        // If the method is an intance member then the enclosing class is 
        // intantiated for each document. If the member is a static member then
        // the enclosing class is NOT intantiated.
        //
        // NOTE: CommandMethod has overloads where you can provide helpid and
        // context menu.

        private static DrawBL.BL bl()
        {
            return new DrawBL.BL();
        }

        //עדכון נקודות מהשרת
        [CommandMethod(CommandsText.UpdateFromServer)]
        public static void UpdateFromServer()
        {
            SyncWizard sw = new SyncWizard(bl());
            Autodesk.AutoCAD.ApplicationServices.Application.ShowModalDialog(sw);
            sw.Dispose();
        }

        //שתילת נקודות חיזוק
        [CommandMethod(CommandsText.InsertPoints)]
        static public void InsertPoints() // This method can have any name
        {
            InsertPointsForm ipf = new InsertPointsForm(bl());
            Autodesk.AutoCAD.ApplicationServices.Application.ShowModalDialog(ipf);
            ipf.Dispose();
        }

        //יצוא נקודות ישנות
        [CommandMethod("uuu")]
        static public void uuu() // This method can have any name
        {
            FishPoints fp = new FishPoints(bl());
            Autodesk.AutoCAD.ApplicationServices.Application.ShowModalDialog(fp);
            fp.Dispose();
        }

        //ספירת נקודות
        [CommandMethod(CommandsText.SelectPoints)]
        static public void SelectPoints() // This method can have any name
        {
            SelectPtsForm spf = new SelectPtsForm(bl());
            Autodesk.AutoCAD.ApplicationServices.Application.ShowModalDialog(spf);
            spf.Dispose();
        }

        //הפעלת נקודות
        [CommandMethod(CommandsText.AddToDatabase)]
        static public void AddToDb() // This method can have any name
        {
            AddToDbForm adb = new AddToDbForm(bl(), false);
            if (adb.Connected)
                Autodesk.AutoCAD.ApplicationServices.Application.ShowModalDialog(adb);
            adb.Dispose();
        }

        //יצוא נקודות לסימון
        [CommandMethod(CommandsText.SendToMark)]
        static public void SendToMark() // This method can have any name
        {
            AddToDbForm adb = new AddToDbForm(bl(), true);
            Autodesk.AutoCAD.ApplicationServices.Application.ShowModalDialog(adb);
            adb.Dispose();
        }

        //עדכון שלב הנקודה
        [CommandMethod(CommandsText.UpdatePointStatus, CommandFlags.UsePickSet)]
        static public void UpdatePointStatus() // This method can have any name
        {
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            PromptIntegerResult pir = ed.GetInteger("\n >>Point Status ID: ");
            if (pir.Status != PromptStatus.OK)
            {
                ed.WriteMessage("\nלא התקבל מספר חוקי");
                return;
            }
            int statusId = pir.Value;

            if ((statusId < 0) || (statusId > 6))
            {
                ed.WriteMessage("\nלא התקבל מספר שלב חוקי");
                return;
            }

            DrawBL.BL myBL = bl();
            List<object[]> PtIds = myBL.GetSelectedPointsIds();

            if (PtIds.Count == 0)
            {
                ed.WriteMessage("\nלא נבחרו נקודות פעילות. הפקודה בוטלה");
                return;
            }
            ed.WriteMessage("\nנבחרו " + PtIds.Count + " נקודות");

            object[] dbIDs = new object[PtIds.Count];
            for (int i = 0; i < PtIds.Count; i++)
                dbIDs[i] = PtIds[i][1];

            DateTime b = DateTime.Now;
            if (myBL.UpdateReport(dbIDs, statusId))
                if (myBL.UpdatePointsStatus(PtIds, statusId))
                {
                    log("העדכון לקח " + (DateTime.Now - b).TotalSeconds.ToString("0.00") + " שניות");
                    myBL.SaveDrawing();
                }
        }

        //עדכון מיקום נקודות
        [CommandMethod(CommandsText.MovePoints, CommandFlags.UsePickSet)]
        static public void UpdatePointPosition() // This method can have any name
        {
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            DrawBL.BL myBL = bl();
            List<object[]> PtIds = myBL.GetSelectedPointsIds();

            if (PtIds.Count == 0)
            {
                ed.WriteMessage("\nלא נבחרו נקודות פעילות. הפקודה בוטלה");
                return;
            }

            PromptResult pr = ed.GetString("האם אתה בטוח שברצונך לעדכן את מיקומן של " + PtIds.Count + " נקודות? (Y/N)");
            string msg;
            double[] pos;
            int ok = 0;

            if (pr.Status == PromptStatus.OK)
                if (pr.StringResult.ToUpper() == "Y" | pr.StringResult.ToUpper() == "YES" | pr.StringResult == "כן" | pr.StringResult == "כ")
                {
                    Transaction ta = myBL.acTools.StartTransaction();
                    foreach (object[] pt in PtIds)
                    {
                        ed.WriteMessage("\nמעדכן נק' מס' " + pt[1] + "...");
                        if (myBL.acTools.GetBlockRefPosition(pt[0], ta, out pos))
                        {
                            //ed.WriteMessage("\nX:" + pos[0] + ", Y:" + pos[1] + ", Z:" + pos[2]);
                            if (myBL.UpdatePointsPosition((int)pt[1], pos, out msg))
                            {
                                ed.WriteMessage("   OK!");
                                ok++;
                            }
                            else ed.WriteMessage("  Error: " + msg);
                        }
                        else
                            ed.WriteMessage("   Error: Position not achieved");
                    }
                    ed.WriteMessage("\nTotal Points Updated: " + ok);
                }
        }

        //עדכון תיבות
        [CommandMethod(CommandsText.UpdateBox)]
        static public void UpdateBox() // This method can have any name
        {
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            DrawBL.BL myBL = bl();


            using (Transaction ta = myBL.acTools.StartTransaction())
            {
                //רשימת קריטריונים לבחירה שמאפשרת בחירת תיבות בלבד
                TypedValue[] acTypValAr = new TypedValue[2];
                acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "INSERT"), 0);
                acTypValAr.SetValue(new TypedValue((int)DxfCode.BlockName, DrawBL.AcadTools.BoxBlockName), 1);

                // Assign the filter criteria to a SelectionFilter object
                SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);

                PromptSelectionResult psr = ed.GetSelection(acSelFtr);
                if (psr.Status == PromptStatus.OK)
                {
                    try
                    {
                        List<ObjectId[]> pairs = new List<ObjectId[]>();
                        DBDictionary groupTbl = (DBDictionary)ta.GetObject(myBL.acTools.acDatabse.GroupDictionaryId, OpenMode.ForRead);
                        foreach (ObjectId id in psr.Value.GetObjectIds())
                        {
                            ObjectId[] pair = GetGroupPair(id, ta, groupTbl, ed);
                            if (pairs != null) pairs.Add(pair);
                        }
                        ed.WriteMessage("\nTotal Boxes: " + pairs.Count);

                        string[] msg;
                        if (pairs.Count > 0)
                        {
                            if (myBL.UpdateBoxes(pairs, out msg))
                                ed.WriteMessage("\nהעדכון הצליח");
                            else
                                ed.WriteMessage("\nאירעו תקלות בעדכון");
                            foreach (string line in msg)
                                ed.WriteMessage("\n" + line);
                        }
                    }
                    catch (System.Exception msg)
                    {
                        ed.WriteMessage("\nError: " + msg);
                    }
                }
            }
        }

        private static ObjectId[] GetGroupPair(ObjectId objectId, Transaction ta, DBDictionary groupTbl, Editor ed)
        {
            foreach (DBDictionaryEntry grp in groupTbl)
            {
                Group g = ta.GetObject(grp.Value, OpenMode.ForRead) as Group;
                try
                {
                    if (!g.Name.StartsWith("box_")) continue;

                    ObjectId[] g_ids = g.GetAllEntityIds();
                    if (g_ids[0] == objectId)
                    {
                        ed.WriteMessage("\n....Group: " + g.Name + "  >> " + g_ids[0] + " , " + g_ids[1]);
                        return g_ids;
                    }
                }
                catch
                {
                    //ed.WriteMessage("\nError: " + msg);
                    continue;
                }
            }
            ed.WriteMessage("No group contains this box");
            return null;
        }


        //עדכון פרט הנקודה
        [CommandMethod(CommandsText.UpdatePointDetail, CommandFlags.UsePickSet)]
        static public void UpdatePointDetail() // This method can have any name
        {
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            PromptIntegerResult pir = ed.GetInteger("\n >>Point Detail ID: ");
            if (pir.Status != PromptStatus.OK)
            {
                ed.WriteMessage("\nלא התקבל מספר חוקי");
                return;
            }
            int detailId = pir.Value;

            DrawBL.BL myBL = bl();
            List<object[]> PtIds = myBL.GetSelectedPointsIds();

            if (PtIds.Count == 0)
            {
                ed.WriteMessage("\nלא נבחרו נקודות פעילות. הפקודה בוטלה");
                return;
            }
            ed.WriteMessage("\nנבחרו " + PtIds.Count + " נקודות");

            string msg;
            if (!myBL.CheckServerConnection(out msg))
            {
                log("לא ניתן ליצור קשר עם השרת");
                return;
            }

            if (myBL.UpdatePointsDetail(PtIds, detailId))
                myBL.SaveDrawing();
        }

        //הצגת דוח נקודה
        [CommandMethod(CommandsText.DisplayReport, CommandFlags.UsePickSet)]
        static public void DisplayReport() // This method can have any name
        {
            DrawBL.BL myBL = bl();
            List<object[]> PtIds = myBL.GetSelectedPointsIds();
            string msg;
            if (PtIds.Count == 0)
            {
                log("לא נבחרו נקודות פעילות. הפקודה בוטלה");
                return;
            }
            else if (PtIds.Count > 1)
            {
                log("יש לבחור נקודה אחת בלבד");
                return;
            }
            else if (!myBL.CheckServerConnection(out msg))
            {
                log("לא ניתן ליצור קשר עם השרת");
                return;
            }

            object[] repVals;
            int ptStatusId;

            if (!myBL.GetPointReport((int)PtIds[0][1], out repVals, out ptStatusId, out msg))
            {
                log("Error: " + msg);
                return;
            }

            ReportForm rf = new ReportForm(repVals, ptStatusId);
            Autodesk.AutoCAD.ApplicationServices.Application.ShowModalDialog(rf);
            rf.Dispose();
        }


        //הדגשת פרטים
        [CommandMethod(CommandsText.EmphasisDetail)]
        static public void EmphasisDetail() // This method can have any name
        {
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            PromptIntegerResult pir = ed.GetInteger("\n >>Point Detail ID: ");
            if (pir.Status != PromptStatus.OK)
            {
                ed.WriteMessage("\nלא התקבל מספר חוקי");
                return;
            }
            int detailId = pir.Value;

            try
            {
                //ביטול הכלל הקודם
                log("Instance is ok, Detail is " + PointRules.detailID);
                Overrule.RemoveOverrule(Entity.GetClass(typeof(BlockReference)), PointRules.instance);
                PointRules.instance.Dispose();
                PointRules.instance = null;
                log("Destroyed!");
            }
            catch (Autodesk.AutoCAD.Runtime.Exception e)
            {
                log("Can't remove rule: " + e.Message);
            }

            PointRules.instance = new PointRules(detailId);
            log("New instance, Detail is " + PointRules.detailID);

            Overrule.AddOverrule(Entity.GetClass(typeof(BlockReference)), PointRules.instance, true);
            log("Overrule added. Detail is " + PointRules.detailID);

            Overrule.Overruling = true;
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            doc.SendStringToExecute("_.regen ", true, false, false);

        }

        //הצגת דוח נקודה
        [CommandMethod(CommandsText.RotateLevel)]
        static public void RotateLevel() // This method can have any name
        {
            string msg;

            DrawBL.BL myBL = bl();
            int levelId;

            if (!myBL.GetDrawingLevel(out levelId))
            {
                log("לא ניתן לקרוא את מספר המפלס מהשרטוט");
                return;
            }
            if (!myBL.CheckServerConnection(out msg))
            {
                log("לא ניתן ליצור קשר עם השרת");
                return;
            }

            //הצג חלון בו יוכל המשתמש להזין את זווית הסיבוב
            RotateLevelForm rlf = new RotateLevelForm(Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor);
            Autodesk.AutoCAD.ApplicationServices.Application.ShowModalDialog(rlf);
            if (rlf.DialogResult == DialogResult.OK)
            {
                double RadAngle = rlf.angleToRotate;
                if (myBL.RotateLevel(levelId, RadAngle, out msg))
                {
                    log("Server Message:\n" + msg + "\n");
                    MessageBox.Show("מסד הנתונים עודכן בהצלחה:\n\n" + RadAngle);
                    myBL.RotateObjects(RadAngle);
                    MessageBox.Show("השרטוט עודכן בהצלחה\n\n" + RadAngle);
                    UpdateFromServer();
                }
                else
                    log("הפעולה נכשלה:\n" + msg);
            }
            else
            {
                log("הפעולה בוטלה על ידי המשתמש");
            }

            //anyway...
            rlf.Dispose();
        }

        private static void log(string p)
        {
            Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n" + p);
        }

    }

}

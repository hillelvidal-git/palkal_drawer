using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
//using Autodesk.AutoCAD.Interop;
using System.IO;
using System.Diagnostics;
using Autodesk.AutoCAD.Geometry;

namespace DrawBL
{
    public class BL
    {
        public AcadTools acTools;
        Drawer.DrawingAdapter drawer;
        WebAdapter connector;
        PointsLocator locator;
        PrinterAdapter printer;
        public bool AutoSave = true;

        public BL()
        {
            this.acTools = new AcadTools();
            this.drawer = new Drawer.DrawingAdapter(this.acTools);
            this.connector = new WebAdapter();
        }

        public bool WriteCertificateToDwg(DateTime serverTime, out string msg)
        {
            return this.drawer.UpdateCertificate(serverTime, out msg);
        }

        public bool CheckServerConnection(out string msg)
        {
            return this.connector.ConnectServer(out msg);
        }

        public bool GetDrawingCertificate(out int levelId, out int blocId, out DateTime lastDwgUpdate, out string msg)
        {
            return this.drawer.ReadCertificate(out levelId, out blocId, out lastDwgUpdate, out msg);
        }

        public bool GetServerNewPoints(int levelId, int blocId, DateTime lastDwgUpdate, out object[][] news, out string msg)
        {
            return this.connector.GetNewPoints(levelId, blocId, lastDwgUpdate, out news, out msg);
        }

        public bool GetServerNewBoxes(int levelId, int blocId, DateTime lastDwgUpdate, out int[] news, out string msg)
        {
            return this.connector.GetNewBoxes(levelId, blocId, lastDwgUpdate, out news, out msg);
        }

        public bool GetServerNewMeasurements(int levelId, int blocId, DateTime lastDwgUpdate, out object[][] news, out string msg)
        {
            return this.connector.GetNewMeasurements(levelId, blocId, lastDwgUpdate, out news, out msg);
        }

        public bool DownloadBox(int dbId, out object[] attributes, out double[][] samples, bool downloadExisting, out string msg)
        {
            ObjectId acId = new ObjectId(); bool ptExists = false;
            foreach (object[] pt in this.acTools.DwgBoxes)
                if ((int)pt[0] == dbId) { acId = (ObjectId)pt[1]; ptExists = true; break; }

            if (ptExists & !downloadExisting)
            {
                attributes = new string[0]; samples = new double[0][];
                msg = "Box Exists in drawing";
                return true;
            }

            return connector.GetBox(dbId, out attributes, out samples, out msg);
        }

        public bool GetServerTime(out DateTime serverTime, out string msg)
        {
            return connector.GetServerTime(out serverTime, out msg);
        }

        public bool DrawBox(int levelId, object[] values, double[][] samples_Rad_Met, out string msg)
        {
            double[] srvPos, orPos;
            object[] srvVals = new object[] { values[2], values[1], 0 };
            object[] orVals = new object[] { levelId, values[3], 2 };

            if (!connector.GetBorePosition(srvVals, out srvPos))
            {
                msg = "SRV not exists yet: " + values[11].ToString() + ", Srv No. " + values[1].ToString();
                return true;
            }
            if (!connector.GetBorePosition(orVals, out orPos))
            {
                msg = "OR not exists yet: Or No. " + values[3].ToString();
                return true;
            }

            return drawer.DrawBox(srvPos, orPos, values, samples_Rad_Met, out  msg);
        }

        public bool DrawMeasurement(object[] values, out string msg)
        {
            //ציור נקודת מדידה עם הערכים הנתונים
            int dbId = (int)values[0];
            //בדיקה אם הנקודה קיימת בשרטוט
            ObjectId acId = new ObjectId(); bool ptExists = false;
            foreach (object[] pt in this.acTools.DwgMeasurements)
                if ((int)pt[0] == dbId) { acId = (ObjectId)pt[1]; ptExists = true; break; }

            if (!ptExists) return drawer.DrawMeasurement(values, out  msg);
            else return drawer.UpdateMeasurement(values, acId, out msg);
        }

        public bool DrawPoint(object[] values, out string msg)
        {
            //ציור נקודת חיזוק עם הערכים הנתונים
            int dbId = (int)values[0];
            //בדיקה אם הנקודה קיימת בשרטוט
            ObjectId acId = new ObjectId(); bool ptExists = false;

            foreach (object[] pt in this.acTools.DwgPoints)
                if ((int)pt[0] == dbId) { acId = (ObjectId)pt[1]; ptExists = true; break; }

            if (!ptExists) return drawer.DrawPoint(values, out msg);
            else return drawer.UpdatePoint(values, acId, out msg);
        }

        public bool ReadDwgPoints(out int pts, out string msg)
        {
            return acTools.ReadPoints(out pts, out msg);
        }

        public bool ReadMeasurements(out int pts, out string msg)
        {
            return acTools.ReadMeasurements(out  pts, out msg);
        }

        public bool ReadDwgSrvs(out int pts, out string msg)
        {
            return acTools.ReadSrvs(out  pts, out msg);
        }

        public bool DrawSrv(object[] values, out string msg)
        {
            //ציור נקודת סקר עם הערכים הנתונים
            int dbId = (int)values[0];
            //בדיקה אם הנקודה קיימת בשרטוט
            ObjectId acId = new ObjectId(); bool ptExists = false;
            foreach (object[] pt in this.acTools.DwgSrvs)
                if ((int)pt[0] == dbId) { acId = (ObjectId)pt[1]; ptExists = true; break; }

            if (!ptExists) return drawer.DrawSrv(values, out  msg);
            else return drawer.UpdateSrv(values, acId, out  msg);
        }

        public bool ReadDwgSurveys(out int boxes, out string msg)
        {
            return acTools.ReadSurveys(out boxes, out msg);
        }

        public string GetDwgName()
        {
            return this.acTools.GetDwgName();
        }

        public bool MovePoint(BlockReference br, Autodesk.AutoCAD.Geometry.Matrix3d transform, out string msg)
        {
            msg = "OK";
            return true;
        }

        public bool IsDwgReady(out string msg)
        {
            return this.acTools.IsDwgReady(out msg);
        }

        public bool GetProjectByLevel(int levelId, out int projectId, out string projectName)
        {
            return connector.GetProjectByLevel(levelId, out projectId, out projectName);
        }

        public object[][] GetEngDetails()
        {
            return connector.GetPointsDetails();
        }

        public bool GetLevelBlocs(int levelId, out object[][] blocs)
        {
            return connector.GetLevelBlocs(levelId, out blocs);
        }

        public bool GetLevelName(int levelId, out string levelName)
        {
            return connector.GetLevelName(levelId, out levelName);
        }

        public void SaveDrawing()
        {
            this.acTools.acDocument.SendStringToExecute("_.qsave ", true, false, true);
        }

        public bool GetFieldId(string name, int blocId, out int fieldId, out string msg)
        {
            return connector.GetFieldId(name, blocId, out fieldId, out msg);
        }

        public bool GetLineId(int intName, int fieldId, out int lineId, out string msg)
        {
            return connector.GetLineId(intName, fieldId, out lineId, out msg);
        }

        public bool UploadNewPoint(int ptNum, int lineId, double[] position, double rotation_Rad, int ptDetail, int statusId, int problem, out string msg, out int newId)
        {
            return connector.UploadNewPoint(ptNum, lineId, position, rotation_Rad, ptDetail, statusId, problem, out msg, out newId);
        }

        public string GetStakeoutFolder()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Bedek\StakeOut";
        }

        public bool PrintTxtFile(string filename)
        {
            try
            {
                printer.PrintFile(filename);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Return a list of "AutocadID, DatabaseID"
        /// </summary>
        /// <returns></returns>
        public List<object[]> GetSelectedPointsIds()
        {
            return acTools.GetSelectedPointsIds();
        }

        public bool UpdatePointsStatus(List<object[]> PtIds, int statusId)
        {
            string msg;
            if (!CheckServerConnection(out msg))
            {
                log("Cannot connect server: " + msg);
                log("הפעולה בוטלה");
                return false;
            }
            log("נוצר קשר עם השרת");

            ObjectId acId; int dbId;
            int updated = 0; int drawed = 0;

            using (Transaction ta = acTools.StartTransaction())
            {
                log("מעדכן שלב נקודה: " + statusId);
                foreach (object[] pt in PtIds)
                {
                    log("    Point ID: " + pt[1].ToString());
                    try
                    {
                        acId = (ObjectId)pt[0];
                        dbId = (int)pt[1];

                        if (!connector.UpdatePointStatus(dbId, statusId, out msg))
                        {
                            throw new System.Exception("        Updating server failed: " + msg);
                        }
                        updated++;
                        logw("  <Server updated: OK>");

                        if (!drawer.UpdatePointStatus(ta, acId, statusId, out msg))
                        {
                            throw new System.Exception("        Updating drawing failed: " + msg);
                        }
                        drawed++;
                        logw("  <Drawing updated: OK>");
                    }
                    catch (System.Exception e)
                    {
                        logw("  [!] Error:" + e.Message);
                        continue;
                    }
                } //iterate all points
                log("Point updated in SERVER: " + updated + ",  Point updated in DRAWING: " + drawed);
                ta.Commit();
            }
            return true;
        }

        public bool UpdatePointsDetail(List<object[]> PtIds, int detailId)
        {
            string msg;
            if (!CheckServerConnection(out msg))
            {
                log("Cannot connect server: " + msg);
                log("הפעולה בוטלה");
                return false;
            }
            log("נוצר קשר עם השרת");

            ObjectId acId; int dbId;
            int updated = 0; int drawed = 0;

            using (Transaction ta = acTools.StartTransaction())
            {
                log("מעדכן פרט נקודה: " + detailId);
                foreach (object[] pt in PtIds)
                {
                    log("    Point ID: " + pt[1].ToString());
                    try
                    {
                        acId = (ObjectId)pt[0];
                        dbId = (int)pt[1];

                        if (!connector.UpdatePointDetail(dbId, detailId, out msg))
                        {
                            throw new System.Exception("        Updating server failed: " + msg);
                        }
                        updated++;
                        logw("  <Server updated: OK>");

                        if (!drawer.UpdatePointDetail(ta, acId, detailId, out msg))
                        {
                            throw new System.Exception("  <Updating drawing failed:> " + msg);
                        }
                        drawed++;
                        logw("  <Drawing updated: OK>");
                    }
                    catch (System.Exception e)
                    {
                        logw("  [!] Error:" + e.Message);
                        continue;
                    }
                } //iterate all points
                log("Point updated in SERVER: " + updated + ",  Point updated in DRAWING: " + drawed);

                ta.Commit();
            }
            return true;
        }

        public void log(string p)
        {
            this.acTools.acEditor.WriteMessage("\n" + p);
        }

        public void logw(string p)
        {
            this.acTools.acEditor.WriteMessage(p);
        }

        public bool UpdateReport(object[] PtIds, int statusId)
        {
            try
            {
                List<object[]> valsToUpdate = new List<object[]>();
                DateTime compDate;
                string reporter, comment;

                //שם המדווח 
                reporter = GetDefaultReporter();
                valsToUpdate.Add(new object[] { "Reporter", reporter });
                log("המדווח: " + reporter);

                //בקש מהמשתמש עוד נתונים בהתאם לשלב
                switch (statusId)
                {
                    case 0:
                    case 1:
                    case 2:
                        break;

                    case 3: //problem :תיאור הבעיה
                        string problem = GetEdString("תיאור הבעיה: ");
                        valsToUpdate.Add(new object[] { "Problem", problem });
                        break;

                    case 4: //done :מנהל, תאריך סיום, תיעוד
                        string manager, docu;

                        manager = GetEdString("מנהל אחראי:", GetDefaultManager());
                        if (manager == "") throw new System.Exception("חובה להזין שם מנהל");
                        valsToUpdate.Add(new object[] { "Manager", manager });
                        SaveDefaultManager(manager);

                        docu = GetEdString("תיעוד:", "");
                        if (!string.IsNullOrEmpty(docu)) valsToUpdate.Add(new object[] { "Documentation", docu });

                        compDate = GetEdTime("תאריך סיום:");
                        valsToUpdate.Add(new object[] { "CompletionDate", compDate });

                        break;

                    case 5: //reported :תאריך דיווח
                        DateTime repDate = GetEdTime("תאריך דיווח: ");
                        valsToUpdate.Add(new object[] { "ReportDate", repDate.Date });
                        break;

                    case 6: //cancelled
                        string reason = GetEdString("סיבת הביטול: ");
                        valsToUpdate.Add(new object[] { "CancellationReason", reason });
                        compDate = GetEdTime("תאריך ביטול: ");
                        valsToUpdate.Add(new object[] { "CompletionDate", compDate });
                        break;
                }

                comment = GetEdString("הערה:", "");
                if (!string.IsNullOrEmpty(comment)) valsToUpdate.Add(new object[] { "Comments", comment });

                //DateTime serverTime; string msg;
                //if (GetServerTime(out serverTime, out msg))
                //    valsToUpdate.Add(new object[] { "Modified", serverTime });

                //foreach (object[] pair in valsToUpdate)


                return UpdatePointsReportValues(PtIds, valsToUpdate);
            }
            catch (System.Exception ee)
            {
                log("\nפעולת העדכון בוטלה");
                log(ee.Message);
                return false;
            }
        }

        private bool UpdatePointsReportValues(object[] PtIds, List<object[]> valsToUpdate)
        {
            int updated = 0; string msg;
            log("\nמעדכן דוחות נקודה...");

            if (this.connector.UpdateReportValues(PtIds, valsToUpdate, out updated, out msg))
            {
                log("\n" + msg);
                log("\nדוחות שעודכנו: " + updated + "\n");
                return true;
            }
            return false;
        }

        private string GetDefaultReporter()
        {
            return Environment.UserName;
        }

        /// <summary>
        /// מבקש מהמשתמש ערך תוך הצגת מחרוזת.
        /// אם הפקודה מבוטלת נזרק חריג
        /// אם מתקבלת מחרוזת ריקה מוחזר ערך ברירת המחדל המסופק
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="defaultStr"></param>
        /// <returns></returns>
        private string GetEdString(string prompt, string defaultStr)
        {
            PromptResult pr = acTools.acEditor.GetString("\n" + prompt + " <" + defaultStr + "> ");
            if (pr.Status != PromptStatus.OK) throw new System.Exception("לא התקבל ערך חוקי");
            if (string.IsNullOrEmpty(pr.StringResult)) return defaultStr;
            else return pr.StringResult;
        }

        private void SaveDefaultManager(string manager)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Bedek\\Logs";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            string file = "PointsDefaultRep.txt";

            try
            {
                using (StreamWriter sw = new StreamWriter(path + "\\" + file))
                {
                    sw.WriteLine(manager);
                }
                //acTools.acEditor.WriteMessage("\nהעדפות הדוח נשמרו לפעמים הבאות");
            }
            catch
            {
                acTools.acEditor.WriteMessage("\nהעדפות לא נשמרו");
            }
        }

        private string GetDefaultManager()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Bedek\\Logs";
            string file = "PointsDefaultRep.txt";
            string manager = "";

            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            try
            {
                using (StreamReader sr = new StreamReader(path + "\\" + file))
                {
                    manager = sr.ReadLine();
                }
                return manager;
            }
            catch
            {
                return "";
            }
        }

        private DateTime GetEdTime(string p)
        {
            string strTime = GetEdString("\n" + p + " <NOW> ");
            if (string.IsNullOrEmpty(strTime)) return DateTime.Now;
            else
                try
                {
                    return Convert.ToDateTime(strTime);
                }
                catch (System.Exception e)
                {
                    throw new System.Exception("לא ניתן לפרש את התאריך:\n" + strTime + "\n" + e.Message);
                }
        }

        private void UpdatePointsReportValue(List<object[]> PtIds, string column, object val)
        {
            int updated = 0; string msg; int repId;
            log("מעדכן: " + column + " << " + val.ToString() + "...");

            foreach (object[] pt in PtIds)
                try
                {
                    if (this.connector.GetPointReportID((int)pt[1], out repId))
                        if (this.connector.UpdateReportValue(repId, column, val, out msg)) updated++;
                }
                catch
                {
                    //TODO:
                }
            log("נקודות שעודכנו: " + updated);
        }

        private string GetEdString(string p)
        {
            return
            Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor
            .GetString("\n" + p)
            .StringResult;
        }

        public bool GetPointReport(int ptId, out object[] repVals, out int ptStatusId, out string msg)
        {
            return this.connector.GetReportValues(ptId, out repVals, out ptStatusId, out msg);
        }

        public bool UpdatePointsPosition(int ptId, double[] pos, out string msg)
        {
            return this.connector.UpdatePointPosition(ptId, pos, out msg);
        }

        public bool UpdateBoxes(List<ObjectId[]> pairs, out string[] msg)
        {
            int ok = 0;
            int boxDbId;
            List<double[]> vertices;
            Point3d p0;
            List<string> msgs = new List<string>();
            string webMsg;
            double boxRotation;

            using (Transaction ta = this.acTools.StartTransaction())
            {
                foreach (ObjectId[] pair in pairs)
                {
                    try
                    {
                        msgs.Add("\nAcadID: " + pair[0] + "  ...");
                        if (this.acTools.GetBoxDbID(pair[0], ta, out boxDbId, out p0, out boxRotation))
                        {
                            msgs.Add("   DbId achieved: " + boxDbId + ".   Rotation: " + boxRotation);
                            if (this.acTools.GetBoxRelativeVertices(pair[1], p0, boxRotation, ta, out vertices))
                            {
                                msgs.Add("   Vertices achieved");
                                if (this.connector.UpdateSurveySamples(boxDbId, vertices, out webMsg))
                                {
                                    msgs.Add("   Updated: " + webMsg);
                                    ok++;
                                }
                                else
                                {
                                    msgs.Add("   !!! Error: " + webMsg);
                                }
                            }
                            else
                            {

                            }
                        }
                        else
                        {

                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            msg = msgs.ToArray();
            return (ok > 0);
        }


        public bool GetDrawingLevel(out int levelId)
        {
            string s_dummy; int i_dummy; DateTime dt_dumy;
            return this.GetDrawingCertificate(out levelId, out i_dummy, out dt_dumy, out s_dummy);
        }

        public bool RotateLevel(int levelId, double RadAngle, out string msg)
        {
            return connector.RotateLevel(levelId, RadAngle, out msg);
        }

        public void RotateObjects(double RadAngle)
        {
            List<bool[]> prevState = this.drawer.OpenAllLayers();
            System.Threading.Thread.Sleep(1);
            this.drawer.RotateAll(RadAngle);
            this.drawer.RestoreLayersState(prevState);
        }

        public void DrawGroupPolylines(GroupDrawer.DidiGroup[] groups)
        {
            drawer.DrawGroupPolylines(groups);
        }


        public void GetPointsDataFromDb(GroupDrawer.DidiGroup[] groups, int levelId, out string errorPoints)
        {
            log("מוריד נתונים מהשרת");
            int g=0;
            string msg;
            errorPoints = "";

            foreach (GroupDrawer.DidiGroup group in groups)
            {
                double[] pos;
                object[] vals = new object[] 
                { 
                    levelId, //LevelId
                    "", //Number - Not Known Yet
                    3 //Class - General
                };

                g++;
                log("--> Downloading Data for Group No. " + g);
                foreach (GroupDrawer.DidiPoint point in group.Points)
                {
                    vals[1] = point.Number.ToString();
                    log("   -->[" + point.Number + "  >>  ");
                    
                    if (connector.GetBorePosition(vals, out pos, out msg))
                    {
                        logw(" X = " + pos[0] + ", Y = " + pos[1]+"]");
                        point.SetValues(-1, pos[0],pos[1]);
                    }
                    else
                    {
                        logw("No Data!]");
                        point.SetStatus("DbError");
                        errorPoints += point.Number.ToString() + ", ";
                    }
                }
                group.Status = "Values"; //מציין שהערכים נקראו ממסד הנתונים
            }
              
        }
    }
}

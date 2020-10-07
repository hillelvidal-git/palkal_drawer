using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace DrawBL
{
    public class AcadTools
    {
        public Editor acEditor;
        public Document acDocument;
        public Database acDatabse;
        public BlockTable acBlockTable;
        public BlockTableRecord acModelspace;
        public List<AttributeDefinition> PointAttributes;
        public List<AttributeDefinition> SrvAttributes;
        public List<AttributeDefinition> MeasurementAttributes;
        public List<AttributeDefinition> BoxAttributes;
        public List<AttributeDefinition> InfoAttributes;
        public List<AttributeDefinition> PlanningPointAttributes;

        public const int PlataDetailID = 0;

        public const string InfoBlockName = "Bedek_DwgInfo";
        public const string StrengthrningPointBlockName = "Bedek_StrengtheningPoint";
        public const string PlanningPointBlockName = "BedekPoint1";
        public const string MeasurementBlockName = "Bedek_Measurement";
        public const string SrvPointBlockName = "Bedek_SrvPoint";
        public const string BoxBlockName = "Bedek_PalkalBox";

        public const string InfoBlockLayer = "Bedek_DwgInfo";
        public const string SrvPointsLayer = "Bedek_SrvPoints";
        public const string BsPointsLayer = "Bedek_BsPoints";
        public const string OrPointsLayer = "Bedek_OrPoints";
        public const string MeasureClassLayer = "Bedek_MeasureClass";
        public const string MeasureNumberLayer = "Bedek_MeasureNumber";
        public const string GenPointsLayer = "Bedek_GenPoints";
        public const string InferiorBoxesLayer = "Bedek_Boxes_Inferior";
        public const string SuperiorBoxesLayer = "Bedek_Boxes_Superior";
        public const string MiddleBoxesLayer = "Bedek_Boxes_Middle";
        public const string PointDetailLayer = "Bedek_PointDetail";
        public const string OldPlanningPointsLayer = "Bedek_OldPlanningPoints";

        public List<object[]> DwgPoints, DwgMeasurements, DwgSrvs, DwgBoxes;
        //Layers Table
        //Blocks Table

        bool AllowModifyReported = false; //קובע האם ניתן לעדכן נקודות מדווחות דרך השרטוט

        public AcadTools()
        {
            this.acDocument = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            this.acEditor = this.acDocument.Editor;
            this.acDatabse = this.acDocument.Database;
            using (Transaction ta = this.acDatabse.TransactionManager.StartTransaction())
            {
                this.acBlockTable = ta.GetObject(acDatabse.BlockTableId, OpenMode.ForWrite) as BlockTable;
                this.acModelspace = ta.GetObject(this.acBlockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                this.PointAttributes = GetBlockAttDefs(AcadTools.StrengthrningPointBlockName, ta);
                this.PlanningPointAttributes = GetBlockAttDefs(AcadTools.PlanningPointBlockName, ta);
                this.SrvAttributes = GetBlockAttDefs(AcadTools.SrvPointBlockName, ta);
                this.MeasurementAttributes = GetBlockAttDefs(AcadTools.MeasurementBlockName, ta);
                this.InfoAttributes = GetBlockAttDefs(AcadTools.InfoBlockName, ta);
                this.BoxAttributes = GetBlockAttDefs(AcadTools.BoxBlockName, ta);
            }
        }

        public Transaction StartTransaction()
        {
            return this.acDocument.TransactionManager.StartTransaction();
        }

        private List<AttributeDefinition> GetBlockAttDefs(string blockname, Transaction ta)
        {
            List<AttributeDefinition> AttributesList = new List<AttributeDefinition>();
            BlockTableRecord blkTR = ta.GetObject(this.acBlockTable[blockname], OpenMode.ForRead) as BlockTableRecord;
            foreach (ObjectId attId in blkTR)
            {
                DBObject obj = ta.GetObject(attId, OpenMode.ForWrite) as DBObject;
                AttributeDefinition attDef = obj as AttributeDefinition;
                if (attDef != null)
                    AttributesList.Add(attDef);
            }
            return AttributesList;
        }


        public static string GetPointStatusLayer(int statusID)
        {
            switch (statusID)
            {
                case -1:
                    return "Points_Planning";
                case 0:
                    return "Points_New";
                case 1:
                    return "Points_Waiting";
                case 2:
                    return "Points_Working";
                case 3:
                    return "Points_Problem";
                case 4:
                    return "Points_Done";
                case 5:
                    return "Points_Reported";
                case 6:
                    return "Points_Canceled";
                default:
                    return "0";
            }
        }

        public static int GetPointStatusLayer(string layerName)
        {
            switch (layerName)
            {
                case "Points_Planning": return -1;
                case "Points_New": return 0;
                case "Points_Waiting": return 1;
                case "Points_Working": return 2;
                case "Points_Problem": return 3;
                case "Points_Done": return 4;
                case "Points_Reported": return 5;
                case "Points_Canceled": return 6;
                default: return -1;
            }
        }

        public bool GetBlockRefPosition(object id, Transaction ta, out double[] pos)
        {
            BlockReference pt;
            try
            {
                pt = ta.GetObject((ObjectId)id, OpenMode.ForRead) as BlockReference;
                pos = new double[] { pt.Position.X, pt.Position.Y, pt.Position.Z };
                return true;
            }
            catch
            {
                pos = new double[0];
                return false;
            }
        }

        internal bool ReadPoints(out int pts, out string msg)
        {
            return ReadBlockIds(AcadTools.StrengthrningPointBlockName, ref this.DwgPoints, 3, out pts, out msg);
        }

        internal bool ReadMeasurements(out int pts, out string msg)
        {
            return ReadBlockIds(AcadTools.MeasurementBlockName, ref this.DwgMeasurements, 3, out pts, out msg);
        }

        private bool ReadBlockIds(string blockName, ref List<object[]> list, int IdAttribute, out int pts, out string msg)
        {
            try
            {
                list = new List<object[]>();
                using (Transaction ta = StartTransaction())
                {
                    //וידוא שניתן לעבוד
                    if (!acBlockTable.Has(blockName))
                        throw new Exception("No " + blockName + " Definition");

                    BlockTableRecord BlockTR = ta.GetObject(acBlockTable[blockName], OpenMode.ForRead) as BlockTableRecord;
                    ObjectIdCollection idColl = BlockTR.GetBlockReferenceIds(true, true);
                    BlockReference ptBR;
                    if (idColl.Count == 0)
                    {
                        pts = 0;
                        msg = "No Blocks";
                        return true;
                    }

                    //מציאת מספרי הזיהוי של הנקודות ושמירתם
                    //כל נקודה נשמרת כ: (int)DbID, (ObjectID)AcadID
                    foreach (ObjectId dwg_id in idColl)
                    {
                        try
                        {
                            ptBR = ta.GetObject(dwg_id, OpenMode.ForRead) as BlockReference;
                            AttributeReference arPtDbId = ta.GetObject(ptBR.AttributeCollection[IdAttribute], OpenMode.ForRead) as AttributeReference;
                            list.Add(new object[] { Convert.ToInt32(arPtDbId.TextString), dwg_id });
                        }
                        catch { }
                    }
                    ta.Commit();
                    pts = list.Count;
                    msg = "OK";
                    return true;
                }
            }
            catch (Exception e)
            {
                pts = 0;
                msg = e.Message;
                return false;
            }
        }

        internal bool ReadSrvs(out int pts, out string msg)
        {
            return ReadBlockIds(AcadTools.SrvPointBlockName, ref this.DwgSrvs, 3, out pts, out msg);
        }

        internal bool ReadSurveys(out int boxes, out string msg)
        {
            return ReadBlockIds(AcadTools.BoxBlockName, ref this.DwgBoxes, 5, out boxes, out msg);
        }

        internal string GetDwgName()
        {
            return Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Name;
        }

        internal bool IsDwgReady(out string msg)
        {
            acEditor.WriteMessage("Checking layers...");
            string msg1, msg2;
            bool layers = PrepareLayers(out msg1);
            acEditor.WriteMessage("Checking blocks...");
            bool blocks = CheckBlockDefs(out msg2);
            msg = "\nLayers: " + msg1 + "\nBlocksL " + msg2;
            return layers && blocks;
        }

        private bool CheckBlockDefs(out string msg)
        {
            BlockTable bt;
            using (Transaction ta = this.acDatabse.TransactionManager.StartTransaction())
            {
                bt = ta.GetObject(acDatabse.BlockTableId, OpenMode.ForRead) as BlockTable;
            }

            bool flag = true;
            msg = "";

            try
            {
                if (!bt.Has(InfoBlockName))
                {
                    throw new Exception();
                }
            }
            catch
            {
                msg += InfoBlockName + " missing!; ";
                flag = false;
            }

            try
            {
                if (!bt.Has(SrvPointBlockName))
                {
                    throw new Exception();
                }
            }
            catch
            {
                msg += SrvPointBlockName + " missing!; ";
                flag = false;
            }

            try
            {
                if (!bt.Has(MeasurementBlockName))
                {
                    throw new Exception();
                }
            }
            catch
            {
                msg += MeasurementBlockName + " missing!; ";
                flag = false;
            }

            try
            {
                if (!bt.Has(StrengthrningPointBlockName))
                {
                    throw new Exception();
                }
            }
            catch
            {
                msg += StrengthrningPointBlockName + " missing!; ";
                flag = false;
            }

            try
            {
                if (!bt.Has(BoxBlockName))
                {
                    throw new Exception();
                }
            }
            catch
            {
                msg += BoxBlockName + " missing!; ";
                flag = false;
            }

            if (flag) msg = "All OK";
            return flag;
        }

        private bool PrepareLayers(out string msg)
        {
            try
            {
                string[] PointsLayers =
                {
                    InfoBlockLayer,
                    SuperiorBoxesLayer, MiddleBoxesLayer, InferiorBoxesLayer, 
                    SrvPointsLayer, BsPointsLayer, OrPointsLayer, GenPointsLayer,
                    MeasureClassLayer, MeasureNumberLayer,
                    GetPointStatusLayer(-1),
                    GetPointStatusLayer(0),
                    GetPointStatusLayer(1),
                    GetPointStatusLayer(2),
                    GetPointStatusLayer(3),
                    GetPointStatusLayer(4),
                    GetPointStatusLayer(5),
                    GetPointStatusLayer(6),
                    PointDetailLayer,
                    OldPlanningPointsLayer
                };

                short[] LayerColorsIndex = 
                { 
                    250, 
                    6, 6, 6,
                    1, 182, 182, 182,
                    182, 40,
                    52, 4, 5, 6, 1, 3, 40, 253,
                    30,
                    5
                };
                   
                // Get the current document and database, and start a transaction
                Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                Database acCurDb = acDoc.Database;
                using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
                {
                    // Returns the layer table for the current database
                    LayerTable acLyrTbl;
                    acLyrTbl = acTrans.GetObject(acCurDb.LayerTableId,
                    OpenMode.ForRead) as LayerTable;
                    string lyrName;
                    short lyrColor;

                    for (int i = 0; i < PointsLayers.Length; i++)
                    {
                        lyrName = PointsLayers[i];
                        lyrColor = LayerColorsIndex[i];
                        // Check to see if layer exists in the Layer table
                        if (acLyrTbl.Has(lyrName) != true)
                        {
                            // Open the Layer Table for write
                            acLyrTbl.UpgradeOpen();
                            // Create a new layer table record and name the layer
                            LayerTableRecord acLyrTblRec = new LayerTableRecord();
                            acLyrTblRec.Name = lyrName;
                            acLyrTblRec.Color = Autodesk.AutoCAD.Colors.Color.FromColorIndex(Autodesk.AutoCAD.Colors.ColorMethod.None, lyrColor);
                            // Add the new layer table record to the layer table and the transaction
                            acLyrTbl.Add(acLyrTblRec);
                            acTrans.AddNewlyCreatedDBObject(acLyrTblRec, true);
                        }
                    }
                    // Commit the changes
                    acTrans.Commit();

                    // Dispose of the transaction
                }
                msg = "All OK";
                return true;
            }
            catch (Exception e)
            {
                msg = e.Message;
                return false;
            }
        }

        public List<object[]> SelectBlocks(string blockName)
        {
            //מאפשר למשתמש לבחור אובייקטים ומחזיר רשימה של כל האובייקטים, ועבור כל אחד שומר את האיידי ואת כל המאפיינים
            AttributeReference ar;
            object[] brValues;
            List<object[]> pts = new List<object[]>();

            //רשימת קריטריונים לבחירה שמאפשרת בחירת בלוק מהסוג הרצוי בלבד
            TypedValue[] acTypValAr;

            acTypValAr = new TypedValue[2];
            acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "INSERT"), 0);
            acTypValAr.SetValue(new TypedValue((int)DxfCode.BlockName, blockName), 1);

            // Assign the filter criteria to a SelectionFilter object
            SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);

            // Start a transaction
            using (Transaction ta = StartTransaction())
            {
                // Request for object to be selected in the drawing area
                PromptSelectionOptions pso = new PromptSelectionOptions();

                PromptSelectionResult acSSPrompt = this.acEditor.GetSelection(pso, acSelFtr);

                // If the prompt status is OK, objects were selected
                if (acSSPrompt.Status == PromptStatus.OK)
                {
                    // Step through the objects in the selection set
                    foreach (SelectedObject acSSObj in acSSPrompt.Value)
                    {

                        // Check to make sure a valid SelectedObject object was returned
                        if (acSSObj != null)
                        {
                            // Open the selected object for read
                            BlockReference acEnt = ta.GetObject(acSSObj.ObjectId, OpenMode.ForRead) as BlockReference;

                            if (acEnt != null)
                            {
                                brValues = new object[1 + acEnt.AttributeCollection.Count];
                                brValues[0] = acEnt.Id;
                                for (int i = 0; i < acEnt.AttributeCollection.Count; i++)
                                {
                                    ar = ta.GetObject(acEnt.AttributeCollection[i], OpenMode.ForRead) as AttributeReference;
                                    brValues[i + 1] = ar.TextString;
                                }
                                pts.Add(brValues);
                            }
                        }
                    } //Each Point
                }
            } //TransAction

            return pts;
        }

        internal List<object[]> GetSelectedPointsIds()
        {
            List<object[]> ids = new List<object[]>();

            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            Database acDB = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database;
            AttributeReference arDBID;
            string strID = ""; int PtDbID;

            //רשימת קריטריונים לבחירה שמאפשרת בחירת נקודות פעילות בלבד
            TypedValue[] acTypValAr = new TypedValue[2];
            acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "INSERT"), 0);
            acTypValAr.SetValue(new TypedValue((int)DxfCode.BlockName, AcadTools.StrengthrningPointBlockName), 1);

            // Assign the filter criteria to a SelectionFilter object
            SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);

            PromptSelectionResult psr = ed.GetSelection(acSelFtr);
            if (psr.Status == PromptStatus.OK)
            {
                ed.WriteMessage(
                  "\nSelected {0} Points.",
                  psr.Value.Count
                );

                using (Transaction ta = StartTransaction())
                {

                    // Step through the objects in the selection set
                    foreach (SelectedObject acSSObj in psr.Value)
                    {
                        // Check to make sure a valid SelectedObject object was returned
                        if (acSSObj != null)
                        {
                            // Open the selected object for read
                            BlockReference acEnt = ta.GetObject(acSSObj.ObjectId, OpenMode.ForRead) as BlockReference;

                            if (acEnt != null)
                            {
                                arDBID = ta.GetObject(acEnt.AttributeCollection[3], OpenMode.ForRead) as AttributeReference;
                                strID = arDBID.TextString;

                                //בדיקה שלא מדובר בנקודה מדווחת
                                if (acEnt.Layer == AcadTools.GetPointStatusLayer(5))
                                {
                                    if (!this.AllowModifyReported)
                                    {
                                        ed.WriteMessage("\n[*] Cannot Modify REPORTED Point: " + strID);
                                        continue;
                                    }
                                    else
                                    {
                                        ed.WriteMessage("\n[!] Modifying REPORTED Point: " + strID);
                                    }
                                }

                                try
                                {
                                    PtDbID = Convert.ToInt32(strID);
                                    //שמור את מז של הנקודה מהשרטוט ומהמסד ברשימה
                                    ids.Add(new object[] { acEnt.Id, PtDbID });
                                }
                                catch
                                {
                                    ed.WriteMessage("\nPoint ID not recognized: " + strID);
                                }
                            }
                        }
                    } //Each Point
                }
            }
            return ids;
        }

        public bool GetBoxRelativeVertices(ObjectId acId, Point3d p0, double rotation, Transaction ta, out List<double[]> vertices)
        {
            vertices = new List<double[]>();
            try
            {
                Polyline br = ta.GetObject(acId, OpenMode.ForRead) as Polyline;
                Double[] vals;

                for (int v = 0; v < br.NumberOfVertices; v++)
                {
                    Point3d vertex = br.GetPoint3dAt(v);
                    if (GetDistAngle(vertex, p0, rotation, out vals))
                        vertices.Add(vals);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool GetDistAngle(Point3d p1, Point3d p0, double rotation, out double[] vals)
        {
            double dx = p1[0] - p0[0];
            double dy = p1[1] - p0[1];
            double angleRad;

            if (dy != 0)
            {
                double tan_a = dx / dy;
                angleRad = Math.Atan(tan_a);

                if ((dx > 0) && (dy < 0)) angleRad -= Math.PI;
                else if ((dx < 0) && (dy < 0)) angleRad += Math.PI;

            }
            else //dy=0, so we can't calculate tangens
            {
                angleRad = Math.Sign(dx) * Math.PI;
            }

            angleRad -= rotation; //הפחתת הזוית שבין הקדח לבין נקודת המכוון, מכיוון שבכל ציור עתידי הזוית הזו תתווסף
            if (angleRad < 0) angleRad += 2 * Math.PI;

            double DistMet = Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));

            vals = new double[] { angleRad, DistMet };
            return true;
        }

        internal bool GetBoxDbID(ObjectId objectId, Transaction ta, out int boxDbId, out Point3d p0, out double boxRotation)
        {
            try
            {
                BlockReference br = ta.GetObject(objectId, OpenMode.ForRead) as BlockReference;
                AttributeReference ar = ta.GetObject(br.AttributeCollection[5], OpenMode.ForRead) as AttributeReference;
                boxDbId = Convert.ToInt32(ar.TextString);
                p0 = br.Position;
                boxRotation = Math.PI / 2 - br.Rotation;
                return true;
            }
            catch
            {
                p0 = new Point3d(-1, -1, -1);
                boxDbId = -1;
                boxRotation = -1;
                return false;
            }
        }

        public static Point3d[] GetUserLine()
        {
            // Get the current database and start the Transaction Manager
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            PromptPointResult pPtRes;
            PromptPointOptions pPtOpts = new PromptPointOptions("");

            // Prompt for the start point
            pPtOpts.Message = "\nEnter the start point of the line: ";
            pPtRes = acDoc.Editor.GetPoint(pPtOpts);
            Point3d ptStart = pPtRes.Value;

            // Exit if the user presses ESC or cancels the command
            if (pPtRes.Status == PromptStatus.Cancel) throw new Exception("Canceled");

            // Prompt for the end point
            pPtOpts.Message = "\nEnter the end point of the line: ";
            pPtOpts.UseBasePoint = true;
            pPtOpts.BasePoint = ptStart;
            pPtOpts.UseDashedLine = true;
            pPtRes = acDoc.Editor.GetPoint(pPtOpts);
            Point3d ptEnd = pPtRes.Value;
            if (pPtRes.Status == PromptStatus.Cancel) throw new Exception("Canceled");

            Point3d[] response = new Point3d[2];
            response[0] = ptStart;
            response[1] = ptEnd;
            return response;
        }

    }
}

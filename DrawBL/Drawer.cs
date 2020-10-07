using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.ApplicationServices;

namespace DrawBL.Drawer
{
    class DrawingAdapter
    {
        AcadTools acTools;
        public DrawingAdapter(AcadTools tools)
        {
            this.acTools = tools;
        }

        internal bool UpdateCertificate(DateTime serverTime, out string msg)
        {
            try
            {
                using (Transaction ta = this.acTools.StartTransaction())
                {
                    //וידוא שניתן לעבוד
                    if (!acTools.acBlockTable.Has(AcadTools.InfoBlockName))
                        throw new Exception("No Info Block Definition");

                    //מציאת בלוק האישור, או יצירת חדש אם אין
                    BlockTableRecord InfoBlockTR = ta.GetObject(acTools.acBlockTable[AcadTools.InfoBlockName], OpenMode.ForRead) as BlockTableRecord;
                    ObjectIdCollection idColl = InfoBlockTR.GetBlockReferenceIds(true, true);
                    BlockReference InfoBlockBR;
                    if (idColl.Count > 0)
                        InfoBlockBR = ta.GetObject(idColl[0], OpenMode.ForWrite) as BlockReference;
                    else
                        InfoBlockBR = CreateNewInfoBlock(ta);

                    UpdateBlockAttribute(ta, InfoBlockBR, 1, serverTime.ToString());
                    ta.Commit();
                    msg = "OK";
                    return true;
                }
            }
            catch (Exception e)
            {
                msg = e.Message;
                return false;
            }
        }

        private void UpdateBlockAttribute(Transaction ta, BlockReference BR, int att, string text)
        {
            //מציאת המאפיינים של הבלוק
            AttributeReference AR = ta.GetObject(BR.AttributeCollection[att], OpenMode.ForWrite) as AttributeReference;
            //עדכון המאפיינים
            AR.TextString = text;
        }

        private BlockReference CreateNewInfoBlock(Transaction ta)
        {
            string[] vals = new string[0];
            Point3d pos = new Point3d(0, 0, 0);
            return InsertBlockReference(AcadTools.InfoBlockName, pos, AcadTools.InfoBlockLayer, 256, this.acTools.InfoAttributes, vals, ta);
        }

        private BlockReference InsertBlockReference(string blockName, Point3d position, string layer, int color, List<AttributeDefinition> attDefCollection, string[] vals, Transaction ta)
        {
            BlockReference newBlock = new BlockReference(position, this.acTools.acBlockTable[blockName]);
            newBlock.SetDatabaseDefaults();
            newBlock.Layer = layer;
            newBlock.ColorIndex = color;

            //add the new block to modelspace:
            BlockTableRecord ms = ta.GetObject(this.acTools.acBlockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
            ObjectId blkRefID = ms.AppendEntity(newBlock);
            ta.AddNewlyCreatedDBObject(newBlock, true);

            //הוסף ומלא את המאפיינים
            AttributeDefinition attDef;
            for (int i = 0; i < attDefCollection.Count; i++)
            {
                attDef = attDefCollection[i];
                AttributeReference attRef = new AttributeReference();
                attRef.SetAttributeFromBlock(attDef, newBlock.BlockTransform);
                attRef.Position = attDef.Position.TransformBy(newBlock.BlockTransform);
                attRef.Tag = attDef.Tag;
                attRef.TextString = vals[i];
                attRef.AdjustAlignment(this.acTools.acDatabse);
                newBlock.AttributeCollection.AppendAttribute(attRef);
                ta.AddNewlyCreatedDBObject(attRef, true);
            }

            return newBlock;
        }

        private bool UpdateBlockReference(ObjectId acId, Point3d position, double rotation_Rad, string layer, int color, string[] blockVals, Transaction ta)
        {
            //עדכון בלוק נתון עם הערכים הנתונים
            try
            {
                //מציאת הבלוק בשרטוט
                BlockReference pt = ta.GetObject(acId, OpenMode.ForWrite) as BlockReference;
                pt.Rotation = rotation_Rad;

                Vector3d displacement = new Vector3d();
                if (pt.Position != position)
                {
                    displacement = pt.Position.GetVectorTo(position);
                    pt.TransformBy(Matrix3d.Displacement(displacement));
                }

                //עדכן את פרטי הבלוק בשרטוט
                AttributeReference ar;
                for (int i = 0; i < blockVals.Length; i++)
                {
                    ar = ta.GetObject(pt.AttributeCollection[i], OpenMode.ForWrite) as AttributeReference;
                    ar.TextString = blockVals[i];
                }
                pt.ColorIndex = color;
                pt.Layer = layer;
                return true;
            }
            catch
            {
                return false;
            }
        }


        internal bool ReadCertificate(out int levelId, out int blocId, out DateTime lastDwgUpdate, out string msg)
        {
            try
            {
                using (Transaction ta = this.acTools.StartTransaction())
                {
                    //וידוא שניתן לעבוד
                    if (!acTools.acBlockTable.Has(AcadTools.InfoBlockName))
                        throw new Exception("No Info Block Definition");

                    BlockTableRecord InfoBlockTR = ta.GetObject(acTools.acBlockTable[AcadTools.InfoBlockName], OpenMode.ForRead) as BlockTableRecord;
                    ObjectIdCollection idColl = InfoBlockTR.GetBlockReferenceIds(true, true);
                    BlockReference InfoBlockBR;
                    if (idColl.Count > 0)
                        InfoBlockBR = ta.GetObject(idColl[0], OpenMode.ForWrite) as BlockReference;
                    else
                        throw new Exception("No Info Block Reference");

                    //מציאת המאפיינים של הבלוק
                    AttributeReference arLevelId = ta.GetObject(InfoBlockBR.AttributeCollection[0], OpenMode.ForRead) as AttributeReference;
                    AttributeReference arUpdateTime = ta.GetObject(InfoBlockBR.AttributeCollection[1], OpenMode.ForWrite) as AttributeReference;
                    levelId = Convert.ToInt32(arLevelId.TextString);
                    lastDwgUpdate = Convert.ToDateTime(arUpdateTime.TextString);
                    blocId = 0; //זמני - כרגע אין התייחסות לבלוק
                    ta.Commit();
                    msg = "OK";
                    return true;
                }
            }
            catch (Exception e)
            {
                msg = e.Message;
                levelId = -1; blocId = -1; lastDwgUpdate = new DateTime(2000, 1, 1);
                return false;
            }
        }

        internal bool DrawBox(double[] srvPos, double[] orPos, object[] values, double[][] samples_Rad_Met, out string msg)
        {
            try
            {
                Point3d pos = new Point3d(srvPos);
                double boxRotation_Rad = Get2DAngle(srvPos, orPos);
                string[] boxVals = new string[]{
                    values[1].ToString(), //BoreNum
                    values[11].ToString(), //Bloc_Field
                    values[4].ToString(), //Date
                    values[5].ToString(), //Machine
                    values[3].ToString(), //Or
                    values[0].ToString() //DbId
                };


                using (Transaction ta = this.acTools.StartTransaction())
                {
                    ObjectIdCollection IdColl = new ObjectIdCollection();

                    // (1) הוספת בלוק המידע של התיבה
                    //קבע את השכבה לתיבה החדשה
                    string boxLayer;
                    int elev;
                    try
                    {
                        elev = (int)values[7];
                    }
                    catch
                    {
                        elev = 0;
                    }
                    if (elev == 0) boxLayer = AcadTools.InferiorBoxesLayer;
                    else if (elev == 1) boxLayer = AcadTools.MiddleBoxesLayer;
                    else boxLayer = AcadTools.SuperiorBoxesLayer;
                    acTools.acEditor.WriteMessage("\nBox Elevetion: " + values[7]);

                    //הכנס בלוק חדש לשרטוט עם הערכים הנתונים
                    BlockReference newBox = InsertBlockReference(AcadTools.BoxBlockName, pos, boxLayer, 256, this.acTools.BoxAttributes, boxVals, ta);
                    newBox.Rotation = Math.PI / 2 - boxRotation_Rad;
                    IdColl.Add(newBox.Id);

                    // (2) הוספת הפוליליין של התיבה עצמה
                    //קבלת שיעורי הקודקודים ממסד הנתונים כולל חישוב המיקום והסיבוב
                    List<Point2d> vertices = CalculateVertices(srvPos, boxRotation_Rad, samples_Rad_Met);
                    Polyline newPoly = new Polyline(vertices.Count);
                    newPoly.Elevation = srvPos[2]; //TODO: כאן צריך להתאים את הגובה למיקום האמיתי של התיבה
                    newPoly.Closed = true;
                    newPoly.Layer = boxLayer;
                    newPoly.ColorIndex = 256; //סימון התיבה כחדשה על ידי צבע כחול

                    int i = 0;
                    foreach (Point2d vertex in vertices)
                    {
                        newPoly.AddVertexAt(i, vertex, 0, 0, 0);
                        i++;
                    }

                    IdColl.Add(this.acTools.acModelspace.AppendEntity(newPoly));
                    ta.AddNewlyCreatedDBObject(newPoly, true);

                    // (3) הוספת קבוצה שתכלול את הבלוק והפוליליין
                    DBDictionary groupTbl = (DBDictionary)ta.GetObject(this.acTools.acDatabse.GroupDictionaryId, OpenMode.ForWrite);

                    //מצא שם לקבוצה החדשה
                    int c = 0;
                    string grpName = "box_" + values[0].ToString() + c.ToString();
                    while (groupTbl.Contains(grpName))
                    {
                        c++;
                        grpName = "box_" + values[0].ToString() + c.ToString();
                    }

                    //הוסף את הקבוצה החדשה
                    Group boxGroup = new Group("Survey", true);
                    ObjectId grpID = groupTbl.SetAt(grpName, boxGroup);
                    ta.AddNewlyCreatedDBObject(boxGroup, true);
                    boxGroup.Append(IdColl);

                    ta.Commit();
                    msg = "OK";
                    return true;
                }
            }
            catch (Exception e)
            {
                msg = e.Message + "\n" + e.StackTrace;
                return false;
            }
        }

        private List<Point2d> CalculateVertices(double[] srvPos, double boxRotation_Rad, double[][] samples_Rad_Met)
        {
            List<Point2d> vertices = new List<Point2d>();
            Point2d newpt;
            double r, a;
            foreach (double[] smaple in samples_Rad_Met)
            {
                a = smaple[0] + boxRotation_Rad;
                r = smaple[1]; //It's  in Meters

                newpt = new Point2d(
                    srvPos[0] + r * Math.Sin(a),
                    srvPos[1] + r * Math.Cos(a));

                vertices.Add(newpt);
            }
            return vertices;
        }

        private double Get2DAngle(double[] p1, double[] p2)
        {
            double a;
            double dx = p2[0] - p1[0];
            double dy = p2[1] - p1[1];

            if (dy != 0)
            {
                double tan_a = dx / dy;
                a = Math.Atan(tan_a);

                if ((dx > 0) && (dy < 0)) a -= Math.PI;
                else if ((dx < 0) && (dy < 0)) a += Math.PI;

                if (a < 0) a += 2 * Math.PI;
            }
            else //dy=0, so we can't calculate tangens
            {
                a = Math.Sign(dx) * Math.PI;
            }
            return a;
        }


        internal bool DrawPoint(object[] values, out string msg)
        {
            int w = 0;
            try
            {
                acTools.acEditor.WriteMessage("\nDrawing Point: " + values[0].ToString());
                Point3d pos = new Point3d((double)values[1], (double)values[2], (double)values[3]);
                string[] blockVals = new string[] //ShortName, BlocName, DetailID, DbID
                { values[7].ToString(), values[8].ToString(), values[4].ToString(), values[0].ToString() };

                using (Transaction ta = this.acTools.StartTransaction())
                {
                    BlockReference pt = InsertBlockReference(AcadTools.StrengthrningPointBlockName, pos, "0", 256, this.acTools.PointAttributes, blockVals, ta);
                    pt.Layer = AcadTools.GetPointStatusLayer((int)values[5]);
                    try
                    {
                        pt.Rotation = (double)values[9];
                    }
                    catch
                    {
                        pt.Rotation = 0;
                    }

                    ta.Commit();
                }
                msg = "OK";
                acTools.acEditor.WriteMessage("...OK (-:");
                return true;
            }
            catch (Exception e)
            {
                acTools.acEditor.WriteMessage("...ERROR )-:");
                msg = w + " -- " + e.Message;
                return false;
            }
        }

        internal bool UpdatePoint(object[] values, ObjectId acId, out string msg)
        {
            //עדכון נקודת חיזוק עם הערכים הנתונים
            try
            {
                acTools.acEditor.WriteMessage("\nUpdating Point: " + values[0].ToString());
                string ptLayer = AcadTools.GetPointStatusLayer((int)values[5]);
                Point3d pos = new Point3d((double)values[1], (double)values[2], (double)values[3]);

                string[] blockVals = new string[] //ShortName, BlocName, DetailID, DbID
                { values[7].ToString(), values[8].ToString(), values[4].ToString(), values[0].ToString() };

                double rotation_Rad;
                try { rotation_Rad = (double)values[9]; }
                catch { rotation_Rad = 0; }

                using (Transaction ta = this.acTools.StartTransaction())
                {
                    UpdateBlockReference(acId, pos, rotation_Rad, ptLayer, 256, blockVals, ta);
                    ta.Commit();
                }
                msg = "OK";
                acTools.acEditor.WriteMessage("...OK (-:");
                return true;
            }
            catch (Exception e)
            {
                msg = e.Message;
                acTools.acEditor.WriteMessage("...ERROR )-:");
                return false;
            }
        }


        internal bool UpdateMeasurement(object[] values, ObjectId acId, out string msg)
        {
            //עדכון נקודת מדידה עם הערכים הנתונים
            try
            {
                Point3d pos = new Point3d((double)values[7], (double)values[8], (double)values[9]);
                string ptClassAndInfo;
                string ptLayer;
                switch (Convert.ToInt16(values[3]))
                {
                    case 1: ptClassAndInfo = "BS";
                        ptLayer = AcadTools.BsPointsLayer;
                        break;
                    case 2: ptClassAndInfo = "OR";
                        ptLayer = AcadTools.OrPointsLayer;
                        break;
                    default: ptClassAndInfo = "";
                        ptLayer = AcadTools.GenPointsLayer;
                        break;
                }
                if (values[5].ToString() != "")
                    ptClassAndInfo += "[" + values[5].ToString().Trim() + "]";

                //הכנת הנתונים להכנסה לבלוק
                string[] blockVals;
                blockVals = new string[] //Number, Class, Date, DbID
                { values[4].ToString().Trim(), ptClassAndInfo, values[6].ToString(), values[0].ToString() };

                using (Transaction ta = this.acTools.StartTransaction())
                {
                    UpdateBlockReference(acId, pos, 0, ptLayer, 256, blockVals, ta);
                    ta.Commit();
                }
                msg = "OK";
                return true;
            }
            catch (Exception e)
            {
                msg = e.Message;
                return false;
            }
        }

        internal bool DrawSrv(object[] values, out string msg)
        {
            //ציור נקודת סקר עם הערכים הנתונים
            try
            {
                Point3d pos = new Point3d((double)values[7], (double)values[8], (double)values[9]);
                string ptLayer;
                ptLayer = AcadTools.SrvPointsLayer;

                string[] blockVals;
                blockVals = new string[] //FieldName_BoreNumber, BlocName, Date, DbID
                { values[12].ToString().Trim() + "_" + values[4].ToString().Trim(), values[13].ToString().Trim(), values[6].ToString(), values[0].ToString() };

                BlockReference pt;

                using (Transaction ta = this.acTools.StartTransaction())
                {
                    pt = InsertBlockReference(AcadTools.SrvPointBlockName, pos, "0", 256, this.acTools.SrvAttributes, blockVals, ta);
                    pt.Layer = ptLayer;
                    ta.Commit();
                }
                msg = "OK";
                return true;
            }
            catch (Exception e)
            {
                msg = e.Message;
                return false;
            }
        }

        internal bool UpdateSrv(object[] values, ObjectId acId, out string msg)
        {
            //עדכון נקודת מדידה עם הערכים הנתונים
            try
            {
                Point3d pos = new Point3d((double)values[7], (double)values[8], (double)values[9]);
                string ptLayer = AcadTools.SrvPointsLayer;

                //הכנת הנתונים להכנסה לבלוק
                string[] blockVals;
                blockVals = new string[] //FieldName_BoreNumber, BlocName, Date, DbID
                { values[12].ToString().Trim() + "_" + values[4].ToString().Trim(), values[13].ToString().Trim(), values[6].ToString(), values[0].ToString() };

                using (Transaction ta = this.acTools.StartTransaction())
                {
                    UpdateBlockReference(acId, pos, 0, ptLayer, 256, blockVals, ta);
                    ta.Commit();
                }
                msg = "OK";
                return true;
            }
            catch (Exception e)
            {
                msg = e.Message;
                return false;
            }
        }

        internal bool DrawMeasurement(object[] values, out string msg)
        {
            //ציור נקודת מדידה עם הערכים הנתונים
            try
            {
                Point3d pos = new Point3d((double)values[7], (double)values[8], (double)values[9]);
                string ptClassAndInfo;
                string ptLayer;
                switch (Convert.ToInt16(values[3]))
                {
                    case 1: ptClassAndInfo = "BS";
                        ptLayer = AcadTools.BsPointsLayer;
                        break;
                    case 2: ptClassAndInfo = "OR";
                        ptLayer = AcadTools.OrPointsLayer;
                        break;
                    default: ptClassAndInfo = "";
                        ptLayer = AcadTools.GenPointsLayer;
                        break;
                }
                if (values[5].ToString().Trim() != "")
                    ptClassAndInfo += "[" + values[5].ToString().Trim() + "]";

                string[] blockVals;
                blockVals = new string[] //Number, Class, Date, DbID
                { values[4].ToString().Trim(), ptClassAndInfo, values[6].ToString(), values[0].ToString() };

                BlockReference pt;

                using (Transaction ta = this.acTools.StartTransaction())
                {
                    pt = InsertBlockReference(AcadTools.MeasurementBlockName, pos, "0", 256, this.acTools.MeasurementAttributes, blockVals, ta);
                    pt.Layer = ptLayer;
                    ta.Commit();
                }
                msg = "OK";
                return true;
            }
            catch (Exception e)
            {
                msg = e.Message;
                return false;
            }
        }

        internal bool UpdatePointStatus(Transaction ta, ObjectId acId, int statusId, out string msg)
        {
            //העבר את הנקודה לשכבה המתאימה לסטטוס החדש
            try
            {
                BlockReference br = ta.GetObject(acId, OpenMode.ForWrite) as BlockReference;
                br.Layer = AcadTools.GetPointStatusLayer(statusId);
                msg = "OK";
                return true;
            }
            catch (Exception e)
            {
                msg = e.Message;
                return false;
            }
        }

        internal bool UpdatePointDetail(Transaction ta, ObjectId acId, int detailId, out string msg)
        {
            //עדכן את מאפיין הפרט בבלוק הנקודה
            try
            {
                BlockReference br = ta.GetObject(acId, OpenMode.ForWrite) as BlockReference;
                AttributeReference ar = ta.GetObject(br.AttributeCollection[2], OpenMode.ForWrite) as AttributeReference;
                ar.TextString = detailId.ToString();
                msg = "OK";
                return true;
            }
            catch (Exception e)
            {
                msg = e.Message;
                return false;
            }
        }

        internal void RotateAll(double RadAngle)
        {

            acTools.acEditor.WriteMessage("\nמסובב את האובייקטים בשרטוט");
            using (Transaction transaction = acTools.acDatabse.TransactionManager.StartTransaction())
            {
                BlockTableRecord btRecord = (BlockTableRecord)transaction.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(acTools.acDatabse), OpenMode.ForRead);
                foreach (ObjectId id in btRecord)
                {
                    Entity entity = (Entity)transaction.GetObject(id, OpenMode.ForWrite);
                    entity.TransformBy(Matrix3d.Rotation(RadAngle, acTools.acEditor.CurrentUserCoordinateSystem.CoordinateSystem3d.Zaxis, new Point3d(0, 0, 0)));
                }
                transaction.Commit();
            }

        }

        internal List<bool[]> OpenAllLayers()
        {
            ///////////////עבור על כל השכבות, רשום את מצבן ופתח אותן לכתיבה
            List<bool[]> layerStates = new List<bool[]>();

            //קבל את רשימת השכבות
            // Get the current document and database, and start a transaction
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            acDoc.Editor.WriteMessage("\n\nפותח את כל השכבות לכתיבה:");
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Returns the layer table for the current database
                LayerTable acLyrTbl;
                acLyrTbl = acTrans.GetObject(acCurDb.LayerTableId,
                                             OpenMode.ForRead) as LayerTable;

                // Step through the Layer table and print each layer name
                foreach (ObjectId acObjId in acLyrTbl)
                {
                    try
                    {//קבל את השכבה
                        LayerTableRecord l;
                        l = acTrans.GetObject(acObjId, OpenMode.ForWrite) as LayerTableRecord;

                        //רשום את מצבה
                        layerStates.Add(new bool[] { l.IsOff, l.IsFrozen, l.IsLocked });
                        //acDoc.Editor.WriteMessage("\n" + l.Name + " -> Off-" + l.IsOff + ", Frozen-" + l.IsFrozen + ", Locked-" + l.IsLocked);

                        //פתח את השכבה לשינויים
                        l.IsOff = false;
                        l.IsFrozen = false;
                        l.IsLocked = false;
                    }
                    catch (Exception el)
                    {
                        acDoc.Editor.WriteMessage("\n<!> Error: " + el.Message);
                    }
                }
                acTrans.Commit();
                // Dispose of the transaction
            }

            return layerStates;
        }

        internal void RestoreLayersState(List<bool[]> layerStates)
        {
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            acDoc.Editor.WriteMessage("\n\nמשחזר את מצב השכבות המקורי:");
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Returns the layer table for the current database
                LayerTable acLyrTbl;
                acLyrTbl = acTrans.GetObject(acCurDb.LayerTableId,
                                             OpenMode.ForRead) as LayerTable;

                // Step through the Layer table and print each layer name
                int n = 0;
                foreach (ObjectId acObjId in acLyrTbl)
                {
                    try
                    {
                        //קבל את השכבה
                        LayerTableRecord l;
                        l = acTrans.GetObject(acObjId, OpenMode.ForWrite) as LayerTableRecord;

                        //עדכן את מצבה
                        l.IsOff = layerStates[n][0];
                        l.IsFrozen = layerStates[n][1];
                        l.IsLocked = layerStates[n][2];

                        //acDoc.Editor.WriteMessage("\n" + l.Name + "\t\t\t -> Off-" + l.IsOff + ",\tFrozen-" + l.IsFrozen + ",\tLocked-" + l.IsLocked);
                    }
                    catch (Exception ell)
                    {
                        acDoc.Editor.WriteMessage("\n<!> Error: " + ell.Message);
                    }
                    n++;
                }
                acTrans.Commit();
                // Dispose of the transaction
            }
        }

        internal bool DrawGroupPolylines(GroupDrawer.DidiGroup[] groups)
        {
            int w = 0;

            try
            {
                List<Polyline> news = new List<Polyline>();
                foreach (GroupDrawer.DidiGroup group in groups)
                {
                    // Each Group
                    acTools.acEditor.WriteMessage("\nCreating Polyline No. " + w++ + " >> ");
                    Polyline newPoly = new Polyline();
                    foreach (GroupDrawer.DidiPoint vertex in group.Points)
                    {
                        if (vertex.Status == "Values")
                        {
                            newPoly.AddVertexAt(0, new Point2d(vertex.X, vertex.Y), 0, 0, 0);
                            acTools.acEditor.WriteMessage("* ");
                        }
                        else
                        {
                            acTools.acEditor.WriteMessage("! ");
                        }
                    }
                    newPoly.Closed = group.IsClosed;
                    newPoly.Layer = "0";
                    if (newPoly.Closed) acTools.acEditor.WriteMessage(" [Close]");
                    else acTools.acEditor.WriteMessage(" [Opene]");
                    //TODO: TAG

                    news.Add(newPoly);
                }

                using (Transaction ta = this.acTools.StartTransaction())
                {
                    BlockTableRecord ms = ta.GetObject(this.acTools.acBlockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    acTools.acEditor.WriteMessage("\nAdding Polylines to ModelSpace >> ");
                    foreach (Polyline newp in news)
                    {
                        ms.AppendEntity(newp);
                        ta.AddNewlyCreatedDBObject(newp, true);
                        acTools.acEditor.WriteMessage("* ");
                    }

                    //אשר את כל העסק
                    ta.Commit();
                }
                acTools.acEditor.WriteMessage("\nהשרטוט הסתיים");


                return true;
            }
            catch
            {
                acTools.acEditor.WriteMessage("\n...ERROR )-:");
                return false;
            }
        }
    }

}

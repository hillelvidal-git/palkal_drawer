using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;

namespace NewPalkalDrawer
{
    class PointRules : Autodesk.AutoCAD.GraphicsInterface.DrawableOverrule
    {
        public static int detailID;
        public static PointRules instance = new PointRules(-1);

        public PointRules(int detail)
        {
            detailID = detail;
        }

        public override bool WorldDraw(Drawable drawable, WorldDraw wd)
        {
            try
            {
                BlockReference br = (BlockReference)drawable;
                //בדוק אם זהו בלוק נקודה והאם זהו הפרט הרצוי
                if (IsBedekPoint(br, detailID))
                {
                    Point3d[][] SunLines = GetDiamondLines(br.Position);
                    foreach (Point3d[] line in SunLines)
                        wd.Geometry.WorldLine(line[0], line[1]);
                }
            }
            catch { }
            return base.WorldDraw(drawable, wd);
        }

        private Point3d[][] GetDiamondLines(Point3d c)
        {
            double r1 = 0.07; double r2 = 0.075;
            Point3d[][] lines = new Point3d[8][]
            {
                new Point3d[] {new Point3d(c[0],c[1]-r1, c[2]), new Point3d(c[0]+r1,c[1],c[2])},
                new Point3d[] {new Point3d(c[0],c[1]+r1, c[2]), new Point3d(c[0]+r1,c[1],c[2])},
                new Point3d[] {new Point3d(c[0],c[1]-r1, c[2]), new Point3d(c[0]-r1,c[1],c[2])},
                new Point3d[] {new Point3d(c[0],c[1]+r1, c[2]), new Point3d(c[0]-r1,c[1],c[2])},
                
                new Point3d[] {new Point3d(c[0],c[1]-r2, c[2]), new Point3d(c[0]+r2,c[1],c[2])},
                new Point3d[] {new Point3d(c[0],c[1]+r2, c[2]), new Point3d(c[0]+r2,c[1],c[2])},
                new Point3d[] {new Point3d(c[0],c[1]-r2, c[2]), new Point3d(c[0]-r2,c[1],c[2])},
                new Point3d[] {new Point3d(c[0],c[1]+r2, c[2]), new Point3d(c[0]-r2,c[1],c[2])},
            };
            return lines;
        }

        private Point3d[][] GetSunLines(Point3d c)
        {
            Point3d[][] lines = new Point3d[20][];
            double r1 = 0.06; double r2 = 0.08;
            double a = 0;
            for (int i = 0; i < 20; i++)
            {
                a += Math.PI / 10;
                lines[i] = new Point3d[]
                {
                    new Point3d(c[0]+r1*Math.Cos(a),c[1]+r1*Math.Sin(a), c[2]), //Start point
                    new Point3d(c[0]+r2*Math.Cos(a),c[1]+r2*Math.Sin(a),c[2]) //End point
                };
            }
            return lines;
        }

        private bool IsBedekPoint(BlockReference br, int detail)
        {
            //בדיקה אם מדובר בנקודה
            if (br.Name != DrawBL.AcadTools.StrengthrningPointBlockName) return false;
            int ptDet;

            //בדיקת הפרט
            using (Transaction ta = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.TransactionManager.StartTransaction())
            {
                AttributeReference ar = ta.GetObject(br.AttributeCollection[2], OpenMode.ForRead) as AttributeReference;
                if (!int.TryParse(ar.TextString, out ptDet))
                    return false;
                return (ptDet == detail);
            }
        }

    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.Windows;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace NewPalkalDrawer
{
    class PointsMenu
    {
        static ContextMenuExtension cme;

        public static void Attach(DrawBL.BL bl)
        {
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;

            cme = new ContextMenuExtension();

            MenuItem mi_4 = new MenuItem("◉ עדכון נקודות פלקל");
            MenuItem mi_4_p = new MenuItem("שלב בתהליך");
            MenuItem mi_4_d = new MenuItem("פרט מבוצע");
            MenuItem mi_4_l = new MenuItem("מיקום הנקודה");
            mi_4.MenuItems.Add(mi_4_p);
            mi_4.MenuItems.Add(mi_4_d);
            mi_4.MenuItems.Add(mi_4_l);
            mi_4_l.Click += new EventHandler(updatePosition_Click);

            MenuItem mi_4_1 = new MenuItem("[0] New: אושר אך טרם סומן");
            MenuItem mi_4_2 = new MenuItem("[1] Waiting: סומן וממתין לביצוע");
            MenuItem mi_4_3 = new MenuItem("[2] Working: העבודה מתבצעת");
            MenuItem mi_4_4 = new MenuItem("[3] Problem: קיימת בעיה");
            MenuItem mi_4_5 = new MenuItem("[4] Done: הביצוע הושלם");
            MenuItem mi_4_6 = new MenuItem("[5] Reported: דווח לחשבון");
            MenuItem mi_4_7 = new MenuItem("[6] Canceled: בוטל על ידי המהנדס");
            mi_4_p.MenuItems.Add(mi_4_1);
            mi_4_p.MenuItems.Add(mi_4_2);
            mi_4_p.MenuItems.Add(mi_4_3);
            mi_4_p.MenuItems.Add(mi_4_4);
            mi_4_p.MenuItems.Add(mi_4_5);
            mi_4_p.MenuItems.Add(mi_4_6);
            mi_4_p.MenuItems.Add(mi_4_7);

            cme.MenuItems.Add(mi_4);

            mi_4_1.Click += (updateStatus_Click);
            mi_4_2.Click += (updateStatus_Click);
            mi_4_3.Click += (updateStatus_Click);
            mi_4_4.Click += (updateStatus_Click);
            mi_4_5.Click += (updateStatus_Click);
            mi_4_6.Click += (updateStatus_Click);
            mi_4_7.Click += (updateStatus_Click);

            AttachDetailsItems(mi_4_d, bl);

            MenuItem mi_2 = new MenuItem("📆 צפה בדוח נקודה");
            //cme.MenuItems.Add(mi_2);
            //mi_2.Click += (displayReport_Click);



            Autodesk.AutoCAD.Runtime.RXClass rxc = Entity.GetClass(typeof(Entity));
            Application.AddObjectContextMenuExtension(rxc, cme);
        }

        private static void AttachDetailsItems(MenuItem parent_mi, DrawBL.BL bl)
        {
            object[][] details = bl.GetEngDetails();
            if (details.Length == 0)
                return;

            MenuItem mi_plata = new MenuItem("[" + DrawBL.AcadTools.PlataDetailID + "] פלטה");
            mi_plata.Click += new EventHandler(updateDetail_Click);
            parent_mi.MenuItems.Add(mi_plata);

            foreach (object[] detail in details)
            {
                MenuItem mi = new MenuItem("[" + detail[0] + "] " + detail[1].ToString().Trim());
                mi.Click += new EventHandler(updateDetail_Click);
                parent_mi.MenuItems.Add(mi);
            }
        }

        private static void log(string p)
        {
            Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n" + p);
        }

        static void updateDetail_Click(object sender, EventArgs e)
        {
            string detailId;
            if (!GetMenuitemNumber(sender, out detailId)) return;
            Document doc = Application.DocumentManager.MdiActiveDocument;
            doc.SendStringToExecute("_." + CommandsText.UpdatePointDetail + " " + detailId + " ", true, false, false);
        }

        private static void updateStatus_Click(object sender, EventArgs e)
        {
            string statusId;
            if (!GetMenuitemNumber(sender, out statusId)) return;
            Document doc = Application.DocumentManager.MdiActiveDocument;
            doc.SendStringToExecute("_." + CommandsText.UpdatePointStatus + " " + statusId + " ", true, false, false);
        }

        private static void updatePosition_Click(object sender, EventArgs e)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            doc.SendStringToExecute("_." + CommandsText.MovePoints + " ", true, false, false);
        }

        public static void displayReport_Click(object sender, EventArgs e)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            doc.SendStringToExecute("_." + CommandsText.DisplayReport, true, false, false);
        }

        public static void Detach()
        {
            Autodesk.AutoCAD.Runtime.RXClass rxc = Entity.GetClass(typeof(Entity));
            Application.RemoveObjectContextMenuExtension(rxc, cme);
        }

        private static bool GetMenuitemNumber(object sender, out string number)
        {
            try
            {
                MenuItem itm = (MenuItem)sender;
                number = itm.Text.Substring(1);
                number = number.Substring(0, number.IndexOf("]"));
                Convert.ToInt32(number);
                return true;
            }
            catch
            {
                number = "-1";
                return false;
            }
        }
    }
}

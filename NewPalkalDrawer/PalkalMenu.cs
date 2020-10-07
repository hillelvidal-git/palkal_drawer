using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NewPalkalDrawer
{
    class PalkalMenu
    {
        static ContextMenuExtension cme;

        public static void Attach(DrawBL.BL bl)
        {
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;

            cme = new ContextMenuExtension();
            cme.Title = "✈ תפריט פלקל";

            MenuItem mi_1 = new MenuItem("⤵ עדכון השרטוט");

            MenuItem mi_3 = new MenuItem("תצוגת נקודות");
            MenuItem mi_3_1 = new MenuItem("בחירת נקודות פלקל");
            MenuItem mi_3_2 = new MenuItem("הדגשת פרטים");
            mi_3.MenuItems.Add(mi_3_1);
            mi_3.MenuItems.Add(mi_3_2);

            AttachDetailsItems(mi_3_2, bl);


            MenuItem mi_2 = new MenuItem("תכנון נקודות");
            MenuItem mi_2_1 = new MenuItem("◉ שתילת נקודות חדשות");
            MenuItem mi_2_2 = new MenuItem("הפעלת נקודות תכנון");
            MenuItem mi_2_3 = new MenuItem("הוצאת נקודת לסימון");
            mi_2.MenuItems.Add(mi_2_1);
            mi_2.MenuItems.Add(mi_2_2);
            mi_2.MenuItems.Add(mi_2_3);

            MenuItem mi_4 = new MenuItem("תיבות");
            MenuItem mi_4_1 = new MenuItem("שמור תיבות שהשתנו");
            mi_4.MenuItems.Add(mi_4_1);

            MenuItem mi_5= new MenuItem("שונות");
            MenuItem mi_5_1 = new MenuItem("סובב את מערכת הצירים של המפלס");
            mi_5.MenuItems.Add(mi_5_1);

            cme.MenuItems.Add(mi_1);
            cme.MenuItems.Add(mi_2);
            cme.MenuItems.Add(mi_3);
            cme.MenuItems.Add(mi_4);

            mi_1.Click += (OnCommand1);
            mi_2_1.Click += (OnCommand21);
            mi_2_2.Click += (OnCommand22);
            mi_3_1.Click += (OnCommand31);
            mi_2_3.Click += (OnCommand5);
            mi_4_1.Click += (OnCommand4);
            mi_5_1.Click += (OnCommand51);

            Application.AddDefaultContextMenuExtension(cme);
        }

        private static void AttachDetailsItems(MenuItem parent_mi, DrawBL.BL bl)
        {
            object[][] details = bl.GetEngDetails();
            if (details.Length == 0)
                return;
            //else...

            MenuItem mi_plata = new MenuItem("[" + DrawBL.AcadTools.PlataDetailID + "] פלטה");
            mi_plata.Click += new EventHandler(markDetail_Click);
            parent_mi.MenuItems.Add(mi_plata);

            foreach (object[] detail in details)
            {
                MenuItem mi = new MenuItem("[" + detail[0] + "] " + detail[1].ToString().Trim());
                mi.Click += new EventHandler(markDetail_Click);
                parent_mi.MenuItems.Add(mi);
            }
        }

        static void markDetail_Click(object sender, EventArgs e)
        {
            string detailId;
            if (!GetMenuitemNumber(sender, out detailId)) return;
            Document doc = Application.DocumentManager.MdiActiveDocument;
            doc.SendStringToExecute("_." + CommandsText.EmphasisDetail + " " + detailId + " ", true, false, false);
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

        public static void Detach()
        {
            Application.RemoveDefaultContextMenuExtension(cme);
        }

        private static void OnCommand1(Object o, EventArgs e)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            doc.SendStringToExecute("_." + CommandsText.UpdateFromServer + " ", true, false, false);
        }

        private static void OnCommand21(Object o, EventArgs e)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            doc.SendStringToExecute("_." + CommandsText.InsertPoints + " ", true, false, false);
        }

        private static void OnCommand22(Object o, EventArgs e)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            doc.SendStringToExecute("_." + CommandsText.AddToDatabase + " ", true, false, false);
        }

        private static void OnCommand31(Object o, EventArgs e)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            doc.SendStringToExecute("_." + CommandsText.SelectPoints + " ", true, false, false);
        }

        private static void OnCommand32(Object o, EventArgs e)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            doc.SendStringToExecute("_." + CommandsText.SelectPoints + " ", true, false, false);
        }

        private static void OnCommand5(Object o, EventArgs e)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            doc.SendStringToExecute("_." + CommandsText.SendToMark + " ", true, false, false);
        }

        private static void OnCommand4(Object o, EventArgs e)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            doc.SendStringToExecute("_." + CommandsText.UpdateBox + " ", true, false, false);
        }

        private static void OnCommand51(Object o, EventArgs e)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            doc.SendStringToExecute("_." + CommandsText.RotateLevel + " ", true, false, false);
        }

    }
}

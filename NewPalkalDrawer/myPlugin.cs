// (C) Copyright 2015 by  
//
using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;

// This line is not mandatory, but improves loading performances
[assembly: ExtensionApplication(typeof(NewPalkalDrawer.MyPlugin))]

namespace NewPalkalDrawer
{

    // This class is instantiated by AutoCAD once and kept alive for the 
    // duration of the session. If you don't do any one time initialization 
    // then you should remove this class.
    public class MyPlugin : IExtensionApplication
    {

        void IExtensionApplication.Initialize()
        {
            try
            {
                string msg;
                bool ready = bl().IsDwgReady(out msg);
                ed(msg);
                if (!ready)
                {
                    ed("⛔ השרטוט איננו מכיל בלוקים ושכבות נצרכים, ולכן תוכנת הפלקל לא נטענה");
                    return;
                }
            }
            catch (System.Exception ec)
            {
                ed("⛔ השרטוט איננו מכיל בלוקים ושכבות נצרכים, ולכן תוכנת הפלקל לא נטענה" +
                    "\n" + ec.Message);
                return;
            }

            DrawBL.BL myBL = bl();
            PalkalMenu.Attach(myBL);
            PointsMenu.Attach(myBL);
            ed("\n✈ ✈ ✈ תכנת הפלקל נטענה בהצלחה");

            //כאן ניתן להכניס פעולות שיבוצעו בכל פעם שפותחים שרטוט
        }

        void IExtensionApplication.Terminate()
        {
            PalkalMenu.Detach();
            PointsMenu.Detach();
        }

        private void ed(string p)
        {
            Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(p);
        }

        private static DrawBL.BL bl()
        {
            return new DrawBL.BL();
        }


    }

}

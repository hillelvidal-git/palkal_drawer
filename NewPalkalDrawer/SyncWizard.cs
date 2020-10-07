using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NewPalkalDrawer
{
    public partial class SyncWizard : Form
    {
        PictureBox[] pbs;
        Label[] lbls;
        List<string> _log;

        DrawBL.BL myBL;
        bool allOk;
        int logLevel;

        public SyncWizard(DrawBL.BL bl)
        {
            InitializeComponent();
            this.myBL = bl;

            logLevel = 0;
            pbs = new PictureBox[] { pb1, pb3, pb4, pb5, pb4, pb5 };
            lbls = new Label[] { lbl1, lbl3, lbl4, lbl5, lbl4, lbl5 };
            _log = new List<string>();
        }

        public void DoSync()
        {
            object[][] newPoints, newMeasurements;
            int[] newBoxes;
            DateTime serverTime;
            this.allOk = true;
            int levelId, blocId;
            DateTime lastDwgUpdate;
            string msg;

            log(DateTime.Now + "  Starting Synchronization...");
            log("Dwg Name: " + myBL.GetDwgName());
            SetLogLevel(1);

            //נסה לתקשר עם השרת
            if (!ConnectServer()) goto End;

            //קרא את אישור העדכניות מהשרטוט
            if (!GetDrawingCertificate(out levelId, out blocId, out lastDwgUpdate)) goto End;

            //הורד את עץ הפרויקטים העדכני
            DownloadTree();

            //קבל את זמן השרת, שישמש בסוף לאישור עדכניות חדש 
            if (!GetServerTime(out serverTime, out msg))
                this.allOk = false;

            //קבל מהשרת את הנקודות החדשות וצייר בשרטוט
            if (GetServerNewPoints(levelId, blocId, lastDwgUpdate, out newPoints))
                DrawPoints(newPoints, lastDwgUpdate);

            //קבל מהשרת את המדידות החדשות וצייר בשרטוט
            if (GetServerNewMeasurements(levelId, blocId, lastDwgUpdate, out newMeasurements))
                DrawMeasuremnets(newMeasurements);

            //קבל מהשרת את התיבות החדשות וצייר בשרטוט
            if (GetServerNewBoxes(levelId, blocId, lastDwgUpdate, out newBoxes))
                DrawBoxes(newBoxes, levelId);

            //אם הכל תקין, שמור אישור עדכניות חדש בשרטוט
            if (this.allOk) SaveCertificate(serverTime);

        End:
            log("");
            SetLogLevel(0);
            log("--- Ended at: " + DateTime.Now.ToString() + " ---");
        }

        private void DrawBoxes(int[] newBoxes, int levelId)
        {
            object[] attributes;
            double[][] samples;
            int ok = 0; int err = 0; int info = 0;
            string msg;
            int boxesInDwg;

            wiz(9, "started", "Downloading and drawing boxes");
            if (!myBL.ReadDwgSurveys(out boxesInDwg, out msg))
            {
                wiz(9, "failed", "Can't read Dwg surveys: " + msg);
                return;
            }
            log("Boxes in drawing: " + boxesInDwg);
            SetLogLevel(2);

            foreach (int boxId in newBoxes)
            {
                log("Box ID: " + boxId);
                try
                {
                    SetLogLevel(3);
                    log("Downloading...");
                    if (GetBox(boxId, out attributes, out samples, false, out msg))
                    {
                        if (msg != "OK")
                        {
                            log("[Info] " + msg);
                            ok++;
                            continue;
                        }

                        log("[OK]");
                        log("Drawing...");
                        if (DrawBox(levelId, attributes, samples, out msg))
                        {
                            if (msg == "OK")
                            {
                                log("[OK]");
                                ok++;
                            }
                            else
                            {
                                log("[Info] " + msg);
                                info++;
                            }
                        }
                        else
                        {
                            log("[Error Drawing, box skipped] " + msg);
                            err++;
                        }
                    }
                    else
                    {
                        log("[Error Downloading, box skipped] " + msg);
                        err++;
                    }
                }
                catch (Exception e)
                {
                    err++;
                    log("[Error: " + e.Message + "]");
                    log(e.StackTrace);
                }
                finally
                {
                    SetLogLevel(2);
                }
            }
            SetLogLevel(1);

            if (err == 0)
            {
                if (info == 0)
                {
                    wiz(9, "done", "All " + ok + " OK", ok);
                    return;
                }
                else
                {
                    wiz(9, "errors", ok + " OK, " + info + " Info, " + err + " Errors", ok);
                    return;
                }
            }

            this.allOk = false;
            if (ok > 0)
                wiz(9, "errors", ok + " OK, " + err + " Errors", ok);
            else
                wiz(9, "failed", "All " + err + " had Errors");
        }

        private void DrawMeasuremnets(object[][] newMeasurements)
        {
            if (newMeasurements.Length == 0) return;
            int ptsInDwg1, ptsInDwg2;
            string msg, msg2;
            if (!myBL.ReadMeasurements(out ptsInDwg1, out msg) | !myBL.ReadDwgSrvs(out ptsInDwg2, out msg2))
            {
                wiz(7, "failed", "Can't read Dwg measurements: ");
                log(msg);
                log(msg2);
                return;
            }

            log("Srv in Dwg: " + ptsInDwg2 + "  Other: " + ptsInDwg1);
            SetLogLevel(2);

            int ok = 0; int err = 0;
            int ptClass;

            wiz(7, "started", " Drawing measurements");
            foreach (object[] one in newMeasurements)
            {
                log("Measurment ID: " + one[0].ToString());
                try
                {
                    SetLogLevel(3);
                    log("Drawing...");
                    ptClass = (int)one[3];

                    if (ptClass == 0)
                    {
                        if (myBL.DrawSrv(one, out msg))
                        {
                            log("[OK]");
                            ok++;
                        }
                        else
                        {
                            log("[Error, measurement skipped] " + msg);
                            err++;
                        }
                    }
                    else
                    {
                        if (myBL.DrawMeasurement(one, out msg))
                        {
                            log("[OK]");
                            ok++;
                        }
                        else
                        {
                            log("[Error, measurement skipped] " + msg);
                            err++;
                        }
                    }
                }
                catch (Exception e)
                {
                    err++;
                    log("[Error: " + e.Message + "]");
                }
                finally
                {
                    SetLogLevel(2);
                }
            }

            SetLogLevel(1);

            if (err == 0)
            {
                wiz(7, "done", "All " + ok + " OK", ok);
                return;
            }

            this.allOk = false;
            if (ok > 0)
                wiz(7, "errors", ok + " OK, " + err + " Errors", ok);
            else
                wiz(7, "failed", "All " + err + " had Errors");
        }

        private void DrawPoints(object[][] newPoints, DateTime DwgUpdated)
        {
            if (newPoints.Length == 0) return;
            string msg;
            int ok = 0; int err = 0;
            int ptInDwg;

            wiz(5, "started", "Drawing new points");
            if (!myBL.ReadDwgPoints(out ptInDwg, out msg))
            {
                wiz(5, "failed", "Can't read Dwg points: " + msg);
                return;
            }
            log("Points in drawing: " + ptInDwg);
            SetLogLevel(2);

            foreach (object[] one in newPoints)
            {
                log("Point ID: " + one[0]);
                try
                {
                    SetLogLevel(3);
                    log("Drawing...");
                    if (myBL.DrawPoint(one, out msg))
                    {
                        log("[OK]");
                        ok++;
                    }
                    else
                    {
                        log("[Error, point skipped] " + msg);
                        err++;
                    }
                }
                catch (Exception e)
                {
                    err++;
                    log("[Error: " + e.Message + "]");
                    log(e.StackTrace);
                }
                finally
                {
                    SetLogLevel(2);
                }
            }
            SetLogLevel(1);

            if (err == 0)
            {
                wiz(5, "done", "All " + ok + " OK", ok);
                return;
            }

            this.allOk = false;
            if (ok > 0)
                wiz(5, "errors", ok + " OK, " + err + " Errors", ok);
            else
                wiz(5, "failed", "All " + err + " had Errors");
        }

        private bool GetDrawingCertificate(out int levelId, out int blocId, out DateTime lastDwgUpdate)
        {
            string msg;
            wiz(2, "started", "Reading drawing certificate");

            try
            {
                if (myBL.GetDrawingCertificate(out levelId, out blocId, out lastDwgUpdate, out msg))
                {
                    wiz(2, "done", "LevelID: " + levelId + ",  BlocID: " + blocId + ",  Time: " + lastDwgUpdate);
                }
                else
                {
                    wiz(2, "errors", "LevelID: " + levelId + ",  BlocID: " + blocId + ",  Time: " + lastDwgUpdate);
                }
                res2.Text = lastDwgUpdate.ToString();
                return true;
            }
            catch (Exception e)
            {
                wiz(2, "failed", e.Message);
                levelId = -1; blocId = -1;
                lastDwgUpdate = new DateTime();
                return false;
            }
        }

        private bool GetServerNewMeasurements(int levelId, int blocId, DateTime lastDwgUpdate, out object[][] news)
        {
            string msg;
            wiz(6, "started", "Retrieving new measurements list from server");

            try
            {
                if (myBL.GetServerNewMeasurements(levelId, blocId, lastDwgUpdate, out news, out msg))
                {
                    wiz(6, "done", news.Length + "  new measurements", news.Length);
                    return true;
                }
                else
                {
                    this.allOk = false;
                    wiz(6, "errors", msg, news.Length);
                    return false;
                }
            }
            catch (Exception e)
            {
                this.allOk = false;
                wiz(6, "failed", e.Message);
                news = new object[0][];
                return false;
            }
        }

        private bool GetServerNewBoxes(int levelId, int blocId, DateTime lastDwgUpdate, out int[] news)
        {
            string msg;
            wiz(8, "started", "Retrieving new boxes list from server");

            try
            {
                if (myBL.GetServerNewBoxes(levelId, blocId, lastDwgUpdate, out news, out msg))
                {
                    wiz(8, "done", news.Length + "  new boxes", news.Length);
                    return true;
                }
                else
                {
                    this.allOk = false;
                    wiz(8, "errors", msg, news.Length);
                    return false;
                }
            }
            catch (Exception e)
            {
                this.allOk = false;
                wiz(4, "failed", e.Message);
                news = new int[0];
                return false;
            }
        }

        private bool GetServerNewPoints(int levelId, int blocId, DateTime lastDwgUpdate, out object[][] news)
        {
            string msg;
            wiz(4, "started", "Retrieving new points list from server");

            try
            {
                if (myBL.GetServerNewPoints(levelId, blocId, lastDwgUpdate, out news, out msg))
                {
                    wiz(4, "done", news.Length + "  new points", news.Length);
                    return true;
                }
                else
                {
                    this.allOk = false;
                    wiz(4, "errors", msg, news.Length);
                    return false;
                }
            }
            catch (Exception e)
            {
                this.allOk = false;
                wiz(4, "failed", e.Message);
                news = new object[0][];
                return false;
            }
        }

        private bool GetBox(int id, out object[] attributes, out double[][] samples, bool downloadExisting, out string msg)
        {
            return myBL.DownloadBox(id, out attributes, out samples, downloadExisting, out msg);
        }

        private bool DrawBox(int levelId, object[] values, double[][] samples_Rad_Met, out string msg)
        {
            return myBL.DrawBox(levelId, values, samples_Rad_Met, out msg);
        }

        private bool GetServerTime(out DateTime serverTime, out string msg)
        {
            try
            {
                if (myBL.GetServerTime(out serverTime, out msg))
                    return true;
                else return false;
            }
            catch (Exception e)
            {
                serverTime = new DateTime(2000, 1, 1);
                msg = e.Message;
                return false;
            }
        }

        private bool ConnectServer()
        {
            string msg;
            wiz(1, "connect", "Connecting server");

            if (myBL.CheckServerConnection(out msg))
            {
                wiz(1, "done", "");
                return true;
            }
            else
            {
                wiz(1, "failed", msg);
                return false;
            }
        }

        private void SaveCertificate(DateTime serverTime)
        {
            string msg;
            wiz(10, "started", "Saving a new certificate to drawing");

            try
            {
                if (myBL.WriteCertificateToDwg(serverTime, out msg))
                {
                    wiz(10, "done", "New Time:" + serverTime);
                    res10.Text = serverTime.ToString();
                    if (this.myBL.AutoSave)
                    {
                        //שמור את השרטוט
                        this.myBL.SaveDrawing();
                        log("Drawing saved");
                    }
                }
                else throw new Exception(msg);
            }
            catch (Exception e)
            {
                wiz(10, "failed", e.Message);
            }
        }

        private void wiz(int wizardStep, string status, string msg)
        {
            wiz(wizardStep, status, msg, -1);
        }

        private void wiz(int wizardStep, string status, string msg, int num)
        {
            try
            {
                //find step controls
                string pb = "pb" + wizardStep.ToString();
                string lbl = "lbl" + wizardStep.ToString();
                PictureBox myPB = (PictureBox)this.Controls[pb];
                Label myLBL = (Label)this.Controls[lbl];

                if (num != -1)
                {
                    string tb = "res" + wizardStep;
                    TextBox myTB = (TextBox)this.Controls[tb];
                    myTB.Text = num.ToString();
                }


                //display step status
                switch (status)
                {
                    case "pre":
                        myPB.Image = null;
                        myPB.Enabled = false;
                        myLBL.Enabled = false;
                        break;
                    case "started":
                        myPB.Image = imageList1.Images[1];
                        myPB.Enabled = true;
                        myLBL.Enabled = true;
                        log("");
                        msg += "...";
                        break;
                    case "errors":
                        myPB.Image = imageList1.Images[2];
                        msg = "[Error]  " + msg;
                        break;
                    case "failed":
                        myPB.Image = imageList1.Images[3];
                        msg = "[Failed]  " + msg;
                        break;
                    case "done":
                        myPB.Image = imageList1.Images[4];
                        msg = "[OK]  " + msg;
                        break;
                    case "connect":
                        myPB.Image = imageList1.Images[0];
                        myPB.Enabled = true;
                        myLBL.Enabled = true;
                        break;
                }

                log(msg);
            }
            catch { }
        }

        private void log(string[] p)
        {
            foreach (string s in p) log(s);
        }

        private void log(string p)
        {
            for (int i = 0; i < this.logLevel; i++)
                p = "     " + p;
            _log.Add(p);
        }

        private void SetLogLevel(int i)
        {
            this.logLevel = i;
        }

        private void SetLogLevel(string p)
        {
            if (p == "+")
                this.logLevel++;
            else if (p == "-")
                if (this.logLevel > 0)
                    this.logLevel--;
        }

        private bool DownloadTree()
        {
            wiz(3, "started", "Downloading projects tree");
            try
            {
                BedekTreeAdapter.TreeAdapter ta = new BedekTreeAdapter.TreeAdapter();
                if (ta.OnlyDownload())
                {
                    wiz(3, "done", "");
                    return true;
                }
                else
                {
                    wiz(3, "failed", "");
                    return false;
                }
            }
            catch (Exception e)
            {
                wiz(3, "failed", e.Message);
                return false;
            }
        }

        private void btnLog_Click(object sender, EventArgs e)
        {
            LogForm.LogForm lf = new LogForm.LogForm(this._log);
            lf.Show();
        }

        private void SyncWizard_Shown(object sender, EventArgs e)
        {
            Application.DoEvents();
            this.DoSync();
        }
    }
}

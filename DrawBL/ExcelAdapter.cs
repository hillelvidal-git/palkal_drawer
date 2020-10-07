using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.Excel;
using Excel = Microsoft.Office.Interop.Excel;


namespace DrawBL
{
    public class ExcelAdapter
    {
        object misVal = System.Reflection.Missing.Value;

        Excel.Application xlApp;
        Excel.Workbook wb;
        Excel.Worksheet ws;
        DrawBL.BL myBl;

        public ExcelAdapter(DrawBL.BL bl)
        {
            this.myBl = bl;
            xlApp = new Excel.Application();
            xlApp.Visible = false;
        }

        public bool OpenDoc(string path)
        {
            try
            {
                this.wb = xlApp.Workbooks.Open(path, misVal, false, misVal, misVal, misVal, misVal, misVal, misVal, misVal, misVal, misVal, misVal, misVal, misVal); ;
                this.ws = wb.Worksheets[1] as Excel.Worksheet;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ReadGroupsData(int levelId, out GroupDrawer.DidiGroup[] groups)
        {
            List<GroupDrawer.DidiGroup> rows = new List<GroupDrawer.DidiGroup>();
            int r = 0;
            int emptyrows = 0;
            string cellContent;

            do
            {
                r++;
                cellContent = GetCellValue(ws, r, 1);
                if (string.IsNullOrEmpty(cellContent))
                {
                    //myBl.logw(" {EMPTY ROW}");
                    emptyrows++;
                    continue;
                }
                else
                {
                    myBl.log("--> Reading Row " + r + ":");
                    rows.Add(ReadGroupFrowRow(r, levelId));
                }
            } while (emptyrows < 9);

            groups = rows.ToArray();
            return true;
        }

        private string GetCellValue(Worksheet ws, int row, int col)
        {
            try
            {
                Range rng = ws.Cells[row, col] as Range;
                return rng.Value.ToString();
            }
            catch
            {
                return null;
            }
        }

        private GroupDrawer.DidiGroup ReadGroupFrowRow(int r, int levelId)
        {
            int c = 0;
            bool stop = false;
            string cell;
            int num;
            List<int> points = new List<int>();
            bool closed = false;
            string tag = "";

            do
            {
                c++;
                //myBl.log("   --> Reading Column: " + c);
                cell = GetCellValue(ws, r, c);
                if (string.IsNullOrEmpty(cell))
                {
                    stop = true;
                    myBl.logw(" [End Of Line]");
                    break;
                }
                else
                {
                    if (int.TryParse(cell, out num))
                    {
                        //מדובר במספר
                        points.Add(num);
                        myBl.logw(" [" + num + "]");

                    }
                    else
                    {
                        //מדובר בטקסט
                        if (cell == "c" || cell == "C")
                        {
                            //זהו סימן שהקבוצה סגורה
                            myBl.log(" [Closed Flag]");
                            closed = true;
                        }
                        else
                        {
                            tag = cell;
                        }
                    }
                }
            } while (!stop);

            GroupDrawer.DidiGroup grp = new GroupDrawer.DidiGroup(levelId, r, closed, tag);
            grp.SetPointList(points);
            myBl.logw(" <Group Saved>");

            return grp;
        }

        public void WriteGroupsColors(GroupDrawer.DidiGroup group)
        {
            throw new NotImplementedException();
        }

        public void Close(bool save)
        {
            wb.Close(save, misVal, misVal);
        }
    }
}

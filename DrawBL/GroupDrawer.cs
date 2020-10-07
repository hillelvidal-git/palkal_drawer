using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DrawBL
{
    public class GroupDrawer
    {
        //==============================

        public class DidiGroup
        {
            public string Status;
            public string Tag;
            public int LevelId;
            public bool IsClosed;
            int ExcelRow;
            internal DidiPoint[] Points;

            public DidiGroup(int levelId, int row, bool isClosed, string tag)
            {
                this.LevelId = levelId;
                this.ExcelRow = row;
                this.IsClosed = isClosed;
                this.Tag = tag;
            }

            internal void SetPointList(List<int> numbers)
            {
                //מקבל את רשימת המספרים של הנקודות ומאחסן אותם ברשימה
                this.Points = new DidiPoint[numbers.Count];
                for (int i = 0; i < numbers.Count; i++)
                {
                    DidiPoint newPoint = new DidiPoint(this, numbers[i]);
                    this.Points[i] = newPoint;
                }
                this.Status = "Read"; //מציין שהמספרים נשמרו

            }

        }

        public class DidiPoint
        {
            DidiGroup Parent;
            public int Number;
            public string Status;
            int DbId;
            public double X;
            public double Y;

            public DidiPoint(DidiGroup parent, int number)
            {
                this.Parent = parent;
                this.Number = number;
                this.Status = "Read";
            }

            public void SetValues(int id, double x, double y)
            {
                this.DbId = id;
                this.X = x;
                this.Y = y;
                this.Status = "Values";
            }

            public void SetStatus(string status)
            {
                this.Status = status;
            }
        }
    }
}

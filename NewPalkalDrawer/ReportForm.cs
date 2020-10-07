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
    public partial class ReportForm : Form
    {
        int statusId;
        public bool inputOK = false;

        public ReportForm(object[] initialValues, int status)
        {
            InitializeComponent();
            this.statusId = status;
            DisplayValues(initialValues);
        }

        private void DisplayValues(object[] v)
        {
            string[] statusList = new string[] { "New", "Waiting", "Working", "Problem", "Done", "Reported", "Cancelled" };
            //בעיה צפויה : null.Trim()
            _ID.Text = ((string)v[0]).Trim();
            _pointID.Text = v[1].ToString();
            _pointAdress.Text = ((string)v[2]).Trim();
            _status.Text = statusList[this.statusId];
            _boxHits.Text = v[3].ToString();
            _problem.Text = ((string)v[4]).Trim();
            _solution.Text = ((string)v[5]).Trim();
            _manager.Text = ((string)v[6]).Trim();
            _completionDate.Value = (DateTime)v[7];
            _cancellationReason.Text = ((string)v[8]).Trim();
            _documentation.Text = ((string)v[9]).Trim();
            _reportDate.Value = (DateTime)v[10];
            _reporter.Text = ((string)v[11]).Trim();
        }

    }
}

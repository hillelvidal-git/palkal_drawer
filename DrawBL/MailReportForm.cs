using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Mail;

namespace DrawBL
{
    public partial class MailReportForm : Form
    {
        private MailMessage myMassage;
        public bool ready;

        public MailReportForm()
        {
            InitializeComponent();
            this.ready = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.myMassage = new MailMessage();
            if (!string.IsNullOrEmpty(tb1.Text) && !string.IsNullOrEmpty(tb2.Text))
                this.myMassage.To.Add(new MailAddress(tb1.Text.Trim() + "@" + tb2.Text.Trim(), tb1.Text));
            if (!string.IsNullOrEmpty(tb3.Text) && !string.IsNullOrEmpty(tb4.Text))
                this.myMassage.To.Add(new MailAddress(tb3.Text.Trim() + "@" + tb4.Text.Trim(), tb3.Text));

            if (this.myMassage.To.Count == 0)
            {
                this.ready = false;
                return;
            }

            this.myMassage.Body = tbBody.Text;
            if (this.myMassage.Body == "") this.myMassage.Body = "מצורף בזאת דוח עדכון שרטוט";

            this.ready = true;
        }

        internal MailMessage GetMessage()
        {
            return this.myMassage;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net.Mail;

namespace LogForm
{
    public partial class LogForm : Form
    {
        public LogForm(List<string> lines)
        {
            InitializeComponent();
            textBox1.Lines = lines.ToArray();
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            textBox1.Font = new System.Drawing.Font(FontFamily.GenericSansSerif, (float)trackBar1.Value);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.saveFileDialog1.FileName = "DwgUpdateLog_" + DateTime.Now.ToString().Replace(":", ".").Replace(" ", ".").Replace("/", ".").Replace("\\", ".");
            if (this.saveFileDialog1.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            using (StreamWriter sw = new StreamWriter(this.saveFileDialog1.FileName))
            {
                foreach (string line in this.textBox1.Lines) sw.WriteLine(line);
            }
            MessageBox.Show("Log Saved to:\n" + this.saveFileDialog1.FileName);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //Firstly, Save the log to a file:
            string localPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Bedek\\Logs\\DwgUpdate";
            if (!Directory.Exists(localPath)) Directory.CreateDirectory(localPath);
            string FileName = "DwgUpdateLog_" + DateTime.Now.ToString().Replace(":", ".").Replace(" ", ".").Replace("/", ".").Replace("\\", ".");
            FileName = localPath + "\\" + FileName + ".txt";

            using (StreamWriter sw = new StreamWriter(FileName))
            {
                foreach (string line in this.textBox1.Lines) sw.WriteLine(line);
            }

            //then, email it as attachment
            DrawBL.MailReportForm mrf = new DrawBL.MailReportForm();
            if (mrf.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (mrf.ready)
                {
                    SendMails(mrf.GetMessage(), FileName);
                }
            }
        }


        internal void SendMails(MailMessage myMassage, string attachment)
        {
            try
            {
                string from = "online.diary.binyan@gmail.com";
                string password = "oldb3121";

                myMassage.Subject = "דוח עדכון שרטוט";
                myMassage.Attachments.Add(new Attachment(attachment));
                myMassage.From = new MailAddress(from, "אתר שליטה - בנין הארץ");

                SmtpClient GmailSmtp = new SmtpClient();
                GmailSmtp.Host = "smtp.gmail.com";
                GmailSmtp.Port = 587;
                GmailSmtp.Credentials = new System.Net.NetworkCredential(from, password);
                GmailSmtp.EnableSsl = true;
                GmailSmtp.Send(myMassage);

                MessageBox.Show("Mail Sent", "OK");
            }
            catch (Exception e)
            {
                MessageBox.Show("Error: " + e.Message, "Error");
            }
        }

    }
}

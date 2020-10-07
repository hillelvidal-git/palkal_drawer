namespace NewPalkalDrawer
{
    partial class AddToDbForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddToDbForm));
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.lblProject = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.cmbBlocs = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btnPickPoints = new System.Windows.Forms.Button();
            this.imgsDB = new System.Windows.Forms.ImageList(this.components);
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.cmbDetails = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.lblLevel = new System.Windows.Forms.Label();
            this.btnAddToDb = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.lblPtNum = new System.Windows.Forms.Label();
            this.imgsDisto = new System.Windows.Forms.ImageList(this.components);
            this.chkPlanningPts = new System.Windows.Forms.RadioButton();
            this.chkActivePts = new System.Windows.Forms.RadioButton();
            this.chkNewsOnly = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeView1
            // 
            this.treeView1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.treeView1.Location = new System.Drawing.Point(12, 171);
            this.treeView1.Name = "treeView1";
            this.treeView1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.treeView1.Size = new System.Drawing.Size(304, 237);
            this.treeView1.TabIndex = 0;
            // 
            // lblProject
            // 
            this.lblProject.AutoSize = true;
            this.lblProject.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.lblProject.ForeColor = System.Drawing.SystemColors.Highlight;
            this.lblProject.Location = new System.Drawing.Point(168, 15);
            this.lblProject.Name = "lblProject";
            this.lblProject.Size = new System.Drawing.Size(58, 20);
            this.lblProject.TabIndex = 2;
            this.lblProject.Text = "Project";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.label4.Location = new System.Drawing.Point(243, 16);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(55, 18);
            this.label4.TabIndex = 3;
            this.label4.Text = "פרויקט:";
            // 
            // cmbBlocs
            // 
            this.cmbBlocs.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.cmbBlocs.FormattingEnabled = true;
            this.cmbBlocs.Location = new System.Drawing.Point(204, 41);
            this.cmbBlocs.Name = "cmbBlocs";
            this.cmbBlocs.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmbBlocs.Size = new System.Drawing.Size(54, 26);
            this.cmbBlocs.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.label3.Location = new System.Drawing.Point(264, 44);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(34, 18);
            this.label3.TabIndex = 5;
            this.label3.Text = "גוש:";
            // 
            // btnPickPoints
            // 
            this.btnPickPoints.ImageIndex = 0;
            this.btnPickPoints.ImageList = this.imgsDB;
            this.btnPickPoints.Location = new System.Drawing.Point(12, 102);
            this.btnPickPoints.Name = "btnPickPoints";
            this.btnPickPoints.Size = new System.Drawing.Size(172, 63);
            this.btnPickPoints.TabIndex = 7;
            this.btnPickPoints.Text = "בחר נקודות";
            this.btnPickPoints.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.btnPickPoints.UseVisualStyleBackColor = true;
            this.btnPickPoints.Click += new System.EventHandler(this.btnPickPoints_Click);
            // 
            // imgsDB
            // 
            this.imgsDB.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgsDB.ImageStream")));
            this.imgsDB.TransparentColor = System.Drawing.Color.Transparent;
            this.imgsDB.Images.SetKeyName(0, "navigate-left32.png");
            this.imgsDB.Images.SetKeyName(1, "navigate-down32.png");
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.cmbDetails);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.lblLevel);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.cmbBlocs);
            this.groupBox1.Controls.Add(this.lblProject);
            this.groupBox1.Location = new System.Drawing.Point(12, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(304, 82);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.label6.Location = new System.Drawing.Point(151, 44);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(37, 18);
            this.label6.TabIndex = 9;
            this.label6.Text = "פרט:";
            // 
            // cmbDetails
            // 
            this.cmbDetails.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.cmbDetails.FormattingEnabled = true;
            this.cmbDetails.Location = new System.Drawing.Point(10, 41);
            this.cmbDetails.Name = "cmbDetails";
            this.cmbDetails.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.cmbDetails.Size = new System.Drawing.Size(135, 26);
            this.cmbDetails.TabIndex = 10;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.label2.Location = new System.Drawing.Point(95, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 18);
            this.label2.TabIndex = 8;
            this.label2.Text = "מפלס:";
            // 
            // lblLevel
            // 
            this.lblLevel.AutoSize = true;
            this.lblLevel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.lblLevel.ForeColor = System.Drawing.SystemColors.Highlight;
            this.lblLevel.Location = new System.Drawing.Point(10, 15);
            this.lblLevel.Name = "lblLevel";
            this.lblLevel.Size = new System.Drawing.Size(46, 20);
            this.lblLevel.TabIndex = 7;
            this.lblLevel.Text = "Level";
            // 
            // btnAddToDb
            // 
            this.btnAddToDb.ImageIndex = 1;
            this.btnAddToDb.ImageList = this.imgsDB;
            this.btnAddToDb.Location = new System.Drawing.Point(154, 423);
            this.btnAddToDb.Name = "btnAddToDb";
            this.btnAddToDb.Size = new System.Drawing.Size(162, 44);
            this.btnAddToDb.TabIndex = 9;
            this.btnAddToDb.Text = "הוסף למסד הנתונים";
            this.btnAddToDb.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.btnAddToDb.UseVisualStyleBackColor = true;
            this.btnAddToDb.Click += new System.EventHandler(this.btnAddToDb_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.label1.Location = new System.Drawing.Point(12, 423);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 18);
            this.label1.TabIndex = 10;
            this.label1.Text = "סה\"כ:";
            // 
            // lblPtNum
            // 
            this.lblPtNum.Font = new System.Drawing.Font("Microsoft Sans Serif", 27.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.lblPtNum.ForeColor = System.Drawing.SystemColors.Highlight;
            this.lblPtNum.Location = new System.Drawing.Point(61, 425);
            this.lblPtNum.Name = "lblPtNum";
            this.lblPtNum.Size = new System.Drawing.Size(87, 41);
            this.lblPtNum.TabIndex = 11;
            this.lblPtNum.Text = "0";
            this.lblPtNum.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // imgsDisto
            // 
            this.imgsDisto.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgsDisto.ImageStream")));
            this.imgsDisto.TransparentColor = System.Drawing.Color.Transparent;
            this.imgsDisto.Images.SetKeyName(0, "Actions-arrow-left-double-icon.png");
            this.imgsDisto.Images.SetKeyName(1, "Actions-arrow-down-icon.png");
            // 
            // chkPlanningPts
            // 
            this.chkPlanningPts.AutoSize = true;
            this.chkPlanningPts.Location = new System.Drawing.Point(207, 102);
            this.chkPlanningPts.Name = "chkPlanningPts";
            this.chkPlanningPts.Size = new System.Drawing.Size(93, 17);
            this.chkPlanningPts.TabIndex = 12;
            this.chkPlanningPts.Text = "נקודות תכנון";
            this.chkPlanningPts.UseVisualStyleBackColor = true;
            this.chkPlanningPts.CheckedChanged += new System.EventHandler(this.chkPlanningPts_CheckedChanged);
            // 
            // chkActivePts
            // 
            this.chkActivePts.AutoSize = true;
            this.chkActivePts.Checked = true;
            this.chkActivePts.Location = new System.Drawing.Point(207, 125);
            this.chkActivePts.Name = "chkActivePts";
            this.chkActivePts.Size = new System.Drawing.Size(102, 17);
            this.chkActivePts.TabIndex = 13;
            this.chkActivePts.TabStop = true;
            this.chkActivePts.Text = "נקודות פעילות";
            this.chkActivePts.UseVisualStyleBackColor = true;
            // 
            // chkNewsOnly
            // 
            this.chkNewsOnly.AutoSize = true;
            this.chkNewsOnly.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.chkNewsOnly.Checked = true;
            this.chkNewsOnly.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkNewsOnly.Location = new System.Drawing.Point(232, 148);
            this.chkNewsOnly.Name = "chkNewsOnly";
            this.chkNewsOnly.Size = new System.Drawing.Size(77, 17);
            this.chkNewsOnly.TabIndex = 14;
            this.chkNewsOnly.Text = "רק חדשות";
            this.chkNewsOnly.UseVisualStyleBackColor = false;
            // 
            // AddToDbForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(328, 479);
            this.Controls.Add(this.chkNewsOnly);
            this.Controls.Add(this.chkActivePts);
            this.Controls.Add(this.chkPlanningPts);
            this.Controls.Add(this.lblPtNum);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnAddToDb);
            this.Controls.Add(this.btnPickPoints);
            this.Controls.Add(this.treeView1);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AddToDbForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "הוספת נקודות למסד הנתונים";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblProject;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cmbBlocs;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnPickPoints;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnAddToDb;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblPtNum;
        public System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.ImageList imgsDB;
        private System.Windows.Forms.ImageList imgsDisto;
        private System.Windows.Forms.RadioButton chkPlanningPts;
        private System.Windows.Forms.RadioButton chkActivePts;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cmbDetails;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblLevel;
        private System.Windows.Forms.CheckBox chkNewsOnly;
    }
}
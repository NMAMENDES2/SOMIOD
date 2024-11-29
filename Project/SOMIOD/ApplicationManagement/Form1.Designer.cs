namespace ApplicationManagement
{
    partial class Form1
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
            this.getApplications = new System.Windows.Forms.Button();
            this.richTextBoxListApplications = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // getApplications
            // 
            this.getApplications.BackColor = System.Drawing.SystemColors.ControlDark;
            this.getApplications.Location = new System.Drawing.Point(29, 38);
            this.getApplications.Name = "getApplications";
            this.getApplications.Size = new System.Drawing.Size(123, 23);
            this.getApplications.TabIndex = 0;
            this.getApplications.Text = "List applications";
            this.getApplications.UseVisualStyleBackColor = false;
            this.getApplications.Click += new System.EventHandler(this.getApplications_Click);
            // 
            // richTextBoxListApplications
            // 
            this.richTextBoxListApplications.Location = new System.Drawing.Point(29, 67);
            this.richTextBoxListApplications.Name = "richTextBoxListApplications";
            this.richTextBoxListApplications.Size = new System.Drawing.Size(199, 195);
            this.richTextBoxListApplications.TabIndex = 3;
            this.richTextBoxListApplications.Text = "";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.richTextBoxListApplications);
            this.Controls.Add(this.getApplications);
            this.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button getApplications;
        private System.Windows.Forms.RichTextBox richTextBoxListApplications;
    }
}


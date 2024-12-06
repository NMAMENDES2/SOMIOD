namespace App_B
{
    partial class App_B
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
            this.buttonON = new System.Windows.Forms.Button();
            this.buttonOff = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonON
            // 
            this.buttonON.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonON.Location = new System.Drawing.Point(37, 49);
            this.buttonON.Name = "buttonON";
            this.buttonON.Size = new System.Drawing.Size(241, 109);
            this.buttonON.TabIndex = 0;
            this.buttonON.Text = "Light ON";
            this.buttonON.UseVisualStyleBackColor = true;
            // 
            // buttonOff
            // 
            this.buttonOff.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOff.Location = new System.Drawing.Point(37, 184);
            this.buttonOff.Name = "buttonOff";
            this.buttonOff.Size = new System.Drawing.Size(241, 109);
            this.buttonOff.TabIndex = 1;
            this.buttonOff.Text = "Light OFF";
            this.buttonOff.UseVisualStyleBackColor = true;
            // 
            // App_B
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(313, 354);
            this.Controls.Add(this.buttonOff);
            this.Controls.Add(this.buttonON);
            this.Name = "App_B";
            this.Text = "App_B";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonON;
        private System.Windows.Forms.Button buttonOff;
    }
}


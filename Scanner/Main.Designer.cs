namespace MediaPanther.Aggregator.ReaperHarness
{
    partial class Main
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
                components.Dispose();
            
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.StopProcessingBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // StopProcessingBtn
            // 
            this.StopProcessingBtn.Location = new System.Drawing.Point(13, 13);
            this.StopProcessingBtn.Name = "StopProcessingBtn";
            this.StopProcessingBtn.Size = new System.Drawing.Size(101, 23);
            this.StopProcessingBtn.TabIndex = 0;
            this.StopProcessingBtn.Text = "Stop Processing";
            this.StopProcessingBtn.UseVisualStyleBackColor = true;
            this.StopProcessingBtn.Click += new System.EventHandler(this.StopProcessingBtn_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(538, 212);
            this.Controls.Add(this.StopProcessingBtn);
            this.Name = "Main";
            this.Text = "Reaper";
            this.Load += new System.EventHandler(this.Main_Load);
            this.ResumeLayout(false);

        }
        #endregion

        private System.Windows.Forms.Button StopProcessingBtn;
    }
}


namespace RedditWFA
{
    partial class QuickRate
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
            this.commentBox = new System.Windows.Forms.TextBox();
            this.zeroScore = new System.Windows.Forms.Button();
            this.eightScore = new System.Windows.Forms.Button();
            this.sixScore = new System.Windows.Forms.Button();
            this.twoScore = new System.Windows.Forms.Button();
            this.fourScore = new System.Windows.Forms.Button();
            this.tenScore = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // commentBox
            // 
            this.commentBox.AcceptsReturn = true;
            this.commentBox.Location = new System.Drawing.Point(22, 25);
            this.commentBox.Margin = new System.Windows.Forms.Padding(4);
            this.commentBox.Multiline = true;
            this.commentBox.Name = "commentBox";
            this.commentBox.ReadOnly = true;
            this.commentBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.commentBox.Size = new System.Drawing.Size(480, 273);
            this.commentBox.TabIndex = 8;
            // 
            // zeroScore
            // 
            this.zeroScore.Location = new System.Drawing.Point(22, 306);
            this.zeroScore.Name = "zeroScore";
            this.zeroScore.Size = new System.Drawing.Size(75, 23);
            this.zeroScore.TabIndex = 9;
            this.zeroScore.Text = "0";
            this.zeroScore.UseVisualStyleBackColor = true;
            this.zeroScore.Click += new System.EventHandler(this.ZeroScore_Click);
            // 
            // eightScore
            // 
            this.eightScore.Location = new System.Drawing.Point(346, 306);
            this.eightScore.Name = "eightScore";
            this.eightScore.Size = new System.Drawing.Size(75, 23);
            this.eightScore.TabIndex = 10;
            this.eightScore.Text = "8";
            this.eightScore.UseVisualStyleBackColor = true;
            this.eightScore.Click += new System.EventHandler(this.EightScore_Click);
            // 
            // sixScore
            // 
            this.sixScore.Location = new System.Drawing.Point(265, 305);
            this.sixScore.Name = "sixScore";
            this.sixScore.Size = new System.Drawing.Size(75, 23);
            this.sixScore.TabIndex = 11;
            this.sixScore.Text = "6";
            this.sixScore.UseVisualStyleBackColor = true;
            this.sixScore.Click += new System.EventHandler(this.SixScore_Click);
            // 
            // twoScore
            // 
            this.twoScore.Location = new System.Drawing.Point(103, 305);
            this.twoScore.Name = "twoScore";
            this.twoScore.Size = new System.Drawing.Size(75, 23);
            this.twoScore.TabIndex = 12;
            this.twoScore.Text = "2";
            this.twoScore.UseVisualStyleBackColor = true;
            this.twoScore.Click += new System.EventHandler(this.TwoScore_Click);
            // 
            // fourScore
            // 
            this.fourScore.Location = new System.Drawing.Point(184, 305);
            this.fourScore.Name = "fourScore";
            this.fourScore.Size = new System.Drawing.Size(75, 23);
            this.fourScore.TabIndex = 13;
            this.fourScore.Text = "4";
            this.fourScore.UseVisualStyleBackColor = true;
            this.fourScore.Click += new System.EventHandler(this.FourScore_Click);
            // 
            // tenScore
            // 
            this.tenScore.Location = new System.Drawing.Point(427, 306);
            this.tenScore.Name = "tenScore";
            this.tenScore.Size = new System.Drawing.Size(75, 23);
            this.tenScore.TabIndex = 14;
            this.tenScore.Text = "10";
            this.tenScore.UseVisualStyleBackColor = true;
            this.tenScore.Click += new System.EventHandler(this.TenScore_Click);
            // 
            // QuickRate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(521, 344);
            this.Controls.Add(this.tenScore);
            this.Controls.Add(this.fourScore);
            this.Controls.Add(this.twoScore);
            this.Controls.Add(this.sixScore);
            this.Controls.Add(this.eightScore);
            this.Controls.Add(this.zeroScore);
            this.Controls.Add(this.commentBox);
            this.Name = "QuickRate";
            this.Text = "QuickRate";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox commentBox;
        private System.Windows.Forms.Button zeroScore;
        private System.Windows.Forms.Button eightScore;
        private System.Windows.Forms.Button sixScore;
        private System.Windows.Forms.Button twoScore;
        private System.Windows.Forms.Button fourScore;
        private System.Windows.Forms.Button tenScore;
    }
}
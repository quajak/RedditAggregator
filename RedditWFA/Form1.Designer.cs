namespace RedditWFA
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
            this.subRedditList = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.postList = new System.Windows.Forms.ListBox();
            this.label3 = new System.Windows.Forms.Label();
            this.commentList = new System.Windows.Forms.ListBox();
            this.commentBox = new System.Windows.Forms.TextBox();
            this.postText = new System.Windows.Forms.TextBox();
            this.commentTree = new System.Windows.Forms.TreeView();
            this.userRating = new System.Windows.Forms.TrackBar();
            ((System.ComponentModel.ISupportInitialize)(this.userRating)).BeginInit();
            this.SuspendLayout();
            // 
            // subRedditList
            // 
            this.subRedditList.FormattingEnabled = true;
            this.subRedditList.Location = new System.Drawing.Point(12, 73);
            this.subRedditList.Name = "subRedditList";
            this.subRedditList.Size = new System.Drawing.Size(120, 290);
            this.subRedditList.TabIndex = 0;
            this.subRedditList.SelectedIndexChanged += new System.EventHandler(this.SubRedditList_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 54);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Sub Reddits";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(139, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(33, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Posts";
            // 
            // postList
            // 
            this.postList.FormattingEnabled = true;
            this.postList.Location = new System.Drawing.Point(142, 73);
            this.postList.Name = "postList";
            this.postList.Size = new System.Drawing.Size(120, 134);
            this.postList.TabIndex = 3;
            this.postList.SelectedIndexChanged += new System.EventHandler(this.PostList_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(268, 53);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Comments";
            // 
            // commentList
            // 
            this.commentList.FormattingEnabled = true;
            this.commentList.Location = new System.Drawing.Point(269, 73);
            this.commentList.Name = "commentList";
            this.commentList.Size = new System.Drawing.Size(247, 82);
            this.commentList.TabIndex = 6;
            this.commentList.SelectedIndexChanged += new System.EventHandler(this.CommentList_SelectedIndexChanged);
            // 
            // commentBox
            // 
            this.commentBox.AcceptsReturn = true;
            this.commentBox.Location = new System.Drawing.Point(523, 73);
            this.commentBox.Multiline = true;
            this.commentBox.Name = "commentBox";
            this.commentBox.ReadOnly = true;
            this.commentBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.commentBox.Size = new System.Drawing.Size(265, 260);
            this.commentBox.TabIndex = 7;
            this.commentBox.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.CommentBox_MouseDoubleClick);
            // 
            // postText
            // 
            this.postText.AcceptsReturn = true;
            this.postText.Location = new System.Drawing.Point(142, 213);
            this.postText.Multiline = true;
            this.postText.Name = "postText";
            this.postText.ReadOnly = true;
            this.postText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.postText.Size = new System.Drawing.Size(120, 150);
            this.postText.TabIndex = 8;
            // 
            // commentTree
            // 
            this.commentTree.Location = new System.Drawing.Point(269, 161);
            this.commentTree.Name = "commentTree";
            this.commentTree.Size = new System.Drawing.Size(248, 202);
            this.commentTree.TabIndex = 9;
            // 
            // userRating
            // 
            this.userRating.Location = new System.Drawing.Point(523, 340);
            this.userRating.Maximum = 5;
            this.userRating.Name = "userRating";
            this.userRating.Size = new System.Drawing.Size(265, 45);
            this.userRating.TabIndex = 10;
            this.userRating.Value = 2;
            this.userRating.Scroll += new System.EventHandler(this.UserRating_Scroll);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 383);
            this.Controls.Add(this.userRating);
            this.Controls.Add(this.commentTree);
            this.Controls.Add(this.postText);
            this.Controls.Add(this.commentBox);
            this.Controls.Add(this.commentList);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.postList);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.subRedditList);
            this.Name = "Form1";
            this.Text = "Random App for Reddit";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.userRating)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox subRedditList;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox postList;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ListBox commentList;
        private System.Windows.Forms.TextBox commentBox;
        private System.Windows.Forms.TextBox postText;
        private System.Windows.Forms.TreeView commentTree;
        private System.Windows.Forms.TrackBar userRating;
    }
}


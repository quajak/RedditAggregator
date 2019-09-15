using Controller;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RedditWFA
{
    public partial class QuickRate : Form
    {
        private readonly List<Comment> comments;
        Comment current;
        public QuickRate(List<Comment> comments)
        {
            InitializeComponent();
            this.comments = comments;
            ShowComment();
        }

        public void ShowComment()
        {
            if (comments.Count == 0)
            {
                Close();
                return;
            }
            current = comments[0];
            comments.RemoveAt(0);
            commentBox.Text = current.text;
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        void UpdateComment(float score)
        {
            current.userScore = score;
            Comment.Comments.Add(current);
            ShowComment();
        }

        private void TwoScore_Click(object sender, EventArgs e)
        {
            UpdateComment(0.2f);
        }

        private void FourScore_Click(object sender, EventArgs e)
        {
            UpdateComment(0.4f);
        }

        private void SixScore_Click(object sender, EventArgs e)
        {
            UpdateComment(0.6f);
        }

        private void EightScore_Click(object sender, EventArgs e)
        {
            UpdateComment(0.8f);
        }

        private void TenScore_Click(object sender, EventArgs e)
        {
            UpdateComment(1f);
        }

        private void ZeroScore_Click(object sender, EventArgs e)
        {
            UpdateComment(0);
        }
    }
}

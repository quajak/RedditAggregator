using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RedditSharp.Things;
using Comment = Controller.Comment;


namespace RedditWFA
{
    public partial class Form1 : Form
    {
        readonly List<string> subReddits = new List<string> { "programming", "whatisthisthing", "politics", "explainlikeimfive" };

        public Form1()
        {
            //RedditAggregator.RedditAggregator.TrainModels();
            Controller.Controller _ = Controller.Controller.Instance;
            //now update all comments
            //Comment.RepredictAll();

            InitializeComponent();
            subRedditList.Items.AddRange(subReddits.ToArray());
            FormClosed += Form1_FormClosed;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Comment.SaveComments(Comment.Comments, true);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        List<Post> posts;
        private void SubRedditList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (subRedditList.SelectedIndex == -1) return;
            posts = SourceApi.RedditApi.GetPosts((string)subRedditList.SelectedItem);
            postList.Items.Clear();
            postList.Items.AddRange(posts.Select(p => p.Title).ToArray());
        }

        List<Comment> comments;
        private void PostList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (postList.SelectedIndex == -1) return;
            Post post = posts[postList.SelectedIndex];
            postText.Text = post.Title + "\r\n"+ posts[postList.SelectedIndex].SelfText;
            comments = SourceApi.RedditApi.GetComments(post);
            commentList.Items.Clear();
            commentList.Items.AddRange(comments.Select(c => c.text).ToArray());
        }

        private void CommentList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (commentList.SelectedIndex == -1) return;
            Comment c = comments[commentList.SelectedIndex];
            scoreLabel.Text= $" {c.score1} - {c.score2} - {c.score3} - {c.totalScore} vs {c.userScore}";
            commentBox.Text = c.text;
        }

        private void UserRating_Scroll(object sender, EventArgs e)
        {
            if (commentList.SelectedIndex == -1) return;
            Comment c = comments[commentList.SelectedIndex];
            c.userScore = userRating.Value / 5f;
            if(!Comment.Comments.Contains(c))
                Comment.Comments.Add(c);
        }

        private void CommentBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (commentList.SelectedIndex == -1) return;
            Comment c = comments[commentList.SelectedIndex];
            c.userScore = userRating.Value / 5f;
            if (!Comment.Comments.Contains(c))
                Comment.Comments.Add(c);
        }

        private void Label4_Click(object sender, EventArgs e)
        {

        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RedditAggregator;
using RedditAggregator.Model;
using RedditSharp.Things;
using Comment = RedditAggregator.Model.Comment;

namespace RedditWFA
{
    public partial class Form1 : Form
    {
        List<string> subReddits = new List<string> { "programming", "whatisthisthing", "politics", "explainlikeimfive" };

        public Form1()
        {
            RedditAggregator.RedditAggregator.TrainModels();
            InitializeComponent();
            subRedditList.Items.AddRange(subReddits.ToArray());
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        List<Post> posts;
        private void SubRedditList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (subRedditList.SelectedIndex == -1) return;
            posts = RedditAggregator.RedditAggregator.GetPosts((string)subRedditList.SelectedItem);
            postList.Items.Clear();
            postList.Items.AddRange(posts.Select(p => p.Title).ToArray());
        }

        List<Comment> comments;
        private void PostList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (postList.SelectedIndex == -1) return;
            Post post = posts[postList.SelectedIndex];
            postText.Text = post.Title + "\r\n"+ posts[postList.SelectedIndex].SelfText;
            comments = RedditApi.GetComments(post);
            commentList.Items.Clear();
            commentList.Items.AddRange(comments.Select(c => c.text).ToArray());
        }

        private void CommentList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (commentList.SelectedIndex == -1) return;
            Comment c = comments[commentList.SelectedIndex];
            commentBox.Text = c.text + $"\r\n {c.score1} - {c.score2}";
        }
    }
}

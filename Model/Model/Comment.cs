using Microsoft.ML.Runtime.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RedditAggregator.Model
{
    public class Comment
    {
        static List<Comment> comments;

        private const string Path = "./comments.txt";
        public int id;
        public string text;
        public float score1;
        public float score2;
        public int upvotes;
        public int downvotes;
        public float userScore;
        public int parentId;
        public float parentScore1;
        public float parentScore2;

        public static List<Comment> Comments { get {
                if (comments is null)
                    LoadComments();
                return comments; }}

        public Comment(int id, string text, float score1, float score2, int upvotes, int downvotes, float userScore, int parentId)
        {
            this.id = id;
            this.text = text;
            this.score1 = score1;
            this.score2 = score2;
            this.upvotes = upvotes;
            this.downvotes = downvotes;
            this.userScore = userScore;
            this.parentId = parentId;
            if(parentId != id && parentId != -1)
            {
                parentScore1 = Comments.FirstOrDefault(c => c.id == parentId)?.score1 ?? 1;
                parentScore2 = Comments.FirstOrDefault(c => c.id == parentId)?.score2 ?? 1;
            }
            else
            {
                parentScore1 = 1; //no idea what to put in here
                parentScore2 = 1;
            }
        }

        public Comment(int id, RedditSharp.Things.Comment comment, float userScore, int parentId) 
            : this(id, Regex.Replace(comment.Body, @"[\n-]", ""), 0, 0,comment.Upvotes, comment.Downvotes, userScore,  parentId)
        {

        }

        /// <summary>
        /// This is used when getting a new comment from a source and predicting its value
        /// </summary>
        /// <param name="id"></param>
        /// <param name="comment"></param>
        /// <param name="parentId"></param>
        public Comment(int id, RedditSharp.Things.Comment comment, int parentId)
            : this(id, Regex.Replace(comment.Body, @"[\n-]", ""), 0, 0, comment.Upvotes, comment.Downvotes, 0.5f, parentId)
        {
            Predict();
        }

        public void Predict()
        {
            var points = Controller.Instance.Predict(this);
            score1 = points.value1;
            score2 = points.value2;
        }

        public static int GetId()
        {
            if (!File.Exists(Path))
                return 0;
    
            return File.ReadAllLines(Path).Length;
        }

        public static void RepredictAll()
        {
            foreach (var comment in comments)
            {
                comment.Predict();
            }
        }

        static void LoadComments()
        {
            if (!File.Exists(Path))
            {
                comments = new List<Comment>();
                return;
            }

            comments = new List<Comment>();
            var lines = File.ReadAllLines(Path);
            foreach (var line in lines)
            {
                var words = line.Split('-').ToList().Where(w => w != "").ToArray();
                int id = int.Parse(words[0]);
                string text = words[1];
                float score1 = float.Parse(words[2]);
                float score2 = float.Parse(words[3]);
                int upvotes = int.Parse(words[4]);
                int downvotes = int.Parse(words[5]);
                float userScore = float.Parse(words[6]);
                int parentId = int.Parse(words[7] == "" ? words[8] : words[7]); //Some weird bug look at it later
                comments.Add(new Comment(id, text, score1, score2, upvotes, downvotes, userScore, parentId));
            }
        }

        public static void SaveComments(List<Comment> comments, bool overwrite = false)
        {
            if (overwrite)
                File.Delete(Path);

            using (TextWriter writer = new StreamWriter(Path, true))
            {
                foreach (var c in comments)
                {
                    string line = $"{c.id}-{c.text}-{c.score1}-{c.score2}-{c.upvotes}-{c.downvotes}-{c.userScore}-{c.parentId}";
                    writer.WriteLine(line);
                }
            }
        }

        static Comment GetComment(int id)
        {
            return Comments.FirstOrDefault(c => c.id == id);
        }

        public static implicit operator CommentData(Comment c)
        {
            return new CommentData(c.text, c.upvotes, c.downvotes, c.userScore);
        }
    }

    public class CommentData
    {
        public string text;
        public float vote;
        public float score;

        public CommentData(string text, int upvotes, int downvotes, float score)
        {
            this.text = text;
            int b = Math.Abs(upvotes) + downvotes;
            vote = b == 0 ? 0 : Math.Abs(upvotes) / b;
            this.score = score;
        }
    }

    public class CommentPrediction
    {
        [Column("0", name: "Score")]
        public float score;
    }
}

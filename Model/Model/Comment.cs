using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Controller
{
    public class Comment
    {
        static List<Comment> comments;

        private const string Path = "./comments.txt";
        public int id;
        public string text;
        /// <summary>
        /// Score of the text model
        /// </summary>
        public float score1;
        /// <summary>
        /// Score of the word model
        /// </summary>
        public float score2;
        /// <summary>
        /// Score of the stat model
        /// </summary>
        public float score3;
        /// <summary>
        /// Score from the combinator model
        /// </summary>
        public float totalScore;
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

        public Comment(int id, string text, float score1, float score2, float score3, int upvotes, int downvotes, float userScore, int parentId)
        {
            this.id = id;
            this.text = text;
            this.score1 = score1;
            this.score2 = score2;
            this.score3 = score3;
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
            : this(id, Regex.Replace(comment.Body, @"[\n-]", ""), 0, 0, 0,comment.Upvotes, comment.Downvotes, userScore,  parentId)
        {

        }

        /// <summary>
        /// This is used when getting a new comment from a source and predicting its value
        /// </summary>
        /// <param name="id"></param>
        /// <param name="comment"></param>
        /// <param name="parentId"></param>
        public Comment(int id, RedditSharp.Things.Comment comment, int parentId)
            : this(id, Regex.Replace(comment.Body, @"[\n-]", ""), 0, 0, 0, comment.Upvotes, comment.Downvotes, 0.5f, parentId)
        {
            Predict();
        }

        public void Predict()
        {
            Controller.Instance.Predict(this);
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
                //Generate garbage so model can learn something
                for (int i = 0; i < 20; i++)
                {
                    comments.Add(new Comment(i, "i am stupid", 0.05f, 0.05f,0.05f, 1, 1, 0, 0));
                }
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
                float score3 = float.Parse(words[4]);
                int upvotes = int.Parse(words[5]);
                int downvotes = int.Parse(words[6]);
                float userScore = float.Parse(words[7]);
                int parentId = int.Parse(words[8] == "" ? words[9] : words[8]); //Some weird bug look at it later
                comments.Add(new Comment(id, text, score1, score2, score3, upvotes, downvotes, userScore, parentId));
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
                    string line = $"{c.id}-{c.text}-{c.score1}-{c.score2}-{c.score3}-{c.upvotes}-{c.downvotes}-{c.userScore}-{c.parentId}";
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
        public string[] words;
        public float vote;
        public float score;

        public CommentData(string text, int upvotes, int downvotes, float score)
        {
            this.text = text;
            words = text.Split(' ');
            int b = Math.Abs(upvotes) + downvotes;
            vote = b == 0 ? 0 : Math.Abs(upvotes) / b;
            this.score = score;
        }
    }

    public class CommentPrediction
    {
        //[Column("0", name: "Score")]
        public float Score;
    }
}

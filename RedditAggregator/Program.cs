
using RedditSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML.Runtime.Api;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Runtime;
using Microsoft.ML.Runtime.Learners;
using Microsoft.ML;
using Microsoft.ML.Legacy;
using Microsoft.ML.Legacy.Transforms;
using Microsoft.ML.Legacy.Trainers;
using RedditSharp.Things;
using System.Threading.Tasks;
using Microsoft.ML.Legacy.Models;

namespace RedditAggregator
{
    public class RedditAggregator
    {
        static PredictionFunction<CommentData, CommentPrediction> model;
        static PredictionModel<CommentData, CommentPrediction> model2;
        static Reddit reddit = new Reddit();

        static void Main(string[] args)
        {
            while (true)
            {
                var command = Console.ReadLine();
                switch (command)
                {
                    case "Exit":
                        goto exit;
                    case "Data":
                        AddData();
                        break;
                    case "Learn":
                        TrainModels();
                        break;
                    case "Use":
                        ShowPost();
                        break;
                    default:
                        Console.WriteLine("Not known command: " + command);
                        break;
                }
            }
            exit:
            Console.WriteLine("Leaving!");
        }

        public async static void LoadModels()
        {
            model2 = await PredictionModel.ReadAsync<CommentData, CommentPrediction>("./model2.model");
        }

        public static void TrainModels()
        {
            model = Train();
            if (!File.Exists("./model2.model"))
                model2 = CreateWordAnalyser();
            else
                LoadModels();
            //save models
            Console.WriteLine("Now saving models!");
            
            model2.WriteAsync("./model2.model");
        }

        static PredictionFunction<CommentData, CommentPrediction> Train()
        {
            Console.WriteLine("Training the model!");
            var env = new ConsoleEnvironment();
            var reader = new TextLoader(env, new TextLoader.Arguments()
            {
                HasHeader = true,
                Separator = "-",
                Column = new[]
                {
                    new TextLoader.Column("Score", DataKind.R4, 0),
                    new TextLoader.Column("Comment", DataKind.Text, 1),
                    new TextLoader.Column("Vote", DataKind.R4, 2),
                }
            });

            var trainingDataView = reader.Read(new MultiFileSource("./data.txt"));

            var pipeline = new TextTransform(env, "Comment", "Comment", s =>
                    {
                        s.KeepDiacritics = true;
                    })
                  .Append(new ConcatEstimator(env, "Features", "Comment", "Vote"))
                  .Append(new SdcaRegressionTrainer(env, new SdcaRegressionTrainer.Arguments(), "Features", "Score"));
            var model = pipeline.Fit(trainingDataView);
            var function = model.MakePredictionFunction<CommentData, CommentPrediction>(env);
            Console.WriteLine("Finished learning");
            return function;
        }

        private static PredictionModel<CommentData, CommentPrediction> CreateWordAnalyser()
        {
            var pipeline = new LearningPipeline
            {
                new Microsoft.ML.Legacy.Data.TextLoader("./data.txt").CreateFrom<CommentData>(useHeader: true, separator: '-'),
                new TextFeaturizer("Words", "Comment"){OutputTokens=true, KeepPunctuations=true},
                new WordEmbeddings(("Words_TransformedText", "CommentWords")){ModelKind=WordEmbeddingsTransformPretrainedModelKind.FastTextWikipedia300D},
                new ColumnConcatenator( "Features", "Vote", "CommentWords", "Words"),
                new StochasticDualCoordinateAscentRegressor() {LabelColumn="Score"}
            };

            var model = pipeline.Train<CommentData, CommentPrediction>();
            return model;
        }

        public static void AddData()
        {
            Post post = GetPost();
            List<CommentData> comments = new List<CommentData>();
            Console.WriteLine(post.SelfText);
            Console.WriteLine("----- Comments starting here -----");
            bool end = false;
            foreach (var comment in post.Comments)
            {
                if (comment.Body != null)
                    end = GetFeedback(comments, end, comment, 1);
                if (end) break;
            }
            Console.WriteLine("Finished adding data! Now saving!");
            using (StreamWriter file = File.AppendText("./data.txt"))
            {
                foreach (var comment in comments)
                {
                    file.WriteLine($"{comment.quality} - {comment.comment.Replace('\n', ' ').Replace('-', ' ')} - {comment.vote}");
                }
            }
            Console.WriteLine("Finished adding data!");
        }

        public async static Task<List<RedditSharp.Things.Comment>> GetComments(Post post)
        {
            List<RedditSharp.Things.Comment> comments = new List<RedditSharp.Things.Comment>();
            var tasks = post.Comments.Select(c => GetSubComment(c)).AsEnumerable();
            var nested = await Task.WhenAll(tasks);
            foreach (var list in nested)
            {
                comments.AddRange(list);
            }
            return comments;
        }

        public async static Task<List<RedditSharp.Things.Comment>> GetSubComment(RedditSharp.Things.Comment comment)
        {
            var tasks = comment.Comments.Select(c => GetSubComment(c)).AsEnumerable();
            var nested = await Task.WhenAll(tasks);
            List<RedditSharp.Things.Comment> comments = new List<RedditSharp.Things.Comment>() { comment };
            foreach (var list in nested)
            {
                comments.AddRange(list);
            }
            return comments;
        }


        static void ShowPost()
        {
            Post post = GetPost();

            Console.WriteLine(post.SelfText);

            List<RedditSharp.Things.Comment> comments = GetComments(post).GetAwaiter().GetResult();

            List<(RedditSharp.Things.Comment comment, float value, float value2)> sComments = comments
                .Select(c => EvaluateComment(c))
                .Where(c => c.Item2 != float.NaN && c.Item3 != float.NaN && c.Item1.Body != null && c.Item1.Body.Length != 0)
                .OrderByDescending(c => c.Item2 + c.Item3)
                .ToList();

            Console.WriteLine($"Loaded {sComments.Count} comments!");

            while (sComments.Count != 0)
            {
                Console.WriteLine(sComments[0].comment.Body);
                Console.WriteLine($"{sComments[0].value:P0} {sComments[0].value2:P0}");
                sComments.RemoveAt(0);
                string input = Console.ReadLine();
                if (input == "Exit")
                    break;
            }
        }

        public static (RedditSharp.Things.Comment c, float, float) EvaluateComment(RedditSharp.Things.Comment c)
        {
            return (c, model.Predict(new CommentData(0, c.Body, GetVote(c))).quality
                                , model2.Predict(new CommentData(0, c.Body, GetVote(c))).quality);
        }

        public static async Task<List<(RedditSharp.Things.Comment comment, float value1, float value2)>> EvaluateComments(Post post)
        {
            List<(RedditSharp.Things.Comment comment, float value1, float value2)> comments = new List<(RedditSharp.Things.Comment comment, float value1, float value2)>();
            var tasks = post.Comments.Where(c => c != null && c.Body != null).Select(c => EvaluateSubComment(c)).AsEnumerable();
            var nested = await Task.WhenAll(tasks);
            foreach (var list in nested)
            {
                if (list != null)
                    comments.AddRange(list);
            }
            return comments.OrderByDescending(c => c.value1 + c.value2).ToList();
        }

        public async static Task<List<(RedditSharp.Things.Comment comment, float value1, float value2)>> EvaluateSubComment(RedditSharp.Things.Comment comment)
        {
            var tasks = comment.Comments.Select(c => EvaluateSubComment(c)).AsEnumerable();
            var nested = await Task.WhenAll(tasks);
            List<(RedditSharp.Things.Comment comment, float value1, float value2)> comments =
                new List<(RedditSharp.Things.Comment comment, float value1, float value2)>() { EvaluateComment(comment) };
            foreach (var list in nested)
            {
                if(list != null)
                    comments.AddRange(list);
            }
            return comments;
        }

        public static List<Post> GetPosts(string subReddit, int count = 10)
        {
            List<Post> posts = new List<Post>();
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    List<Post> _p = reddit.GetSubreddit(subReddit).Posts.Take(count).ToList();
                    if(_p.Count > posts.Count)
                        posts = _p;
                }
                catch
                {
                }
                if (posts.Count == count)
                    return posts;
            }
            return posts;
        }

        static Post GetPost()
        {
            Console.Write("Subreddit: r/");
            string subRedditString = Console.ReadLine();
            var subReddit = reddit.GetSubreddit(subRedditString);
            var post = subReddit.Posts.Take(1).ToList()[0];
            foreach (var pPost in subReddit.Posts)
            {
                Console.WriteLine(pPost.Title);
                var r = Console.ReadLine();
                if (r == "y")
                {
                    post = pPost;
                    break;
                }
            }

            return post;
        }

        private static bool GetFeedback(List<CommentData> comments, bool end, RedditSharp.Things.Comment comment, int depth)
        {
            List<string> lines = comment.Body.Split('\n').ToList();
            List<string> shortenedLines = new List<string>();
            for (int i = 0; i < lines.Count; i++)
            {
                string current = lines[i];
                while(current.Length > Console.WindowWidth - depth -1)
                {
                    shortenedLines.Add(current.Substring(0, Console.WindowWidth- depth - 1));
                    current = current.Substring(Console.WindowWidth-depth - 1);
                }
                if(current != "")
                    shortenedLines.Add(current);
            }
            foreach (var line in shortenedLines)
            {
                Console.WriteLine(new string('-', depth) + line);
            }

            float vote = GetVote(comment);
            Console.WriteLine(new string('-', depth) + model?.Predict(new CommentData(0f, comment.Body, vote)).quality);
            Console.WriteLine(new string('-', depth) + model2?.Predict(new CommentData(0f, comment.Body, vote)).quality);
            while (true)
            {
                string s = Console.ReadLine();
                if (float.TryParse(s, out float quality))
                {
                    comments.Add(new CommentData(quality, comment.Body, vote));
                    break;
                }
                else
                {
                    if (s == "Exit") { end = true; break; }
                    if (s == "Skip" || s.ToLower() == "s") break;
                    Console.WriteLine($" {s} is not a valid input!");
                }
            }

            IList<RedditSharp.Things.Comment> subCommentsSorted = comment.Comments.OrderByDescending(c => model?.Predict(new CommentData(0f, c.Body, GetVote(comment)))).Take(10).ToList(); ;
            if (subCommentsSorted.Count != 0)
                Console.WriteLine($"Showing {subCommentsSorted.Count} sub comments! Depth: {depth}");
            else
                Console.WriteLine("No sub comments!");

            foreach (var subComment in subCommentsSorted)
            {
                if(subComment.Body != null)
                    end = GetFeedback(comments, end, subComment, depth+1);
                if (end) break;
            }

            return end;
        }

        private static float GetVote(RedditSharp.Things.Comment comment)
        {
            return (float)comment.Upvotes / (comment.Upvotes + comment.Downvotes);
        }

        class CommentPrediction
        {
            [ColumnName("Score")]
            public float quality;
        }

        public class CommentData
        {
            [Column(ordinal: "0", name:"Score")]
            public float quality;
            [Column(ordinal: "1", name:"Comment")]
            public string comment;
            [Column(ordinal: "2", name:"Vote")]
            public float vote;

            public CommentData(float quality, string comment, float vote)
            {
                this.quality = quality;
                this.comment = comment;
                this.vote = vote;
            }
        }
    }
}

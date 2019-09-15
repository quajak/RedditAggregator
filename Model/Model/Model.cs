using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;
using Microsoft.ML.Transforms.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Controller
{
    abstract class Model
    {
        internal static MLContext env = new MLContext();
        public abstract void TrainModel();

        public abstract void LoadModel();

        public abstract float Predict(Comment comment);
    }

    class BasicTextModel : Model
    {
        public PredictionEngine<CommentData, CommentPrediction> function;

        public override void TrainModel()
        {
            List<CommentData> data = Comment.Comments.Select(c => (CommentData)c).ToList();
            var dataView = env.Data.LoadFromEnumerable(data);

            var pipeline = env.Transforms.Text.FeaturizeText("Features", 
                new TextFeaturizingEstimator.Options() { KeepNumbers = true }, "text")
            .Append(env.Regression.Trainers.LbfgsPoissonRegression("score", "Features")); //TODO: Why are we using this regression?

            var model = pipeline.Fit(dataView);
            function = env.Model.CreatePredictionEngine<CommentData, CommentPrediction>(model, dataView.Schema);
            var metrics = env.Regression.Evaluate(model.Transform(dataView), "Score", "score");
            Trace.TraceInformation($"{metrics.MeanAbsoluteError} {metrics.MeanSquaredError} {metrics.RootMeanSquaredError} {metrics.RSquared}");
        }

        public override void LoadModel()
        {
            TrainModel();
        }

        public override float Predict(Comment comment)
        {
            CommentPrediction commentPrediction = function.Predict(new CommentData(comment.text, comment.upvotes, comment.downvotes, comment.userScore));
            return commentPrediction.Score;
        }
    }
    /// <summary>
    /// This model is used to combine the output of the other two models for a final score
    /// </summary>
    class CombinatorModel : Model
    {
        PredictionEngine<ModelData, CommentPrediction> function;

        public override void TrainModel()
        {
            env = new MLContext();
            List<ModelData> data = Comment.Comments.Select(c => new ModelData(c.score2, c.score1, c.userScore, c.score3)).ToList();
            var dataView = env.Data.LoadFromEnumerable(data);

            var pipeline = env.Transforms.Concatenate("Features", "wordModel", "textModel", "statModel")
            .Append(env.Regression.Trainers.Sdca("userScore","Features"));

            var model = pipeline.Fit(dataView);
            function = env.Model.CreatePredictionEngine<ModelData, CommentPrediction>(model);
            var transData = model.Transform(dataView);
            var metrics = env.Regression.Evaluate(transData, "Score", "userScore");
            Trace.TraceInformation("CombinatorModel");
            Trace.TraceInformation($"{metrics.MeanAbsoluteError} {metrics.MeanSquaredError} {metrics.RootMeanSquaredError} {metrics.RSquared}");
            var featureImportance = env.Regression.PermutationFeatureImportance(model.LastTransformer, transData, "Score");

            for (int i = 0; i < featureImportance.Count(); i++)
            {
                Trace.TraceInformation($"Feature{i}: Difference in RMS - {featureImportance[i].RootMeanSquaredError.Mean}");
            }
        }

        public override void LoadModel()
        {
            TrainModel();
        }

        public override float Predict(Comment comment)
        {
            float score = function.Predict(new ModelData(comment.score2, comment.score1, comment.userScore, comment.score3)).Score;
            return score;
        }
    }

    class EmbeddingData
    {
        [VectorType(25)]
        public float[] Features;
        public float[] embeddings;
        public float score;

        public EmbeddingData(float[] features)
        {
            Features = features;
        }

        public EmbeddingData()
        {

        }
    }

    class CommentStats
    {
        //public float votes; - until we get stats about total post votes / subreddit this is useless
        public float upvotePercentage;
        public float length;
        public float punctuationPercent;
        public float links;
        public float score;
        public float emojiPercentage;

        public CommentStats(Comment comment)
        {
            upvotePercentage = (comment.upvotes + comment.downvotes) != 0 ? comment.upvotes / (comment.upvotes + comment.downvotes) : 50; 
            length = (float)Math.Log(comment.text.Length);
            punctuationPercent = (float)Regex.Replace(comment.text, @"[A-Za-z ]", "").Length / comment.text.Length;
            links = (float)Math.Log(Regex.Matches(comment.text, @"[\w+\.(com|org|net|ca|us|co|gov)]").Count, 2);
            score = comment.userScore;
            string regex = "(?:0\x20E3|1\x20E3|2\x20E3|3\x20E3|4\x20E3|5\x20E3|6\x20E3|7\x20E3|8\x20E3|9\x20E3|#\x20E3|\\*\x20E3|\xD83C(?:\xDDE6\xD83C(?:\xDDE8|\xDDE9|\xDDEA|\xDDEB|\xDDEC|\xDDEE|\xDDF1|\xDDF2|\xDDF4|\xDDF6|\xDDF7|\xDDF8|\xDDF9|\xDDFA|\xDDFC|\xDDFD|\xDDFF)|\xDDE7\xD83C(?:\xDDE6|\xDDE7|\xDDE9|\xDDEA|\xDDEB|\xDDEC|\xDDED|\xDDEE|\xDDEF|\xDDF1|\xDDF2|\xDDF3|\xDDF4|\xDDF6|\xDDF7|\xDDF8|\xDDF9|\xDDFB|\xDDFC|\xDDFE|\xDDFF)|\xDDE8\xD83C(?:\xDDE6|\xDDE8|\xDDE9|\xDDEB|\xDDEC|\xDDED|\xDDEE|\xDDF0|\xDDF1|\xDDF2|\xDDF3|\xDDF4|\xDDF5|\xDDF7|\xDDFA|\xDDFB|\xDDFC|\xDDFD|\xDDFE|\xDDFF)|\xDDE9\xD83C(?:\xDDEA|\xDDEC|\xDDEF|\xDDF0|\xDDF2|\xDDF4|\xDDFF)|\xDDEA\xD83C(?:\xDDE6|\xDDE8|\xDDEA|\xDDEC|\xDDED|\xDDF7|\xDDF8|\xDDF9|\xDDFA)|\xDDEB\xD83C(?:\xDDEE|\xDDEF|\xDDF0|\xDDF2|\xDDF4|\xDDF7)|\xDDEC\xD83C(?:\xDDE6|\xDDE7|\xDDE9|\xDDEA|\xDDEB|\xDDEC|\xDDED|\xDDEE|\xDDF1|\xDDF2|\xDDF3|\xDDF5|\xDDF6|\xDDF7|\xDDF8|\xDDF9|\xDDFA|\xDDFC|\xDDFE)|\xDDED\xD83C(?:\xDDF0|\xDDF2|\xDDF3|\xDDF7|\xDDF9|\xDDFA)|\xDDEE\xD83C(?:\xDDE8|\xDDE9|\xDDEA|\xDDF1|\xDDF2|\xDDF3|\xDDF4|\xDDF6|\xDDF7|\xDDF8|\xDDF9)|\xDDEF\xD83C(?:\xDDEA|\xDDF2|\xDDF4|\xDDF5)|\xDDF0\xD83C(?:\xDDEA|\xDDEC|\xDDED|\xDDEE|\xDDF2|\xDDF3|\xDDF5|\xDDF7|\xDDFC|\xDDFE|\xDDFF)|\xDDF1\xD83C(?:\xDDE6|\xDDE7|\xDDE8|\xDDEE|\xDDF0|\xDDF7|\xDDF8|\xDDF9|\xDDFA|\xDDFB|\xDDFE)|\xDDF2\xD83C(?:\xDDE6|\xDDE8|\xDDE9|\xDDEA|\xDDEB|\xDDEC|\xDDED|\xDDF0|\xDDF1|\xDDF2|\xDDF3|\xDDF4|\xDDF5|\xDDF6|\xDDF7|\xDDF8|\xDDF9|\xDDFA|\xDDFB|\xDDFC|\xDDFD|\xDDFE|\xDDFF)|\xDDF3\xD83C(?:\xDDE6|\xDDE8|\xDDEA|\xDDEB|\xDDEC|\xDDEE|\xDDF1|\xDDF4|\xDDF5|\xDDF7|\xDDFA|\xDDFF)|\xDDF4\xD83C\xDDF2|\xDDF5\xD83C(?:\xDDE6|\xDDEA|\xDDEB|\xDDEC|\xDDED|\xDDF0|\xDDF1|\xDDF2|\xDDF3|\xDDF7|\xDDF8|\xDDF9|\xDDFC|\xDDFE)|\xDDF6\xD83C\xDDE6|\xDDF7\xD83C(?:\xDDEA|\xDDF4|\xDDF8|\xDDFA|\xDDFC)|\xDDF8\xD83C(?:\xDDE6|\xDDE7|\xDDE8|\xDDE9|\xDDEA|\xDDEC|\xDDED|\xDDEE|\xDDEF|\xDDF0|\xDDF1|\xDDF2|\xDDF3|\xDDF4|\xDDF7|\xDDF8|\xDDF9|\xDDFB|\xDDFD|\xDDFE|\xDDFF)|\xDDF9\xD83C(?:\xDDE6|\xDDE8|\xDDE9|\xDDEB|\xDDEC|\xDDED|\xDDEF|\xDDF0|\xDDF1|\xDDF2|\xDDF3|\xDDF4|\xDDF7|\xDDF9|\xDDFB|\xDDFC|\xDDFF)|\xDDFA\xD83C(?:\xDDE6|\xDDEC|\xDDF2|\xDDF8|\xDDFE|\xDDFF)|\xDDFB\xD83C(?:\xDDE6|\xDDE8|\xDDEA|\xDDEC|\xDDEE|\xDDF3|\xDDFA)|\xDDFC\xD83C(?:\xDDEB|\xDDF8)|\xDDFD\xD83C\xDDF0|\xDDFE\xD83C(?:\xDDEA|\xDDF9)|\xDDFF\xD83C(?:\xDDE6|\xDDF2|\xDDFC)))|[\xA9\xAE\x203C\x2049\x2122\x2139\x2194-\x2199\x21A9\x21AA\x231A\x231B\x2328\x23CF\x23E9-\x23F3\x23F8-\x23FA\x24C2\x25AA\x25AB\x25B6\x25C0\x25FB-\x25FE\x2600-\x2604\x260E\x2611\x2614\x2615\x2618\x261D\x2620\x2622\x2623\x2626\x262A\x262E\x262F\x2638-\x263A\x2648-\x2653\x2660\x2663\x2665\x2666\x2668\x267B\x267F\x2692-\x2694\x2696\x2697\x2699\x269B\x269C\x26A0\x26A1\x26AA\x26AB\x26B0\x26B1\x26BD\x26BE\x26C4\x26C5\x26C8\x26CE\x26CF\x26D1\x26D3\x26D4\x26E9\x26EA\x26F0-\x26F5\x26F7-\x26FA\x26FD\x2702\x2705\x2708-\x270D\x270F\x2712\x2714\x2716\x271D\x2721\x2728\x2733\x2734\x2744\x2747\x274C\x274E\x2753-\x2755\x2757\x2763\x2764\x2795-\x2797\x27A1\x27B0\x27BF\x2934\x2935\x2B05-\x2B07\x2B1B\x2B1C\x2B50\x2B55\x3030\x303D\x3297\x3299]|\xD83C[\xDC04\xDCCF\xDD70\xDD71\xDD7E\xDD7F\xDD8E\xDD91-\xDD9A\xDE01\xDE02\xDE1A\xDE2F\xDE32-\xDE3A\xDE50\xDE51\xDF00-\xDF21\xDF24-\xDF93\xDF96\xDF97\xDF99-\xDF9B\xDF9E-\xDFF0\xDFF3-\xDFF5\xDFF7-\xDFFF]|\xD83D[\xDC00-\xDCFD\xDCFF-\xDD3D\xDD49-\xDD4E\xDD50-\xDD67\xDD6F\xDD70\xDD73-\xDD79\xDD87\xDD8A-\xDD8D\xDD90\xDD95\xDD96\xDDA5\xDDA8\xDDB1\xDDB2\xDDBC\xDDC2-\xDDC4\xDDD1-\xDDD3\xDDDC-\xDDDE\xDDE1\xDDE3\xDDEF\xDDF3\xDDFA-\xDE4F\xDE80-\xDEC5\xDECB-\xDED0\xDEE0-\xDEE5\xDEE9\xDEEB\xDEEC\xDEF0\xDEF3]|\xD83E[\xDD10-\xDD18\xDD80-\xDD84\xDDC0]";
            emojiPercentage = (float)Regex.Matches(comment.text, regex).Count/comment.text.Length;
        }
    }

    class StatModel : Model
    {
        PredictionEngine<CommentStats, CommentPrediction> function;

        public override void TrainModel()
        {
            List<CommentStats> data = Comment.Comments.Select(c => new CommentStats(c)).ToList();
            var dataView = env.Data.LoadFromEnumerable(data);

            var pipeline = env.Transforms.Concatenate("Features", "upvotePercentage", "length", "punctuationPercent", "links", "emojiPercentage")
                .Append(env.Regression.Trainers.Sdca("score", "Features"));
            var model = pipeline.Fit(dataView);

            function = env.Model.CreatePredictionEngine<CommentStats, CommentPrediction>(model);
            var transData = model.Transform(dataView);
            var metrics = env.Regression.Evaluate(transData, "score", "Score");
            Trace.TraceInformation("StatModel");
            Trace.TraceInformation($"{metrics.MeanAbsoluteError} {metrics.MeanSquaredError} {metrics.RootMeanSquaredError} {metrics.RSquared}");
            var featureImportance = env.Regression.PermutationFeatureImportance(model.LastTransformer, transData, "score");

            for (int i = 0; i < featureImportance.Count(); i++)
            {
                Trace.TraceInformation($"Feature{i}: Difference in RMS - {featureImportance[i].RootMeanSquaredError.Mean}");
            }
            env.Model.Save(model, dataView.Schema, "StatModel.zip");
        }

        public override void LoadModel()
        {
            if (File.Exists("StatModel.zip"))
            {
                var model = env.Model.Load("StatModel.zip", out DataViewSchema _);
                function = env.Model.CreatePredictionEngine<CommentStats, CommentPrediction>(model);
            }
            else
            {
                TrainModel();
            }            
        }

        public override float Predict(Comment comment)
        {
            return function.Predict(new CommentStats(comment)).Score;
        }
    }

    class BasicWordModel : Model
    {
        PredictionEngine<CommentData, EmbeddingData> function;
        PredictionEngine<EmbeddingData, CommentPrediction> function2;
        public override void TrainModel()
        {
            List<CommentData> data = Comment.Comments.Select(c => (CommentData)c).ToList();
            var dataView = env.Data.LoadFromEnumerable(data);

            var pipeline = env.Transforms.Text.NormalizeText("text", "text")
                .Append(env.Transforms.Text.TokenizeIntoWords("tokens", "text"))
                .Append(env.Transforms.Text.ApplyWordEmbedding("embeddings", inputColumnName: "tokens",
                modelKind: WordEmbeddingEstimator.PretrainedModelKind.GloVeTwitter25D));

            var model = pipeline.Fit(dataView);

            var transformedData = model.Transform(dataView);

            // Inspect some columns of the resulting dataset.
            var embeddings = transformedData.GetColumn<float[]>(transformedData.Schema["embeddings"])
                .Select(d => new EmbeddingData(d.Where((f,i) => (i + 1) % 3 == 0).ToArray())).ToArray();
            for (int i = 0; i < embeddings.Length; i++)
            {
                embeddings[i].score = data[i].score;
            }
            dataView = env.Data.LoadFromEnumerable(embeddings);

            var secondPipe = env.Transforms.NormalizeLogMeanVariance("Features", "Features")
                .Append(env.Regression.Trainers.OnlineGradientDescent("score"));
            var nmodel = secondPipe.Fit(dataView);

            function = env.Model.CreatePredictionEngine<CommentData, EmbeddingData>(model);
            function2 = env.Model.CreatePredictionEngine<EmbeddingData, CommentPrediction>(nmodel);
            var t = nmodel.Transform(dataView);
            var metrics = env.Regression.Evaluate(t, "Score", "score");
            Trace.TraceInformation($"{metrics.MeanAbsoluteError} {metrics.MeanSquaredError} {metrics.RootMeanSquaredError} {metrics.RSquared}");
        }

        public override void LoadModel()
        {
            TrainModel();
        }

        public override float Predict(Comment comment)
        {
            var data = function.Predict(new CommentData(comment.text, comment.upvotes, comment.downvotes, comment.userScore));
            data.Features = data.embeddings.Where((f, i) => (i + 1) % 3 == 0).ToArray();
            var score = function2.Predict(data).Score;
            return score;
        }
    }

    class ModelData
    {
        public float wordModel;
        public float textModel;
        public float statModel;
        public float userScore;

        public ModelData(float wordModel, float textModel, float userScore, float statModel)
        {
            this.wordModel = wordModel;
            this.textModel = textModel;
            this.userScore = userScore;
            this.statModel = statModel;
        }
    }
}

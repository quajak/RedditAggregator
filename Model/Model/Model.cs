using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;
using Microsoft.ML.Transforms.Text;
using System;
using System.Collections.Generic;
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
        PredictionEngine<ModelData, ModelPrediction> function;

        public override void TrainModel()
        {
            env = new MLContext();
            List<ModelData> data = Comment.Comments.Select(c => new ModelData(c.score2, c.score1, c.userScore, c.score3)).ToList();
            var dataView = env.Data.LoadFromEnumerable(data);

            var pipeline = env.Transforms.Concatenate("Features", "wordModel", "textModel", "statModel")
            .Append(env.Regression.Trainers.Sdca("userScore","Features"));

            var model = pipeline.Fit(dataView);
            function = env.Model.CreatePredictionEngine<ModelData, ModelPrediction>(model);
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
        public float Score;

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

        public CommentStats(Comment comment)
        {
            upvotePercentage = (comment.upvotes + comment.downvotes) != 0 ? comment.upvotes / (comment.upvotes + comment.downvotes) : 50; 
            length = (float)Math.Log(comment.text.Length);
            punctuationPercent = (float)Regex.Replace(comment.text, @"[A-Za-z ]", "").Length / comment.text.Length;
            links = (float)Math.Log(Regex.Matches(comment.text, @"[\w+\.(com|org|net|ca|us|co|gov)]").Count, 2);
            score = comment.userScore;
        }
    }

    class StatModel : Model
    {
        PredictionEngine<CommentStats, CommentPrediction> function;

        public override void TrainModel()
        {
            List<CommentStats> data = Comment.Comments.Select(c => new CommentStats(c)).ToList();
            var dataView = env.Data.LoadFromEnumerable(data);

            var pipeline = env.Transforms.Concatenate("Features", "upvotePercentage", "length", "punctuationPercent", "links")
                .Append(env.Regression.Trainers.Sdca("score", "Features"));
            var model = pipeline.Fit(dataView);

            function = env.Model.CreatePredictionEngine<CommentStats, CommentPrediction>(model);
        }

        public override void LoadModel()
        {
            TrainModel();
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
                embeddings[i].Score = data[i].score;
            }
            dataView = env.Data.LoadFromEnumerable(embeddings);

            var secondPipe = env.Transforms.NormalizeLogMeanVariance("Features", "Features")
                .Append(env.Regression.Trainers.OnlineGradientDescent("Score"));
            var nmodel = secondPipe.Fit(dataView);

            function = env.Model.CreatePredictionEngine<CommentData, EmbeddingData>(model);
            function2 = env.Model.CreatePredictionEngine<EmbeddingData, CommentPrediction>(nmodel);
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

    public class ModelPrediction
    {
        public float Score;
    }
}

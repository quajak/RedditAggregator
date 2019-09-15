using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;
using Microsoft.ML.Transforms.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
            env = new MLContext();
            //DataViewSchema schema = (new DataViewSchema.Builder()).;// .Create(typeof(CommentData));
            List<CommentData> data = Comment.Comments.Select(c => (CommentData)c).ToList();
            var dataView = env.Data.LoadFromEnumerable(data);

            var pipeline = env.Transforms.Text.FeaturizeText("ftext", new TextFeaturizingEstimator.Options() { KeepNumbers = true }, "text")
            .Append(env.Transforms.Concatenate("Features", "ftext", "vote"))
            .Append(env.Regression.Trainers.LbfgsPoissonRegression("score", "Features"));

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
            float score = commentPrediction.Score;
            return score;
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
            //var schema = SchemaDefinition.Create(typeof(ModelData));
            List<ModelData> data = Comment.Comments.Select(c => new ModelData(c.score2, c.score1, c.userScore)).ToList();
            var dataView = env.Data.LoadFromEnumerable(data);

            var pipeline = env.Transforms.Concatenate("Features", "wordModel", "textModel")
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
            float score = function.Predict(new ModelData(comment.score2, comment.score1, comment.userScore)).Score;
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

    class BasicWordModel : Model
    {
        PredictionEngine<CommentData, EmbeddingData> function;
        PredictionEngine<EmbeddingData, CommentPrediction> function2;
        public override void TrainModel()
        {
            env = new MLContext();
            //DataViewSchema schema = (new DataViewSchema.Builder()).;// .Create(typeof(CommentData));
            //SchemaDefinition columns = SchemaDefinition.Create(typeof(CommentData));
            List<CommentData> data = Comment.Comments.Select(c => (CommentData)c).ToList();
            var dataView = env.Data.LoadFromEnumerable(data);

            var pipeline = env.Transforms.Text.FeaturizeText("ftext", "text")
                .Append(env.Transforms.Text.NormalizeText("text", "text"))
                .Append(env.Transforms.Text.TokenizeIntoWords("tokens", "text"))
                .Append(env.Transforms.Text.ApplyWordEmbedding("embeddings", inputColumnName: "tokens",
                modelKind: WordEmbeddingEstimator.PretrainedModelKind.GloVeTwitter25D));
                //.Append(env.Transforms.Concatenate("Features", "ftext", "embeddings"))
                //.Append(env.Regression.Trainers.Sdca("score", "embeddings"));

            var model = pipeline.Fit(dataView);

            var transformedData = model.Transform(dataView);

            // Inspect some columns of the resulting dataset.
            var embeddings = transformedData.GetColumn<float[]>(transformedData.Schema["embeddings"])
                .Select(d => new EmbeddingData(d.Where((f,i) => i % 3 == 0).ToArray())).ToArray();
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
            data.Features = data.embeddings.Where((f, i) => i % 3 == 0).ToArray();
            var score = function2.Predict(data).Score;
            return score;
        }
    }

    class ModelData
    {
        public float wordModel;
        public float textModel;
        public float userScore;

        public ModelData(float wordModel, float textModel, float userScore)
        {
            this.wordModel = wordModel;
            this.textModel = textModel;
            this.userScore = userScore;
        }
    }

    public class ModelPrediction
    {
        //[Column("0", name: "Score")]
        public float Score;
    }
}

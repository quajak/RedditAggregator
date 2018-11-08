using Microsoft.ML.Legacy;
using Microsoft.ML.Legacy.Data;
using Microsoft.ML.Legacy.Trainers;
using Microsoft.ML.Legacy.Transforms;
using Microsoft.ML.Runtime.Api;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Runtime.Learners;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RedditAggregator.Model
{
    abstract class Model
    {
        internal static ConsoleEnvironment env = new ConsoleEnvironment();
        public abstract void TrainModel();

        public abstract void LoadModel();

        public abstract float Predict(Comment comment);
    }

    class BasicTextModel : Model
    {
        PredictionFunction<CommentData, CommentPrediction> function;

        public override void TrainModel()
        {
            env = new ConsoleEnvironment();
            var shema = SchemaDefinition.Create(typeof(CommentData));
            List<CommentData> data = Comment.Comments.Select(c => (CommentData)c).ToList();
            var dataView = env.CreateDataView(data, shema);

            var pipeline = new TextTransform(env, "text", "text", s =>
            {
                s.KeepDiacritics = true;
            })
            .Append(new ConcatEstimator(env, "Features", "text", "vote"))
            .Append(new SdcaRegressionTrainer(env, new SdcaRegressionTrainer.Arguments() { FeatureColumn = "Features", LabelColumn = "score" }
            , "text", "score"));

            var model = pipeline.Fit(dataView);
            function = model.MakePredictionFunction<CommentData, CommentPrediction>(env);
        }

        public override void LoadModel()
        {
            TrainModel();
        }

        public override float Predict(Comment comment)
        {
            float score = function.Predict(comment).score;
            return score;
        }
    }

    class BasicWordModel : Model
    {
        PredictionModel<CommentData, CommentPrediction> model;
        public override void TrainModel()
        {
            var pipeline = new LearningPipeline
            {
                CollectionDataSource.Create(Comment.Comments.Select(c => (CommentData)c)),
                new TextFeaturizer("Words", "text"){OutputTokens=true, KeepPunctuations=true},
                new WordEmbeddings(("Words_TransformedText", "textWords")){ModelKind=WordEmbeddingsTransformPretrainedModelKind.GloVe50D},
                new ColumnConcatenator( "Features", "vote", "textWords", "Words"),
                new StochasticDualCoordinateAscentRegressor() {LabelColumn="score"}
            };
            model = pipeline.Train<CommentData, CommentPrediction>();
        }

        public override void LoadModel()
        {
            if (!File.Exists("./basicwordmodel.model"))
                TrainModel();
            else
                model = PredictionModel.ReadAsync<CommentData, CommentPrediction>("./basicwordmodel.model").Result;
        }

        public override float Predict(Comment comment)
        {
            float score = model.Predict(comment).score;
            return score;
        }
    }
}

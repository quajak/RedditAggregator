using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controller
{
    public class Controller
    {
        private static Controller instance;
        readonly BasicTextModel basicTextModel;
        readonly BasicWordModel basicWordModel;
        readonly CombinatorModel combinatorModel;
        readonly StatModel statModel;

        Controller()
        {
            basicTextModel = new BasicTextModel();
            basicTextModel.LoadModel();
            basicWordModel = new BasicWordModel();
            basicWordModel.LoadModel();
            combinatorModel = new CombinatorModel();
            combinatorModel.LoadModel();
            statModel = new StatModel();
            statModel.LoadModel();
        }

        public static Controller Instance {
            get {
                if (instance is null)
                    instance = new Controller();
                return instance;
            } }

        /// <summary>
        /// used to predict the value of a comment
        /// </summary>
        /// <param name="c"></param>
        public void Predict(Comment c)
        {
            c.score1 = basicTextModel.Predict(c);
            c.score2 = basicWordModel.Predict(c);
            c.score3 = statModel.Predict(c);
            c.totalScore = combinatorModel.Predict(c);
        }
    }
}

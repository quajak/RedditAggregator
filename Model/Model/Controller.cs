using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditAggregator.Model
{
    public class Controller
    {
        private static Controller instance;
        BasicTextModel basicTextModel;
        BasicWordModel basicWordModel;

        Controller()
        {
            basicTextModel = new BasicTextModel();
            basicTextModel.LoadModel();
            basicWordModel = new BasicWordModel();
            basicWordModel.LoadModel();
        }

        public static Controller Instance {
            get {
                if (instance is null)
                    instance = new Controller();
                return instance;
            } }

        public (float value1, float value2) Predict(Comment c)
        {
            float v = basicTextModel.Predict(c);
            float v1 = basicWordModel.Predict(c);
            return (v, v1);
        }
    }
}

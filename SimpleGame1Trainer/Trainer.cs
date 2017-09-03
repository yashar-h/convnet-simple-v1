using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConvNetSharp.Core;
using ConvNetSharp.Core.Layers.Double;
using ConvNetSharp.Core.Training.Double;
using ConvNetSharp.Volume;
using ConvNetSharp.Volume.Double;
using Newtonsoft.Json;

namespace SimpleGame1Trainer
{
    public static class TrainerConfig
    {
        public static int InputNodesCount => 10;
        public static int HiddenLayerNodesCount => 5;
        public static int OutputNodesCount => 2;

    }

    public class Trainer
    {
        public Net<double> Net;
        public SgdTrainer NetTrainer;
        public void Init()
        {
            // species a 2-layer neural network with one hidden layer of 20 neurons
            Net = new Net<double>();
            NetTrainer = new SgdTrainer(Net) { LearningRate = 0.02, L2Decay = 0.005 };

            // input layer declares size of input. here: 2-D data
            // ConvNetJS works on 3-Dimensional volumes (width, height, depth), but if you're not dealing with images
            // then the first two dimensions (width, height) will always be kept at size 1
            //30 input nodes, one for each 10px of 300px ground to show obstacles as features
            Net.AddLayer(new InputLayer(1, 1, TrainerConfig.InputNodesCount));

            // declare 20 neurons
            Net.AddLayer(new FullyConnLayer(TrainerConfig.HiddenLayerNodesCount));

            // declare a ReLU (rectified linear unit non-linearity)
            Net.AddLayer(new ReluLayer());

            // declare a fully connected layer that will be used by the softmax layer
            Net.AddLayer(new FullyConnLayer(2));

            // declare the linear classifier on top of the previous hidden layer
            Net.AddLayer(new SoftmaxLayer(2));

            var batch = 50;
            for (var j = 0; j < batch; j++)
                Train(GenerateTrainData(), true);

            //var x =
            //    new Volume(new[] {0.0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0}, new Shape(30));

            //var prob = Forward(x);

            //// prob is a Volume. Volumes have a property Weights that stores the raw data, and WeightGradients that stores gradients
            //Console.WriteLine(prob.Get(0) >= 0.5 ? "Jump" : "Stay"); // prints e.g. 0.50101

            //NetTrainer.Train(x, new Volume(new[] { 0.0 }, new Shape(1, 1, 1, 1))); // train the network, specifying that x is class zero

            //var prob2 = Net.Forward(x);
            //Console.WriteLine("probability that x is class 0: " + prob2.Get(0));
            // now prints 0.50374, slightly higher than previous 0.50101: the networks
            // weights have been adjusted by the Trainer to give a higher probability to
            // the class we trained the network with (zero)
        }

        public int i { get; set; }
        public Tuple<double,double> Forward(JumpyReflexData data)
        {
            i++;
            // forward a random data point through the network
            //var x = new Volume(new[] { 0.3, -0.5 }, new Shape(2));
            if(i==2)
            { }

            var prob = Net.Forward(new Volume(data.Features, new Shape(TrainerConfig.InputNodesCount))).Get(0);
            var prob2 = Net.Forward(new Volume(data.Features, new Shape(TrainerConfig.InputNodesCount))).Get(1);
            return new Tuple<double, double>(prob,prob2);
        }

        public void Train(List<JumpyReflexData> frameInput, bool goodJob)
        {
            foreach (var reflex in frameInput)
            {
                var resp = new double[2];
                if (goodJob)
                {
                    if (reflex.Jump)
                    {
                        resp = new[] {1.0, 0.0};
                    }
                    else
                    {
                        resp = new[] {0.0, 1.0};
                    }
                }
                else
                {
                    if (reflex.Jump)
                    {
                        resp = new[] {0.0, 1.0};
                    }
                    else
                    {
                        resp = new[] {1.0, 0.0};
                    }
                }

                if (reflex.AllfeaturesEmpty) continue;
                    NetTrainer.Train(new Volume(reflex.Features, new Shape(TrainerConfig.InputNodesCount)),
                        new Volume(resp, new Shape(1, 1, 2)));
            }
        }


        private List<JumpyReflexData> GenerateTrainData()
        {
            var trainData = new List<JumpyReflexData>();
            Random r1 = new Random(DateTime.Now.Second);
            Random r2 = new Random();
            for (var i = 0; i < 1000; i++)
            {
                var first = r1.Next(0, 9);
                var second = r2.Next(0, 9);
                var temp = new JumpyReflexData {Features = new[] {0.0, 1, 0, 0, 0, 0, 0, 0, 0, 0}, Jump = false};
                temp.Features[first] = 1.0;
                temp.Features[second] = 1.0;
                if ((first < 4 && first > 1) || (second < 4 && second > 1))
                {
                    temp.Jump = true;
                }
                trainData.Add(temp);
            }
            return trainData;
        }
    }

    public class JumpyReflexData
    {
        public double[] Features { get; set; }
        public bool AllfeaturesEmpty { get { return Features.Count(item => item > 0.5) < 2; } }
        public bool Jump { get; set; }
    }
}

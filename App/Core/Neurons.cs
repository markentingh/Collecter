using System;
using System.Collections.Generic;
using System.Linq;

namespace Collector.Neural
{
    #region -- Enum --
    public enum TrainingType
    {
        Epoch,
        MinimumError
    }
    #endregion

    public class Network
    {
        #region -- Properties --
        public double LearnRate { get; set; }
        public double Momentum { get; set; }
        public List<Neuron> InputLayer { get; set; }
        public List<List<Neuron>> HiddenLayer { get; set; }
        public List<Neuron> OutputLayer { get; set; }
        #endregion

        #region -- Globals --
        private static readonly Random Random = new Random();
        #endregion

        #region -- Constructor --
        public Network(int inputSize, int[] hiddenLayers, int outputSize, double learnRate = 0.4, double momentum = 0.9)
        {
            List<Neuron> parentLayer;
            LearnRate = learnRate;
            Momentum = momentum;
            InputLayer = new List<Neuron>();
            HiddenLayer = new List<List<Neuron>>();
            OutputLayer = new List<Neuron>();

            for (var i = 0; i < inputSize; i++)
            {
                InputLayer.Add(new Neuron());
            }
            parentLayer = InputLayer;
            for (var i = 0; i < hiddenLayers.Length; i++)
            {
                HiddenLayer.Add(new List<Neuron>());
                if(i > 0) { parentLayer = HiddenLayer[i - 1]; }
                for(var x = 0; x < hiddenLayers[i]; x++)
                {
                    HiddenLayer[i].Add(new Neuron(parentLayer));
                }
            }
                

            for (var i = 0; i < outputSize; i++)
            {
                OutputLayer.Add(new Neuron(HiddenLayer[HiddenLayer.Count - 1]));
            }
        }
        #endregion

        #region -- Training --
        public void Train(List<DataSet> dataSets, int numEpochs)
        {
            for (var i = 0; i < numEpochs; i++)
            {
                foreach (var dataSet in dataSets)
                {
                    ForwardPropagate(dataSet.Values);
                    BackPropagate(dataSet.Targets);
                }
            }
        }

        public void Train(List<DataSet> dataSets, double minimumError)
        {
            var error = 1.0;
            var numEpochs = 0;

            while (error > minimumError && numEpochs < int.MaxValue)
            {
                var errors = new List<double>();
                foreach (var dataSet in dataSets)
                {
                    ForwardPropagate(dataSet.Values);
                    BackPropagate(dataSet.Targets);
                    errors.Add(CalculateError(dataSet.Targets));
                }
                error = errors.Average();
                numEpochs++;
            }
        }

        private void ForwardPropagate(params double[] inputs)
        {
            var i = 0;
            InputLayer.ForEach(a => a.Value = inputs[i++]);
            HiddenLayer.ForEach(a => a.ForEach(b => b.CalculateValue()));
            OutputLayer.ForEach(a => a.CalculateValue());
        }

        private void BackPropagate(params double[] targets)
        {
            var i = 0;
            OutputLayer.ForEach(a => a.CalculateGradient(targets[i++]));
            for(var x = 0; x < HiddenLayer.Count; x++)
            {
                HiddenLayer[x].ForEach(b => b.CalculateGradient());
                HiddenLayer[x].ForEach(b => b.UpdateWeights(LearnRate, Momentum));
            }
            OutputLayer.ForEach(a => a.UpdateWeights(LearnRate, Momentum));
        }

        public double[] Query(params double[] inputs)
        {
            ForwardPropagate(inputs);
            return OutputLayer.Select(a => a.Value).ToArray();
        }

        private double CalculateError(params double[] targets)
        {
            var i = 0;
            return OutputLayer.Sum(a => Math.Abs(a.CalculateError(targets[i++])));
        }
        #endregion

        #region "-- Memory --"
        public double[][][][] Weights
        {
            get
            {
                //get weights (bias, delta) from all neurons
                var layers = new List<double[][][]>();

                //get neurons from input layer
                var neurons = new List<double[][]>();
                double[][] n;
                for (var x = 0; x < InputLayer.Count; x++)
                {
                    neurons.Add(NeuronWeights(InputLayer[x]));
                }
                layers.Add(neurons.ToArray());

                //get neurons from hidden layer(s)
                for(var x = 0; x < HiddenLayer.Count; x++)
                {
                    neurons = new List<double[][]>();
                    for (var y = 0; y < HiddenLayer[x].Count; y++)
                    {
                        neurons.Add(NeuronWeights(HiddenLayer[x][y]));
                    }
                    layers.Add(neurons.ToArray());
                }

                //get neurons from output layer
                neurons = new List<double[][]>();
                for (var x = 0; x < OutputLayer.Count; x++)
                {
                    neurons.Add(NeuronWeights(OutputLayer[x]));
                }
                layers.Add(neurons.ToArray());

                return layers.ToArray();
            }
            set
            {
                //load weights from previously trained neural network
                if (value.Length < 3) { return; }
                var e = value.Length - 1;


                //load all input neuron weights
                for (var x = 0; x < InputLayer.Count; x++)
                {
                    InputLayer[x].Bias = value[0][x][0][0];
                    InputLayer[x].BiasDelta = value[0][x][0][1];
                    InputLayer[x].Gradient = value[0][x][0][2];
                }

                //load neural weights for all hidden layers
                for (var x = 0; x < HiddenLayer.Count; x++)
                {
                    for(var y = 0; y < HiddenLayer[x].Count; y++)
                    {
                        HiddenLayer[x][y].Bias = value[x + 1][y][0][0];
                        HiddenLayer[x][y].BiasDelta = value[x + 1][y][0][1];
                        HiddenLayer[x][y].Gradient = value[x + 1][y][0][2];
                    }
                }

                //load neural weights for output layer
                for (var x = 0; x < OutputLayer.Count; x++)
                {
                    OutputLayer[x].Bias = value[e][x][0][0];
                    OutputLayer[x].BiasDelta = value[e][x][0][1];
                    OutputLayer[x].Gradient = value[e][x][0][2];
                }
            }
        }

        public double[][] NeuronWeights(Neuron neuron)
        {
            var weights = new List<double[]>();
            Synapse synapse;
            weights.Add(new double[] { CapWeight(neuron.Bias), CapWeight(neuron.BiasDelta), CapWeight(neuron.Gradient) });
            for(var x = 0; x < neuron.InputSynapses.Count; x++)
            {
                synapse = neuron.InputSynapses[x];
                weights.Add(new double[] { CapWeight(synapse.Weight), CapWeight(synapse.WeightDelta)});
            }
            return weights.ToArray();
        }

        public double CapWeight(double weight)
        {
            return double.Parse(weight.ToString("#.##"));
        }
        #endregion

        #region -- Helpers --
        public static double GetRandom()
        {
            return 2 * Random.NextDouble() - 1;
        }
        #endregion
    }

    public class Neuron
    {
        #region -- Properties --
        public List<Synapse> InputSynapses { get; set; }
        public List<Synapse> OutputSynapses { get; set; }
        public double Bias { get; set; }
        public double BiasDelta { get; set; }
        public double Gradient { get; set; }
        public double Value { get; set; }
        #endregion

        #region -- Constructors --
        public Neuron()
        {
            InputSynapses = new List<Synapse>();
            OutputSynapses = new List<Synapse>();
            Bias = Network.GetRandom();
        }

        public Neuron(IEnumerable<Neuron> inputNeurons) : this()
        {
            foreach (var inputNeuron in inputNeurons)
            {
                var synapse = new Synapse(inputNeuron, this);
                inputNeuron.OutputSynapses.Add(synapse);
                InputSynapses.Add(synapse);
            }
        }
        #endregion

        #region -- Values & Weights --
        public virtual double CalculateValue()
        {
            return Value = Sigmoid.Output(InputSynapses.Sum(a => a.Weight * a.InputNeuron.Value) + Bias);
        }

        public double CalculateError(double target)
        {
            return target - Value;
        }

        public double CalculateGradient(double? target = null)
        {
            if (target == null)
                return Gradient = OutputSynapses.Sum(a => a.OutputNeuron.Gradient * a.Weight) * Sigmoid.Derivative(Value);

            return Gradient = CalculateError(target.Value) * Sigmoid.Derivative(Value);
        }

        public void UpdateWeights(double learnRate, double momentum)
        {
            var prevDelta = BiasDelta;
            BiasDelta = learnRate * Gradient;
            Bias += BiasDelta + momentum * prevDelta;

            foreach (var synapse in InputSynapses)
            {
                prevDelta = synapse.WeightDelta;
                synapse.WeightDelta = learnRate * Gradient * synapse.InputNeuron.Value;
                synapse.Weight += synapse.WeightDelta + momentum * prevDelta;
            }
        }
        #endregion
    }

    public class Synapse
    {
        #region -- Properties --
        public Neuron InputNeuron { get; set; }
        public Neuron OutputNeuron { get; set; }
        public double Weight { get; set; }
        public double WeightDelta { get; set; }
        #endregion

        #region -- Constructor --
        public Synapse(Neuron inputNeuron, Neuron outputNeuron)
        {
            InputNeuron = inputNeuron;
            OutputNeuron = outputNeuron;
            Weight = Network.GetRandom();
        }
        #endregion
    }

    public static class Sigmoid
    {
        public static double Output(double x)
        {
            return x < -45.0 ? 0.0 : x > 45.0 ? 1.0 : 1.0 / (1.0 + Math.Exp(-x));
        }

        public static double Derivative(double x)
        {
            return x * (1 - x);
        }
    }

    public class DataSet
    {
        #region -- Properties --
        public double[] Values { get; set; } //input values
        public double[] Targets { get; set; } //expected output
        #endregion

        #region -- Constructor --
        public DataSet(double[] values, double[] targets)
        {
            Values = values;
            Targets = targets;
        }
        #endregion
    }
}
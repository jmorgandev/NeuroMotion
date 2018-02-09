using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace NeuroMotion {

    public delegate float Activation(float value);

    public class NetworkTopology {
        public string neurons;
        public string activations;
    }

    public class Neuron {
        public List<float> weights;

        public Neuron(int inputs, bool random = false) {
            weights = new List<float>();
            for (int i = 0; i < inputs; i++) {
                weights.Add(0.0f);
            }
        }
    }

    public class NeuronLayer {
        public List<Neuron> neurons;
        private Activation activationFunction;

        public NeuronLayer(int neuronCount, int neuronInputs, Activation function) {
            neurons = new List<Neuron>();
            for (int i = 0; i < neuronCount; i++) {
                neurons.Add(new Neuron(neuronInputs + 1));  //+ 1 for bias input
            }
            activationFunction = function;
        }

        public List<float> FeedForward(List<float> inputs) {
            var outputs = new List<float>();
            foreach (var neuron in neurons) {
                float sum = 0.0f;
                foreach (var weight in neuron.weights) {
                    foreach (var input in inputs) {
                        sum += weight * input;
                    }
                }
                outputs.Add(activationFunction(sum));
            }
            return outputs;
        }
    }

    public class NeuralNetwork {
        private List<NeuronLayer> layers;
        private Activation activationFunction;

        private float bias = 1.0f;

        public NeuralNetwork() {
            layers = new List<NeuronLayer>();
            activationFunction = Sigmoid;
        }

        public NeuralNetwork(NetworkTopology topology, List<float> genome) : this() {
            int gene = 0;
            string neurons = topology.neurons;
            string activations = topology.activations;
            if(neurons.Length - 1 != activations.Length) {
                activations = "";
                for(int i = 0; i < neurons.Length; i++) {
                    activations += 's';
                }
            }

            for (int i = 1; i < neurons.Length; i++) {
                Activation activation = CharToActivation(activations[i - 1]);
                int inputCount = (int)char.GetNumericValue(neurons[i - 1]);
                int neuronCount = (int)char.GetNumericValue(neurons[i]);
                layers.Add(new NeuronLayer(neuronCount, inputCount, activation));
            }

            foreach (var layer in layers) {
                foreach (var neuron in layer.neurons) {
                    for (int i = 0; i < neuron.weights.Count; i++) {
                        neuron.weights[i] = genome[gene++];
                    }
                }
            }
        }

        public List<float> FeedForward(List<float> inputs) {
            int inputWeightCount = layers[0].neurons[0].weights.Count - 1;

            if (inputs.Count != inputWeightCount) {
                Debug.LogError("Input size " + inputs.Count + " does not equal network input size of " + inputWeightCount);
                return null;
            }

            List<float> propValues = new List<float>(inputs);

            foreach (var layer in layers) {
                //Add bias input
                propValues.Add(bias);
                propValues = layer.FeedForward(propValues);
            }

            return propValues;
        }

        static float Sigmoid(float x) {
            return 1.0f / (1.0f + (float)Math.Exp(-x));
        }

        static float Tanh(float x) {
            return (float)Math.Tanh(x);
        }

        static float ReLU(float x) {
            return Math.Max(0.0f, x);
        }

        static float Linear(float x) {
            return x;
        }

        static Activation CharToActivation(char c) {
            switch(c) {
                case 's': return Sigmoid;
                case 't': return Tanh;
                case 'r': return ReLU;
                case 'l': return Linear;
                default: return Sigmoid;
            }
        }
    }

}

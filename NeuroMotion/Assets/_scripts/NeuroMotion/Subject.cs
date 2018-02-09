using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
// Each Motion Subject contains it's own neural network, it's own set
// input and output mappings, and it's own fitness function
//
namespace NeuroMotion {

    public abstract class Subject : MonoBehaviour {

        private List<float>     genome;
        protected NeuralNetwork neuralNetwork;
        private NetworkTopology networkTopology;

        protected float fitness  = 0.0f;
        private bool    disabled = false;

        public float Fitness {
            get { return fitness; }
        }
        public bool IsDisabled {
            get { return disabled; }
        }
        public List<float> Genome {
            get { return genome; }
        }
        public NetworkTopology NetworkTopology {
            get { return networkTopology; }
        }

        public void Generate(NetworkTopology topology) {
            int weightCount = 0;
            string neurons = topology.neurons;
            for(int i = 1; i < neurons.Length; i++) {
                int inputCount  = (int)char.GetNumericValue(neurons[i - 1]) + 1; // +1 for bias input
                int neuronCount = (int)char.GetNumericValue(neurons[i]);
                weightCount += inputCount * neuronCount;
            }

            genome = new List<float>(weightCount);
            for(int i = 0; i < weightCount; i++) {
                genome.Add(Random.Range(-1.0f, 1.0f));
            }
            networkTopology = topology;
            neuralNetwork = new NeuralNetwork(networkTopology, genome);
        }

        public void Generate(NetworkTopology topology, List<float> genome) {
            this.genome = genome;
            networkTopology = topology;
            neuralNetwork = new NeuralNetwork(networkTopology, genome);
        }

        public void DisableSubject() {
            disabled = true;
            gameObject.SetActive(false);
        }

        public void EnableSubject() {
            disabled = false;
            gameObject.SetActive(true);
        }

        public abstract void UpdateFitness();
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace NeuroMotion {

    public enum CrossoverMethods {
        SinglePoint,
        TwoPoint,
        Uniform
    }

    public enum MutationMethods {
        Perturb,
        Invert,
        Randomize
    }

    public enum SelectionMethods {
        Best,
        Roulette,
        Random
    }

    public class ExperimentDirector : MonoBehaviour {

        public ExperimentCanvas experimentCanvas = null;

        private bool running = false;

        public int    populationSize     = 10;
        public string neuronTopology    = "452";
        public string activationTopology = "tt";

        private bool sandbox = false;
        private bool sandboxWinner = false;

        private List<Subject> population;

        public float crossoverRate;
        public float mutationRate;
        public float mutationMagnitude;

        public GameObject subjectTemplate;
        private NetworkTopology subjectTopology;

        public CrossoverMethods crossoverMethod;
        public MutationMethods  mutationMethod;
        public SelectionMethods selectionMethod;

        private float totalFitness = 0.0f;
        private Subject sandboxSubject = null;

        private float bestFitness = 0.0f;
        private int currentGeneration = 0;
        private List<float> bestGenome = null;

        private void Awake() {
            population = new List<Subject>();
        }

        private void Start() {
            experimentCanvas = GameObject.Find("ExperimentCanvas").GetComponent<ExperimentCanvas>();
            BeginExperiment();
        }

        private void Update() {

        }

        private void QueueSandbox() {
            sandboxWinner = true;
        }
        private void CancelSandbox() {
            sandboxWinner = false;
            if(sandbox) {
                EndSandbox();
            }
        }

        private void BeginSandbox() {
            sandbox = true;
            sandboxSubject = Instantiate(subjectTemplate, transform).GetComponent<Subject>();
            sandboxSubject.Generate(subjectTopology, bestGenome);
        }

        private void EndSandbox() {
            sandbox = false;
            Destroy(sandboxSubject.gameObject);
            ResumeExperiment();
        }

        private void ResetSandbox() {
            Destroy(sandboxSubject.gameObject);
            BeginSandbox();
        }

        private void PauseExperiment() {
            foreach (var subject in population) {
                subject.gameObject.SetActive(false);
            }
        }

        private void ResumeExperiment() {
            foreach (var subject in population) {
                subject.gameObject.SetActive(true);
            }
        }

        private void LateUpdate() {
            if (running) {
                if (sandbox) {
                    if (sandboxSubject.IsDisabled)
                        ResetSandbox();
                }
                else {
                    int activeSubjects = 0;
                    population.ForEach(item => {
                        if (item.IsDisabled == false)
                            activeSubjects++;
                    });

                    foreach (var subject in population) {
                        if (!subject.IsDisabled) {
                            subject.UpdateFitness();
                            activeSubjects++;
                        }
                    }

                    if (activeSubjects == 0) {
                        Epoch();
                    }
                }
            }
        }

        private void UpdateStats() {
            totalFitness = 0.0f;
            bestFitness = population[0].Fitness;
            bestGenome = population[0].Genome;
            foreach (var subject in population) {
                totalFitness += subject.Fitness;
                if(subject.Fitness > bestFitness) {
                    bestFitness = subject.Fitness;
                    bestGenome = subject.Genome;
                }
            }

            if (experimentCanvas) {
                experimentCanvas.BestFitnessNumber(bestFitness);
                experimentCanvas.AverageFitnessNumber(totalFitness / population.Count);
            }
        }

        private void BeginExperiment() {
            currentGeneration = 0;
            running = true;
            subjectTopology = new NetworkTopology {
                neurons = neuronTopology,
                activations = activationTopology
            };

            for (int i = 0; i < populationSize; i++) {
                Subject newSubject = Instantiate(subjectTemplate, transform).GetComponent<Subject>();
                newSubject.Generate(subjectTopology);
                population.Add(newSubject);
            }
        }

        private void Epoch() {
            UpdateStats();
            var newPopulation = new List<Subject>();

            //This check will generate a new population with an even count, even if the initial population count was odd.
            while (newPopulation.Count < population.Count) {
                Subject parent0 = null;
                Subject parent1 = null;

                SelectParents(out parent0, out parent1);

                List<float> genome0 = null;
                List<float> genome1 = null;

                if (Random.value <= crossoverRate) {
                    genome0 = new List<float>();
                    genome1 = new List<float>();
                    switch (crossoverMethod) {
                        case CrossoverMethods.SinglePoint:
                            GeneticCrossover.SinglePoint(parent0.Genome, parent1.Genome, ref genome0, ref genome1);
                            break;
                        case CrossoverMethods.TwoPoint:
                            GeneticCrossover.TwoPoint(parent0.Genome, parent1.Genome, ref genome0, ref genome1);
                            break;
                        case CrossoverMethods.Uniform:
                            GeneticCrossover.Uniform(parent0.Genome, parent1.Genome, ref genome0, ref genome1);
                            break;
                    }
                }
                else {
                    genome0 = new List<float>(parent0.Genome);
                    genome1 = new List<float>(parent1.Genome);
                }

                if (Random.value <= mutationRate) {
                    switch (mutationMethod) {
                        case MutationMethods.Perturb:
                            GeneticMutation.Perturb(ref genome0, mutationMagnitude);
                            GeneticMutation.Perturb(ref genome1, mutationMagnitude);
                            break;
                        case MutationMethods.Invert:
                            GeneticMutation.Invert(ref genome0, mutationMagnitude);
                            GeneticMutation.Invert(ref genome1, mutationMagnitude);
                            break;
                        case MutationMethods.Randomize:
                            GeneticMutation.Randomize(ref genome0, mutationMagnitude);
                            GeneticMutation.Randomize(ref genome1, mutationMagnitude);
                            break;
                    }
                }

                Subject child0 = Instantiate(subjectTemplate, transform).GetComponent<Subject>();
                child0.Generate(parent0.NetworkTopology, genome0);
                newPopulation.Add(child0);

                Subject child1 = Instantiate(subjectTemplate, transform).GetComponent<Subject>();
                child1.Generate(parent1.NetworkTopology, genome1);
                newPopulation.Add(child1);
            }

            foreach (var subject in population) {
                Destroy(subject.gameObject);
            }
            population.Clear();
            population = newPopulation;

            if (sandboxWinner) {
                sandboxWinner = false;
                PauseExperiment();
                BeginSandbox();
            }

            currentGeneration++;
            if (experimentCanvas) {
                experimentCanvas.GenerationNumber(currentGeneration);
            }
        }

        private void SelectParents(out Subject parent0, out Subject parent1) {
            parent0 = null;
            parent1 = null;

            switch (selectionMethod) {
                case SelectionMethods.Best:
                    parent0 = BestSelection();
                    parent1 = BestSelection(parent0);
                    break;
                case SelectionMethods.Roulette:
                    parent0 = RouletteSelection();
                    parent1 = RouletteSelection(parent0);
                    break;
                case SelectionMethods.Random:
                    parent0 = RandomSelection();
                    parent1 = RandomSelection(parent0);
                    break;
            }
        }

        private Subject BestSelection(Subject exclude = null) {
            Subject result = RandomSelection(exclude);
            foreach (var subject in population) {
                if(subject != exclude && subject.Fitness > result.Fitness) {
                    result = subject;
                }
            }
            return result;
        }
        

        private Subject RouletteSelection(Subject exclude = null) {
            Subject result = RandomSelection(exclude);
            float slice = Random.Range(0.0f, totalFitness);
            float partialSum = 0.0f;
            foreach (var subject in population) {
                partialSum += subject.Fitness;
                if (partialSum >= slice && subject != exclude) {
                    result = subject;
                }
            }
            return result;
        }

        private Subject RandomSelection(Subject exclude = null) {
            //Because who REALLY cares about fitness? (You should, don't use this)
            Subject result = exclude;
            while (result == exclude) result = population[Random.Range(0, population.Count)];
            return result;
        }
    }

}

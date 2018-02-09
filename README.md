# NeuroMotion

## Overview

A small C# library for Unity which allows intelligent agents to develop a solution to an arbitrary user defined task using NeuroEvolution.

Each subjects neural network topology can be specifically tweaked to allow for any neuron-layer size and activation function using topology strings to define the network.
For example, "452" and "tr" defines a neural network with 4 inputs, 2 outputs, and one hidden layer of 5 neurons. The second string then controls the activation functions of each **non-input** layer (In this case tanh for the hidden layer, and ReLU for the output layer).

In the context of NeuroEvolution, the selection, crossover, and mutation methods can be selected from the Experiment Director in order to find a combination of methods that best suit a user implemented agent to be able to converge towards a solution quicker. The crossover rate, mutation rate, and mutation magnitude can also be adjusted from the Experiment Director.

## Usage

To use the library, attach the "ExperimentDirector.cs" script to an empty root-level gameobject and assign the "Subject Template" inspector variable with a prefab containing your subject.

To create a subject, create a MonoBehaviour that inherits from the Subject base class (```NeuroMotion.Subject```) and override the ```UpdateFitness``` function with a fitness evaluation function of your choosing (Which can be absolute or accumulative).

```csharp
public override void UpdateFitness() {
    fitness += Time.deltaTime;
}
```

To have your subject perform actions based upon the outputs of its NeuralNetwork, call the inherited member ```neuralNetwork.FeedForward(...)``` and pass in a ```List<float>``` of inputs. The FeedForward function returns a ```List<float>``` of outputs, which you can then use to control certain elements of your agent.

```csharp
var sensors = new List<float>();
//Populate sensor list
outputs = neuralNetwork.FeedForward(sensors);
//Perform actions with output list
```

You must also define a **failure** condition for your agent. It's probably a good idea to put this in ```Update``` or ```FixedUpdate```. You can trigger the failure of your agent by calling ```DisableSubject()```. Disabling your agent is the only way to signal to the Experiment Director when to generate a new population (When every subject in the current population has failed).

```csharp
if (transform.position.x < 5.0f) {
	DisableSubject();
}
```

Finally set the appropriate values in the Experiment Director. Higher population numbers can potentially converge on a solution quicker (Using roulette selection for higher numbers typically works well). The neuron topology string **must only consist of numbers to define the structure**. The activation topology **must always be one character less than the neuron topology**, since no activation function is assigned to the input layer.

The choice of activation functions are:
-'s' : Sigmoid
-'t' : Tanh
-'r' : ReLU
-'l' : Linear

If you do everything right, you should have your own agent with it's own sensors, outputs, and fitness function ready to attempt to evolve an optimal solution using NeuroEvolution.

## Example/Demo Scene

The Unity Project & Package comes with an example scene showing NeuroEvolution of a balancing agent. This can also serve as a guide to follow when using the library for yourself, and can also just be fun to watch anyway. Try adjusting the values of the Experiment Director (Including the Neural Network topologies) to see which combination makes the balancers converge faster.

## Installation

Download the "NeuroMotion" project and open it in Unity, or import "NeuroMotion.unitypackage".

Last updated 09/02/2018 with Unity 2017.3.0f3
/*
 * AUTHOR: Billy Parmenter
 * DESCRIPTION:
 *      This file contains classes for creating using a Neural Network.
 *          The NeuralNet class is initialized with an array of integers.
 *          each integer is a layer and the value of the integer is the 
 *          number of neurons that should be in the layer. I sugest having 
 *          the last layer have a value of one, multiple outputs has not
 *          been tested.
 */

using System;
using System.Collections.Generic;
using System.Linq;



/// <summary>
/// A basic configurable neural network.
/// </summary>
public class NeuralNet : IComparable<NeuralNet>
{
    private float fitness;
    public List<Layer> layers = new List<Layer>();



    /// <summary>
    /// The NeuralNet constructor
    /// </summary>
    /// <param name="layerSizes">The number of neurons in each layer to be created</param>
    public NeuralNet(int[] layerSizes)
    {
        InitLayers(layerSizes);
        InitWeights();
    }





    /// <summary>
    /// Initializes the layer list using the given layer sizes
    /// </summary>
    /// <param name="layerSizes">The number of neurons in each layer to be created</param>
    private void InitLayers(int[] layerSizes)
    {

        foreach (int layerSize in layerSizes)
        {
            layers.Add(new Layer(layerSize));
        }
    }





    /// <summary>
    /// Deep copy constructor 
    /// </summary>
    /// <param name="copyNetwork">Network to deep copy</param>
    public NeuralNet(NeuralNet copyNetwork)
    {
        layers = new List<Layer>();

        CopyWeights(copyNetwork.layers);
    }





    /// <summary>
    /// Copy weights from list of layers to the current layers
    /// </summary>
    /// <param name="layers">layers to copy</param>
    private void CopyWeights(List<Layer> layers)
    {
        this.layers.Add(new Layer(layers[0].neurons));

        for (int i = 1; i < layers.Count; i++)
        {
            this.layers.Add(new Layer(layers[i].neurons, this.layers[i - 1]));
        }
    }





    /// <summary>
    /// Create weights matrix.
    /// </summary>
    private void InitWeights()
    {
        for (int i = 1; i < layers.Count; i++)
        {
            layers[i].SetWeights(layers[i - 1]);
        }
    }





    /// <summary>
    /// Feed forward this neural network with a given input array
    /// </summary>
    /// <param name="inputs">Inputs to network</param>
    /// <returns>
    /// float[] of output values
    /// </returns>
    public float[] FeedForward(float[] inputs)
    {
        if (inputs.Length != layers[0].neurons.Count)
        {
            throw new Exception("Invalid number of inputs given to FeedForward!");
        }

        for (int i = 0; i < inputs.Length; i++)
        {
            layers[0].neurons[i].value = inputs[i];
        }

        foreach (Layer layer in layers.Skip(1))
        {
            foreach (Neuron neuron in layer.neurons)
            {
                neuron.CalculateValue();
            }
        }

        return layers[layers.Count - 1].GetValues();
    }





    /// <summary>
    /// Mutate neural network weights
    /// </summary>
    public void AsexualMutation()
    {
        foreach (Layer layer in layers)
        {
            foreach (Neuron neuron in layer.neurons)
            {
                neuron.FullAsexualMutation();
            }
        }
    }





    /// <summary>
    /// Mutates a single value in the NeuralNet
    /// </summary>
    /// <returns>The current NeuralNet</returns>
    public NeuralNet SingleValueMutation()
    {
        int randomLayerIndex = UnityEngine.Random.Range(0, layers.Count);
        List<Neuron> randomLayerNeurons = layers[randomLayerIndex].neurons;

        int randomNeuronIndex = UnityEngine.Random.Range(0, randomLayerNeurons.Count);
        Neuron randomNeuron = randomLayerNeurons[randomNeuronIndex];

        randomNeuron.SingleValueMutation();

        return this;
    }





    /// <summary>
    /// Increases the fitness value 
    /// </summary>
    /// <param name="fit">The ammount to add to the fitness</param>
    public void AddFitness(float fit)
    {
        fitness += fit;
    }





    /// <summary>
    /// Sets the fitness value
    /// </summary>
    /// <param name="fit">The value to set</param>
    public void SetFitness(float fit)
    {
        fitness = fit;
    }





    /// <summary>
    /// Returns the fitnes value
    /// </summary>
    /// <returns>The fitness value</returns>
    public float GetFitness()
    {
        return fitness;
    }





    /// <summary>
    /// Compare two neural networks and sort based on fitness
    /// </summary>
    /// <param name="other">Network to be compared to</param>
    /// <returns></returns>
    public int CompareTo(NeuralNet other)
    {
        if (other == null) return 1;

        if (fitness > other.fitness)
            return 1;
        else if (fitness < other.fitness)
            return -1;
        else
            return 0;
    }





    /// <summary>
    /// Gets the wights and values from each neuron and synaps in the neural net and creates a list of values
    /// </summary>
    /// <returns>The thought process</returns>
    public List<float> GetThoughtProcess()
    {
        List<float> thoughtProcess = new List<float>();

        foreach (Layer layer in layers)
        {
            foreach (Neuron neuron in layer.neurons)
            {
                thoughtProcess.Add(neuron.value);

                foreach (Synapse synapse in neuron.synapses)
                {
                    thoughtProcess.Add(synapse.weight);
                }
            }
        }

        return thoughtProcess;
    }





    /// <summary>
    /// Sets the thought weights and values of the NeuralNet to that of the given thought process
    /// </summary>
    /// <param name="thoughtProcess">The weights and values to be set</param>
    public void SetThoughtProcess(List<float> thoughtProcess)
    {
        int i = 0;

        foreach (Layer layer in layers)
        {
            foreach (Neuron neuron in layer.neurons)
            {
                neuron.value = thoughtProcess[i];
                i++;

                foreach (Synapse synapse in neuron.synapses)
                {
                    synapse.weight = thoughtProcess[i];
                    i++;
                }
            }
        }
    }
}






/// <summary>
/// The connection between neurons
/// </summary>
public class Synapse
{
    public Neuron neuron;
    public float weight;

    public Synapse(Neuron neuron, float weight)
    {
        this.neuron = neuron;
        this.weight = weight;
    }
}







/// <summary>
/// A single neuron in a layer that contains synapses
/// </summary>
public class Neuron
{
    public List<Synapse> synapses = new List<Synapse>();
    public float value;



    /// <summary>
    /// Default constructor
    /// </summary>
    public Neuron()
    {
    }





    /// <summary>
    /// Constructor that sets the value of the neuron
    /// </summary>
    /// <param name="value">the value to be set</param>
    public Neuron(float value)
    {
        this.value = value;
    }





    /// <summary>
    /// Constructor that sets the value and synapses of the neuron
    /// </summary>
    /// <param name="neuron">The neuron to get the value and wieghts from</param>
    /// <param name="previousLayer">The layer to copy the synapses from</param>
    public Neuron(Neuron neuron, Layer previousLayer)
    {
        synapses = new List<Synapse>();

        value = neuron.value;

        List<Neuron> neurons = previousLayer.neurons;

        for (int i = 0; i < neurons.Count; i++)
        {
            synapses.Add(new Synapse(neurons[i], neuron.synapses[i].weight));
        }
    }





    /// <summary>
    /// Sets the weights based on a given list of neurons
    /// </summary>
    /// <param name="neurons">The list of neurons to copy the weights from</param>
    public void SetWeights(List<Neuron> neurons)
    {
        foreach (Neuron neuron in neurons)
        {
            float randomWeight = UnityEngine.Random.Range(-0.5f, 0.5f);
            synapses.Add(new Synapse(neuron, randomWeight));
        }
    }





    /// <summary>
    /// Mutates a single wieght in the synaps list
    /// </summary>
    public void SingleValueMutation()
    {
        List<Synapse> newSynapses = new List<Synapse>();
        int synapsToMutate = UnityEngine.Random.Range(0, synapses.Count);

        foreach (Synapse synaps in synapses)
        {
            float weight = synaps.weight;

            if (synapses.IndexOf(synaps) == synapsToMutate)
            {
                int increaseOrDecrease = UnityEngine.Random.Range(0, 2);

                if (increaseOrDecrease == 1)
                {
                    //randomly increase by 0% to 50%
                    float factor = UnityEngine.Random.Range(0f, 0.5f) + 1f;
                    weight *= factor;
                }
                else
                {
                    //randomly decrease by 0% to 50%
                    float factor = UnityEngine.Random.Range(0f, 0.5f);
                    weight *= factor;
                }
            }

            newSynapses.Add(new Synapse(synaps.neuron, weight));
        }

        synapses = newSynapses;

    }





    /// <summary>
    /// Randomly mutates some weights in the list of synapses
    /// </summary>
    public void FullAsexualMutation()
    {
        List<Synapse> newSynapses = new List<Synapse>();

        foreach (Synapse synaps in synapses)
        {
            float weight = synaps.weight;

            //mutate weight value 
            float randomNumber = UnityEngine.Random.Range(0f, 100f);

            if (randomNumber <= 2)
            {
                //flip sign of weight
                weight *= -1f;
            }
            else if (randomNumber <= 4)
            {
                //pick random weight between -1 and 1
                weight = UnityEngine.Random.Range(-0.5f, 0.5f);
            }
            else if (randomNumber <= 6)
            { //if 3
              //randomly increase by 0% to 100%
                float factor = UnityEngine.Random.Range(0f, 1f) + 1f;
                weight *= factor;
            }
            else if (randomNumber <= 8)
            { //if 4
              //randomly decrease by 0% to 100%
                float factor = UnityEngine.Random.Range(0f, 1f);
                weight *= factor;
            }

            newSynapses.Add(new Synapse(synaps.neuron, weight));
        }

        synapses = newSynapses;
    }





    /// <summary>
    /// Calculates the neurons value
    /// </summary>
    internal void CalculateValue()
    {
        value = 0;

        foreach (Synapse synaps in synapses)
        {
            value += synaps.weight * synaps.neuron.value;
        }
    }
}








/// <summary>
/// A layer of the NeuralNet that contains multiple neurons
/// </summary>
public class Layer
{
    public List<Neuron> neurons = new List<Neuron>();



    /// <summary>
    /// Constructor to intialize the layer with random values
    /// </summary>
    /// <param name="numberOfNeurons"></param>
    public Layer(int numberOfNeurons)
    {
        for (int i = 0; i < numberOfNeurons; i++)
        {
            neurons.Add(new Neuron());
        }
    }





    /// <summary>
    /// Constructor to initialize the layer with given values
    /// </summary>
    /// <param name="neurons">The values to copy</param>
    public Layer(List<Neuron> neurons)
    {
        this.neurons = new List<Neuron>();

        foreach (Neuron neuron in neurons)
        {
            this.neurons.Add(new Neuron(neuron.value));
        }
    }





    /// <summary>
    /// Constructor to initialize the layer with given values and synaps weights
    /// </summary>
    /// <param name="neurons">The values to copy</param>
    /// <param name="previousLayer">The weights to copy</param>
    public Layer(List<Neuron> neurons, Layer previousLayer)
    {
        this.neurons = new List<Neuron>();

        foreach (Neuron neuron in neurons)
        {
            this.neurons.Add(new Neuron(neuron, previousLayer));
        }

    }





    /// <summary>
    /// Sets the existing weights to new ones
    /// </summary>
    /// <param name="layer">Layer containing the weights to copy from</param>
    public void SetWeights(Layer layer)
    {
        List<Neuron> neurons = layer.neurons;

        foreach (Neuron neuron in this.neurons)
        {
            neuron.SetWeights(neurons);
        }
    }





    /// <summary>
    /// Get all the values of each neuron
    /// </summary>
    /// <returns>Float array of the neuron values</returns>
    public float[] GetValues()
    {
        List<float> values = new List<float>();

        foreach (Neuron neuron in neurons)
        {
            values.Add(neuron.value);
        }

        return values.ToArray();
    }
}
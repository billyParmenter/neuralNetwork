/*
 * AUTHOR: Billy Parmenter
 */

using System;
using System.Collections.Generic;
using System.Linq;



/// <summary>
/// Manages multiple neural netowrks when dealing with multiple generations
/// </summary>
public class NetManager
{
    public List<NeuralNet> nets;
    private List<NeuralNet> tmpNets;

    private readonly int populationSize = 20;
    private readonly int[] layerSizes;

    private int netIndex = 0;
    private int tmpNetsLength = 0;

    private float keepTopPercent = 0.1f;
    private float mutatePercent = 0.1f;
    private float breedPercent = 0.2f;



    /// <summary>
    /// Constructor to set the layer sizes, population size and initialize the neural networks
    /// </summary>
    /// <param name="layerSizes">The layers to be used in the neural network</param>
    /// <param name="populationSize">The number of neural networks to be used</param>
    public NetManager(int[] layerSizes, int populationSize)
    {
        //population must be even, just setting it to 20 incase it's not
        if (populationSize % 2 == 0)
        {
            this.populationSize = populationSize;
        }

        if (layerSizes.Length != 0 && layerSizes.Contains(0) == false)
        {
            this.layerSizes = layerSizes;
        }
        else
        {
            throw new ArgumentException("ERROR - layerSizes can not have a length less than 3 and cannot contain 0s.");
        }

        nets = new List<NeuralNet>();

        for (int i = 0; i < this.populationSize; i++)
        {
            NeuralNet net = new NeuralNet(layerSizes);
            nets.Add(net);
        }
    }





    /// <summary>
    /// Sets the percentages of neural networks when creating the next generation. The the sum of the percentages is 
    ///     less than 1 then the remaining neural nets in the next generation will be created with random values
    /// </summary>
    /// <param name="keepTopPercent">The amount of the best(based on fitness) neural nets to keep in the next generation</param>
    /// <param name="mutatePercent">The amount of the best neural nets to mutate into the next generation</param>
    /// <param name="breedPercent">The amount of the best neural nets to breed with eachother for ther next genreation</param>
    public void SetPercentages(float keepTopPercent, float mutatePercent, float breedPercent)
    {
        float total = keepTopPercent + mutatePercent + breedPercent;

        if (total > 1)
        {
            throw new Exception("Error - The sum of the precentages cannot be greater than 1");
        }
        else if (keepTopPercent > 1 || mutatePercent > 1 || breedPercent > 1)
        {
            throw new Exception("Error - None of the percentages can be over 1");
        }
        else
        {
            this.keepTopPercent = keepTopPercent;
            this.mutatePercent = mutatePercent;
            this.breedPercent = breedPercent;
        }



    }





    /// <summary>
    /// Creates the next generation of neural nets
    /// </summary>
    public void GenerateNextGeneration()
    {
        nets.Sort();
        nets.Reverse();

        tmpNets = new List<NeuralNet>();
        netIndex = 0;

        AddTopScoringNets();
        AddMutatedTopScoringNets();
        AddBredTopScoringNets();
        AddRandomNets();

        ResetFitness();

        nets = tmpNets;
    }





    /// <summary>
    /// Resets the fitness of each neural net to 0
    /// </summary>
    private void ResetFitness()
    {
        foreach (NeuralNet net in tmpNets)
        {
            net.SetFitness(0f);
        }
    }





    /// <summary>
    /// Adds the random neural nets to the next generation
    /// </summary>
    private void AddRandomNets()
    {
        int stopIndex = nets.Count;

        for (; netIndex < stopIndex; netIndex++)
        {
            tmpNets.Add(new NeuralNet(layerSizes));
        }
    }





    /// <summary>
    /// Adds the bred neural nets to the next generation
    /// </summary>
    private void AddBredTopScoringNets()
    {
        int stopIndex = (int)(populationSize * breedPercent) + netIndex;
        int breedingIndex = netIndex;

        for (; netIndex < stopIndex;)
        {
            int randomBreedingPartner = UnityEngine.Random.Range(0, tmpNetsLength);
            //List<NeuralNet> breadNets = CrossoverBreeding(tmpNets[breedingIndex % tmpNetsLength], tmpNets[randomBreedingPartner]);
            //Possible issue with not returning like above
            CrossoverBreeding(tmpNets[breedingIndex % tmpNetsLength], tmpNets[randomBreedingPartner]);

            tmpNets.Add(new NeuralNet(tmpNets[breedingIndex % tmpNetsLength]));

            breedingIndex++;
            netIndex++;

            if (netIndex < stopIndex)
            {
                tmpNets.Add(new NeuralNet(tmpNets[randomBreedingPartner]));
                netIndex++;
            }
        }
    }





    /// <summary>
    /// Adds the mutated neural nets to the next generation
    /// </summary>
    private void AddMutatedTopScoringNets()
    {
        int stopIndex = (int)(populationSize * mutatePercent) + netIndex;

        for (; netIndex < stopIndex; netIndex++)
        {
            tmpNets.Add(new NeuralNet(nets[netIndex % tmpNetsLength].SingleValueMutation()));
        }
    }





    /// <summary>
    /// Adds the top neural nets to the next generation
    /// </summary>
    private void AddTopScoringNets()
    {
        int stopIndex = (int)(populationSize * keepTopPercent) + netIndex;

        for (; netIndex < stopIndex; netIndex++)
        {
            tmpNets.Add(new NeuralNet(nets[netIndex]));
        }

        tmpNetsLength = tmpNets.Count;
    }





    /// <summary>
    /// Breeds two neural nets and adds two neural nets to the next generation
    /// </summary>
    /// <param name="net1"></param>
    /// <param name="net2"></param>
    private void CrossoverBreeding(NeuralNet net1, NeuralNet net2)
    {
        List<float> net1ThoughtProcess = net1.GetThoughtProcess();
        List<float> net2ThoughtProcess = net2.GetThoughtProcess();
        List<float> newNet1ThoughtProcess = new List<float>();
        List<float> newNet2ThoughtProcess = new List<float>();

        int thoughtProcessLength = net1ThoughtProcess.Count;
        int startingIndex = UnityEngine.Random.Range(0, thoughtProcessLength);
        int stopIndex = UnityEngine.Random.Range(startingIndex, thoughtProcessLength);

        for (int i = 0; i < startingIndex; i++)
        {
            newNet1ThoughtProcess.Add(net1ThoughtProcess[i]);
            newNet2ThoughtProcess.Add(net2ThoughtProcess[i]);
        }

        for (int i = startingIndex; i < stopIndex; i++)
        {
            newNet1ThoughtProcess.Add(net2ThoughtProcess[i]);
            newNet2ThoughtProcess.Add(net1ThoughtProcess[i]);
        }

        for (int i = stopIndex; i < thoughtProcessLength; i++)
        {
            newNet1ThoughtProcess.Add(net1ThoughtProcess[i]);
            newNet2ThoughtProcess.Add(net2ThoughtProcess[i]);
        }

        net1.SetThoughtProcess(newNet1ThoughtProcess);
        net2.SetThoughtProcess(newNet2ThoughtProcess);
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneticAlgorithm : MonoBehaviour
{
    [Range(0, 100)]
    public int mutationRate;

    public int totalMutations = 0;
    private int numGenes = 5;

    public int[] Begin(Animal p1, Animal p2)
    {
        int[] p1Chromosome = CreateChromosome(p1);
        int[] p2Chromosome = CreateChromosome(p2);
        int[] offspringChromosome = Crossover(p1Chromosome, p2Chromosome);
        offspringChromosome = Mutation(offspringChromosome);
        return offspringChromosome;
    }

    private int[] CreateChromosome(Animal animal)
    {
        // create an array of variables that will be mutated (string of genes into a chromosome)
        int[] genes = new int[numGenes];
        genes[0] = animal.speed;
        genes[1] = animal.strength;
        genes[2] = animal.vision;
        genes[3] = animal.energy;
        genes[4] = animal.puerperal;
        return genes;
    }

    private int[] Crossover(int[] p1, int[] p2)
    {
        // Specify a crossover point randomly and create offspring chromosome
        int[] chromosome = new int[numGenes];
        int cp = Random.Range(0, numGenes);
        for (int i = 0; i < numGenes; i++)
        {
            if (i < cp)
                chromosome[i] = p1[i];  // take gene from p1
            else if (i >= cp)
                chromosome[i] = p2[i];  // take gene from p2
        }
        return chromosome;
    }

    private int[] Mutation(int[] chromosome)
    {
        // Mutate genes of chromosome based on mutation chance specified
        int rand = Random.Range(0, 101);
        if (mutationRate - rand >= 0)
        {
            totalMutations++;
            int mutations = Random.Range(1, 5);     // amount of genes to mutate (can mutate the same one multiple times)
            for (int i = 0; i < mutations; i++)
            {
                int gene = Random.Range(0, numGenes);      // gene to mutate
                bool positiveMutation;
                if (Random.value >= 0.5f)
                    positiveMutation = true;
                else
                    positiveMutation = false;

                if (positiveMutation)
                    chromosome[gene] = chromosome[gene] + 1;
                else if (!positiveMutation && chromosome[gene] > 1) 
                    chromosome[gene] = chromosome[gene] - 1;
            }
        }
        return chromosome;
    }
}

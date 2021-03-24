
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneticAlgorithm : MonoBehaviour
{
    [Range(0,100)]
    public int mutationRate;

    public int totalMutations = 0;

    public int[] Begin(Chicken offspring, Chicken p1, Chicken p2)
    {
        int[] p1Chromosome = CreateChromosome(p1);
        int[] p2Chromosome = CreateChromosome(p2);
        int[] offspringChromosome = Crossover(p1Chromosome, p2Chromosome);
        offspringChromosome = Mutation(offspringChromosome);
        return offspringChromosome;
    }

    private int[] CreateChromosome(Chicken chicken)
    {
        // create an array of variables that will be mutated (string of genes into a chromosome)
        int[] genes = new int[4];
        genes[0] = chicken.speed;
        genes[1] = chicken.health;
        genes[2] = chicken.vision;
        genes[3] = chicken.energy;
        return genes;
    }

    private int[] Crossover(int[] p1, int[] p2)
    {
        // Specify a crossover point randomly and create offspring chromosome
        int[] chromosome = new int[4];
        int cp = UnityEngine.Random.Range(0, 3);
        for (int i = 0; i < chromosome.Length; i++)
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
        int rand = Random.Range(0, 100);
        if (mutationRate - rand >= 0) 
        {
            totalMutations++;
            int mutations = Random.Range(1, 4);     // amount of genes to mutate (can mutate the same one multiple times)
            for (int i = 0; i < mutations; i++)
            {
                int gene = Random.Range(0, 4);      // gene to mutate
                bool positiveMutation;
                if (Random.value >= 0.5f)
                    positiveMutation = true;
                else
                    positiveMutation = false;

                if (positiveMutation)
                    chromosome[gene] = chromosome[gene] + 1;
                else
                    chromosome[gene] = chromosome[gene] - 1;
            }
        }
        return chromosome;
    }
}

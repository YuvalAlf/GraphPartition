﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Utils.DataStructures;
using Utils.ExtensionMethods;

namespace Optimizations.GeneticAlgorithm
{
    public sealed class Genetic<SolutionInstance> : OptimizationSolver<SolutionInstance, GeneticSettings>
        where SolutionInstance : IGeneticSolver<SolutionInstance>
    {
        public override IEnumerable<SolutionInstance> Run(Func<Random, SolutionInstance> genRandom, GeneticSettings settings,
            object runPauseLock, ConcurrentSignal killTaskSignal, ConcurrentSignal taskKilledSignal, StrongBox<bool> finished, Random rnd)
        {
            var population = new SolutionInstance[settings.Population];
            var newPopulation = population.Copy();

            for (int i = 0; i < population.Length; i++)
                population[i] = genRandom(rnd);
            Array.Sort(population, this);

            double lastNegativePrice = double.MaxValue;

            finished.Value = false;
            while (!killTaskSignal.TryProcessSignal())
                lock(runPauseLock)
                {
                    double newNegativePrice = population[0].NegativePrice;
                    if (newNegativePrice < lastNegativePrice)
                    {
                        yield return population[0];
                        lastNegativePrice = newNegativePrice;
                    }
                    
                    int index = 0;
                    for (int i = 0; i < settings.ElitismAmount && index < newPopulation.Length; i++)
                        newPopulation[index] = population[index++];
                    for (int i = 0; i < settings.NewGenesAmount && index < newPopulation.Length; i++)
                        newPopulation[index++] = genRandom(rnd);
                    while (index < newPopulation.Length)
                    {
                        var mom = population.ChooseRandomly(rnd);
                        var dad = population.ChooseRandomly(rnd);
                        var son = mom.Mate(dad, rnd);
                        if (rnd.NextDouble() <= settings.MutationRate)
                            son = son.Mutate(rnd);
                        newPopulation[index++] = son;
                    }

                    var temp = population;
                    population = newPopulation;
                    newPopulation = temp;

                    Array.Sort(population, this);
                }
            taskKilledSignal.Signal();
        }
    }
}

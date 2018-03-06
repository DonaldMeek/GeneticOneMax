/*

Donald Meek
2-22-2016

This project creates a population of random bit strings where the user defines the population size and the length of the bit strings. 
The project then gets two children by tournament selection from two sets of two parents. Next, the algorithm applies uniform crossover
with a probability of .6, then mutates each individual bit in the children with probability of 1/n. The most fit parent is added to the 
child population, which then replaces the parent population. The program finds and displays statistics on the best, worst, and average
fitness for the generation. If an optimal bit string (with all 1s) is found, or if average fitness does not improve, the program ends. 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticOneMax
{
    class OneMax
    {
        int[,] parentPopulation;
        int[,] childPopulation;
        int[][] parents; // 2 parents selected from tournament selection for crossover
        int[][] children; // 2 children selected from crossover
        int[] randomBit1;
        int[] randomBit2;
        int[] tempArray;
        int childIndex;
        int genNum; // Counts the number of generations. Initialized at 1 in the constructor
        int bitLength; // The length of a bit string
        int populationSize; // The size of populations
        Random random = new Random();

        // Fitness for current generation
        double bestFitness;
        double worstFitness;
        double averageFitness;

        double pastAverageFitness; // Used to terminate the program if average fitness doesn't improve. Initialized at -1 for a sentinel 
        Boolean endGen; // True when a new generation just started and hasn't been changed yet

        public OneMax()
        {
            getUserInput(); // let the user set populationSize and bitLength
            setParentPopulation();
            setChildPopulation();
            parents = new int[2][];
            children = new int[2][];
            randomBit1 = new int[bitLength];
            randomBit2 = new int[bitLength];
            endGen = false;
            childIndex = 0;
            genNum = 1;
            averageFitness = 0;
            bestFitness = 0;
            worstFitness = 0;
            pastAverageFitness = -1; // Set a sentinel to mark the first generation
            for (int i = 0; i < 2; i++) parents[i] = new int[bitLength];
            for (int i = 0; i < 2; i++) children[i] = new int[bitLength];
        }

        private void getUserInput()
        {
            bool inputAccepted = true;

            do
            {
                try
                {
                    Console.WriteLine("Enter the population size: ");
                    populationSize = Int32.Parse(Console.ReadLine());
                    inputAccepted = true;
                }
                catch (FormatException e)
                {
                    Console.WriteLine(e.Message);
                    inputAccepted = false;
                }
            } while (inputAccepted == false);
            do
            {
                try
                {
                    Console.WriteLine("Enter the length of the bit strings: ");
                    bitLength = Int32.Parse(Console.ReadLine());
                    inputAccepted = true;
                }
                catch (FormatException f)
                {
                    Console.WriteLine(f.Message);
                    inputAccepted = false;
                }
            } while (inputAccepted == false);
            Console.WriteLine(); // New line
        }

        // Allocate memory for parentPopulation and fill it with random bit strings
        private void setParentPopulation()
        {
            parentPopulation = new int[populationSize, bitLength];
            for (int i = 0; i < populationSize; i++)
            {
                for (int j = 0; j < bitLength; j++)
                {
                    parentPopulation[i, j] = random.Next(2); // Fill with random numbers that are strictly less than 2
                }
            }
            checkOptimum(parentPopulation);
        }

        // The child population is defined with 2 in each value for a sentinel
        private void setChildPopulation()
        {
            childPopulation = new int[populationSize, bitLength];
            for (int i = 0; i < populationSize; i++)
            {
                for (int j = 0; j < bitLength; j++)
                {
                    childPopulation[i, j] = 2; // Fill with 2
                }
            }
        }

        public Boolean getEndGen()
        {
            return endGen;
        }

        public void createGeneration()
        {
            do
            {
                tournament();
                uniformCrossover();
                mutate(); // Checks for global optimum (all 1s) before and after each mutation. Since the initial random population is checked, each bit string will be checked for the global optimum 
                endGen = addChildren(children[0]); // checks for the end of a generation
                endGen = addChildren(children[1]);
            } while (!endGen);
            setStats(); // Finds the best, worst, and average fitness.
            displayStats();
            checkGen();
            genNum++;

            // Place the child population in the parentPopulation 2D array
            for (int i = 0; i < populationSize; i++) for (int j = 0; j < bitLength; j++) parentPopulation[i, j] = childPopulation[i, j];
            for (int i = 0; i < populationSize; i++) for (int j = 0; j < bitLength; j++) childPopulation[i, j] = 2;
        }

        private void checkOptimum(int[,] arr)
        {
            int oneCount = 0;
            for (int i = 0; i < populationSize; i++)
            {
                for (int j = 0; j < bitLength; j++) if (arr[i, j] == 1) oneCount++;
                if (oneCount == bitLength)
                {
                    Console.WriteLine("The global optimum was found");
                    Environment.Exit(0);
                }
                oneCount = 0;
            }
        }
        private void checkOptimum(int[] array)
        {
            int oneCount = 0;
            for (int j = 0; j < bitLength; j++) if (array[j] == 1) oneCount++;
            if (oneCount == bitLength)
            {
                Console.WriteLine("The global optimum was found");
                Environment.Exit(0);
            }
            oneCount = 0;
        }

        // This function takes two random bit strings and compares fitness, selecting bit strings from the entire population so
        // duplicates may be compared with each other. This process is repeated twice so that two sets of two bit strings produce one set of two parents. 
        private void tournament()
        {
            tournament(ref parents[0]);
            tournament(ref parents[1]);
        }
        private void tournament(ref int[] champ)
        {
            tempArray = new int[bitLength];
            setRandomBit(ref this.randomBit1);
            setRandomBit(ref this.randomBit2);
            tempArray = compareFitness(ref this.randomBit1, ref this.randomBit2);
            for (int i = 0; i < bitLength; i++) champ[i] = tempArray[i];
            tempArray = null;
        }
        private void setRandomBit(ref int[] b)
        {
            int randomParent = random.Next(populationSize);
            for (int i = 0; i < bitLength; i++) b[i] = parentPopulation[randomParent, i];
        }
        private int[] compareFitness(ref int[] b1, ref int[] b2)
        {
            int bitCount1 = 0;
            int bitCount2 = 0;

            for (int i = 0; i < bitLength; i++)
            {
                if (b1[i] == 1) bitCount1++;
                if (b2[i] == 1) bitCount2++;
            }
            if (bitCount1 >= bitCount2) return b1;
            else return b2;
        }

        // Applies uniform crossover with probability of .6 and copies parents with probability of .4
        private void uniformCrossover()
        {
            // Generate a random integer between 1 and 100 and use it to determine if we will perform crossover
            int doCrossover = random.Next(101);
            int chooseParent = random.Next(2); // chooses which parent for children[0] to take a bit from       
            if (doCrossover <= 60)
            {
                for (int i = 0; i < bitLength; i++)
                {
                    children[0][i] = parents[chooseParent][i];
                    children[1][i] = parents[1 - chooseParent][i];
                    chooseParent = random.Next(2);
                }
            }
            else if (doCrossover > 60)
            {
                for (int i = 0; i < 2; i++) for (int j = 0; j < bitLength; j++) children[i][j] = parents[i][j];
            }
        }

        // This function checks each child for the global optimum (a bit string of all 1s) and ends the program if it is found.
        // This function mutates each individual bit for both children with probability of 1/n by flipping the zeros to ones and ones to zeros
        // The function checks for global optimum after each mutation
        private void mutate()
        {
            for (int i = 0; i < 2; i++)
            {
                checkOptimum(children[i]);
                mutate(ref children[i]);
                checkOptimum(children[i]);
            }
        }
        private void mutate(ref int[] child)
        {
            double doMutation = random.NextDouble();
            for (int i = 0; i < bitLength; i++)
            {
                if (doMutation <= (1 / (double)bitLength))
                {
                    child[i] = 1 - child[i];
                }
                doMutation = random.NextDouble();
            }
        }

        // Adds a child to the child population. The last bit string in the child population is the parent with the highest fitness
        private bool addChildren(int[] child)
        {
            if (childPopulation[(populationSize - 2), 0] != 2)
            {
                childIndex = 0;
                endGen = true;
                return endGen;
            }
 
            for (int i = 0; i < bitLength; i++) childPopulation[childIndex, i] = child[i];
            childIndex++;
            return false;
        }

        // The parent with the highest fitness stays in the next generation and is added to the child population.
        // This function finds the parent with the highest fitness and also finds statistics on the best, worst, and average
        // fitness for the population
        private void setStats()
        {
            double highestParentFitnessIndex = 0;
            double highestParentFitness = 0;
            double parentFitnessCount = 0;
            double childFitnessCount = 0;
            double populationFitnessTotal = 0;

            // Reset the stats from the previous generation if there was a previous generation
            if (averageFitness != -1)
            {
                pastAverageFitness = averageFitness;
                bestFitness = 0;
                worstFitness = 0;
                averageFitness = 0;
            }

            // Find the highest fitness in the parent population and the stats for the child population
            for (int j = 0; j < bitLength; j++) if (childPopulation[0, j] == 1) worstFitness++; // The worst fitness starts with the first child, and the remaining children are compared to find it.
            for (int i = 0; i < populationSize; i++)
            {
                for (int j = 0; j < bitLength; j++)
                {
                    if (parentPopulation[i, j] == 1) parentFitnessCount++;
                    if ( (i != (populationSize - 1) ) && (childPopulation[i, j] == 1) )
                    {
                        childFitnessCount++;
                        populationFitnessTotal++;
                    }
                }
                if (parentFitnessCount > highestParentFitness)
                {
                    highestParentFitnessIndex = i;
                    highestParentFitness = parentFitnessCount;
                }
                if ( (i != (populationSize - 1) ) && (childFitnessCount < worstFitness) ) worstFitness = childFitnessCount;
                if ( (i != (populationSize - 1) ) && (childFitnessCount > bestFitness) ) bestFitness = childFitnessCount;
                childFitnessCount = 0;
                parentFitnessCount = 0; 
            }

            // Add the parent with the highest fitness to the child population
            for (int i = 0; i < bitLength; i++) childPopulation[populationSize - 1, i] = parentPopulation[(int)highestParentFitnessIndex, i];
            if (highestParentFitness > bestFitness) bestFitness = highestParentFitness;

            // Get the average
            populationFitnessTotal += highestParentFitness;
            averageFitness = populationFitnessTotal / (double)populationSize;
        }

        // Checks if the average fitness did not improve since the last generation. The program ends in this case
        private void checkGen()
        {        
            if ((pastAverageFitness != -1) && (averageFitness <= pastAverageFitness) )
            {
                Console.WriteLine("Average fitness did not improve compared to the previous generation.");
                Environment.Exit(0);
            }
        }

        private void displayStats()
        {
            Console.WriteLine("Generation Number: " + genNum);
            Console.WriteLine("Average Fitness: " + averageFitness);
            Console.WriteLine("Best Fitness: " + bestFitness);
            Console.WriteLine("Worst Fitness: " + worstFitness + "\n");
        }

        public void test()
        {
            for (int j = 0; j < 2; j++)
            {
                Console.WriteLine("\n" + "Parent " + j + ": ");
                for (int i = 0; i < bitLength; i++) Console.Write(parents[j][i]);
                Console.WriteLine("\n" + "Child " + j + ": ");
                for (int i = 0; i < bitLength; i++) Console.Write(children[j][i]);
            }
            Console.WriteLine("\n" + "Mutations: ");

            // Mutate
            mutate();
            Console.WriteLine("\n" + "Child: ");
            for (int j = 0; j < 2; j++)
            {
                for (int i = 0; i < bitLength; i++)
                {
                    Console.Write(children[j][i]);
                }
                Console.WriteLine();
            }
        }
        static void Main(string[] args)
        {
            OneMax oneMax = new OneMax();

            // Loop termination happened when either the global optimum (all 1s) was found or when the average fitness failed to improve
            while (true)
            {
                oneMax.createGeneration();
            }
        }
    }
}

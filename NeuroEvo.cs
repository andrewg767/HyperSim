using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetworks
{
    public enum FitnessType
    {
        fStatic,
        fDynamic
    }

    public class NeuroEvo
    {
        MathClasses.Probability Prob = MathClasses.Probability.Instance;
        MathClasses.Statistics Stats = new MathClasses.Statistics();

        List<Network> population = new List<Network>();
        int popSize = 20;
        int numInputs = 3;
        int numHidden = 8;
        int numOutputs = 2;
        double weightInitSTD = 1.0;
        double mutateSTD = 1.0;
        int numMutationsMatrix1 = 2;
        int numMutationsMatrix2 = 2;
        IEvo domain;
        int epochs = 1000;
        int statRuns = 3;
        FitnessType fitType = FitnessType.fStatic;
        Network bestNetwork = new Network();

        public List<Network> Population
        {
            get
            {
                return population;
            }
            set
            {
                population = value;
            }
        }
        public int PopSize
        {
            get
            {
                return popSize;
            }
            set
            {
                popSize = value;
            }
        }
        public int NumInputs
        {
            get
            {
                return numInputs;
            }
            set
            {
                numInputs = value;
            }
        }
        public int NumHidden
        {
            get
            {
                return numHidden;
            }
            set
            {
                numHidden = value;
            }
        }
        public int NumOutputs
        {
            get
            {
                return numOutputs;
            }
            set
            {
                numOutputs = value;
            }
        }
        public double WeightInitSTD
        {
            get
            {
                return weightInitSTD;
            }
            set
            {
                weightInitSTD = value;
            }
        }
        public double MutateSTD
        {
            get
            {
                return mutateSTD;
            }
            set
            {
                mutateSTD = value;
            }
        }
        public int NumMutationsMatrix1
        {
            get
            {
                return numMutationsMatrix1;
            }
            set
            {
                numMutationsMatrix1 = value;
            }
        }
        public int NumMutationsMatrix2
        {
            get
            {
                return numMutationsMatrix2;
            }
            set
            {
                numMutationsMatrix2 = value;
            }
        }
        public IEvo Domain
        {
            get
            {
                return domain;
            }
            set
            {
                domain = value;
            }
        }
        public int Epochs
        {
            get
            {
                return epochs;
            }
            set
            {
                epochs = value;
            }
        }
        public int StatRuns
        {
            get
            {
                return statRuns;
            }
            set
            {
                statRuns = value;
            }
        }
        public FitnessType FitType
        {
            get
            {
                return fitType;
            }
            set
            {
                fitType = value;
            }
        }
        public Network BestNetwork
        {
            get
            {
                return bestNetwork;
            }
            set
            {
                bestNetwork = value;
            }
        }

        // method to randomly initialize population
        void InitializePopulation()
        {
            Population = new List<Network>();

            Network tempNetwork;
            for (int i = 0; i < PopSize; i++)
            {
                tempNetwork = new Network();
                tempNetwork.NumInputs = NumInputs;
                tempNetwork.NumHidden = NumHidden;
                tempNetwork.NumOutputs = NumOutputs;
                tempNetwork.WeightInitSTD = WeightInitSTD;
                tempNetwork.InitializeRandomNetwork();
                Population.Add(tempNetwork);
            }

            // if best network hasn't already been initialized, initialize it
            if (BestNetwork.Fitness == double.MinValue / 2.0)
            {
                BestNetwork.NumInputs = NumInputs;
                BestNetwork.NumHidden = NumHidden;
                BestNetwork.NumOutputs = NumOutputs;
                BestNetwork.InitializeRandomNetwork();
            }
        }

        // method to mutate one weight matrix
        double[,] MutateOneMatrix(double[,] matrix, int numMutations)
        {
            // find dimensions of matrix
            int dim1 = matrix.GetLength(0);
            int dim2 = matrix.GetLength(1);

            // create and populate copied weight matrix
            double[,] output = new double[dim1, dim2];
            for (int i = 0; i < dim1; i++)
            {
                for (int j = 0; j < dim2; j++)
                {
                    output[i, j] = matrix[i, j];
                }
            }

            // for each mutation, randomly select one value and mutate it
            int tempDim1;
            int tempDim2;
            for (int i = 0; i < numMutations; i++)
            {
                tempDim1 = Prob.Next(dim1);
                tempDim2 = Prob.Next(dim2);
                output[tempDim1, tempDim2] += Prob.Gaussian(MutateSTD, 0.0);
            }

            // return mutated matrix
            return output;
        }

        // method to create a mutated copy of a neural network
        Network CreateMutatedNetwork(Network inputNetwork)
        {
            // create a new neural network
            Network output = new Network();

            // assign network properties
            output.NumInputs = NumInputs;
            output.NumHidden = NumHidden;
            output.NumOutputs = NumOutputs;
            output.WeightInitSTD = WeightInitSTD;

            // assign network weights as mutated versions of input network's weights
            output.WeightMat1 = MutateOneMatrix(inputNetwork.WeightMat1, NumMutationsMatrix1);
            output.WeightMat2 = MutateOneMatrix(inputNetwork.WeightMat2, NumMutationsMatrix2);

            // return mutated copy of the neural network
            return output;
        }

        // method to mutate entire population (doubles population size)
        void MutatePopulation()
        {
            // for each population member, create a mutated copy and add it to the population
            Network tempNetwork;
            for (int i = 0; i < PopSize; i++)
            {
                tempNetwork = new Network();
                tempNetwork = CreateMutatedNetwork(Population[i]);
                Population.Add(tempNetwork);
            }
        }

        // method to assign fitness to each population member
        void AssignFitness()
        {
            // for each population member, assign fitness
            for (int i = 0; i < PopSize * 2; i++)
            {
                // if we have static fitness assignment, only assign fitness to population members who haven't been evaluated yet
                if (FitType == FitnessType.fStatic && Population[i].Fitness != Double.MinValue / 2.0)
                {
                    Population[i].Fitness = Domain.Fitness(Population[i]);
                }
                // if fitness assignment is dynamic, assign fitness to every population member
                else
                {
                    Population[i].Fitness = Domain.Fitness(Population[i]);
                }

                // if population member is the best we have seen, save it as the best network
                if (Population[i].Fitness > BestNetwork.Fitness)
                {
                    BestNetwork.WeightMat1 = Population[i].WeightMat1;
                    BestNetwork.WeightMat2 = Population[i].WeightMat2;
                    BestNetwork.Fitness = Population[i].Fitness;
                }
            }
        }

        // method to reorder population based on fitness
        void ReorderPopulation()
        {
            List<Network> newPopulation = Population.OrderByDescending(o => o.Fitness).ToList();
            Population.Clear();
            Population.AddRange(newPopulation);
            newPopulation.Clear();
        }

        // method to select population members to survive
        void SelectPopulation()
        {
            List<Network> tempPop = new List<Network>();
            tempPop.Add(Population[0]);
            Population.RemoveAt(0);
            int index1;
            int index2;
            int tempVal;

            for (int i = 1; i < PopSize; i++)
            {
                index1 = Prob.Next(Population.Count());
                index2 = Prob.Next(Population.Count());
                while (index2 == index1)
                {
                    index2 = Prob.Next(Population.Count());
                }
                if (Population[index1].Fitness > Population[index2].Fitness)
                {
                    tempPop.Add(Population[index1]);
                }
                else
                {
                    tempPop.Add(Population[index2]);
                }
                if (index1 > index2)
                {
                    tempVal = index1;
                    index1 = index2;
                    index2 = tempVal;
                }
                Population.RemoveAt(index1);
                Population.RemoveAt(index2 - 1);
            }

            Population.Clear();
            Population.AddRange(tempPop);
        }

        // method to run one evolutionary algorithm
        public double[] SingleEA()
        {
			int reportingInterval = Epochs/10; //Reporting interval for 10% progeress report
            InitializePopulation();
            double[] output = new double[Epochs];
            for (int ep = 0; ep < Epochs; ep++)
            {
                MutatePopulation();
                AssignFitness();
                ReorderPopulation();
                SelectPopulation();
                output[ep] = Population[0].Fitness;

				if (ep % reportingInterval == 0)
				{
					Console.Write(100*ep/Epochs); Console.WriteLine ("%");
				}
            }
            return output;
        }

        public double[,] StatRunsEA()
        {
            Console.WriteLine("Beginning Neuroevolutionary Algorithm");
            List<double[]> statsData = new List<double[]>();

            for (int sr = 0; sr < StatRuns; sr++)
            {
                Console.WriteLine("Beginning Statistical Run {0}", sr + 1);
                statsData.Add(SingleEA());
                Console.WriteLine("Statistical Run {0} Complete, Best Fitness: {1}", sr + 1, statsData[sr][Epochs - 1]);
            }

            //Parallel.For(0, StatRuns, sr =>
            //{
            //    Console.WriteLine("Beginning Statistical Run {0}", sr + 1);
            //    statsData.Add(SingleEA());
            //    Console.WriteLine("Statistical Run {0} Complete, Best Fitness: {1}", sr + 1, statsData[sr][Epochs - 1]);
            //});



            double[,] postProcessedData = Stats.RunStats(statsData);
            return postProcessedData;
        }
    }
}

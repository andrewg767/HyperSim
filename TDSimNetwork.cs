using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetworks
{
    public class TDSimNetwork : IEvo
    {
        // call relevant classes
        TDSimDataImport DI = new TDSimDataImport();
        DataIO.DataExport DE = new DataIO.DataExport();
        DataIO.DataImport Imp = new DataIO.DataImport();
        BackpropNetwork NN = new BackpropNetwork();
        ScaleData Scale = new ScaleData();
        MathClasses.LinearAlgebra LA = new MathClasses.LinearAlgebra();
        MathClasses.Probability Prob = MathClasses.Probability.Instance;

        // object variables associated with data import
        int numLinesToRemove = 2;
        int numPoints = 1000;
        int numControlVars = 4;
        int numValidationPoints = 250;
        string dataExtension = "coldAir";
        List<double[]> unscaledTrainingInputs; // set by DI
        List<double[]> unscaledTrainingOutputs;
        List<double[]> unscaledValidationInputs;
        List<double[]> unscaledValidationOutputs;
        List<double[]> inputs; // scale from DI then set
        List<double[]> outputs;
        List<double[]> validationInputs;
        List<double[]> validationOutputs;
        List<double[]> scalingVars;
        List<double[]> allUnscaledInputs;
        List<double[]> allUnscaledOutputs;
        double[] initialState;

        int simTimeSteps = 200;

        List<double[]> setPoint = new List<double[]>(); // list container for setpoint arrays

        // object variables for neural network
        int numInputs = 5; // number of network input nodes
        int numHidden = 10; // number of network hidden nodes
        int numOutputs = 3; // number of network output nodes
        double weightInitSTD = 0.5; // STD of normal distribution for weight initialization
        double eta = 0.5; // network learning rate
        int episodes = 1000; // number of training episodes
        double momentum = 0.25; // momentum term value
        int numRestarts = 10; // how many times to restart the network training (to avoid local minima)
        ToggleShuffle shuffle = ToggleShuffle.yes; // do we shuffle data sets between each episode?
        int numMSEReportingPoints = 100; // number of MSE points to report
		int variableOfInterest =4;
        double noiseSTD =1.0;
		double noisePercent = 0.01;

        //

        // object properties
        public int NumLinesToRemove
        {
            get
            {
                return numLinesToRemove;
            }
            set
            {
                numLinesToRemove = value;
                DI.NumLinesToRemove = numLinesToRemove;
            }
        }

		public int VariableOfInterest
		{
			get{return variableOfInterest;}
			set{variableOfInterest = value;}
		}

        public int NumPoints
        {
            get
            {
                return numPoints;
            }
            set
            {
                numPoints = value;
                DI.NumPoints = numPoints;
            }
        }
        public int NumControlVars
        {
            get
            {
                return numControlVars;
            }
            set
            {
                numControlVars = value;
                DI.NumControlVars = numControlVars;
            }
        }
        public int NumValidationPoints
        {
            get
            {
                return numValidationPoints;
            }
            set
            {
                numValidationPoints = value;
                DI.NumValidationPoints = numValidationPoints;
            }
        }
        public string DataExtension
        {
            get
            {
                return dataExtension;
            }
            set
            {
                dataExtension = value;
                DI.DataExtension = dataExtension;
            }
        }
        public List<double[]> UnscaledTrainingInputs
        {
            get
            {
                return unscaledTrainingInputs;
            }
            set
            {
                unscaledTrainingInputs = value;
                Scale.UnscaledTrainingInputs = unscaledTrainingInputs;
            }
        }
        public List<double[]> UnscaledTrainingOutputs
        {
            get
            {
                return unscaledTrainingOutputs;
            }
            set
            {
                unscaledTrainingOutputs = value;
                Scale.UnscaledTrainingOutputs = unscaledTrainingOutputs;
            }
        }
        public List<double[]> UnscaledValidationInputs
        {
            get
            {
                return unscaledValidationInputs;
            }
            set
            {
                unscaledValidationInputs = value;
                Scale.UnscaledValidationInputs = unscaledValidationInputs;
            }
        }
        public List<double[]> UnscaledValidationOutputs
        {
            get
            {
                return unscaledValidationOutputs;
            }
            set
            {
                unscaledValidationOutputs = value;
                Scale.UnscaledValidationOutputs = unscaledValidationOutputs;
            }
        }
        public List<double[]> Inputs
        {
            get
            {
                return inputs;
            }
            set
            {
                inputs = value;
                NN.Inputs = inputs;
            }
        }
        public List<double[]> Outputs
        {
            get
            {
                return outputs;
            }
            set
            {
                outputs = value;
                NN.Outputs = outputs;
            }
        }
        public List<double[]> ValidationInputs
        {
            get
            {
                return validationInputs;
            }
            set
            {
                validationInputs = value;
                NN.ValidationInputs = validationInputs;
            }
        }
        public List<double[]> ValidationOutputs
        {
            get
            {
                return validationOutputs;
            }
            set
            {
                validationOutputs = value;
                NN.ValidationOutputs = validationOutputs;
            }
        }
        public List<double[]> ScalingVars
        {
            get
            {
                return scalingVars;
            }
            set
            {
                scalingVars = value;
            }
        }
        public List<double[]> AllUnscaledInputs
        {
            get
            {
                return allUnscaledInputs;
            }
            set
            {
                allUnscaledInputs = value;
            }
        }
        public List<double[]> AllUnscaledOutputs
        {
            get
            {
                return allUnscaledOutputs;
            }
            set
            {
                allUnscaledOutputs = value;
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
                NN.NumInputs = numInputs;
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
                NN.NumHidden = numHidden;
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
                NN.NumOutputs = numOutputs;
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
                NN.WeightInitSTD = weightInitSTD;
            }
        }
        public double NoiseSTD 
        { 
			get { return noiseSTD; } set { noiseSTD = value; } }
		public double NoisePercent 
		{ 
			get { return noisePercent; } 
			set { noisePercent = value; } }

        public double Eta
        {
            get
            {
                return eta;
            }
            set
            {
                eta = value;
                NN.Eta = eta;
            }
        }
        public int Episodes
        {
            get
            {
                return episodes;
            }
            set
            {
                episodes = value;
                NN.Episodes = episodes;
            }
        }
        public double Momentum
        {
            get
            {
                return momentum;
            }
            set
            {
                momentum = value;
                NN.Momentum = momentum;
            }
        }
        public int NumRestarts
        {
            get
            {
                return numRestarts;
            }
            set
            {
                numRestarts = value;
                NN.NumRestarts = numRestarts;
            }
        }
        public ToggleShuffle Shuffle
        {
            get
            {
                return shuffle;
            }
            set
            {
                shuffle = value;
                NN.Shuffle = shuffle;
            }
        }
        public int NumMSEReportingPoints
        {
            get
            {
                return numMSEReportingPoints;
            }
            set
            {
                numMSEReportingPoints = value;
                NN.NumMSEReportingPoints = numMSEReportingPoints;
            }
        }
        public double[] InitialState
        {
            get
            {
                return initialState;
            }
            set
            {
                initialState = value;
            }
        }
        public int SimTimeSteps
        {
            get
            {
                return simTimeSteps;
            }
            set
            {
                simTimeSteps = value;
            }
		}
		
		public List<double[]> SetPoint{get{return setPoint;} set{setPoint=value;}}

        // method to import and save datasets, then scale datasets, then train network and export all the meaningful data
        public void TrainNetwork()
		{
            // import unscaled data
            DI.ImportDataToInputOutput();
            AllUnscaledInputs = DI.AllInputs;
            AllUnscaledOutputs = DI.AllOutputs;
            UnscaledTrainingInputs = DI.TrainingInputs;
            UnscaledTrainingOutputs = DI.TrainingOutputs;
            UnscaledValidationInputs = DI.ValidationInputs;
            UnscaledValidationOutputs = DI.ValidationOutputs;

            // scale data
            Scale.ScaleAllData();
            Inputs = Scale.TrainingInputs;
            Outputs = Scale.TrainingOutputs;
            ValidationInputs = Scale.ValidationInputs;
            ValidationOutputs = Scale.ValidationOutputs;
            ScalingVars = Scale.ScalingVars;

            // train network
            NN.TrainNetworkRandomRestart();
            NN.TestNetworkOutputValidationTimeDomain(11);

            int[] DIparams = new int[] { DI.NumLinesToRemove, DI.NumPoints, DI.NumControlVars, DI.NumValidationPoints };
            DE.Export1DIntArray(DIparams, "diParams");
        }

        // method to import fully trained neural network
        public void ImportTrainedNetwork()
        {
            // import unscaled data
            DI.ImportDataToInputOutput();
            AllUnscaledInputs = DI.AllInputs;
            AllUnscaledOutputs = DI.AllOutputs;
            UnscaledTrainingInputs = DI.TrainingInputs;
            UnscaledTrainingOutputs = DI.TrainingOutputs;
            UnscaledValidationInputs = DI.ValidationInputs;
            UnscaledValidationOutputs = DI.ValidationOutputs;

            // scale data
            Scale.ScaleAllData();
            Inputs = Scale.TrainingInputs;
            Outputs = Scale.TrainingOutputs;
            ValidationInputs = Scale.ValidationInputs;
            ValidationOutputs = Scale.ValidationOutputs;
            ScalingVars = Scale.ScalingVars;

            // import other parameters
            int[] DIparams = Imp.Import1DIntArray("diParams");
            double[] networkPerfVars = Imp.Import1DDoubleArray("networkPerfVars");
            int[] networkTopology = Imp.Import1DIntArray("networkTopology");
            double[,] weightMat1 = Imp.parseCSV("weightMat1");
            double[,] weightMat2 = Imp.parseCSV("weightMat2");
            NumLinesToRemove = DIparams[0];
            NumPoints = DIparams[1];
            NumControlVars = DIparams[2];
            NumValidationPoints = DIparams[3];
            
            
            NN.NumInputs = networkTopology[0];
            NN.NumHidden = networkTopology[1];
            NN.NumOutputs = networkTopology[2];
            NN.InitializeNetwork();
            NN.InitializeBestNetwork();
            NN.BestNetwork.Fitness = networkPerfVars[0];
            NN.BestNetwork.MSETraining = networkPerfVars[1];
            NN.BestNetwork.MSEValidation = networkPerfVars[2];
            NN.BestNetwork.WeightMat1 = weightMat1;
            NN.BestNetwork.WeightMat2 = weightMat2;
        }

        // method to run a time domain simulation based based on control policy used in data
        public void TDSimNoControl(int dataIndex)
        {
            NN.TestNetworkOutputValidationTimeDomain(dataIndex);
        }

        // method to run a time domain simulation based on initial state and neural network controller
        public List<double[]> TDSim(Network controller, double[] setpoint)
        {
            List<double[]> output = new List<double[]>();
            output.Add(InitialState);

            double[] action;
            double[] state;
            double[] netInput;            

            for (int i = 0; i < SimTimeSteps; i++)
            {
				action = controller.ForwardPass(LA.Join(output[i],setpoint)); // Append setpoint to controller inputs
                netInput = LA.Join(output[i], action);
                state = NN.BestNetwork.ForwardPass(netInput);
                output.Add(state);
            }
            return output;
        }

		// overload method to run a simulation with a list of setpoints for each timestep
		public List<double[]> TDSim(Network controller, List<double[]> setpoints)
		{
			SimTimeSteps = setpoints.Count;
			List<double[]> output = new List<double[]>();
			output.Add(InitialState);

			double[] action;
			double[] state;
			double[] netInput;            

			for (int i = 0; i < SimTimeSteps; i++)
			{
				action = controller.ForwardPass(LA.Join(output[i] , setpoints[i])); // Append setpoint to controller inputs
				netInput = LA.Join(output[i], action);
				state = NN.BestNetwork.ForwardPass(netInput);
				output.Add(state);
			}
			return output;
		}
		public List<List<double[]>> TDSim_Noise(Network controller, List<double[]> setpoints) // todo tonight:: make output a list of two doubles
																						// One list is actual state, one list noisy state
																						// Publish both to csv's in Program.cs
		{
			SimTimeSteps = setpoints.Count;
			List<List<double[]>> output = new List<List<double[]>>();
			List<double[]> noisyState = new List<double[]> ();
			List<double[]> realState = new List<double[]> ();
			realState.Add(InitialState);
			noisyState.Add(addSensorNoise(InitialState));
			double[] action;
			double[] state;
			double[] netInput;            

			for (int i = 0; i < SimTimeSteps; i++)
			{
				action = controller.ForwardPass(LA.Join(noisyState[i] , setpoints[i])); // Append setpoint to controller inputs
				netInput = LA.Join(noisyState[i], action); 
				state = NN.BestNetwork.ForwardPass(netInput);
				realState.Add (state);
				noisyState.Add(addSensorNoise(state));
			}
			output.Add (realState);
			output.Add (noisyState);
			return output;
		}

		public List<List<double[]>> TDSim_Act_Noise(Network controller, List<double[]> setpoints) // todo tonight:: make output a list of two doubles
			// One list is actual state, one list noisy state
			// Publish both to csv's in Program.cs
		{
			SimTimeSteps = setpoints.Count;
			List<List<double[]>> output = new List<List<double[]>>();
			List<double[]> noisyState = new List<double[]> ();
			List<double[]> realState = new List<double[]> ();
			realState.Add(InitialState);
			noisyState.Add((InitialState));
			double[] action;
			double[] state;
			double[] netInput;            

			for (int i = 0; i < SimTimeSteps; i++)
			{
				action = addSensorNoise(controller.ForwardPass(LA.Join(noisyState[i] , setpoints[i])) ); // Append setpoint to controller inputs
				netInput = LA.Join(noisyState[i], action); 
				state = NN.BestNetwork.ForwardPass(netInput);
				realState.Add (state);
				noisyState.Add((state));
			}
			output.Add (realState);
			output.Add (noisyState);
			return output;
		}

//        public List<double[]> TDSim_Noise(Network controller,)
//        {
//            List<double[]> output = new List<double[]>();
//            output.Add(InitialState);
//            double[] action;
//            double[] state;
//            double[] netInput;
//
//            for (int i = 0; i < SimTimeSteps; i++)
//            {
//                action = controller.ForwardPass(output[i]);
//                netInput = LA.Join(output[i], action);
//                state = NN.BestNetwork.ForwardPass(netInput);
//                output.Add(addSensorNoise(state));
//            }
//            return output;
//        }

        //method to add noise to sensor output
        //double[] input State, output noiseyState
        public double[] addSensorNoise(double[] state)
        {
            int temp= state.Length;
            double[] noiseyState = new double[temp];
            for (int i = 0; i < temp; i++)
            {
                noiseyState[i] = state[i] + NoisePercent*Prob.Gaussian(NoiseSTD,0.0);
            }

            return noiseyState;
        }

        // method to find fitness of a controller
        public double Fitness(Network controller)
        {
            double output = 0.0;
            double[] initState = outputs[0];
            
            
            for (int sp = 0; sp < SetPoint.Count; sp++) //training over several desired setpoints
            {
			controller.StateError = new double[NumOutputs];

				List<double[]> states = TDSim(controller,SetPoint[sp]);

                for (int i = 0; i < SimTimeSteps; i++)
                {


					double tempError = (states [i] [variableOfInterest] - SetPoint [sp] [variableOfInterest]);
					//Console.WriteLine (tempError);
					//controller.StateError[4] += tempError;// keeps track of sum error for each state
					//Console.WriteLine (conroller.StateError [j]);
					output += Math.Pow(tempError,2);

//					for (int j = 0; j < SetPoint[0].Length; j++)
//                    {
//						double tempError = (states [i] [j] - SetPoint [sp] [j]);
//						//Console.WriteLine (tempError);
//						controller.StateError[j] += tempError;// keeps track of sum error for each state
//						//Console.WriteLine (controller.StateError [j]);
//                        output += Math.Exp(tempError);
////                    }
                }
            }
            output = 1.0 / (1.0 + output);
            return output;
        }

		// method to compute error of controll data and setpoint data accross all variables? Perhaps a class for error only

    }
}

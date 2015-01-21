using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyperModel
{
    class Program

    {
        static void Main(string[] args)
        {
            // import relevant classes
            NeuralNetworks.TDSimNetwork NN = new NeuralNetworks.TDSimNetwork();
            NeuralNetworks.Network Controller = new NeuralNetworks.Network();
            NeuralNetworks.NeuroEvo NE = new NeuralNetworks.NeuroEvo();
            DataIO.DataExport DE = new DataIO.DataExport();

            // data management stuff
            NN.NumLinesToRemove = 2;
            NN.NumPoints = 25;
            NN.NumControlVars = 2;
            NN.NumValidationPoints = 100;
            
            // set network learning variables for learning model
            NN.NumInputs = 21;
            NN.NumHidden = 15;
            NN.NumOutputs = 19;
            NN.WeightInitSTD = 0.75;
            NN.Eta = 0.1; //.25 // was most recently 0.2
            NN.Episodes = 2500;
            NN.Momentum = 0.7; //.5 // was most recently 0.7
            NN.NumRestarts = 750;
            NN.Shuffle = NeuralNetworks.ToggleShuffle.yes;
            NN.NumMSEReportingPoints = NN.Episodes;
            NN.DataExtension = "ColdAir";

            // train network 
            //NN.TrainNetwork();

            // import network and test it
            NN.ImportTrainedNetwork();

          //  NN.TDSimNoControl(4);

            // set simulation parameters: initial state, time steps, and desired setpoint
            NN.InitialState = NN.Outputs[0];
            NN.SimTimeSteps = 100;

			//NN.SetPoint.Add(NN.Outputs[100]);
           	NN.SetPoint.Add(NN.Outputs[200]);
			NN.SetPoint.Add (NN.Outputs [250]);
			//NN.SetPoint.Add (NN.Outputs [150]);
			//NN.SetPoint.Add (NN.Outputs [125]);
			//NN.SetPoint.Add (NN.Outputs [175]);
			//NN.SetPoint.Add (NN.Outputs [20]);






//            // set neuroevolutionary parameters
//            NE.NumInputs = NN.NumOutputs*2;
//            NE.NumHidden = 15;
//            NE.NumOutputs = 2;
//            NE.WeightInitSTD = 2.0;
//            NE.PopSize = 10;
//            NE.MutateSTD = 1.0;
//            NE.NumMutationsMatrix1 = 30;
//            NE.NumMutationsMatrix2 =5;
//            NE.Domain = NN;
//            NE.Epochs = 1000;
//			  NE.StatRuns = 2;
//            NE.FitType = NeuralNetworks.FitnessType.fStatic;
//
//            // run neuroevolutionary control algorithms, and export data
//            double[,] eaData = NE.StatRunsEA();
//            DE.Export2DArray(eaData, "eaData");
//
//            Controller = NE.BestNetwork;
			//NE.BestNetwork.ExportController ();
            //Controller.ExportController();

			Controller.ImportController();

            // pick a parameter to plot controlled var vs. desired setpoint
            int parameterOfInterest = 4;
			NN.VariableOfInterest = parameterOfInterest;

            // simulate using learned controller, and export true vs. desired datapoints
			//Desired set points for a sim
			List<double[]> setPointTrajectory = new List<double[]>();
			int step = NN.SimTimeSteps/2;
			for (int i=0; i<step; i++) 
			{
				setPointTrajectory.Add (NN.SetPoint [1]);
			}
			for (int i=step; i<2*step; i++) 
			{
				setPointTrajectory.Add(NN.SetPoint[0]);
			}


			List<List<double[]>> allStateData = NN.TDSim_Act_Noise(Controller,setPointTrajectory);

            double[,] neControlData = new double[NN.SimTimeSteps, 2];
			double[,] noiseControlData = new double[NN.SimTimeSteps,2]; 
            double[,] setPointData = new double[NN.SimTimeSteps, 2];
            for (int i = 0; i < NN.SimTimeSteps; i++)
            {
                neControlData[i, 0] = Convert.ToDouble(i);
                neControlData[i, 1] = allStateData[0][i][parameterOfInterest];


                setPointData[i, 0] = Convert.ToDouble(i);
                setPointData[i, 1] = setPointTrajectory[i][parameterOfInterest];

				noiseControlData [i, 0] = Convert.ToDouble (i);
				noiseControlData [i, 1] = allStateData [1] [1] [parameterOfInterest];
            }

            DE.Export2DArray(neControlData, "neControlData");
			DE.Export2DArray (noiseControlData, "noiseControlData");
          //  DE.Export2DArray(setPointData, "setPointData");

            /*
            // simulate with sensor noise
            List<double[]> allStateData_Noise = NN.TDSim_Noise(NE.BestNetwork);
            double[,] neControlData_Noise = new double[NN.SimTimeSteps, 2];
            //double[,] setPointData_Noise = new double[NN.SimTimeSteps, 2];
            for (int i = 0; i < NN.SimTimeSteps; i++)
            {
                //neControlData_Noise[i, 0] = Convert.ToDouble(i);
               // neControlData_Noise[i, 1] = allStateData_Noise[i][parameterOfInterest];
                setPointData[i, 0] = Convert.ToDouble(i);
                setPointData[i, 1] = NN.SetPoint[0][parameterOfInterest];
            }

            DE.Export2DArray(neControlData_Noise, "neControlData_with_Noise");
            // DE.Export2DArray(setPointData_Noise, "setPointData");
            */
            
            Console.WriteLine("Press ENTER to Continue...");
            Console.ReadLine();
        }
    }
}

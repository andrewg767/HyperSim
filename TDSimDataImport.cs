using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetworks
{
    public enum ValidationPointsType
    {
        random,
        last
    }
    public class TDSimDataImport
    {
        // call relevant classes
        DataIO.DataImport DI = new DataIO.DataImport();
        MathClasses.DataManagement DM = new MathClasses.DataManagement();
        MathClasses.Probability Prob = MathClasses.Probability.Instance;

        // object variables
        int numLinesToRemove = 2;
        int numPoints = 1000;
        int numControlVars = 4;
        int numValidationPoints = 250;
        string dataExtension = "coldAir";
        List<double[]> allInputs;
        List<double[]> allOutputs;
        List<double[]> trainingInputs;
        List<double[]> trainingOutputs;
        List<double[]> validationInputs;
        List<double[]> validationOutputs;

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
            }
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
            }
        }
        public List<double[]> AllInputs
        {
            get
            {
                return allInputs;
            }
            set
            {
                allInputs = value;
            }
        }
        public List<double[]> AllOutputs
        {
            get
            {
                return allOutputs;
            }
            set
            {
                allOutputs = value;
            }
        }
        public List<double[]> TrainingInputs
        {
            get
            {
                return trainingInputs;
            }
            set
            {
                trainingInputs = value;
            }
        }
        public List<double[]> TrainingOutputs
        {
            get
            {
                return trainingOutputs;
            }
            set
            {
                trainingOutputs = value;
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
            }
        }

        // method to split data into training and validation sets randomly
        public void SeparateTrainingValidation(List<double[]> inputSet, List<double[]> outputSet)
        {
            List<double[]> tempInputs = new List<double[]>();
            tempInputs.AddRange(inputSet);
            List<double[]> tempOutputs = new List<double[]>();
            tempOutputs.AddRange(outputSet);
            TrainingInputs = new List<double[]>();
            TrainingOutputs = new List<double[]>();
            ValidationInputs = new List<double[]>();
            ValidationOutputs = new List<double[]>();
            int index;
            for (int i = 0; i < numValidationPoints; i++)
            {
                index = Prob.Next(tempInputs.Count());
                ValidationInputs.Add(tempInputs[index]);
                ValidationOutputs.Add(tempOutputs[index]);
                tempInputs.RemoveAt(index);
                tempOutputs.RemoveAt(index);
            }
            TrainingInputs.AddRange(tempInputs);
            TrainingOutputs.AddRange(tempOutputs);
        }

        // method to import data and create input and output data
        public void ImportDataToInputOutput()
        {
            // import raw data
            double[,] rawData = DI.parseCSVWithoutFirstLines(DataExtension, NumLinesToRemove);

            // downsample data with running average
            double[,] rawDataSampled = DM.MeanSampleData(rawData, NumPoints);

            // find dimensions of data set
            int numDataPoints = rawDataSampled.GetLength(0);
            int numVariables = rawDataSampled.GetLength(1);
            int numStateVars = numVariables - NumControlVars;

            // create input and output data sets
            AllInputs = new List<double[]>();
            AllOutputs = new List<double[]>();
            double[] tempInput;
            double[] tempOutput;
            for (int i = 0; i < numDataPoints - 1; i++)
            {
                tempInput = new double[numVariables];
                tempOutput = new double[numStateVars];
                for (int j = 0; j < numVariables; j++)
                {
                    tempInput[j] = rawDataSampled[i, j];
                }
                for (int j = 0; j < numStateVars; j++)
                {
                    tempOutput[j] = rawDataSampled[i + 1, j];
                }
                AllInputs.Add(tempInput);
                AllOutputs.Add(tempOutput);
            }

            // need to split allInputs and allOutputs to training and validation sets
            SeparateTrainingValidation(AllInputs, AllOutputs);
        }

    }
}

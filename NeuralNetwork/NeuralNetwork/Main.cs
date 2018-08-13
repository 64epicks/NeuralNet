#region Main.cs
#region Using
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDB;
using static System.Net.Mime.MediaTypeNames;
#endregion
#region [NS] NeuralNetwork
namespace NeuralNetwork
{
    #region [Class] NeuralNet
    public class NeuralNet
    {
        #region Var
        #region Map arrays
        #region Map
        /*
         * Visual representation of Map array with params: 5 input nodes, 3 nodes in each midlayer, 5 outputNodes
         * 
         *                                                                                   float[][][][] Map
         *                                                                                   /                \
         *                                                                                  /                  \
         *                                                                         InNodes & Outnodes       MiddleNodes
         *                                                                         ------------------      ------------- 
         *                                                                        /                 |                   \
         *                                                                       /                  |                    \ 
         *                                         InNodes----------------------/                   |                     \
         *                               --------------------------                                 |                      \
         *                              /      |      |      |     \                                |                    MidLayer [NUM]   
         *                            IN0IN  IN1IN  IN2IN  IN3IN  IN4IN                            /                     --------------         
         *                            -----  -----  -----  -----  -----                           /                     /       |      \ 
         *                              |      |      |      |      |                            /                    MD0IN   MD1IN   MD2IN
         *                            NDID    NDID   NDID   NDID   NDID                         /                    /    /   |   |   \   \       
         *                                                                                     /                    /    |    |   |    |   \           
         *                                                                                    /                    /     |    |   |    |    \ 
         *                                            OutNodes-------------------------------/                    /      |    |   |    |     \        
         *                                   --------------------------                                          /      /     |   |     \     \              
         *                                  /      |      |      |     \                                       NDID   BIAS  NDID BIAS  NDID  BIAS
         *                                 /      /       |       \     \
         *                                /      |        |        |     \
         *                               /       |        |        |      \
         *                              /        |        |        |       \
         *                             /         |        |        |        \
         *                            /          |        |        |         \
         *                         OU0IN       OU1IN    OU2IN    OU3IN      OU4IN
         *                         -----       -----    -----    -----      -----
         *                        /    |      /    |    |   |    |    \     |    \
         *                       /     |     |     |    |   |    |     |    |     \
         *                      /      |     |     |    |   |    |     |    |      \
         *                     /       |     |     |    |   |    |     |    |       \
         *                    /        |     |     |    |   |    |     |    |        \
         *                  NDID     BIAS  NDID  BIAS NDID BIAS NDID BIAS  NDID      BIAS
         *        
         *                                                                 
         *                 IN[NUMBER]IN = InNode [NUMBER] info.      OU[NUMBER]IN = OutNode [NUMBER] info.  NDID = NodeID                                                
         */
        private float[][][][] Map;
        #endregion
        #region WeightMap
        /*
         * COMING SOON
         */
        private float[][][][] WeightMap;
        #endregion
        #endregion
        #region Private
        private Random random = new Random();
        private DB db = new DB(Environment.CurrentDirectory);
        private string NetName;
        private long inNodes;
        private int midNodes;
        private int midNodesLength;
        private int outNodes;
        private string[] outNodesName;
        private bool loadedToMem = false;
        #endregion
        #endregion
        #region Init
        /*
         * <summary>Sets up the class</summary>
         * <param name="newNet">Should the function create a new network[TRUE], Or should the function load an existing[FALSE]</param>
         * <param name="name">The name of the network</param>
         * <param name="inputNodes">The number of nodes in the inputLayer</param>
         * <param name="middleNodes">The number of middleLayers</param>
         * <param name="middleNodesLentgh">The number of nodes in each middleLayer</param>
         * <param name="outputNodes">The Number of nodes in the output layer</param>
         * <param name="outputNodesName">The Names of each output node (CURRENTLY NOT USED)</param>
         */
        public NeuralNet(bool newNet, string name, long inputNodes, int middleNodes, int middeNodesLentgh, int outputNodes, string[] outputNodesName)
        {
            //Set network variables & init db
            NetName = name;
            inNodes = inputNodes;
            midNodes = middleNodes;
            midNodesLength = middeNodesLentgh;
            outNodes = outputNodes;
            outNodesName = outputNodesName;

            if (newNet == true) CreateNet();

            db.EnterDatabase(NetName);

        }
        #endregion
        #region CreateNet
        /*
         * <summary>Creates all local files and generates random weights and biases</summary>
         */
        private void CreateNet()
        {
            //Create database for Net
            db.CreateDatabase(NetName);
            db.EnterDatabase(NetName);

            //Create tables for all layers
            db.CreateTable("Input", "Id;Weight");
            for(int i = 0; i < midNodes; i++)
            {
                db.CreateTable("MidLayer " + i, "Id;Weight;Bias");
            }
            db.CreateTable("Output", "Id;Name;Bias");

            //Insert random data about weight and bias for input layer
            for(int i = 0; i < inNodes; i++)
            {
                string ran = "";
                for(int t = 0; t < midNodesLength; t++)
                {
                    ran += rnd(-1 / Math.Sqrt(inNodes), 1 / Math.Sqrt(inNodes)) + "|";
                }
                db.Insert("Input", i.ToString() + ";" + ran);
            }
            //Insert random data about weight and bias for all midnodes
            for (int i = 0; i < midNodes; i++)
            {
                for(int e = 0; e < midNodesLength; e++)
                {
                    string ran = "";
                    if (i == midNodes - 1)
                    {
                        for (int t = 0; t < outNodes; t++)
                        {
                            ran += rnd(-1 / Math.Sqrt(midNodesLength), 1 / Math.Sqrt(midNodesLength)) + "|";
                        }
                    }
                    else
                    {
                        for (int t = 0; t < midNodesLength; t++)
                        {
                            ran += rnd(-1 / Math.Sqrt(midNodesLength), 1 / Math.Sqrt(midNodesLength)) + "|";
                        }
                    }
                    db.Insert("MidLayer " + i, e + ";" + ran + ";" + NextFloat(random, 0, 1));

                }
            }
            //Insert random data about bias for output layer
            for (int i = 0; i < outNodes; i++)
            {
                db.Insert("Output", i + ";" + outNodesName[i] + ";" + NextFloat(random, 0, 1));
            }
        }
        #endregion
        #region Run Init
        /*
         * <summary>Tests the network with inputNodes activaton being the grayscale of the pixels from an image
         * <param name="ImageLocation">Points to where the image is located</param>
         * <returns>Float array with all the outputNodes activation</returns>
         */
        public float[] RunImage(string Imagelocation)
        {
            //Convert image to grayscale float array
            Bitmap bitmap = new Bitmap(Imagelocation);
            if (bitmap.Height != bitmap.Width || bitmap.Height * bitmap.Width != inNodes) return new float[] { 0 };
            else
            {
                var ImgMapList = new List<float>();

                for (int i = 0; i < bitmap.Height; i++)
                {
                    for (int t = 0; t < bitmap.Width; t++)
                    {
                        //Changes the color cycle from 0-255 to 0-1
                        float c = (bitmap.GetPixel(i, t).R + bitmap.GetPixel(i, t).G + bitmap.GetPixel(i, t).B) / 3;
                        ImgMapList.Add(c/255);
                    }
                }
                //Returns float values
                return RunNetworkViaMemory(ImgMapList.ToArray());
                
            }
        }
        #endregion
        #region Run
        /*
         * <summary>Runs the whole network from memory</summary>
         * <param name="InputMap">The activations for the inputLayer</param>
         * <returns>Float array with all the activatons from the output layer</returns>
         */
        private float[] RunNetworkViaMemory(float[] InputMap)
        {
            if (loadedToMem == false) throw new Exception("Network not loaded to memory (Have you ran LoadToMemory()?)");
            #region Init nodes
            //
            //Sets up all the variables for calculation
            //
            //Set init nodes values
            float[][][] nodeValue;
            var nodeListIn = new List<float>();
            for (int i = 0; i < InputMap.Length; i++) nodeListIn.Add(InputMap[i]);


            //Set midnodes value
            var nodeListMid = new List<float[]>();
            var nodeListMidTemp = new List<float>();
            for (int i = 0; i < db.CountTables(NetName) - 2; i++)
            {
                for (int t = 0; t < db.TableLength(NetName, "MidLayer 0"); t++)
                {
                    nodeListMidTemp.Add(0);
                }
                nodeListMid.Add(nodeListMidTemp.ToArray());
            }

            var nodeListOut = new List<float>();
            for (int i = 0; i < db.TableLength(NetName, "Output"); i++) nodeListOut.Add(0);

            nodeValue = new float[][][] { new float[][] { nodeListIn.ToArray(), nodeListOut.ToArray() }, nodeListMid.ToArray() };
            #endregion
            #region Calc
            //Calculates activations for every node exept the input layer
            /*
             * Node activation formula:
             * Weight0 * Activation0 + Weight1 * Activation1 + ... + Bias
             */
            #region MidNodes
            //For every midnode layer
            for (int i = 0; i < midNodes; i++)
            {
                //For every node in layer
                for (int t = 0; t < midNodesLength; t++)
                {
                    float sum = 0;
                    if (i == 0)
                    {
                        //Calculates the sum of weights and activations from the previus layer (This case it's the inputLayer)
                        for (int d = 0; d < inNodes; d++)
                        {
                            sum = sum + WeightMap[0][0][d][t] * nodeValue[0][0][d];

                        }
                        //Add the bias
                        sum = sum + Map[1][i][t][1];
                    }
                    else
                    {
                        //Same as the other but for midNodeLayer
                        for (int d = 0; d < midNodesLength; d++)
                        {
                            sum = sum + WeightMap[1][i - 1][d][t] * nodeValue[1][i - 1][d];
                        }
                        sum = sum + Map[1][i][t][1];
                    }
                    //Takes the sum and squishicating it
                    float y = Sigmoid(sum);
                    //Sets the activation for the current node
                    nodeValue[1][i][t] = y;
                }
            }
            #endregion
            #region OutPut
            //For every node in outputLayer
            for(int i = 0; i < outNodes; i++)
            {
                float sum = 0;
                //Calculates the sum of weights and activations from the last midLayer
                for (int t = 0; t < midNodesLength; t++)
                {
                    sum = sum + WeightMap[1][midNodes - 1][t][i] * nodeValue[1][midNodes - 1][t];
                }
                sum = sum + Map[0][1][i][1];

                float y = Sigmoid(sum);
                nodeValue[0][1][i] = y;
            }
            #endregion
            #endregion
            //Add all output nodes activations to a list
            var retList = new List<float>();
            for (int i = 0; i < outNodes; i++) retList.Add(nodeValue[0][1][i]);
            //Return list
            return retList.ToArray();

        }
        #endregion
        #region Load
        /*
         * <summary>Loads network from database and store it in memory</summary>
         */
        public void LoadToMemory()
        {
            //Load 'Input' table
            var InList = new List<float[]>();
            var InWeightList = new List<float[]>();
            db.EnterDatabase(NetName);
            for(int i = 0; i < db.TableLength(NetName, "Input"); i++)
            {
                string[] WeiArr = db.Get("Weight", "Input", "Id=" + i)[0].Split('|');
                float[] WeiArrFloat = new float[WeiArr.Length];
                for (int d = 0; d < WeiArr.Length - 1; d++) WeiArrFloat[d] = float.Parse(WeiArr[d]);
                InList.Add(new float[] { i });
                InWeightList.Add(WeiArrFloat);
            }

            //Load all 'Mid'
            var MidListTemp = new List<float[]>();
            var MidWeightListTemp = new List<float[]>();
            var MidWeightList = new List<float[][]>();
            var MidList = new List<float[][]>();
            for(int i = 0; i < db.CountTables(NetName) - 2; i++)
            {
                for(int t = 0; t < db.TableLength(NetName, "MidLayer " + i); t++)
                {
                    MidListTemp.Add(new float[] { t, float.Parse(db.Get("Bias", "MidLayer " + i, "Id=" + t)[0]) });
                    string[] WeiArr = db.Get("Weight", "MidLayer " + i, "Id=" + t)[0].Split('|');
                    float[] WeiArrFloat = new float[WeiArr.Length];
                    for (int d = 0; d < WeiArr.Length - 1; d++) WeiArrFloat[d] = float.Parse(WeiArr[d]);
                    MidWeightListTemp.Add(WeiArrFloat);
                }
                MidList.Add(MidListTemp.ToArray());
                MidWeightList.Add(MidWeightListTemp.ToArray());
                MidListTemp.Clear();
                MidWeightListTemp.Clear();
            }

            var OutList = new List<float[]>();
            for (int i = 0; i < db.TableLength(NetName, "Output"); i++)
            {
                OutList.Add(new float[] { i, float.Parse(db.Get("Bias", "Output", "Id=" + i)[0])});
            }
            float[][][] MidListArr = MidList.ToArray();
            float[][][] MidWeightListArr = MidWeightList.ToArray();
            Map = new float[][][][] { new float[][][] { InList.ToArray(), OutList.ToArray() }, MidListArr,  };
            WeightMap = new float[][][][] { new float[][][] { InWeightList.ToArray() }, MidWeightList.ToArray()};

            inNodes = db.TableLength(NetName, "Input");
            midNodes = db.CountTables(NetName) - 2;
            midNodesLength = db.TableLength(NetName, "MidLayer 0");
            outNodes = db.TableLength(NetName, "Output");
            var OutNodeName = new List<string>();
            for (int i = 0; i < db.TableLength(NetName, "Output"); i++) OutNodeName.Add(db.Get("Name", "Output", "Id=" + i)[0]);
            outNodesName = OutNodeName.ToArray();

            loadedToMem = true;
        }
        #endregion
        #region Tools
        /*
         * <summary>Calculates the rating of the neural-network based on the cost and length of the output layer</summary>
         * 
         * <param name="cost">Cost of the current test. (0 <= lenth of output layer)</param>
         * 
         * <returns>A rating between 0 and 4 (0 = A, 1 = B ... 4 = E)</returns>
         */
        public int GetRating(float cost)
        {
            float gradeDiff = outNodes / 5;
            if (cost >= 0 && cost <= gradeDiff) return 0;
            if (cost > gradeDiff && cost <= gradeDiff * 2) return 1;
            if (cost > gradeDiff * 2 && cost <= gradeDiff * 3) return 2;
            if (cost > gradeDiff * 3 && cost <= gradeDiff * 4) return 3;
            if (cost > gradeDiff * 4 && cost <= gradeDiff * 5) return 4;
            return -1;
        }
        /*
         * <summary>Calculates grade based on rating (A-E)
         * <param name="rating">The rating of test</param>
         * <returns>Char between A and E</returns>
         */
        public char Grade(int rating)
        {
            switch (rating)
            {
                case 0:
                    {
                        return 'A';
                    }
                case 1:
                    {
                        return 'B';
                    }
                case 2:
                    {
                        return 'C';
                    }
                case 3:
                    {
                        return 'D';
                    }
                case 4:
                    {
                        return 'E';
                    }
            }
            return 'F';
        }
        /*
         * <summary>Calculates the cost of the current test</summary>
         * <param name="got">Array of all outputs from the output layer</param>
         * <param name="expect">Array of all the expected outputs</param>
         * <returns>A float between 0 and the length of the output layer(outNodes)</returns>
         */
        public float Cost(float[] got, float[] expect)
        {
            if (got.Length != expect.Length) return 0;
            float sum = 0;
            for(int i = 0; i < got.Length; i++)
            {
                sum += (float)Math.Pow(got[i] - expect[i], 2);
            }
            return sum;
        }
        /*
         * <summary>https://en.wikipedia.org/wiki/Sigmoid_function</summary>
         * <param name="value">Number to squishify</param>
         * <returns>Float between 0 and 1 (value=0; return 0.5)</returns>
         */
        private float Sigmoid(double value)
        {
            return 1.0f / (1.0f + (float)Math.Exp(-value));
        }
        /*
         * <summary>Generates a random number between a and b</summary>
         * <param name="a">Lowest number that can be returned</param>
         * <param name="b">Highest number that can be returned</param>
         * <returns>A random double between a and b</returns> 
         */
        private double rnd(double a, double b)
        {
            return a + random.NextDouble() * (b - a);
        }
        /*
         * <summary>Generates a random number between x and y</summary>
         * <param name="x">Lowest number that can be returned</param>
         * <param name="y">Highest number that can be returned</param>
         * <returns>A random float between x and y</returns>
         */
        private float NextFloat(Random random, int x, int y)
        {
            double mantissa = (random.NextDouble() * 2.0) - 1.0;
            double exponent = Math.Pow(2.0, random.Next(x, y));
            return (float)(mantissa * exponent);
        }
        #endregion
    }
    #endregion
}
#endregion
#endregion
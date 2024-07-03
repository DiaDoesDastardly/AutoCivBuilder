using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Numerics;
using System.IO.Compression;

namespace PenroseEngine{
    
    public class rendererPipeline
    {
        public static int xSize = 400;
        public static int ySize = 400;
        public static double screenResolution = 1;
        public static double triangleDensity = 1.5;
        public static double[][][] screenInfo = new double[(int)(screenResolution*xSize)][][];
        public static double frameCounter = 0;
        public static double[,] rotationMatrixGenerator(double theta, double phi){
            //Converting input degrees into radians
            theta = (theta/180)*Math.PI;
            phi = (phi/180)*Math.PI;
            //Make rotational matrx 
            double[,] rotationalMatrix = new double[,]{
                {Math.Cos(theta), 0 , -Math.Sin(theta)},
                {-Math.Sin(theta)*Math.Sin(phi), Math.Cos(phi),-Math.Cos(theta)*Math.Sin(phi)},
                {Math.Sin(theta)*Math.Cos(phi), Math.Sin(phi),Math.Cos(theta)*Math.Cos(phi)}
            }; 

            return rotationalMatrix;
        }
        public static double[] rotatePoints(double[,] rotationalMatrix, double[] pointA, double[] offset){
            //Move point by offset 
            pointA = new double[]{
                pointA[0]-offset[0],
                pointA[1]-offset[1],
                pointA[2]-offset[2]
            };
            //Multiply rotated point
            double[] outputPoint = new double[]{
                rotationalMatrix[0,0]*pointA[0]+rotationalMatrix[0,1]*pointA[1]+rotationalMatrix[0,2]*pointA[2],
                rotationalMatrix[1,0]*pointA[0]+rotationalMatrix[1,1]*pointA[1]+rotationalMatrix[1,2]*pointA[2],
                rotationalMatrix[2,0]*pointA[0]+rotationalMatrix[2,1]*pointA[1]+rotationalMatrix[2,2]*pointA[2],
            };
            return outputPoint;
        }
        public static double[][][] rotateTriangles(double[,] rotationalMatrix, gameObject renderableObject,double scale){
            vector3[] vertexHolder = new vector3[renderableObject.vertices.Length];
            for(int index = 0; index < vertexHolder.Length; index++){
                vertexHolder[index] = new vector3();
            }
            //int[][] triangleHolder = triangleList;
            //Each pixel has the info of {depth, interacted, color}
            //interacted values are 
            //  0.0 == not interacted
            //  1.0 == interacted
            
            /*
            for(int z = 0; z < rendererPipeline.screenInfo .Length; z++){
                rendererPipeline.screenInfo [z] = new double[rendererPipeline.ySize][];
                for(int x = 0; x < rendererPipeline.screenInfo [z].Length; x++){
                    rendererPipeline.screenInfo [z][x] = new double[5];
                }
            }
            */
            double[] tempPoint = new double[]{0.0,0.0,0.0};
            double[] deltaAB = new double[]{0.0,0.0,0.0};
            double[] deltaAC = new double[]{0.0,0.0,0.0};
            double[] targetPoint = new double[]{0.0,0.0,0.0};
            double distAB = 0.0;
            double distAC = 0.0;
            //Rotating all of the points of the object by the rotational matrix
            //For now the offset will be {0,0,0}

            for(int index = 0; index< renderableObject.vertices.Length; index++){
                tempPoint = rotatePoints(
                    rotationalMatrix, 
                    new double[]{
                        renderableObject.vertices[index].x+renderableObject.position.x,
                        renderableObject.vertices[index].y+renderableObject.position.y,
                        renderableObject.vertices[index].z+renderableObject.position.z
                    }, 
                    new double[]{0,0,0}
                );
                vertexHolder[index].x = tempPoint[0]*scale;
                vertexHolder[index].y = tempPoint[1]*scale;
                vertexHolder[index].z = tempPoint[2];
            }

            //Rendering the rotated triangles into the screen data 
            /*
            for(int index = 0; index < renderableObject.triangles.Length; index++){
                //Finding the deltaAB and deltaAC for this triangle 
                deltaAB = new double[]{
                    vertexHolder[renderableObject.triangles[index][1]][0]-vertexHolder[renderableObject.triangles[index][0]][0],
                    vertexHolder[renderableObject.triangles[index][1]][1]-vertexHolder[renderableObject.triangles[index][0]][1],
                    vertexHolder[renderableObject.triangles[index][1]][2]-vertexHolder[renderableObject.triangles[index][0]][2],
                };
                deltaAC = new double[]{
                    vertexHolder[renderableObject.triangles[index][2]][0]-vertexHolder[renderableObject.triangles[index][0]][0],
                    vertexHolder[renderableObject.triangles[index][2]][1]-vertexHolder[renderableObject.triangles[index][0]][1],
                    vertexHolder[renderableObject.triangles[index][2]][2]-vertexHolder[renderableObject.triangles[index][0]][2],
                };
                //Doing backface culling at this step
                if(deltaAB[0]*deltaAC[1] - deltaAC[0]*deltaAB[1] < 0){
                    continue;
                }
                //Finding the distance from point A and point B as well as from point A and point C
                distAB = Math.Sqrt(
                    deltaAB[0]*deltaAB[0]+
                    deltaAB[1]*deltaAB[1]+
                    deltaAB[2]*deltaAB[2]
                );
                distAC = Math.Sqrt(
                    deltaAC[0]*deltaAC[0]+
                    deltaAC[1]*deltaAC[1]+
                    deltaAC[2]*deltaAC[2]
                );
                //Going through all of the pixels in the triangle (using the i+j <= 1 rule)
                for(double i = 0; i <= 1; i += 1/(triangleDensity*distAB)){                    
                    for(double j = 0; j+i <= 1; j += 1/(triangleDensity*distAC)){
                        //Finding the point in 3d space
                        targetPoint = new double[]{
                            vertexHolder[renderableObject.triangles[index][0]][0]+(xSize/2)+i*deltaAB[0]+j*deltaAC[0],
                            vertexHolder[renderableObject.triangles[index][0]][1]+(ySize/2)+i*deltaAB[1]+j*deltaAC[1],
                            0
                        };
                        //Making sure the point is on the screen
                        if(
                            targetPoint[0]>0 && 
                            xSize>targetPoint[0] && 
                            targetPoint[1]>0 && 
                            ySize>targetPoint[1]
                        ){
                            //Calculating depth only if pixel is on screen
                            targetPoint[2] = vertexHolder[renderableObject.triangles[index][0]][2]+i*deltaAB[2]+j*deltaAC[2];
                            //Seeing if this pixel has been interacted and if depth is lower than current value
                            if(
                                screenInfo[(int)(screenResolution*targetPoint[0])][(int)(screenResolution*targetPoint[1])][1] != frameCounter || 
                                screenInfo[(int)(screenResolution*targetPoint[0])][(int)(screenResolution*targetPoint[1])][0] < targetPoint[2]
                            ){
                                //Setting the pixel to interacted and the depth value to the targetPoint's 
                                screenInfo[(int)(screenResolution*targetPoint[0])][(int)(screenResolution*targetPoint[1])] = new double[]{
                                    targetPoint[2],
                                    frameCounter,
                                    (int)(120*i+120*j),
                                    i,
                                    j
                                };
                            }
                        }
                    }
                }
                
            }
            */
            for(int index = 0; index < renderableObject.triangles.Length; index++){
                //Finding the deltaAB and deltaAC for this triangle 
                deltaAB = new double[]{
                    vertexHolder[renderableObject.triangles[index][1]].x-vertexHolder[renderableObject.triangles[index][0]].x,
                    vertexHolder[renderableObject.triangles[index][1]].y-vertexHolder[renderableObject.triangles[index][0]].y,
                    vertexHolder[renderableObject.triangles[index][1]].z-vertexHolder[renderableObject.triangles[index][0]].z,
                };
                deltaAC = new double[]{
                    vertexHolder[renderableObject.triangles[index][2]].x-vertexHolder[renderableObject.triangles[index][0]].x,
                    vertexHolder[renderableObject.triangles[index][2]].y-vertexHolder[renderableObject.triangles[index][0]].y,
                    vertexHolder[renderableObject.triangles[index][2]].z-vertexHolder[renderableObject.triangles[index][0]].z,
                };
                //Doing backface culling at this step
                if(deltaAB[0]*deltaAC[1] - deltaAC[0]*deltaAB[1] < 0){
                    continue;
                }
                drawLine(
                    vertexHolder[renderableObject.triangles[index][0]],
                    vertexHolder[renderableObject.triangles[index][1]]
                );
                drawLine(
                    vertexHolder[renderableObject.triangles[index][1]],
                    vertexHolder[renderableObject.triangles[index][2]]
                );
                drawLine(
                    vertexHolder[renderableObject.triangles[index][0]],
                    vertexHolder[renderableObject.triangles[index][2]]
                );
            }
            //returning the screen data
            return screenInfo;
        } 
        public static void drawLine(vector3 pointA, vector3 pointB){
            vector3 interactedPoint = new vector3(0,0,0);
            double rateOfChange;
            int inverse;
            if(Math.Abs(pointB.x-pointA.x)>=Math.Abs(pointB.y-pointA.y)){
                rateOfChange = (pointB.y-pointA.y)/(pointB.x-pointA.x);
                inverse = Math.Sign(pointB.x-pointA.x);
                for(int index = 0; index<Math.Abs(pointB.x-pointA.x); index++){
                    interactedPoint = new vector3(
                        inverse*Math.Floor((double)index)+Math.Floor(pointA.x)+xSize/2,
                        inverse*Math.Floor(rateOfChange*index)+Math.Floor(pointA.y)+ySize/2,
                        index/Math.Abs(pointB.x-pointA.x)*pointA.z+
                        (1-index/Math.Abs(pointB.x-pointA.x))*pointB.z
                    );
                    screenInfo[(int)interactedPoint.x][(int)interactedPoint.y] = new double[]{
                        interactedPoint.z,
                        frameCounter,
                        0,
                        0,
                        0
                    };
                }
                
            }else{
                rateOfChange = (pointB.x-pointA.x)/(pointB.y-pointA.y);
                inverse = Math.Sign(pointB.y-pointA.y);
                for(int index = 0; index<Math.Abs(pointB.y-pointA.y); index++){
                    interactedPoint = new vector3(
                        inverse*Math.Floor(rateOfChange*index)+Math.Floor(pointA.x)+xSize/2,
                        inverse*Math.Floor((double)index)+Math.Floor(pointA.y)+ySize/2,
                        index/Math.Abs(pointB.x-pointA.x)*pointA.z+
                        (1-index/Math.Abs(pointB.x-pointA.x))*pointB.z
                    );
                    screenInfo[(int)interactedPoint.x][(int)interactedPoint.y] = new double[]{
                        interactedPoint.z,
                        frameCounter,
                        0,
                        0,
                        0
                    };
                }
                
            }
        }

        public static Image renderToScreen(double[][][] screenInfo){
            Bitmap screenImage = new Bitmap(xSize, ySize);
            //Creating the tempColor holder that will be used to color the triangles
            Color tempColor = Color.FromArgb(255,0,0,0);
            for(int x = 0; x< xSize; x++){
                //screenOutput[x] = new int[screenInfo[x].Length][];
                for(int y = 0; y< ySize; y++){
                    //screenOutput[x][y] = new int[screenInfo[x][y].Length];
                    if(screenInfo[(int)(screenResolution*x)][(int)(screenResolution*y)][1] == frameCounter){
                        //Pulling the color from screenData
                        tempColor = Color.FromArgb(
                            255,
                            (int)screenInfo[(int)(screenResolution*x)][(int)(screenResolution*y)][2],
                            (int)screenInfo[(int)(screenResolution*x)][(int)(screenResolution*y)][2],
                            (int)screenInfo[(int)(screenResolution*x)][(int)(screenResolution*y)][2]
                        );
                        screenImage.SetPixel(x,y,tempColor);
                    }else{
                        screenInfo[(int)(screenResolution*x)][(int)(screenResolution*y)] = new double[5];
                        //If the pixel has not been interacted with, then set color to white
                        //tempColor = Color.FromArgb(255,255,255,255);
                        //screenImage.SetPixel(x,y,tempColor);
                    }
                }
            }
            Image output = (Image)screenImage;
            return output;
        }
	}
    public class gameObject{
        public string name;
        public vector3[] vertices;
        public int[][] triangles;
        public int[] triangleColors;
        public vector3[] faceNormals;

        public vector3 position;

        public gameObject(){
            //Empty Case
        }
        public gameObject(string filePath){
            //If the file type is not obj then throw an exception
            if(
                Char.ToString(filePath[filePath.Length-3])+
                Char.ToString(filePath[filePath.Length-2])+
                Char.ToString(filePath[filePath.Length-1]) != "obj"
                ){
                throw new Exception("Cannot import: file format "+filePath[filePath.Length-3]+filePath[filePath.Length-2]+filePath[filePath.Length-1]+" is not supported ");
            }
            //Getting the contents of the obj file into a line by line format
            string[] fileContents = File.ReadAllText(filePath).Split("\n");
            //Creating lists to hold the triangles and vertices we find
            List<vector3> foundVertices = new List<vector3>();
            List<int[]> foundTriangles = new List<int[]>();
            List<vector3> foundNormals = new List<vector3>();
            //Creating array that will hold the contents of a line when we remove all of the spaces
            string[] splitLineContents;
            //Array of bools that indicate if a node has 
            List<int> nodeIndex = new List<int>();
            //Checking each line for which starting characters they have and acting accordingly 
            for(int index = 0; index<fileContents.Length-1; index++){
                //We check the first two characters to make sure we don't misread anything
                if(
                    Char.ToString(fileContents[index][0])+
                    Char.ToString(fileContents[index][1])=="o "){
                    name = fileContents[index].Split(" ")[1];
                }
                if(
                    Char.ToString(fileContents[index][0])+
                    Char.ToString(fileContents[index][1])=="vn"){
                    splitLineContents = fileContents[index].Split(" ");
                    foundNormals.Add(new vector3(
                        Convert.ToDouble(splitLineContents[1]),
                        Convert.ToDouble(splitLineContents[2]),
                        Convert.ToDouble(splitLineContents[3])
                    ));
                }
                if(
                    Char.ToString(fileContents[index][0])+
                    Char.ToString(fileContents[index][1])=="v "){
                    splitLineContents = fileContents[index].Split(" ");
                    foundVertices.Add(new vector3(
                        Convert.ToDouble(splitLineContents[1]),
                        Convert.ToDouble(splitLineContents[2]),
                        Convert.ToDouble(splitLineContents[3])
                    ));
                }
                if(
                    Char.ToString(fileContents[index][0])+
                    Char.ToString(fileContents[index][1])=="f "){
                    splitLineContents = fileContents[index].Split(" ");
                    nodeIndex.Clear();
                    for(int ni = 1; ni<splitLineContents.Length; ni++){
                        nodeIndex.Add(ni);
                    }
                    while(nodeIndex.Count > 2){
                        foundTriangles.Add(new int[]{
                            Convert.ToInt32(splitLineContents[nodeIndex[0]].Split("/")[0])-1,
                            Convert.ToInt32(splitLineContents[nodeIndex[1]].Split("/")[0])-1,
                            Convert.ToInt32(splitLineContents[nodeIndex[2]].Split("/")[0])-1                        
                        });
                        nodeIndex.RemoveAt(1);
                    }
                    
                }
            }
            vertices = foundVertices.ToArray();
            triangles = foundTriangles.ToArray();
            triangleColors = new int[triangles.Length];
            for(int index = 0; index<triangles.Length; index++){
                triangleColors[index] = 120;
            }
            position = new vector3(0,0,0);
        }
    }
    public class component{
        //Component Class for future ECS system
    }
    public class vector3{
        public double x;
        public double y;
        public double z;
        public vector3(){
            //Empty Case
        }
        public vector3(double x, double y, double z){
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public static vector3 add(vector3 first, vector3 second){
            return new vector3(
                first.x + second.x,
                first.y + second.y,
                first.z + second.z
            );
        }
        public static vector3 subtract(vector3 first, vector3 second){
            return new vector3(
                first.x - second.x,
                first.y - second.y,
                first.z - second.z
            );
        }
        public void addThis(vector3 first){
            x += first.x;
            y += first.y;
            z += first.z;
        }
        public void subtractThis(vector3 first){
            x -= first.x;
            y -= first.y;
            z -= first.z;
        }
    }
    public partial class MyForm : Form
    {
        //private rendererPipeline render;
        private System.Windows.Forms.Timer timer;
        private int intervalMilliseconds = (int)(1000/60); // Change this to set the interval in milliseconds
        private Random random = new Random();
        private PictureBox pictureBox1;
        private double[,] rotationalMatrix;
        private gameObject renderObject;
        private double scale;
        private TrackBar trackBar1;
        private TrackBar trackBar2;

        public MyForm(double[,] rotationalMatrix, gameObject renderObject,double scale)
        {
            //render = new rendererPipeline();
            this.rotationalMatrix = rotationalMatrix;
            this.renderObject = renderObject;
            this.scale = scale;
            //InitializeComponent();
            pictureBox1 = new PictureBox();
            //pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;
            for(int z = 0; z < rendererPipeline.screenInfo.Length; z++){
                rendererPipeline.screenInfo[z] = new double[(int)(rendererPipeline.ySize*rendererPipeline.screenResolution)][];
                for(int x = 0; x < rendererPipeline.screenInfo[z].Length; x++){
                    rendererPipeline.screenInfo [z][x] = new double[5];
                }
            }
            InitializeComponent();
            InitializeTimer();
        }
        private void InitializeComponent()
        {
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            // Create a new TrackBar control
            trackBar1 = new TrackBar();

            // Set the properties of the TrackBar
            trackBar1.Minimum = -180;
            trackBar1.Maximum = 180;
            trackBar1.Value = 0; // Initial value
            trackBar1.TickStyle = TickStyle.TopLeft;
            trackBar1.TickFrequency = 10;
            trackBar1.Width = 200;
            trackBar1.Location = new System.Drawing.Point(0, 400);

            // Create a new TrackBar control
            trackBar2 = new TrackBar();

            // Set the properties of the TrackBar
            trackBar2.Minimum = -180;
            trackBar2.Maximum = 180;
            trackBar2.Value = 0; // Initial value
            trackBar2.TickStyle = TickStyle.TopLeft;
            trackBar2.TickFrequency = 10;
            trackBar2.Width = 200;
            trackBar2.Location = new System.Drawing.Point(200, 400);

            // Add the TrackBar to the form's Controls collection
            Controls.Add(trackBar1);
            Controls.Add(trackBar2);

            // PictureBox
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(400, 400);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            
            // Form
            this.Controls.Add(this.pictureBox1);
            this.ClientSize = new System.Drawing.Size(400, 440);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
        }

        private void InitializeTimer()
        {
            timer = new System.Windows.Forms.Timer();
            timer.Interval = intervalMilliseconds;
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        // Timer tick event handler
        private void Timer_Tick(object sender, EventArgs e)
        {
            // Call the function to generate the bitmap
            //Bitmap originalImage = GenerateBitmap();

            // Call the edit function
            //Bitmap editedImage = EditPicture(originalImage);
            //renderObject.position.y += 0.001;
            rendererPipeline.frameCounter += 1;
            if(rendererPipeline.frameCounter >= 60){
                rendererPipeline.frameCounter = 1;
            }
            rendererPipeline.triangleDensity += 0.001;
            if(rendererPipeline.triangleDensity >= 1.5){
                rendererPipeline.triangleDensity = .1;
            }
            rotationalMatrix = rendererPipeline.rotationMatrixGenerator(trackBar1.Value,trackBar2.Value);
            rendererPipeline.rotateTriangles(rotationalMatrix,renderObject, scale);
            Image frame = rendererPipeline.renderToScreen(rendererPipeline.screenInfo);

            // Update the PictureBox with the edited image
            pictureBox1.Image = frame;
        }
    }
}

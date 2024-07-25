using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Numerics;
using System.IO.Compression;
using System.Reflection.Metadata.Ecma335;
using System.Drawing.Drawing2D;
using System.Diagnostics.Eventing.Reader;

namespace PenroseEngine{
    public class rendererPipeline
    {
        public static int xSize = 400;
        public static int ySize = 400;
        public static double screenResolution = 1;
        public static double triangleDensity = 1.5;
        public static double[][][] screenInfo = new double[(int)(screenResolution*xSize)][][];
        public static double frameCounter = 0;
        public static int rowAssignments = 0;
        public static long totalTimeTaken = 0;
        public static long lastMiliCheck = 0;
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
            vector3 deltaAB;
            vector3 deltaAC;
            double[] targetPoint = new double[]{0.0,0.0,0.0};
            rowComponent lineData;
            //List<rowComponent> linesData = new List<rowComponent>();
            int lowestY;
            int highestY;
            //Rotating all of the points of the object by the rotational matrix
            //For now the offset will be {0,0,0}

            for(int index = 0; index< renderableObject.vertices.Length; index++){
                tempPoint = rotatePoints(
                    rotationalMatrix, 
                    new double[]{
                        (renderableObject.vertices[index].x*renderableObject.scale.x)+renderableObject.position.x,
                        (renderableObject.vertices[index].y*renderableObject.scale.y)+renderableObject.position.y,
                        (renderableObject.vertices[index].z*renderableObject.scale.z)+renderableObject.position.z
                    }, 
                    new double[]{0,0,0}
                );
                vertexHolder[index].x = tempPoint[0]*scale;
                vertexHolder[index].y = tempPoint[1]*scale;
                vertexHolder[index].z = tempPoint[2];
            }

            for(int index = 0; index < renderableObject.triangles.Length; index++){
                //Finding the deltaAB and deltaAC for this triangle 
                deltaAB = vector3.subtract(
                    vertexHolder[renderableObject.triangles[index][1]], 
                    vertexHolder[renderableObject.triangles[index][0]]
                );
                deltaAC = vector3.subtract(
                    vertexHolder[renderableObject.triangles[index][2]], 
                    vertexHolder[renderableObject.triangles[index][0]]
                );
                //Doing backface culling at this step
                if(deltaAB.x*deltaAC.y - deltaAC.x*deltaAB.y >= 0){
                    continue;
                }

                //Please rewrite
                //This code finds the highest and lowest y on the triangle
                lowestY = ySize;
                highestY = -1;
                if(vertexHolder[renderableObject.triangles[index][0]].y+ySize/2 > 0){
                    highestY = (int)vertexHolder[renderableObject.triangles[index][0]].y+ySize/2;
                    lowestY = (int)vertexHolder[renderableObject.triangles[index][0]].y+ySize/2;
                }
                if(
                    highestY < vertexHolder[renderableObject.triangles[index][1]].y+ySize/2
                ) 
                highestY = (int)vertexHolder[renderableObject.triangles[index][1]].y+ySize/2;
                if(
                    lowestY > vertexHolder[renderableObject.triangles[index][1]].y+ySize/2
                ) 
                lowestY = (int)vertexHolder[renderableObject.triangles[index][1]].y+ySize/2;

                if(
                    highestY < vertexHolder[renderableObject.triangles[index][2]].y+ySize/2
                ) 
                highestY = (int)vertexHolder[renderableObject.triangles[index][2]].y+ySize/2;
                if(
                    lowestY > vertexHolder[renderableObject.triangles[index][2]].y+ySize/2 
                ) 
                lowestY = (int)vertexHolder[renderableObject.triangles[index][2]].y+ySize/2;
                
                if(
                    (lowestY < 0 && 
                    highestY < 0) ||
                    (lowestY > ySize && 
                    highestY > ySize) 
                ) continue;
                
                lastMiliCheck = DateTime.Now.Ticks;
                
                for(int row = lowestY-1; row < highestY+1; row++){
                    if(row < 0 || row >= ySize)continue;
                    rowAssignments++;
                    //lastMiliCheck = DateTime.Now.Ticks;
                    lineData = getRowFromTriangle(
                        vector3.add(vertexHolder[renderableObject.triangles[index][0]], new (xSize/2,ySize/2,0)),
                        deltaAB,
                        deltaAC,
                        row,
                        index
                    );        
                          
                    for(int i = lineData.rowStart; i < lineData.rowEnd; i++){
                        if(i >= xSize || i < 0) continue;
                        /*
                        screenInfo[i][row] = new double[]{
                            0,//lineData.depthStart + (lineData.depthEnd-lineData.depthStart)*rateChange,
                            frameCounter,
                            0,
                            0,//lineData.iStart + (lineData.iEnd-lineData.iStart)*rateChange,
                            0 //lineData.jStart + (lineData.jEnd-lineData.jStart)*rateChange
                        };
                        */
                        screenInfo[i][row][1] = frameCounter;
                    }
                }
                totalTimeTaken += DateTime.Now.Ticks - lastMiliCheck;  
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
                    if(
                        interactedPoint.x < 0 || 
                        interactedPoint.x >= xSize || 
                        interactedPoint.y < 0 || 
                        interactedPoint.y >= ySize
                    ) continue;
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
                    if(
                        interactedPoint.x < 0 || 
                        interactedPoint.x >= xSize || 
                        interactedPoint.y < 0 || 
                        interactedPoint.y >= ySize
                    ) return;
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
        public static rowComponent getRowFromTriangle(
                vector3 pointA, 
                vector3 deltaB, 
                vector3 deltaC, 
                int row,
                int triangleID
            ){
            //This is written badly, I need to rewrite 

            //Please rework this code
            double iIntersectActual;
            double jIntersectActual;
            double iPlusJIntersectActual;

            rowComponent lineData = new rowComponent(){
                rowStart = xSize,
                rowEnd = -1,
                triangleID = triangleID
            };

            //Where the line intersects with the pure i side of the triangle
            double iIntersect;
            //Where the line intersects with the pure j side of the triangle
            double jIntersect;
            //Where the line intersects with the i+j side of the triangle
            double iPlusJIntersect;
            
            //Checks are in place to prevent devide by 0 error
            if(deltaB.y != 0) {
                iIntersect =  (row - pointA.y)/deltaB.y;
                if(iIntersect >= 0 && iIntersect < 1){
                    iIntersectActual = deltaB.x * iIntersect + pointA.x;
                    if(iIntersectActual<lineData.rowStart) lineData.rowStart = (int)iIntersectActual;
                    if(iIntersectActual>lineData.rowEnd) lineData.rowEnd = (int)iIntersectActual;                    
                }                
            }
            if(deltaC.y != 0){
                jIntersect =  (row - pointA.y)/deltaC.y;
                if(jIntersect >= 0 && jIntersect < 1){
                    jIntersectActual = deltaC.x * jIntersect + pointA.x;
                    if(jIntersectActual<lineData.rowStart) lineData.rowStart = (int)jIntersectActual;
                    if(jIntersectActual>lineData.rowEnd) lineData.rowEnd = (int)jIntersectActual;
                }
                
            } 
            //Gives the i intersect on the i+j = 1 axis
            //Compute 1 - i to find j
            if(deltaC.y-deltaB.y  != 0){
                iPlusJIntersect = (row - pointA.y - deltaB.y ) / (deltaC.y-deltaB.y);
                if(
                    iPlusJIntersect >= 0 && 
                    iPlusJIntersect < 1
                ){
                    iPlusJIntersectActual = iPlusJIntersect*deltaC.x + deltaB.x*(1-iPlusJIntersect) + pointA.x;
                    if(iPlusJIntersectActual<lineData.rowStart) lineData.rowStart = (int)iPlusJIntersectActual;
                    if(iPlusJIntersectActual>lineData.rowEnd) lineData.rowEnd = (int)iPlusJIntersectActual;
                }
                
            } 
            //If all the deltas are 0 then return
            if(
                lineData.rowEnd == -1 &&
                lineData.rowStart == xSize
            )return new rowComponent();
            //lineData.calculateIntercepts();
            lineData.triangleID = triangleID;
            return lineData;
        }   
        public static Image renderToScreen(double[][][] screenInfo){
            Bitmap screenImage = new Bitmap(xSize, ySize);
            //Creating the tempColor holder that will be used to color the triangles
            Color tempColor;
            int screenX;
            int screenY;
            for(int x = 0; x< xSize; x++){
                screenX = (int)(screenResolution*x);
                for(int y = 0; y< ySize; y++){
                    screenY = (int)(screenResolution*y);
                    if(screenInfo[screenX][screenY][1] == frameCounter){
                        //Pulling the color from screenData
                        tempColor = Color.FromArgb(
                            255,
                            (int)screenInfo[screenX][screenY][2],
                            (int)screenInfo[screenX][screenY][2],
                            (int)screenInfo[screenX][screenY][2]
                        );
                        screenImage.SetPixel(x,y,tempColor);
                    }else{
                        screenInfo[screenX][screenY][1] = -1;
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

        public vector3 scale;

        public gameObject(){
            //Empty Case
        }
        public gameObject(string filePath){
            //If the file type is not obj then throw an exception
            if(filePath == "..\\..\\..\\Objects\\"){
                vertices = new vector3[0];
                triangles = new int[0][];
                triangleColors = new int[triangles.Length];
                for(int index = 0; index<triangles.Length; index++){
                    triangleColors[index] = 120;
                }
                position = new (0,0,0);
                return;
            }
            scale = new vector3(1,1,1);
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
        public static vector3 scale(double scale, vector3 first){
            return new(
                first.x * scale,
                first.y * scale,
                first.z * scale
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
    public class rowComponent{
        //Where in the row does our section start
        public int rowStart = -1;
        //Where in the row does our section end
        public int rowEnd = -1;
        //What is the i value at the start of the section
        public double iStart;
        //What is the i value at the end of the section
        public double iEnd;
        //What is the j value at the start of the section
        public double jStart;
        //What is the j value at the end of the section
        public double jEnd;
        //What is the ID of the triangle that this section is from
        public int triangleID;
        //What is the depth value at the start of the section
        public double depthStart;
        //What is the depth value at the end of the section
        public double depthEnd;
        //What is the rate that the depth value changes over the length of the
        //line segment 
        public double riseOverRun;
        //Where does our rate of change cross the y axis
        public double yIntercept;
        public rowComponent(){
            //Empty Class
        }
        public rowComponent(rowComponent target){
            //This method is for copying other rowComponents
            rowStart = target.rowStart;
            rowEnd = target.rowEnd;
            iStart = target.iStart;
            iEnd = target.iEnd;
            jStart = target.jStart;
            jEnd = target.jEnd;
            triangleID = target.triangleID;
            depthStart = target.depthStart;
            depthEnd = target.depthEnd;
            calculateIntercepts();
        }
        //Calcualte the x and y intercepts for the line section
        //This is used to find out where two line segments cross.
        public Boolean calculateIntercepts(){
            if(rowEnd-rowStart == 0) return false;
            riseOverRun = (depthEnd-depthStart)/(rowEnd-rowStart);
            yIntercept = depthStart-rowStart*riseOverRun;
            return true;
        }
    }
    public class lineContainer{
        public int lineStart;
        public int lineEnd;
        public List<rowComponent> lines;
        public lineContainer(){
            //Empty Case
        }

        public Boolean checkIntersection(rowComponent line){
            //Check to make sure our line is not of 0 length
            if(line.rowStart - line.rowEnd == 0) return false;
            //Variable to hold the intersection point between the lines
            double crossingPoint;
            //Where is the left side of our section
            double leftIntersect;
            //Where is the right side of our section
            double rightIntersect;
            //Is our intersect in our section
            Boolean insideLine;
            //Variable for the left hand side of the section
            rowComponent leftLine;
            //Variable for the right hand side of the section
            rowComponent rightLine;
            //We check to make sure that either end of our line falls within our lineContainer
            if(lineStart <= line.rowStart || lineEnd >= line.rowEnd){
                foreach(rowComponent containedLine in lines){
                    //Setting insideLine to false at beginning of loop
                    insideLine = false;
                    //Resetting line variables
                    leftLine = new rowComponent();
                    rightLine = new rowComponent();
                    //Finding the left and right sides of our section
                    leftIntersect = (
                        Math.Abs(line.rowStart-containedLine.rowStart) + 
                        line.rowStart + containedLine.rowStart
                    )/2;
                    rightIntersect = (
                        line.rowEnd + containedLine.rowEnd - 
                        Math.Abs(containedLine.rowEnd-line.rowEnd)
                    )/2;
                    //If leftIntersect is greater than right, then continue to next interation
                    if(rightIntersect<leftIntersect)continue;
                    //Make sure that (line.riseOverRun-containedLine.riseOverRun) is not 0 
                    //If it is 0 then our lines are parallel
                    if(line.riseOverRun-containedLine.riseOverRun != 0){
                        //Finding the point of crossing
                        crossingPoint = (containedLine.yIntercept-line.yIntercept)/
                                        (line.riseOverRun-containedLine.riseOverRun);
                        //Checking if our intersect is inside of our section
                        if(leftIntersect<crossingPoint && rightIntersect>crossingPoint){
                            leftLine = new rowComponent{
                                rowStart = (int)leftIntersect,
                                rowEnd = (int)crossingPoint,
                                triangleID = -1
                            };
                            rightLine = new rowComponent{
                                rowStart = (int)crossingPoint,
                                rowEnd = (int)rightIntersect,
                                triangleID = -1
                            };
                            insideLine = true;
                        }
                    }
                    //If our intersect is not in our section or our lines are parallel
                    if(!insideLine){
                        leftLine = new rowComponent{
                            rowStart = (int)leftIntersect,
                            rowEnd = (int)rightIntersect,
                            triangleID = -1
                        };
                    }
                    //Checking if our line is on top to the left
                    if(line.riseOverRun-containedLine.riseOverRun > 0){
                        leftLine.triangleID = line.triangleID;
                        leftLine.riseOverRun = line.riseOverRun;
                        leftLine.yIntercept = line.yIntercept;

                        rightLine.triangleID = containedLine.triangleID;
                        rightLine.riseOverRun = containedLine.riseOverRun;
                        rightLine.yIntercept = containedLine.yIntercept;
                    }
                    //Checking if our containedLine is on top to the left
                    else if(line.riseOverRun-containedLine.riseOverRun < 0){
                        leftLine.triangleID = containedLine.triangleID;
                        leftLine.riseOverRun = containedLine.riseOverRun;
                        leftLine.yIntercept = containedLine.yIntercept;

                        rightLine.triangleID = line.triangleID;
                        rightLine.riseOverRun = line.riseOverRun;
                        rightLine.yIntercept = line.yIntercept;
                    }
                    //If our lines are parallel, check which line has a higher depth value
                    else{
                        if(line.yIntercept > containedLine.yIntercept){
                            leftLine.triangleID = line.triangleID;
                            leftLine.riseOverRun = line.riseOverRun;
                            leftLine.yIntercept = line.yIntercept;
                        }else{
                            leftLine.triangleID = containedLine.triangleID;
                            leftLine.riseOverRun = containedLine.riseOverRun;
                            leftLine.yIntercept = containedLine.yIntercept;
                        }
                    }
                    //Now we know what line is on top where
                }
                return true;
            }
            return false;
        }
    }
    public partial class MyForm : Form
    {
        //private rendererPipeline render;
        private System.Windows.Forms.Timer timer;
        private int intervalMilliseconds = (int)(1000/120); // Change this to set the interval in milliseconds
        private Random random = new Random();
        private PictureBox pictureBox1;
        private double[,] rotationalMatrix;
        private gameObject[] renderObjects;
        private double scale;
        private TrackBar trackBar1;
        private TrackBar trackBar2;
        private TrackBar trackBar3;

        private long lastMiliCheck;
        private double rowAssignmentTimer = 0;
        private double rowAssignmentCounter = 0;
        private int highestRowAssignment = 0;
        private long renderToScreenTimer = 0;
        private long totalRenderTime = 0;

        public MyForm(double[,] rotationalMatrix, gameObject[] renderObjects,double scale)
        {
            //render = new rendererPipeline();
            this.rotationalMatrix = rotationalMatrix;
            this.renderObjects = renderObjects;
            this.scale = scale;
            lastMiliCheck = DateTime.Now.Ticks/TimeSpan.TicksPerMillisecond;
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
            trackBar1 = new TrackBar
            {
                // Set the properties of the TrackBar
                Minimum = -180,
                Maximum = 180,
                Value = 0, // Initial value
                TickStyle = TickStyle.TopLeft,
                TickFrequency = 10,
                Width = 200,
                Location = new System.Drawing.Point(0, 400)
            };

            // Create a new TrackBar control
            trackBar2 = new TrackBar
            {
                // Set the properties of the TrackBar
                Minimum = -180,
                Maximum = 180,
                Value = 0, // Initial value
                TickStyle = TickStyle.TopLeft,
                TickFrequency = 10,
                Width = 200,
                Location = new System.Drawing.Point(200, 400)
            };

            trackBar3 = new TrackBar
            {
                // Set the properties of the TrackBar
                Minimum = 1,
                Maximum = 100,
                Value = 1, // Initial value
                TickStyle = TickStyle.TopLeft,
                TickFrequency = 10,
                Width = 200,
                Location = new System.Drawing.Point(0, 440)
            };
            // Add the TrackBar to the form's Controls collection
            Controls.Add(trackBar1);
            Controls.Add(trackBar2);
            Controls.Add(trackBar3);

            // PictureBox
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(400, 400);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            
            // Form
            this.Controls.Add(this.pictureBox1);
            this.ClientSize = new System.Drawing.Size(400, 480);
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
            rendererPipeline.frameCounter += 1;
            rendererPipeline.rowAssignments = 0;
            rendererPipeline.totalTimeTaken = 0;
            renderToScreenTimer = 0;
            if(DateTime.Now.Ticks/TimeSpan.TicksPerMillisecond - lastMiliCheck > 1000){
                Console.WriteLine(
                    "Average time for row assignments in ticks "+(rowAssignmentTimer/TimeSpan.TicksPerMillisecond/rendererPipeline.frameCounter)+"\n "+
                    "Average row assignments "+(rowAssignmentCounter/rendererPipeline.frameCounter)+"\n "+
                    //"Total average time in milliseconds "+((rowAssignmentCounter*rowAssignmentTimer)/(rendererPipeline.frameCounter*10000))+"\n "+
                    "Highest number of row assignments "+highestRowAssignment+"\n "+
                    "Total Row Assignment time "+(rowAssignmentTimer/TimeSpan.TicksPerMillisecond)+"\n "+
                    "Total Render to screen time "+(totalRenderTime/TimeSpan.TicksPerMillisecond)
                );
                Console.WriteLine(rendererPipeline.frameCounter);
                lastMiliCheck = DateTime.Now.Ticks/TimeSpan.TicksPerMillisecond;
                rendererPipeline.frameCounter = 1;
                rowAssignmentTimer = 0;
                rowAssignmentCounter = 0;
                highestRowAssignment = 0;
                totalRenderTime = 0;
            }

            scale = trackBar3.Value;
            rotationalMatrix = rendererPipeline.rotationMatrixGenerator(trackBar1.Value,trackBar2.Value);
            foreach(gameObject item in renderObjects)
            rendererPipeline.rotateTriangles(rotationalMatrix,item, scale);
            renderToScreenTimer = DateTime.Now.Ticks;
            Image frame = rendererPipeline.renderToScreen(rendererPipeline.screenInfo);
            totalRenderTime += (DateTime.Now.Ticks-renderToScreenTimer);
            rowAssignmentTimer += rendererPipeline.totalTimeTaken;
            rowAssignmentCounter += rendererPipeline.rowAssignments;
            if(highestRowAssignment < rendererPipeline.rowAssignments) highestRowAssignment = rendererPipeline.rowAssignments;
            // Update the PictureBox with the edited image
            pictureBox1.Image = frame;
        }
    }
}

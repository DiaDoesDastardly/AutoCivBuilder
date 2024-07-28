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
using System.ComponentModel;

namespace PenroseEngine{
    public class rendererPipeline
    {
        public static int xSize = 400;
        public static int ySize = 400;
        public static double screenResolution = .75;
        public static double[][][] screenInfo = new double[(int)(xSize)][][];
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

            double[] tempPoint = new double[]{0.0,0.0,0.0};
            vector3 deltaAB;
            vector3 deltaAC;
            vector3 normalDelta;
            rowComponent lineData = new rowComponent();
            //List<rowComponent> linesData = new List<rowComponent>();
            int lowestY;
            int highestY;
            int lowestX;
            int highestX;
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
                normalDelta = vector3.scale(.5,vector3.add(deltaAB, deltaAC));
                normalDelta.scale(1/Math.Sqrt(
                    normalDelta.x*normalDelta.x + 
                    normalDelta.y*normalDelta.y + 
                    normalDelta.z*normalDelta.z
                ));
                //Doing backface culling at this step
                if(deltaAB.x*deltaAC.y - deltaAC.x*deltaAB.y >= 0){
                    continue;
                }

                //Creating a modified pointA so we don't have to do it multiple times per frame
                vector3 deltaA = vector3.add(vertexHolder[renderableObject.triangles[index][0]], new (xSize/2,ySize/2,0));
                vector3 weh = vector3.add(vertexHolder[renderableObject.triangles[index][1]], new (xSize/2,ySize/2,0));

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

                lowestX = xSize;
                highestX = -1;
                if(vertexHolder[renderableObject.triangles[index][0]].x+xSize/2 > 0){
                    highestX = (int)vertexHolder[renderableObject.triangles[index][0]].x+xSize/2;
                    lowestX = (int)vertexHolder[renderableObject.triangles[index][0]].x+xSize/2;
                }
                if(
                    highestX < vertexHolder[renderableObject.triangles[index][1]].x+xSize/2
                ) 
                highestX = (int)vertexHolder[renderableObject.triangles[index][1]].x+xSize/2;
                if(
                    lowestX > vertexHolder[renderableObject.triangles[index][1]].x+xSize/2
                ) 
                lowestX = (int)vertexHolder[renderableObject.triangles[index][1]].x+xSize/2;

                if(
                    highestX < vertexHolder[renderableObject.triangles[index][2]].x+xSize/2
                ) 
                highestX = (int)vertexHolder[renderableObject.triangles[index][2]].x+xSize/2;
                if(
                    lowestX > vertexHolder[renderableObject.triangles[index][2]].x+xSize/2 
                ) 
                lowestX = (int)vertexHolder[renderableObject.triangles[index][2]].x+xSize/2;
                
                if(
                    (lowestX < 0 && 
                    highestX < 0) ||
                    (lowestX > xSize && 
                    highestX > xSize) 
                ) continue;

                
                double rateChange;
                double depthCheck;
                /*
                vector3 normalBC = vector3.normalize(vector3.subtract(deltaAC,deltaAB));
                vector3 normalAB = vector3.normalize(deltaAB);
                vector3 normalAC = vector3.normalize(deltaAC);

                double changeInDepthA = normalBC.x/normalBC.y; 
                double yInterceptA = (normalBC.y*weh.x-normalBC.x*weh.y)/normalBC.y;
                
                double changeInDepthAB = normalAB.x/normalAB.y; 
                double yInterceptAB = (normalAB.y*deltaA.x-normalAB.x*deltaA.y)/normalAB.y;

                double changeInDepthAC = normalAC.x/normalAC.y; 
                double yInterceptAC = (normalAC.y*deltaA.x-normalAC.x*deltaA.y)/normalAC.y;
                */
                /*
                changeInDepth = 
                        (1/normalAB.y*normalAB.z-1/normalAC.y*normalAC.z)/
                        (1/normalAB.y*normalAB.x-1/normalAC.y*normalAC.x); 
                */
                
                for(int row = (int)(lowestY*screenResolution)-1; row < (int)(highestY*screenResolution)+1; row++){
                    if(row < 0 || row >= ySize*screenResolution)continue;
                    //yIntercept = (row - vertexHolder[renderableObject.triangles[index][0]].y)/normalAC.y*normalAC.z;
                    rowAssignments++;
                    //lastMiliCheck = DateTime.Now.Ticks;
                    
                    lineData = getRowFromTriangle(
                        deltaA,
                        deltaAB,
                        deltaAC,
                        (int)(row/screenResolution),
                        index
                    );  
                    
                    /*
                    lineData.rowStart = xSize;
                    lineData.rowEnd = -1;
                    if(
                        lineData.rowStart > changeInDepthA * row + yInterceptA &&
                        changeInDepthA * row + yInterceptA >= lowestX &&
                        changeInDepthA * row + yInterceptA <= highestX
                    )
                    lineData.rowStart = (int)(changeInDepthA * row + yInterceptA);
                    if(
                        lineData.rowEnd < changeInDepthA * row + yInterceptA &&
                        changeInDepthA * row + yInterceptA >= lowestX &&
                        changeInDepthA * row + yInterceptA <= highestX
                    )
                    lineData.rowEnd = (int)(changeInDepthA * row + yInterceptA);

                    if(
                        lineData.rowStart > changeInDepthAB * row + yInterceptAB &&
                        changeInDepthAB * row + yInterceptAB >= lowestX &&
                        changeInDepthAB * row + yInterceptAB <= highestX
                    )
                    lineData.rowStart = (int)(changeInDepthAB * row + yInterceptAB);
                    if(
                        lineData.rowEnd < changeInDepthAB * row + yInterceptAB &&
                        changeInDepthAB * row + yInterceptAB >= lowestX &&
                        changeInDepthAB * row + yInterceptAB <= highestX
                    )
                    lineData.rowEnd = (int)(changeInDepthAB * row + yInterceptAB);

                    if(
                        lineData.rowStart > changeInDepthAC * row + yInterceptAC &&
                        changeInDepthAC * row + yInterceptAC >= lowestX &&
                        changeInDepthAC * row + yInterceptAC <= highestX
                    )
                    lineData.rowStart = (int)(changeInDepthAC * row + yInterceptAC);
                    if(
                        lineData.rowEnd < changeInDepthAC * row + yInterceptAC &&
                        changeInDepthAC * row + yInterceptAC >= lowestX &&
                        changeInDepthAC * row + yInterceptAC <= highestX
                    )
                    lineData.rowEnd = (int)(changeInDepthAC * row + yInterceptAC);
                    */
                    //if(lineData.rowStart == lineData.rowEnd) continue;
                    //if(lineData.rowStart >= xSize || lineData.rowStart < 0) continue;
                    //if(lineData.rowEnd >= xSize || lineData.rowEnd < 0) continue;    
                    
                    for(int i = (int)(lineData.rowStart*screenResolution); i < (int)(lineData.rowEnd*screenResolution); i++){
                        if(i >= xSize*screenResolution || i < 0) continue;
                        rateChange = (i/screenResolution-lineData.rowStart)/(lineData.rowEnd-lineData.rowStart);
                        depthCheck = lineData.depthStart+(lineData.depthEnd-lineData.depthStart)*rateChange;
                        /*
                        screenInfo[i][row] = new double[]{
                            0,//lineData.depthStart + (lineData.depthEnd-lineData.depthStart)*rateChange,
                            frameCounter,
                            0,
                            0,//lineData.iStart + (lineData.iEnd-lineData.iStart)*rateChange,
                            0 //lineData.jStart + (lineData.jEnd-lineData.jStart)*rateChange
                        };
                        */
                        if(
                            i/screenResolution >= xSize ||
                            row/screenResolution >= ySize 
                        ) continue;
                        if(
                            (depthCheck > screenInfo[(int)(i/screenResolution)][(int)(row/screenResolution)][0] && 
                            screenInfo[(int)(i/screenResolution)][(int)(row/screenResolution)][1] == frameCounter)
                        )
                        continue;
                        screenInfo[(int)(i/screenResolution)][(int)(row/screenResolution)][0] = depthCheck;
                        screenInfo[(int)(i/screenResolution)][(int)(row/screenResolution)][1] = frameCounter;
                        if(i == lineData.rowStart*screenResolution || i == lineData.rowEnd*screenResolution)                        
                        screenInfo[(int)(i/screenResolution)][(int)(row/screenResolution)][2] = 0;
                        else
                        screenInfo[(int)(i/screenResolution)][(int)(row/screenResolution)][2] = 120;
                        //screenInfo[(int)(i/screenResolution)][(int)(row/screenResolution)][2] = Math.Abs(250*(i/screenResolution-lineData.rowStart)/(lineData.rowEnd-lineData.rowStart));
                        //if(screenInfo[(int)(i/screenResolution)][(int)(row/screenResolution)][2] > 255) screenInfo[(int)(i/screenResolution)][(int)(row/screenResolution)][2] = 255;
                        //if(screenInfo[(int)(i/screenResolution)][(int)(row/screenResolution)][2] < 0) screenInfo[(int)(i/screenResolution)][(int)(row/screenResolution)][2] = 0;
                    }
                    
                    /*
                    if(
                        screenInfo[lineData.rowStart][row][1] == frameCounter &&
                        !(lineData.rowStart >= xSize-1)
                    ){
                        screenInfo[lineData.rowStart+1][row][0] = screenInfo[lineData.rowStart][row][0];
                        screenInfo[lineData.rowStart+1][row][1] = screenInfo[lineData.rowStart][row][1];
                        screenInfo[lineData.rowStart+1][row][2] = screenInfo[lineData.rowStart][row][2];
                        screenInfo[lineData.rowStart+1][row][3] = lineData.rowStart+1;
                        screenInfo[lineData.rowStart+1][row][4] = screenInfo[lineData.rowStart][row][4];
                    }
                    */
                    /*
                    screenInfo[lineData.rowStart][row][0] = deltaA.z;
                    screenInfo[lineData.rowStart][row][1] = frameCounter;
                    screenInfo[lineData.rowStart][row][2] = 12*Math.Abs(deltaA.z);
                    screenInfo[lineData.rowStart][row][3] = lineData.rowStart;
                    screenInfo[lineData.rowStart][row][4] = lineData.rowEnd;
                    if(screenInfo[lineData.rowStart][row][2] > 255) screenInfo[lineData.rowStart][row][2] = 255;
                    */
                    /*
                    if(
                        screenInfo[lineData.rowEnd][row][1] == frameCounter &&
                        !(lineData.rowEnd <=0)
                    ){
                        screenInfo[lineData.rowEnd-1][row][0] = screenInfo[lineData.rowEnd][row][0];
                        screenInfo[lineData.rowEnd-1][row][1] = screenInfo[lineData.rowEnd][row][1];
                        screenInfo[lineData.rowEnd-1][row][2] = screenInfo[lineData.rowEnd][row][2];
                        screenInfo[lineData.rowEnd-1][row][3] = screenInfo[lineData.rowEnd][row][3];
                        screenInfo[lineData.rowEnd-1][row][4] = lineData.rowEnd-1;
                    }
                    */
                    /*
                    screenInfo[lineData.rowEnd-1][row][0] = deltaA.z;
                    screenInfo[lineData.rowEnd-1][row][1] = frameCounter;
                    screenInfo[lineData.rowEnd-1][row][2] = 12*Math.Abs(deltaA.z);
                    screenInfo[lineData.rowEnd-1][row][3] = lineData.rowStart;
                    screenInfo[lineData.rowEnd-1][row][4] = lineData.rowEnd-1;
                    if(screenInfo[lineData.rowEnd-1][row][2] > 255) screenInfo[lineData.rowEnd-1][row][2] = 255;
                    */
                    
                }
                
            }

            //returning the screen data
            return screenInfo;
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
                    if(iIntersectActual<lineData.rowStart) {
                        lineData.rowStart = (int)iIntersectActual;
                        lineData.depthStart = deltaB.z * iIntersect + pointA.z;
                    }
                    if(iIntersectActual>lineData.rowEnd) {
                        lineData.rowEnd = (int)iIntersectActual;
                        lineData.depthEnd = deltaB.z * iIntersect + pointA.z;
                    }                  
                }                
            }
            if(deltaC.y != 0){
                jIntersect =  (row - pointA.y)/deltaC.y;
                if(jIntersect >= 0 && jIntersect < 1){
                    jIntersectActual = deltaC.x * jIntersect + pointA.x;
                    if(jIntersectActual<lineData.rowStart) {
                        lineData.rowStart = (int)jIntersectActual;
                        lineData.depthStart = deltaC.z * jIntersect + pointA.z;
                    }
                    if(jIntersectActual>lineData.rowEnd) {
                        lineData.rowEnd = (int)jIntersectActual;
                        lineData.depthEnd = deltaC.z * jIntersect + pointA.z;
                    }
                }
            } 
            //Gives the i intersect on the i+j = 1 axis
            //Compute 1 - i to find j
            if(deltaC.y-deltaB.y  != 0){
                iPlusJIntersect = (row - pointA.y - deltaB.y ) / (deltaC.y-deltaB.y);
                if(iPlusJIntersect >= 0 && iPlusJIntersect < 1){
                    iPlusJIntersectActual = iPlusJIntersect*deltaC.x + deltaB.x*(1-iPlusJIntersect) + pointA.x;
                    if(iPlusJIntersectActual<lineData.rowStart) {
                        lineData.rowStart = (int)iPlusJIntersectActual;
                        lineData.depthStart = iPlusJIntersect*deltaC.z + deltaB.z*(1-iPlusJIntersect) + pointA.z;
                    }
                    if(iPlusJIntersectActual>lineData.rowEnd) {
                        lineData.rowEnd = (int)iPlusJIntersectActual;
                        lineData.depthEnd = iPlusJIntersect*deltaC.z + deltaB.z*(1-iPlusJIntersect) + pointA.z;
                    }
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
            int endLine = -1;
            int savedColor = 0;
            for(int y = 0; y< ySize; y++){
                screenY = (int)((int)(y*screenResolution)/screenResolution);
                //if(screenY >= ySize) continue;
                endLine = -1;
                savedColor = 0;
                for(int x = 0; x< xSize; x++){
                    screenX = (int)(((int)(x*screenResolution))/screenResolution);
                    //if(screenX >= xSize) continue;
                    if(screenInfo[screenX][screenY][1] == frameCounter){
                        //endLine = (int)screenInfo[screenX][screenY][4];
                        savedColor = (int)screenInfo[screenX][screenY][2];
                        //if((int)screenInfo[screenX][screenY][4] == (int)screenInfo[screenX][screenY][3])
                        //continue;
                        /*
                        if(endLine == -1 && !(screenX >= (int)screenInfo[screenX][screenY][4])){
                            endLine = (int)screenInfo[screenX][screenY][4];
                            savedColor = (int)screenInfo[screenX][screenY][2];
                        }*/
                        /*
                        else if(endLine != (int)screenInfo[screenX][screenY][4]){
                            endLine = (int)screenInfo[screenX][screenY][4];
                            savedColor = (int)screenInfo[screenX][screenY][2];
                        }  
                        */     
                        /*                 
                        if(endLine <= screenX){
                            endLine = -1;
                            savedColor = 0;
                        }
                        */
                        //Pulling the color from screenData
                        tempColor = Color.FromArgb(
                            255,
                            savedColor,
                            savedColor,
                            savedColor
                        );
                        screenImage.SetPixel(x,y,tempColor);
                        //screenImage.SetPixel(endLine,screenY,tempColor);
                    }else{
                        if(endLine != -1){         
                            /*
                            tempColor = Color.FromArgb(
                                255,
                                savedColor,
                                savedColor,
                                savedColor
                            );
                            screenImage.SetPixel(screenX,screenY,tempColor);
                            screenInfo[screenX][screenY][1] = frameCounter;
                            if(endLine <= screenX){
                                endLine = -1;
                                savedColor = 0;
                            }
                            */
                            screenInfo[screenX][screenY][1] = -1;
                        }else{
                            screenInfo[screenX][screenY][1] = -1;
                        }
                        
                    }
                }
            }
            Image output = screenImage;
            return output;
        }
	}
    
    public class component{
        //Component Class for future ECS system
    }
    
    
    
}
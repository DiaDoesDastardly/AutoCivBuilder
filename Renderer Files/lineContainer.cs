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
public class rowComponent{
        //Where in the row does our section start
        public int rowStart = -1;
        //Where in the row does our section end
        public int rowEnd = -1;
        //Where in the row does our section start
        public double depthStart = -1;
        //Where in the row does our section end
        public double depthEnd = -1;
        //What is the ID of the triangle that this section is from
        public int triangleID;
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
            triangleID = target.triangleID;
        }
    }
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
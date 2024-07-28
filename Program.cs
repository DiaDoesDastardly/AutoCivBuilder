using PenroseEngine;
// See https://aka.ms/new-console-template for more information
/*
CivBuilder civ = new CivBuilder();
String objectFolder = "..\\..\\..\\Objects\\";
gameObject[] buildingModels = new gameObject[civ.mapSizeX*civ.mapSizeY];
while(civ.initTurn()){
    civ.currentGameStats();
    civ.printMap();
    if(civ.turnCount == 200){
        break;
    }
    
    for(int xPos = 0; xPos < civ.mapSizeX; xPos++)
    for(int yPos = 0; yPos < civ.mapSizeY; yPos++){
    buildingModels[xPos * civ.mapSizeY + yPos] = new gameObject(objectFolder + civ.cityMap[xPos, yPos].model)
    {
        position = new vector3((xPos - civ.mapSizeX / 2) * 2, 0, (yPos - civ.mapSizeY / 2) * 2)
        //scale = new vector3(.7, civ.cityMap[xPos,yPos].level+1, .7)        
    };
}
}
civ.currentGameStats();
civ.printMap();
*/




/*
gameObject[] buildingModels = new gameObject[]{
    new(objectFolder+"cube.obj")
};
*/

double[,] rotationMatrix = rendererPipeline.rotationMatrixGenerator(45,-45);
MyForm form = new MyForm(rotationMatrix,new gameObject[0],50);
Application.Run(form);
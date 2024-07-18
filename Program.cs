using PenroseEngine;
// See https://aka.ms/new-console-template for more information
CivBuilder civ = new CivBuilder();
String objectFolder = "..\\..\\..\\Objects\\";
while(civ.initTurn()){
    civ.currentGameStats();
    civ.printMap();
    if(civ.turnCount == 200){
        break;
    }
}
civ.currentGameStats();
civ.printMap();


gameObject[] buildingModels = new gameObject[civ.mapSizeX*civ.mapSizeY];
for(int xPos = 0; xPos < civ.mapSizeX; xPos++)
for(int yPos = 0; yPos < civ.mapSizeY; yPos++){
    buildingModels[xPos * civ.mapSizeY + yPos] = new gameObject(objectFolder + civ.cityMap[xPos, yPos].model)
    {
        position = new vector3((xPos - civ.mapSizeX / 2) * 2, -civ.cityMap[xPos,yPos].level, (yPos - civ.mapSizeY / 2) * 2),
        scale = new vector3(.7, civ.cityMap[xPos,yPos].level+1, .7)        
    };
}

/*
gameObject[] buildingModels = new gameObject[]{
    new(objectFolder+"monkey.obj")
};
*/
double[,] rotationMatrix = rendererPipeline.rotationMatrixGenerator(0,0);
MyForm form = new MyForm(rotationMatrix,buildingModels,100);
Application.Run(form);
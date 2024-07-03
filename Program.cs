using PenroseEngine;
// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");
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
Console.WriteLine(civ.mapSizeX*civ.mapSizeY);
for(int xPos = 0; xPos < civ.mapSizeX; xPos++)
for(int yPos = 0; yPos < civ.mapSizeY; yPos++){
        buildingModels[xPos * civ.mapSizeY + yPos] = new gameObject(objectFolder + civ.cityMap[xPos, yPos].model)
        {
            position = new vector3((xPos - civ.mapSizeX / 2) * 2, -civ.cityMap[xPos,yPos].level, (yPos - civ.mapSizeY / 2) * 2),
            scale = new vector3(1, civ.cityMap[xPos,yPos].level+1, 1)
            
        };
        Console.WriteLine(civ.cityMap[xPos,yPos].level);
    }
/*
gameObject[] stuff = [
    new(objectFolder+"cube.obj"),
    new(objectFolder+"cube.obj"),
    new(objectFolder+"cube.obj"),
    new(objectFolder+"monkey.obj")
];
stuff[0].position = new (2,0,0);
stuff[1].position = new (0,2,0);
stuff[2].position = new (0,0,2);
stuff[3].position = new (0,0,-2);
*/
//foreach(gameObject item in buildingModels)Console.WriteLine(item.vertices[0].x);
double[,] rotationMatrix = rendererPipeline.rotationMatrixGenerator(0,0);
MyForm form = new MyForm(rotationMatrix,buildingModels,25);
Application.Run(form);
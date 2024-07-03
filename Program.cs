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

Console.WriteLine(Environment.CurrentDirectory);

Console.WriteLine("Hello, World!");
gameObject cube = new gameObject(objectFolder+"cube.obj");
double[,] rotationMatrix = rendererPipeline.rotationMatrixGenerator(0,0);
MyForm form = new MyForm(rotationMatrix,cube,50);
Application.Run(form);
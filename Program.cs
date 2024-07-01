// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");
CivBuilder civ = new CivBuilder();
while(civ.initTurn()){
    civ.currentGameStats();
    civ.printMap();
    if(civ.getCurrentTurn() == 100){
        break;
    }
}
civ.currentGameStats();
civ.printMap();
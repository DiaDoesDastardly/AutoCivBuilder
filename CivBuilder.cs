using System.Diagnostics.Tracing;
using System.Reflection.Emit;

public class CivBuilder{
    //What turn is it
    public int turnCount;
    //Size of map
    public int mapSizeX;
    public int mapSizeY;
    //How many people live in the city
    public int population;
    //How many people work in the city
    public int workerCount;
    //How much housing is open.
    public int housingCount;    
    //Types of resources
    public Resources[] resourceType; 
    //Map of the city
    public Building[,] cityMap;
    public upgradableBuilding test;
    public upgradableBuilding farm;
    //List of all citizens
    public List<Citizen> citizens = new List<Citizen>();
    public CivBuilder(){
        //Initializing variables
        turnCount = 0;
        population = 0;
        mapSizeX = 11;
        mapSizeY = 8;
        cityMap = new Building[mapSizeX,mapSizeY];
        //Setting every tile on the map to an empty building spot
        for(int initX = 0; initX < mapSizeX; initX++){
            for(int initY = 0; initY < mapSizeY; initY++){
                cityMap[initX, initY] = new Building();
            }
        }

        //Adding used resources to the game
        resourceType = new Resources[]{
            new("Food",20),
            new("Wood",20),
            new("Stone",20),
            new("Coal",20),
            new("Wehite",20)
        };
        
        //Adding the first citizens
        for(int i = 0; i < 4; i++){
            citizens.Add(new Citizen("Newbie",0));
        }
        //Setting population count
        population = citizens.Count;
        //Setting population consumables
        Citizen.baseResources = new Resources[]{
            new("Food",0,-1)
        };

        test = new upgradableBuilding( 
            "Test", //Building type name
            //Upgrade names
            [
                "Weh",
                "Weh weh"
            ], 
            1, // Building rank
            1, // Pop housing
            1, // Employee Max
            //Building resource cost and per turn
            [
                new Resources("Wood",-1,0),
                new Resources("Wehite",0,1)
            ], 
            false, //Needs neighbors
            true //Build on outskirts 
        );
        addBuilding(test.createPrefab(0),1,1,true);
        addBuilding(test.createPrefab(1),2,1,true);
        
        //Adding starter structures to of the map
        addBuilding(Building.prefabHouse(0), mapSizeX/2, mapSizeY/2, true);
        addBuilding(Building.prefabFarm(0), 0, 0, true);
        addBuilding(Building.prefabMine(0), 0, 1, true);
        addBuilding(Building.prefabLumber(0), 0, 2, true);
    }
    public Boolean initTurn(){
        //Resetting trackers for resource production and housing
        housingCount = 0;
        workerCount = 0;
        foreach(Resources item in resourceType){
            item.perTurn = 0;
            item.demand = 0;
        }
        //Incrementing turn counter
        turnCount++;
        //Housing and employing pops and producing resources
        buildingAction();
        //Removing food eaten
        //resourceType[0].count -= population;
        foreach(Resources item in resourceType)
        item.findThenAdd(Citizen.baseResources,population,false);
        //Removing those who starve
        if(resourceType[0].count<=0){
            //population += foodCount;
            resourceType[0].demand = Math.Abs(resourceType[0].count);
            Citizen.removeCitizens(Math.Abs(resourceType[0].count), this);
            resourceType[0].count = 0;
        }
        //Removing those who die from the elements
        if(population>housingCount){
            resourceType[0].demand -= Math.Abs((population-housingCount)/2);
            Citizen.removeCitizens(Math.Abs((population-housingCount)/2),this);
        }
        //If every one is gone then return false
        if(population<=0){
            return false;
        }

        resourceType[0].perTurn -= population;
        if(population<housingCount){
            for(int i = 0; i<population/2; i++){
                citizens.Add(new("Newbie",turnCount));
            }
            //Citizen.addCitizen(population/2);
        }else{
            for(int i = 0; i<population/10; i++){
                citizens.Add(new("Newbie",turnCount));
            }
            //Citizen.addCitizen(population/10);
        }
        
        if(resourceType[0].demand>0){
            if(!replaceBuildings(Building.prefabFarm(2), Building.prefabFarm(3), 1))
            if(!replaceBuildings(Building.prefabFarm(1), Building.prefabFarm(2), 1))
            if(!replaceBuildings(Building.prefabFarm(0), Building.prefabFarm(1), 1))
            buildBuildings(Building.prefabFarm(0),1);
        }else{
            if(!replaceBuildings(Building.prefabHouse(2), Building.prefabHouse(3), 1))
            if(!replaceBuildings(Building.prefabHouse(1), Building.prefabHouse(2), 1))
            if(!replaceBuildings(Building.prefabHouse(0), Building.prefabHouse(1), 1))
            buildBuildings(Building.prefabHouse(0),1);
        }

        if(resourceType[2].perTurn<=turnCount/2)
        if(!replaceBuildings(Building.prefabMine(2), Building.prefabMine(3), 1))
        if(!replaceBuildings(Building.prefabMine(1), Building.prefabMine(2), 1))
        if(!replaceBuildings(Building.prefabMine(0), Building.prefabMine(1), 1))
        buildBuildings(Building.prefabMine(0),1);

        if(resourceType[1].perTurn<=turnCount/2)
        if(!replaceBuildings(Building.prefabLumber(2), Building.prefabLumber(3), 1))
        if(!replaceBuildings(Building.prefabLumber(1), Building.prefabLumber(2), 1))
        if(!replaceBuildings(Building.prefabLumber(0), Building.prefabLumber(1), 1))
        buildBuildings(Building.prefabLumber(0),1);

        if(resourceType[4].perTurn<=turnCount/2)
        for(int level = test.upgradeNames.Length - 1; level > 0; level--)
        if(!replaceBuildings(test.createPrefab(level-1), test.createPrefab(level), 1)) break;


        //Setting population count
        population = citizens.Count;

        return true;
    }
    public void currentGameStats(){
        Console.WriteLine("Turn: "+turnCount);
        Console.WriteLine("Population: "+population);
        foreach(Resources item in resourceType){
            Console.WriteLine(item.name + ": " + item.count + " || per turn: " + item.perTurn);
        }
        Console.WriteLine("Total housing: "+housingCount);
        Console.WriteLine("Number of workers: "+workerCount);
        Console.WriteLine("\n");
    }
    public void printMap(){
        int longestString = 0;
        for(int initX = 0; initX < mapSizeX; initX++){
            for(int initY = 0; initY < mapSizeY; initY++){
                if(cityMap[initX, initY].name.Length > longestString){
                    longestString = cityMap[initX, initY].name.Length;
                }
            }
        }
        for(int initX = 0; initX < mapSizeX; initX++){
            Console.Write("Map line "+initX+" ");
            for(int initY = 0; initY < mapSizeY; initY++){
                Console.Write(cityMap[initX, initY].name+" ");
                for(int i = 0; i < longestString-cityMap[initX, initY].name.Length; i++){
                    Console.Write(" ");
                }
            }
            Console.Write("\n");
        }
    }
    public void buildingAction(){
        double productionMod;
        for(int initX = 0; initX < mapSizeX; initX++){
            for(int initY = 0; initY < mapSizeY; initY++){
                if(cityMap[initX, initY].emptyTile == false){
                    if(cityMap[initX, initY].workerCount < cityMap[initX, initY].employeeCount){
                        for(int i = 0; i < citizens.Count; i++){
                            if(!citizens[i].employed){
                                citizens[i].employed = true;
                                citizens[i].placeOfEmployment = new int[]{initX,initY};
                                cityMap[initX, initY].workerCount++;
                            }
                            if(cityMap[initX, initY].workerCount == cityMap[initX, initY].employeeCount) break;
                        }
                    }
                    if(cityMap[initX, initY].populationCount < cityMap[initX, initY].popHousing){
                        for(int i = 0; i < citizens.Count; i++){
                            if(!citizens[i].housed){
                                citizens[i].housed = true;
                                citizens[i].placeOfResidence = new int[]{initX,initY};
                                cityMap[initX, initY].populationCount++;
                            }
                            if(cityMap[initX, initY].populationCount == cityMap[initX, initY].popHousing) break;
                        }
                    }
                    if(cityMap[initX, initY].employeeCount > 0){
                        if(!Resources.checkStock(resourceType,cityMap[initX, initY].resources, true)){
                            productionMod = 0;
                        }else productionMod = (double)cityMap[initX, initY].workerCount/(double)cityMap[initX, initY].employeeCount;
                        for(int count = 0; count<resourceType.Length; count++){
                            resourceType[count].findThenAdd(cityMap[initX, initY].resources,productionMod,false);
                        }
                        workerCount += cityMap[initX, initY].workerCount;
                    }
                    if(cityMap[initX, initY].popHousing > 0){
                        if(
                            !Resources.checkStock(resourceType,cityMap[initX, initY].resources, true)
                        )continue;
                        housingCount += cityMap[initX, initY].popHousing;
                    }
                }
            }
        }
    }
    public Boolean buildBuildings(Building targetBuilding, int buildingCount){
        List<int[]> targetBuildingSites = new List<int[]>();
        //A bonus that is calcualted to try and encourage the computer to build towards the middle
        int buildBonus;
        Boolean sortedItem;
        //Going through every tile on the list to see if there are any buildable spots 
        for(int initX = 0; initX < mapSizeX; initX++){
            for(int initY = 0; initY < mapSizeY; initY++){
                buildBonus = mapSizeX/2+mapSizeY/2;
                //If building needs to be on the outskirts
                //Also check if building needs to be built next to others and if spot is adjacent to a building
                if(
                    (
                        (
                            targetBuilding.buildOnOutskirts && 
                            (initX == 0 || initY == 0 || initX == mapSizeX-1 || initY == mapSizeY-1)
                        ) || 
                        targetBuilding.buildOnOutskirts == false
                    ) &&
                    cityMap[initX,initY].emptyTile &&
                    (
                        targetBuilding.needsNeighbors == false || 
                        cityMap[initX,initY].buildingRank > 0
                    )             
                ){
                    //This equation should make it so that the bonus gets lower the further from the center it gets
                    buildBonus = (int)-Math.Sqrt(
                        (initX-mapSizeX/2)*(initX-mapSizeX/2)+(initY-mapSizeY/2)*(initY-mapSizeY/2)
                    );
                    buildBonus += cityMap[initX,initY].buildingRank;
                    sortedItem = false;
                    //If available spots are found, add them to the target building sites
                    if(targetBuildingSites.Count == 0)
                        targetBuildingSites.Add(new int[]{initX,initY,buildBonus});
                    else{
                        for(int i = 0; i< targetBuildingSites.Count; i++){
                            if(targetBuildingSites[i][2] < buildBonus){
                                targetBuildingSites.Insert(i, new int[]{initX,initY,buildBonus});
                                sortedItem = true;
                                break;
                            }
                        }
                        if(!sortedItem) targetBuildingSites.Add(new int[]{initX,initY,buildBonus});
                    }
                }
            }
        }
        //If no spots are found then return false
        if(targetBuildingSites.Count == 0){
            return false;
        }
        //Buildings added using the spots found
        for(int buildCount = 0; buildCount < buildingCount; buildCount++){
            //If the computer wants to build more buildings then we have spots, we build as 
            //many as we can and then break the for loop
            if(buildCount >= targetBuildingSites.Count){
                break;
            }
            //Console.WriteLine(targetBuildingSites[buildCount][0]+","+targetBuildingSites[buildCount][1]);
            if(!addBuilding(targetBuilding, targetBuildingSites[buildCount][0],targetBuildingSites[buildCount][1], false)){
                return false;
            }
        }
        //Console.WriteLine(buildingCount);
        return true;
    }
    public Boolean addBuilding(Building targetBuilding, int xLocation, int yLocation, Boolean adminBuild){
        //Check if building has the resources to be built
        if(
            !Resources.checkStock(resourceType, targetBuilding.resources, false) &&
            !adminBuild
            ){
            //If not enough resources exist then return false
            return false;
        }
        //Check to see if building spot is already occupied
        //If the spot is open then build
        cityMap[xLocation,yLocation] = targetBuilding.buildSelf();
        if(!adminBuild){
            for(int i = 0; i<resourceType.Length; i++){
                resourceType[i].findThenAdd(targetBuilding.resources,1, true);
            }
        }
        //Increase adjacent building ranks 
        cityMap[xLocation,yLocation].buildingRank += targetBuilding.buildingRank;
        if(xLocation-1>=0) cityMap[xLocation-1,yLocation].buildingRank += targetBuilding.buildingRank;
        if(yLocation-1>=0) cityMap[xLocation,yLocation-1].buildingRank += targetBuilding.buildingRank;
        if(xLocation+1<mapSizeX) cityMap[xLocation+1,yLocation].buildingRank += targetBuilding.buildingRank;
        if(yLocation+1<mapSizeY) cityMap[xLocation,yLocation+1].buildingRank += targetBuilding.buildingRank;

        if(xLocation-1>=0 && yLocation-1>=0) cityMap[xLocation-1,yLocation-1].buildingRank += targetBuilding.buildingRank;
        if(yLocation-1>=0 && xLocation+1<mapSizeX) cityMap[xLocation+1,yLocation-1].buildingRank += targetBuilding.buildingRank;
        if(xLocation+1<mapSizeX && yLocation+1<mapSizeY) cityMap[xLocation+1,yLocation+1].buildingRank += targetBuilding.buildingRank;
        if(yLocation+1<mapSizeY && xLocation-1>=0) cityMap[xLocation-1,yLocation+1].buildingRank += targetBuilding.buildingRank;

        return true;
    }
    public Boolean replaceBuildings(Building targetBuilding, Building replacingBuilding, int buildingCount){
        List<int[]> targetBuildingSites = new List<int[]>();
        int buildBonus;
        Boolean sortedItem;
        //Going through every tile on the list to see if there are any buildable spots 
        for(int initX = 0; initX < mapSizeX; initX++){
            for(int initY = 0; initY < mapSizeY; initY++){
                //If building needs to be on the outskirts
                //Also check if building needs to be built next to others and if spot is adjacent to a building
                if(cityMap[initX,initY].name == targetBuilding.name){
                    //This equation should make it so that the bonus gets lower the further from the center it gets
                    buildBonus = (int)-Math.Sqrt(
                        (initX-mapSizeX/2)*(initX-mapSizeX/2)+(initY-mapSizeY/2)*(initY-mapSizeY/2)
                    );
                    buildBonus += cityMap[initX,initY].buildingRank;
                    sortedItem = false;
                    //If available spots are found, add them to the target building sites
                    //We order them from greatest to least to make proper placement a bit easier
                    if(targetBuildingSites.Count == 0 || targetBuilding.buildOnOutskirts)
                        targetBuildingSites.Add(new int[]{initX,initY,buildBonus});
                    else{
                        for(int i = 0; i< targetBuildingSites.Count; i++){
                            if(targetBuildingSites[i][2] < buildBonus){
                                targetBuildingSites.Insert(i, new int[]{initX,initY,buildBonus});
                                sortedItem = true;
                                break;
                            }
                        }
                        if(!sortedItem) targetBuildingSites.Add(new int[]{initX,initY,buildBonus});
                    }
                }
            }
        }
        //If no spots are found then return false
        if(targetBuildingSites.Count == 0){
            return false;
        }
        //Buildings added using the spots found
        for(int buildCount = 0; buildCount < buildingCount; buildCount++){
            //If the computer wants to build more buildings then we have spots, we build as 
            //many as we can and then break the for loop
            if(buildCount >= targetBuildingSites.Count){
                break;
            }
            //Console.WriteLine(targetBuildingSites[buildCount][0]+","+targetBuildingSites[buildCount][1]);
            if(!addBuilding(replacingBuilding, targetBuildingSites[buildCount][0],targetBuildingSites[buildCount][1], false)){
                return false;
            }
        }
        //Console.WriteLine(buildingCount);
        return true;
    }
}


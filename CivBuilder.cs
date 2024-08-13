using System.Diagnostics.Tracing;
using System.Reflection.Emit;

public class CivBuilder{
    //What turn is it
    public int turnCount;
    //Size of map
    public int mapSizeX;
    public int mapSizeY;
    //Types of resources
    public Resources[] resourceType; 
    //Map of the city
    public Building[,] cityMap;
    public upgradableBuilding farm, house, logger, mine, storehouse, laboratory;
    //List of all citizens
    public List<Citizen> citizens = new List<Citizen>();
    public CivBuilder(){
        //Initializing variables
        turnCount = 0;
        mapSizeX = 11;
        mapSizeY = 11;
        cityMap = new Building[mapSizeX,mapSizeY];
        //Setting every tile on the map to an empty building spot
        for(int initX = 0; initX < mapSizeX; initX++){
            for(int initY = 0; initY < mapSizeY; initY++){
                cityMap[initX, initY] = new Building();
            }
        }

        //Adding used resources to the game
        resourceType = new Resources[]{
            new("Food",100),
            new("Wood",100),
            new("Stone",100),
            new("Coal",30),
            new("Population",0,0,20),
            new("Workers"),
            new("Science")
        };
        
        //Adding the first citizens
        for(int i = 0; i < 4; i++) citizens.Add(new Citizen("Newbie",0));
        
        resourceType[4].addCount(4);

        //Setting population consumables
        Citizen.baseResources = new Resources[]{
            new("Food",0,-1)
        };
        
        //Creating upgradable structures
        farm = new upgradableBuilding( 
            "Farm", //Building type name
            //Model Names
            [
                "berryBushes.obj"
            ],
            //Upgrade names
            [
                "Berry Bush"                
            ], 
            //Researchable model names
            [
                "berryBushes.obj",
                "buildingTwo.obj",
                "buildingTwo.obj"
            ], 
            //Researchable upgrade names
            [
                "Basic Farm",
                "Farm",
                "Factory Farm"
            ],
            //Cost to research each level
            [
                new ("Science",-10),
                new ("Science",-10),
                new ("Science",-10)
            ],
            0, // Building rank
            0, // Pop housing
            4, // Employee Max
            //Building resource cost and per turn
            [
                new ("Wood",-10,0),
                new ("Stone",-10,0),
                new ("Food",0,20),
                new ("Workers",0,0,4)
            ], 
            false, //Needs neighbors
            true //Build on outskirts 
        );
        house = new upgradableBuilding( 
            "House", //Building type name
            //Model names
            [
                "huts.obj"
            ], 
            //Upgrade names
            [
                "Small hut"
            ], 
            //Researchable model names
            [
                "house.obj",
                "house.obj",
                "buildingOne.obj"
            ], 
            //Researchable upgrade names
            [
                "Wooden House",
                "Townhouse",
                "Apartment Complex"
            ],
            //Cost to research each level
            [
                new ("Science",-10),
                new ("Science",-10),
                new ("Science",-10)
            ],
            5, // Building rank
            0, // Pop housing
            0, // Employee Max
            //Building resource cost and per turn
            [
                new ("Wood",-12,0),
                new ("Stone",-12,0),
                new ("Population",0,0,10)
            ], 
            true, //Needs neighbors
            false //Build on outskirts 
        );
        logger = new upgradableBuilding( 
            "Logger", //Building type name
            //Model names
            [
                "buildingFour.obj"
            ], 
            //Upgrade names
            [
                "Stick Gatherers"                
            ], 
            //Researchable model names
            [
                "buildingFour.obj",
                "buildingFour.obj",
                "buildingFour.obj"
            ], 
            //Researchable upgrade names
            [
                "Lumberjack hut",
                "Log Mill",
                "Logging Company"
            ],
            //Cost to research each level
            [
                new ("Science",-10),
                new ("Science",-10),
                new ("Science",-10)
            ],
            0, // Building rank
            0, // Pop housing
            4, // Employee Max
            //Building resource cost and per turn
            [
                new ("Wood",0,4),
                new ("Stone",-10,0),
                new ("Workers",0,0,4)
            ], 
            false, //Needs neighbors
            true //Build on outskirts 
        );
        mine = new upgradableBuilding( 
            "Mine", //Building type name
            //Model names
            [
                "buildingThree.obj"
            ], 
            //Upgrade names
            [
                "Stone Gatherers"                
            ], 
            //Researchable model names
            [
                "buildingThree.obj",
                "buildingThree.obj",
                "buildingThree.obj"
            ], 
            //Researchable upgrade names
            [
                "Stone Mine",
                "Stone Quarry",
                "Mountain Digger"
            ],
            //Cost to research each level
            [
                new ("Science",-10),
                new ("Science",-10),
                new ("Science",-10)
            ],
            0, // Building rank
            0, // Pop housing
            4, // Employee Max
            //Building resource cost and per turn
            [
                new ("Wood",-10,0),
                new ("Stone",0,4),
                new ("Workers",0,0,4)
            ], 
            false, //Needs neighbors
            true //Build on outskirts 
        );
        storehouse = new upgradableBuilding( 
            "Storehouse", //Building type name
            //Model names
            [
                "buildingFive.obj"
            ],
            //Upgrade names
            [
                "Storehouse"
            ], 
            0, // Building rank
            0, // Pop housing
            0, // Employee Max
            //Building resource cost and per turn
            [
                new Resources("Food",0,0,1000),
                new Resources("Wood",-10,0,1000),
                new Resources("Stone",-10,0,1000),
                new Resources("Coal",0,0,1000)
            ], 
            false, //Needs neighbors
            true //Build on outskirts 
        );
        laboratory = new upgradableBuilding( 
            "Laboratory", //Building type name
            //Model names
            [
                "cube.obj"
            ],
            //Upgrade names
            [
                "Laboratory"
            ], 
            0, // Building rank
            0, // Pop housing
            4, // Employee Max
            //Building resource cost and per turn
            [
                new ("Wood",-10,0),
                new ("Stone",-10,0),
                new ("Science",0,10,1000),
                new ("Workers",0,0,4)
            ], 
            false, //Needs neighbors
            true //Build on outskirts 
        );
                
        //Adding starter structures to of the map
        addBuilding(house.createPrefab(0), mapSizeX/2, mapSizeY/2, false);
        addBuilding(farm.createPrefab(0), 0, 0, false);
        addBuilding(logger.createPrefab(0), 0, 1, false);
        addBuilding(mine.createPrefab(0), 0, 2, false);
        addBuilding(storehouse.createPrefab(0), 0, 3, false);
        addBuilding(laboratory.createPrefab(0), 0, 4, false);
    }
    public Boolean initTurn(){
        //Resetting trackers for resource production and housing
        foreach(Resources item in resourceType){
            //Setting our Worker count to 0
            if(item.name == "Workers"){
                item.count = 0;
                item.storage = 0;
            }
            item.perTurn = 0;
            item.demand = 0;
        }

        //Incrementing turn counter
        turnCount++;

        //Housing and employing pops and producing resources
        buildingAction();

        //Removing the resources that citizens need and the citizens that don't use them
        //Also producing new citizens
        Citizen.citizenUpkeep(this);
        //Removing resources that are not stored away
        foreach(Resources item in resourceType){
            if(item.count > item.storage){
                item.count = item.storage;
                item.perTurn += item.storage-item.count;
                //If the storage type is for population, then remove any additional population that doesn't have housing
                if(item.name == "Population") {
                    Citizen.removeCitizens(Math.Abs(item.storage-item.count),this);
                    resourceType[0].demand = 2*item.count;
                }
            }
            if(item.name == "Population" && item.count<=0) {
                if(item.count<=0) return false;//If every one is gone then return false
            }
        }
        //If there is enough science to research then do some research
        
        //If the people demand food then make farms
        if(resourceType[0].demand>0) farm.build(this);
        
        //Make houses if we don't have enough room
        if(resourceType[4].demand>0) house.build(this);

        //Creating mines and lumberyards to keep up with expansion demand
        if(resourceType[2].perTurn<=turnCount/2)mine.build(this);
        if(resourceType[1].perTurn<=turnCount/2)logger.build(this);

        //If all processes work then return true
        return true;
    }
    public void currentGameStats(){
        Console.WriteLine("Turn: "+turnCount);
        foreach(Resources item in resourceType){
            Console.WriteLine(
                item.name + ": " + item.count + 
                " || per turn: " + item.perTurn + 
                " || demand: " + item.demand + 
                " || storage: " + item.storage
            );
        }
        Console.Write("\n");
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
        Console.Write("\n");
    }
    /*
    public void buildingAction(){
        double productionMod;
        for(int initX = 0; initX < mapSizeX; initX++){
            for(int initY = 0; initY < mapSizeY; initY++){
                if(!cityMap[initX, initY].emptyTile){
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
                    if(cityMap[initX, initY].employeeCount > 0){
                        if(!Resources.checkStock(resourceType,cityMap[initX, initY].resources, true)) productionMod = 0;
                        else productionMod = (double)cityMap[initX, initY].workerCount/(double)cityMap[initX, initY].employeeCount;
                        foreach(Resources item in resourceType) item.findThenAdd(cityMap[initX, initY].resources,productionMod,false);
                        workerCount += cityMap[initX, initY].workerCount;
                    }
                    if(cityMap[initX, initY].popHousing > 0){
                        if(!Resources.checkStock(resourceType,cityMap[initX, initY].resources, true)) continue;
                        //housingCount += cityMap[initX, initY].popHousing;
                    }
                }
            }
        }
    }
    */
    public void buildingAction(){
        double productionMod;
        int workerIndex;
        int cityWorkerIndex = Resources.getIndex(resourceType, new("Workers"));
        int populationIndex = Resources.getIndex(resourceType, new("Population"));
        for(int initX = 0; initX < mapSizeX; initX++){
            for(int initY = 0; initY < mapSizeY; initY++){
                if(!cityMap[initX,initY].emptyTile){
                    if(resourceType[cityWorkerIndex].count < resourceType[populationIndex].count){
                        workerIndex = Resources.getIndex(cityMap[initX,initY].resources, new("Workers"));
                        if(workerIndex == -1) continue;

                        cityMap[initX,initY].resources[workerIndex].count = cityMap[initX,initY].resources[workerIndex].storage;

                        resourceType[cityWorkerIndex].count += cityMap[initX,initY].resources[workerIndex].count;
                        resourceType[cityWorkerIndex].storage += cityMap[initX,initY].resources[workerIndex].storage;

                        if(cityMap[initX,initY].resources[workerIndex].storage == 0){
                            productionMod = 1;
                        }else{
                            productionMod = cityMap[initX,initY].resources[workerIndex].count/cityMap[initX,initY].resources[workerIndex].storage;
                        }
                        foreach(Resources item in resourceType){
                            item.findThenAdd(cityMap[initX,initY].resources, productionMod, false);
                        }
                    }                 
                }
            }
        }
        /*
        Console.WriteLine("Works: "+resourceType[cityWorkerIndex].count);
        Console.WriteLine("Worksplace: "+resourceType[cityWorkerIndex].storage);
        Console.WriteLine("Peeps: "+resourceType[populationIndex].count);
        */
    }
    public Boolean buildBuildings(Building targetBuilding, int buildingCount){
        List<int[]> targetBuildingSites = new List<int[]>();
        //A bonus that is calcualted to try and encourage the computer to build towards the middle
        int buildBonus;
        Boolean sortedItem;
        //Going through every tile on the list to see if there are any buildable spots 
        for(int initX = 0; initX < mapSizeX; initX++){
            for(int initY = 0; initY < mapSizeY; initY++){
                //If building needs to be on the outskirts
                //Also check if building needs to be built next to others and if spot is adjacent to a building
                if(
                    (
                        (
                            targetBuilding.buildOnOutskirts && 
                            (initX == 0 || initY == 0 || initX == mapSizeX-1 || initY == mapSizeY-1)
                        ) || targetBuilding.buildOnOutskirts == false) &&
                    cityMap[initX,initY].emptyTile &&
                    (
                        targetBuilding.needsNeighbors == false || 
                        cityMap[initX,initY].buildingRank > 0
                    )             
                ){
                    //This equation should make it so that the bonus gets lower the further from the center it gets
                    buildBonus = (int)-Math.Sqrt((initX-mapSizeX/2)*(initX-mapSizeX/2)+(initY-mapSizeY/2)*(initY-mapSizeY/2));
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
        if(targetBuildingSites.Count == 0) return false;
        
        //Buildings added using the spots found
        for(int buildCount = 0; buildCount < buildingCount; buildCount++){
            //If the computer wants to build more buildings then we have spots, we build as 
            //many as we can and then break the for loop
            if(buildCount >= targetBuildingSites.Count) break;
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
        if(!adminBuild) for(int i = 0; i<resourceType.Length; i++)
            resourceType[i].findThenAdd(targetBuilding.resources, 1, true);

        //Increase adjacent building ranks 
        for(int xMod = -1; xMod < 2; xMod++)
        for(int yMod = -1; yMod < 2; yMod++)
        if(xLocation + xMod < mapSizeX && xLocation + xMod >= 0 && yLocation + yMod < mapSizeY && yLocation + yMod >= 0)
        cityMap[xLocation + xMod,yLocation + yMod].buildingRank += targetBuilding.buildingRank;

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
        if(targetBuildingSites.Count == 0) return false;
        
        //Buildings added using the spots found
        for(int buildCount = 0; buildCount < buildingCount; buildCount++){
            //If the computer wants to build more buildings then we have spots, we build as 
            //many as we can and then break the for loop
            if(buildCount >= targetBuildingSites.Count) break;
            
            //Console.WriteLine(targetBuildingSites[buildCount][0]+","+targetBuildingSites[buildCount][1]);
            if(!addBuilding(replacingBuilding, targetBuildingSites[buildCount][0],targetBuildingSites[buildCount][1], false)) return false;            
        }
        //Console.WriteLine(buildingCount);
        return true;
    }
}


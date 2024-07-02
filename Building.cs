using System.ComponentModel;

public class Building{
    //Building name
    public String name;
    //How many pops can stay in the building
    public int popHousing;
    //How many people need to work at the building
    public int employeeCount;
    //What kind and how many resources does it cost to produce and how many per turn
    public Resources[] resources;
    //Must the building be next to other buildings?
    public Boolean needsNeighbors;
    //Does the building perfer to be on the outskirts of the city
    public Boolean buildOnOutskirts;
    //What is the building's rank (more important the higher the rank)
    public int buildingRank;

    //------------------------

    //How many people currently live in the building
    public int populationCount;
    //How many people currently work in the building
    public int workerCount;
    //Is the building abandoned?
    public Boolean emptyBuilding;
    //Is the building an empty buildable tile
    public Boolean emptyTile;


    public Building(){
        name = "empty";
        emptyTile = true;
        buildingRank = 0;
        resources = new Resources[0];
    }
    public Building(
            String name, 
            int buildingRank, 
            int popHousing, 
            int employeeCount, 
            Resources[] resources,
            Boolean needsNeighbors,
            Boolean buildOnOutskirts
        ){
        this.name = name;
        this.buildingRank = buildingRank;
        this.popHousing = popHousing;
        this.employeeCount = employeeCount;
        this.resources = resources;
        emptyBuilding = false;
        this.needsNeighbors = needsNeighbors;
        emptyTile = false;
        this.buildOnOutskirts = buildOnOutskirts;
    }
    public static Building prefabFarm(int buildingLevel){
        String[] upgradeNames = new String[]{
            "Berry Bush",
            "Basic Farm",
            "Farm",
            "Factory Farm"
        };
        return new Building( upgradeNames[buildingLevel], // Building Name
            0, // Building rank
            0, // Pop housing
            4*(buildingLevel+1), // Workers needed
            //What resources are needed to build and how many per turn
            new Resources[]{
                new Resources("Wood",-10*buildingLevel),
                new Resources("Stone",-10*buildingLevel),
                new Resources("Food",0,10*(buildingLevel+1)*(buildingLevel+1))
            },
            false, //Needs neighbors?
            true //Build on edge of map?
        );
    }
    public static Building prefabLumber(int buildingLevel){
        String[] upgradeNames = new String[]{
            "Stick Gatherers",
            "Lumberjack hut",
            "Log Mill",
            "Logging Company"
        };
        return new Building( upgradeNames[buildingLevel], // Building Name
            0, // Building rank
            0, // Pop housing
            2*(buildingLevel+1), // Workers needed
            //What resources are needed to build and how many per turn
            new Resources[]{
                new Resources("Wood",0,2*(buildingLevel+1)),
                new Resources("Stone",-10*buildingLevel),
            },
            false, //Needs neighbors?
            true //Build on edge of map?
        );
    }
    public static Building prefabMine(int buildingLevel){
        String[] upgradeNames = new String[]{
            "Stone Gatherers",
            "Stone Mine",
            "Stone Quarry",
            "Mountain Digger"
        };
        return new Building( upgradeNames[buildingLevel], // Building Name
            0, // Building rank
            0, // Pop housing
            2*(buildingLevel+1), // Workers needed
            //What resources are needed to build and how many per turn
            new Resources[]{
                new Resources("Wood",-10*buildingLevel),
                new Resources("Stone",0,2*(buildingLevel+1)),
                new Resources("Coal", 0, 2*(buildingLevel+1))
            },
            false, //Needs neighbors?
            true //Build on edge of map?
        );
    }
    public static Building prefabHouse(int buildingLevel){
        String[] upgradeNames = new String[]{
            "Small hut",
            "Wooden House",
            "Townhouse",
            "Apartment Complex"
        };
        return new Building( upgradeNames[buildingLevel], // Building Name
            5*(buildingLevel+1), // Building rank
            10*(buildingLevel+1)*(buildingLevel+1), // Pop housing
            0, // Workers needed
            //What resources are needed to build and how many per turn
            new Resources[]{
                new Resources("Wood",-12*(buildingLevel+1)),
                new Resources("Stone",-12*(buildingLevel+1)),
            },
            true, //Needs neighbors?
            false //Build on edge of map?
        );
    }
    public Building buildSelf(){
        return new Building(
            name,
            buildingRank,
            popHousing,
            employeeCount,
            resources,
            needsNeighbors,
            buildOnOutskirts
        );
    }
    public void printName(){
        Console.WriteLine(name);
    }
}

public class upgradableBuilding : Building{
    public String[] upgradeNames;
    //public String[] upgradeScaling;
    public upgradableBuilding(){
        //Empty Case
    }

    public upgradableBuilding(
            String[] upgradeNames,
            //String[] upgradeScaling, 
            Building baseBuilding
        ){
        this.upgradeNames = upgradeNames;
        //this.upgradeScaling = upgradeScaling;
        //this.baseBuilding = baseBuilding;
        name = baseBuilding.name;
        buildingRank = baseBuilding.buildingRank;
        popHousing = baseBuilding.popHousing;
        employeeCount = baseBuilding.employeeCount;
        resources = baseBuilding.resources;
        needsNeighbors = baseBuilding.needsNeighbors;
        buildOnOutskirts = baseBuilding.buildOnOutskirts;
    }

    public upgradableBuilding(
            String buildingTypeName,
            String[] upgradeNames,
            //String[] upgradeScaling, 
            int buildingRank,
            int popHousing,
            int employeeCount,
            Resources[] resources,
            Boolean needsNeighbors,
            Boolean buildOnOutskirts
        ){
        this.upgradeNames = upgradeNames;
        //this.upgradeScaling = upgradeScaling;
        name = buildingTypeName;
        this.buildingRank = buildingRank;
        this.popHousing = popHousing;
        this.employeeCount = employeeCount;
        this.resources = resources;
        this.needsNeighbors = needsNeighbors;
        this.buildOnOutskirts = buildOnOutskirts;
    }

    public Building createPrefab(int buildLevel){
        if(buildLevel>upgradeNames.Length-1) return new Building();

        Resources[] upgradedResource = new Resources[resources.Length];

        for(int i = 0; i < upgradedResource.Length; i++)
        upgradedResource[i] = new Resources(
            resources[i].name,
            resources[i].count*(buildLevel+1),
            resources[i].perTurn*(buildLevel+1)
        );

        return new Building(
            upgradeNames[buildLevel],
            buildingRank * (buildLevel+1),
            popHousing * (buildLevel+1),
            employeeCount,
            upgradedResource,
            needsNeighbors,
            buildOnOutskirts
        );
    }
}
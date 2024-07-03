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
    //What model does the building use
    public string model;

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
            String model,
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
    public Building buildSelf(){
        return new Building(
            name,
            model,
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
    public upgradableBuilding(){
        //Empty Case
    }

    public upgradableBuilding(
            String[] upgradeNames,
            Building baseBuilding
        ){
        this.upgradeNames = upgradeNames;
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
            String model,
            String[] upgradeNames,
            int buildingRank,
            int popHousing,
            int employeeCount,
            Resources[] resources,
            Boolean needsNeighbors,
            Boolean buildOnOutskirts
        ){
        this.upgradeNames = upgradeNames;
        this.model = model;
        name = buildingTypeName;
        this.buildingRank = buildingRank;
        this.popHousing = popHousing;
        this.employeeCount = employeeCount;
        this.resources = resources;
        this.needsNeighbors = needsNeighbors;
        this.buildOnOutskirts = buildOnOutskirts;
    }

    public Building createPrefab(int buildLevel){
        //If the input build level is higher than the amount of upgrades a building has
        //we then output an empty building
        if(buildLevel>upgradeNames.Length-1) return new Building();

        //Creating new resource array for the new building
        Resources[] upgradedResource = new Resources[resources.Length];

        //Creating new resource object that is scaled by the building level and 
        //storing it in the new resources array
        for(int i = 0; i < upgradedResource.Length; i++)
        upgradedResource[i] = new Resources(
            resources[i].name,
            resources[i].count*(buildLevel+1),
            resources[i].perTurn*(buildLevel+1),
            resources[i].storage*(buildLevel+1)
        );

        return new Building(
            upgradeNames[buildLevel],
            model,
            buildingRank * (buildLevel+1),
            popHousing * (buildLevel+1),
            employeeCount,
            upgradedResource,
            needsNeighbors,
            buildOnOutskirts
        );
    }

    public Boolean autoUpgrade(CivBuilder city){
        for(int level = 1; level< upgradeNames.Length; level++)
        if(city.replaceBuildings(createPrefab(level-1), createPrefab(level), 1)) return true;
        return false;
    }
    public Boolean build(CivBuilder city){
        if(!autoUpgrade(city)) 
        if(city.buildBuildings(createPrefab(0),1))
        return true;
        return false;
    }
}
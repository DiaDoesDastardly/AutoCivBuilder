using System.ComponentModel;
using System.DirectoryServices.ActiveDirectory;

public class Building{
    //Building name
    public String name;
    //How many pops can stay in the building
    public int popHousing;
    //How many people need to work at the building
    public int employeeCount;
    //What kind and how many resources does it cost to produce and how many per turn
    public Resources[] resources;
    //List of conditions on the building (it is a list for ease of addition and removal of conditions)
    public List<TileCondition> tileConditions;
    //Must the building be next to other buildings?
    public Boolean needsNeighbors;
    //Does the building perfer to be on the outskirts of the city
    public Boolean buildOnOutskirts;
    //Does the building need minable resources
    public Boolean needsMinableResources;
    //Does the building need growable soil
    public Boolean needsGrowableSoil;
    //What is the building's rank (more important the higher the rank)
    public int buildingRank;
    //What model does the building use
    public string model;
    //What level is the building
    public int level;

    //------------------------

    //Current health of the building (when at 0 the building returns to an empty tile)
    public int buildingHealth;
    //Is the building an empty buildable tile
    public Boolean emptyTile;

    public Building(){
        name = "empty";
        model = "trees.obj";
        emptyTile = true;
        buildingRank = 0;
        resources = [];
    }
    public Building(
            String name, 
            String model,
            int level,
            int buildingRank, 
            Resources[] resources,
            Boolean needsNeighbors,
            Boolean buildOnOutskirts
        ){
        this.name = name;
        this.model = model;
        this.level = level;
        this.buildingRank = buildingRank;
        this.resources = resources;
        this.needsNeighbors = needsNeighbors;
        emptyTile = false;
        this.buildOnOutskirts = buildOnOutskirts;
    }
    public Building buildSelf(){
        return new Building(
            name,
            model,
            level,
            buildingRank,
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
    public ResearchTree techTree;
    public upgradableBuilding(){
        //Empty Case
    }
    /*
    public upgradableBuilding(
            String[] upgradeNames,
            Building baseBuilding
        ){
        this.upgradeNames = upgradeNames;
        name = baseBuilding.name;
        level = baseBuilding.level;
        buildingRank = baseBuilding.buildingRank;
        popHousing = baseBuilding.popHousing;
        employeeCount = baseBuilding.employeeCount;
        resources = baseBuilding.resources;
        needsNeighbors = baseBuilding.needsNeighbors;
        buildOnOutskirts = baseBuilding.buildOnOutskirts;
    }
    */
    public upgradableBuilding(
            String buildingTypeName,
            String[] modelNames,
            String[] upgradeNames,
            int buildingRank,
            Resources[] resources,
            Boolean needsNeighbors,
            Boolean buildOnOutskirts
        ){
        this.techTree = new(
            upgradeNames,
            modelNames
        );
        /*
        this.upgradeNames = upgradeNames;
        this.modelNames = modelNames;
        */
        name = buildingTypeName;
        this.buildingRank = buildingRank;
        this.popHousing = popHousing;
        this.employeeCount = employeeCount;
        this.resources = resources;
        this.needsNeighbors = needsNeighbors;
        this.buildOnOutskirts = buildOnOutskirts;
    }
    public upgradableBuilding(
            String buildingTypeName,
            String[] modelNames,
            String[] upgradeNames,
            String[] researchModelNames,
            String[] researchUpgradeNames,
            Resources[] researchCosts,
            int buildingRank,
            Resources[] resources,
            Boolean needsNeighbors,
            Boolean buildOnOutskirts
        ){
        this.techTree = new(
            upgradeNames,
            researchUpgradeNames,
            modelNames,
            researchModelNames,
            researchCosts
        );
        //this.upgradeNames = upgradeNames;
        //this.modelNames = modelNames;
        name = buildingTypeName;
        this.buildingRank = buildingRank;
        this.resources = resources;
        this.needsNeighbors = needsNeighbors;
        this.buildOnOutskirts = buildOnOutskirts;

        //Creating tech tree for the building
    }

    public Building createPrefab(int buildLevel){
        //Creating a variable to hold the name of our model
        //This is so we can still output a model even if the upgradableBuilding doesn't hold
        //a model name for the level of building
        //Using a cube for right now to show when a building has no model
        String modelName = "cube.obj";
        //If the input build level is higher than the amount of upgrades a building has
        //we then output an empty building
        if(buildLevel>techTree.upgradeNames.Count-1) return new Building();
        //If the input build level is lower than the number of model names then we output the
        //name of the model at that build level
        if(buildLevel<techTree.modelNames.Count) modelName = techTree.modelNames[buildLevel];
        else modelName = techTree.modelNames[0];
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
            techTree.upgradeNames[buildLevel],
            modelName,
            buildLevel,
            buildingRank * (buildLevel+1),
            upgradedResource,
            needsNeighbors,
            buildOnOutskirts
        );
    }
    public Boolean researchBuilding(CivBuilder city){
        //The index of the science resource in the city
        int scienceIndex = 0;
        //Making sure that the next node isn't blank
        if(techTree.firstNode.nextNode.upgradeName != ""){
            //Getting the index for science in the city resources
            scienceIndex = techTree.firstNode.nextNode.science.getIndex(city.resourceType);
            //Check if the city can afford to do the research
            if(city.resourceType[scienceIndex].count + techTree.firstNode.nextNode.science.count >=0){
                //Subtract the cost of the research from the city resources
                city.resourceType[scienceIndex].addCount(techTree.firstNode.nextNode.science.count);
                //Adding the researched model and name to the list
                techTree.modelNames.Add(techTree.firstNode.nextNode.modelName);
                techTree.upgradeNames.Add(techTree.firstNode.nextNode.upgradeName);
                //Setting the current node to the researched node
                techTree.firstNode = techTree.firstNode.nextNode;
            }else{
                return false;
            }
        }
        return false;
    }
    public Boolean autoUpgrade(CivBuilder city){
        researchBuilding(city);
        for(int level = 1; level < techTree.upgradeNames.Count; level++)
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
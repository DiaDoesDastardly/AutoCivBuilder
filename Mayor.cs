using System.Xml.Serialization;

public class Mayor{
    //A mayor will have weights between 0 and 1 as to what kinds of resources they want to gather
    double[] resourceWeights;
    //The number of building actions they can do
    int buildingActionCount;
    //Name of the mayor
    String name;
    //What turn will the mayor retire
    int retirementTurn;
    public Mayor(){
        //Empty Case
    }
    public Mayor(
        String name,
        int lengthOfResources
    ){
        this.name = name;
        this.resourceWeights = new double[lengthOfResources];
        for(int index = 0; index < lengthOfResources; index++){
            resourceWeights[index] = 0.5;
        }
    }
    public Mayor(
        String name, 
        double[] resourceWeights
    ){
        this.name = name;
        this.resourceWeights = resourceWeights;
    }
    public void choosingBuilding(CivBuilder civ){
        //This is a function that controls what building will be built
        List<Resources> sortedResources = new List<Resources>();
        List<upgradableBuilding> sortedBuildings= new List<upgradableBuilding>();
        int listSortingIndex;
        Boolean sortedItem;
        //Apply demand weights to all resource types
        for(int index = 0; index < civ.resourceType.Length; index++){
            sortedItem = false;
            civ.resourceType[index].demand = (int)(civ.resourceType[index].demand*resourceWeights[index]);
            //Sort list by what resource is in demand
            if(sortedResources.Count() == 0){
                sortedResources.Add(civ.resourceType[index]);
                continue;
            }
            //Skip to next iteration if demand is below 0
            if(civ.resourceType[index].demand <= 0 ) continue;
            for(int sortingIndex = 0; sortingIndex < sortedResources.Count(); sortingIndex++){
                if(sortedResources[sortingIndex].demand < civ.resourceType[index].demand){
                    sortedResources.Insert(sortingIndex,civ.resourceType[index]);
                    sortedItem = true;
                    break;
                }
            }
            if(!sortedItem) sortedResources.Add(civ.resourceType[index]);
        }
        //If there is no resource in high demand then return function
        if(sortedResources.Count() == 0) return;
        //Sort building list by how much of demanded resource they produce per turn
        for(int index = 0; index < civ.buildingList.Count(); index++){
            sortedItem = false;
            if(Resources.getIndex(civ.buildingList[index].resources, sortedResources[0]) == -1) continue;
            listSortingIndex = Resources.getIndex(civ.buildingList[index].resources, sortedResources[0]);
            if(sortedBuildings.Count() == 0){
                sortedBuildings.Add(civ.buildingList[index]);
                continue;
            }
            for(int sortingIndex = 0; sortingIndex < sortedBuildings.Count(); sortingIndex++){
                if(Resources.getIndex(sortedBuildings[sortingIndex].resources, sortedResources[0]) == -1) continue;
                if(
                    civ.buildingList[index].resources[listSortingIndex].perTurn > 
                    sortedBuildings[sortingIndex].resources[
                        Resources.getIndex(sortedBuildings[sortingIndex].resources, sortedResources[0])
                    ].perTurn
                ){
                    sortedBuildings.Insert(sortingIndex,civ.buildingList[index]);
                    sortedItem = true;
                    break;
                }
                
            }
            if(!sortedItem) sortedBuildings.Add(civ.buildingList[index]);            
        }    

        //For testing we'll just use the current build command 
        sortedBuildings[0].build(civ);


        //The comments are for stuff to work on in the future
        //Find building that fills the demand the best out off all of them and 
        //doesn't consume too many resources (building near the top of the list that doesn't cause 
        //any per turn values to go negative)

        //Check if there is a building that can be upgraded to the needed level to fulfil demand

        //If there is such an upgradable building then upgrade it

        //If there is not then try to build a new building at the needed level (or highest accessable level)
    }
}
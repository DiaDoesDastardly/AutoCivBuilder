public class Citizen{
    public string name;
    public int birthTurn;
    public int[] placeOfEmployment;
    public int[] placeOfResidence;
    public Boolean employed;
    public Boolean housed;
    public static Resources[] baseResources;
    public Citizen(){
        name = "null";
    }
    public Citizen(string name, int turn){
        this.name = name;
        employed = false;
        housed = false;
        birthTurn = turn;
    }
    public static void removeCitizen(int citizenId, CivBuilder city){
        if(city.citizens[citizenId].employed){
            city.cityMap[
                city.citizens[citizenId].placeOfEmployment[0],
                city.citizens[citizenId].placeOfEmployment[1]
            ].workerCount -= 1;
        }
        if(city.citizens[citizenId].housed){
            city.cityMap[
                city.citizens[citizenId].placeOfResidence[0],
                city.citizens[citizenId].placeOfResidence[1]
            ].populationCount -= 1;
        }
        city.citizens.RemoveAt(citizenId);
    }
    public static void removeCitizens(int removalCount, CivBuilder city){
        for(int i = 0; i < removalCount; i++){
            if(city.citizens.Count == 0) break;
            removeCitizen(0, city);
        }
    }
    public static Boolean citizenUpkeep(CivBuilder city){
        
        int populationIndex = Resources.getIndex(city.resourceType, new("Population"));
        int population = Resources.findThenReturn(new("Population"), city.resourceType).count;
        foreach(Resources item in city.resourceType){
            //Increasing resource demand to be parallel with consumption 
            if(Resources.getIndex(baseResources, item) != -1){
                item.demand -= population * baseResources[Resources.getIndex(baseResources, item)].perTurn;
            }
            //city.resourceType[Resources.getIndex(city.resourceType, item)].demand += population;
            if(item.findThenAdd(baseResources, population, false)){
                if(item.count<0){
                    removeCitizens(Math.Abs(item.count), city);
                    city.resourceType[populationIndex].count += item.count;
                    city.resourceType[populationIndex].perTurn += item.perTurn;
                    //item.demand = Math.Abs(item.count);
                    item.count = 0;                    
                } 
            }
        }
        if(city.resourceType[populationIndex].count < 0) return false;
        for(int i = 0; i < city.resourceType[populationIndex].count/10; i++){
            city.citizens.Add(new("Newbie",city.turnCount));
            
            if(i+city.resourceType[populationIndex].count >= city.resourceType[populationIndex].storage){
                city.resourceType[populationIndex].demand = 10;
                break;
            }
        }
        city.resourceType[populationIndex].perTurn += city.resourceType[populationIndex].count/2;
        city.resourceType[populationIndex].count += city.resourceType[populationIndex].count/2;
        return true;
    }
}
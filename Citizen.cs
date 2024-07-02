public class Citizen{
    public string name;
    public int birthTurn;
    public int[] placeOfEmployment;
    public int[] placeOfResidence;
    public Boolean employed;
    public Boolean housed;
    public static Resources[] resources;
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
    public static void addCitizen(int newCount){
        for(int i = 0; i<resources.Length; i++) {
            resources[i].count += baseResources[i].count * newCount;
        }
    }

    public static void removeCitizen(int newCount){
        for(int i = 0; i<resources.Length; i++) {
            resources[i].count -= baseResources[i].count * newCount;
        }
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
        //removeCitizen(removalCount);
        city.population = city.citizens.Count;
    }
}
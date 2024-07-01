using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

//Object class to track different kinds of reources without having to hard code another type
public class Resources{
    //The name of the resource
    public String name;
    //How many resources of the type do we have
    public int count;
    //How many resources of the type per turn 
    public int perTurn;
    //How much is the resource in demand
    public int demand;
    public Resources(){
        //Empty case
    }
    public Resources(String name, int count){
        this.name = name;
        this.count = count;
        perTurn = 0;
        demand = 0;
    }
    public Resources(String name, int count, int perTurn){
        this.name = name;
        this.count = count;
        this.perTurn = perTurn;
        demand = 0;
    }

    public static Boolean find(String name, Resources[] resourceArray){
        for(int i = 0; i<resourceArray.Length; i++){
            if(name == resourceArray[i].name){
                return true;
            }
        }
        return false;
    }

    public Boolean findThenAdd(Resources[] resourceArray, double scalar, Boolean includeCount){
        for(int i = 0; i<resourceArray.Length; i++){
            if(name == resourceArray[i].name){
                if(includeCount)count += (int)(resourceArray[i].count*scalar);
                else count += (int)(resourceArray[i].perTurn*scalar);
                perTurn += (int)(resourceArray[i].perTurn*scalar);
                return true;
            }
        }
        return false;
    }
    //Check to see if we have enough resources for the cost
    public static Boolean checkStock(Resources[] stock, Resources[] cost, Boolean checkPerTurn){
        for(int costTrack = 0; costTrack<cost.Length; costTrack++){
            //Check and see if the resource exists in our stock
            if(!find(cost[costTrack].name,stock)) return false;
            for(int stockTrack = 0; stockTrack<stock.Length; stockTrack++){
                if(cost[costTrack].name == stock[stockTrack].name){
                    if(!checkPerTurn && Math.Abs(cost[costTrack].count) > stock[stockTrack].count) return false;
                    if(checkPerTurn && Math.Abs(cost[costTrack].perTurn) < 0 && stock[stockTrack].count < 0) return false;
                }
            }
        }
        return true;
    }
}
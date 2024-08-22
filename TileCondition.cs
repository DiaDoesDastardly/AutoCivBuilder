public class TileCondition{
    //What is the name of the condition
    String conditionName;
    //Can the condition spread to other tiles
    Boolean tileSpread;
    //Makes all resource production 0 on tile
    Boolean zeroProduction;
    //Can buildings be built on the tile with this effect (doesn't remove already existing builds)
    Boolean buildableArea;
    //Has the tile already completed it's action for the turn
    Boolean actionRest;
    //How many tiles can the tile spread
    int spreadCount;
    //Chance to spread to other tiles
    double spreadChance;
    //Turns left before expiration (if -1 then )
    int expirationTime;
    //Damage to buildings per turn
    int buildingDamage;
    //Damage to citizens per turn
    int citizenDamage;
    //Resource production scalars (the count is the scalar)
    Resources[] resourceProductionScalars;
    public TileCondition(){
        //Empty Case
    }
    public TileCondition(
        String conditionName,
        Boolean tileSpread,
        Boolean zeroProduction,
        Boolean buildableArea,
        Boolean actionRest,
        int spreadCount,
        double spreadChance,
        int expirationTime,
        int buildingDamage,
        int citizenDamage,
        Resources[] resourceProductionScalars
    ){
        //Case with all of the individual variables
    }
    public Boolean spreadToTiles(CivBuilder city, int xPosition, int yPosition){
        int tileSpreadLeft = spreadCount;
        //If the tile can spread then spread, otherwise return false
        //If the tile has already acted or was just created then return false
        if(tileSpread || !actionRest){
            for(int xPlusOrMinus = -1; xPlusOrMinus < 1; xPlusOrMinus++){
                for(int yPlusOrMinus = -1; yPlusOrMinus < 1; yPlusOrMinus++){
                    //If our selected tile is off the map then continue to next iteration

                    //If we cannot spread more then break from loop
                    if(tileSpreadLeft <= 0) break;
                    //Use randomization to see if the condition spreads to the specified tile

                    //If condition spreads then spread

                    //Actually add the randomization here

                    //Subtract the tile spread count by 1
                    tileSpreadLeft -= 1;
                    //Make sure that the condition doesn't already exist on tile

                    //Add the condition to the selected tile
                    city.cityMap[xPosition,yPosition].tileConditions.Add(this);
                }
            }
            return true;
        }
        return false;
    }
}

/*
Things tile conditions can do
-Spreadable fire
-Spreadable diseases
-Earthquakes
-Tornados
-Weather conditions 
-Different resource production bonuses or detriments (Allowing for zones of no or high production of certain resources)
-Different kinds of biomes?
*/

/*
Things to maybe add
-Model for the condition
*/
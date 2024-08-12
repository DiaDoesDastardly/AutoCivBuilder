//The tech tree for a building, only one branch can be chosen per level
using System.Configuration;

public class  ResearchTree{
    //Holds first node of the researchTree
    public researchTreeItem firstNode;
    //Holds the current upgrade names for the upgradableBuilding
    public List<String> upgradeNames;
    //Holds the current model names for the upgradableBuilding
    public List<String> modelNames;
    //Empty case
    public ResearchTree(){
        upgradeNames = new List<String>();
        modelNames = new List<String>();
        firstNode = new();
        firstNode.nextNode = new();
    }
    public ResearchTree(
        String[] upgradeNames,
        String[] modelNames
    ){
        this.upgradeNames = new List<String>(upgradeNames);
        this.modelNames = new List<String>(modelNames);
        this.firstNode = new();
        this.firstNode.nextNode = new();
    }
    public ResearchTree(
        String[] upgradeNames,
        String[] researchUpgradeNames,
        String[] modelNames,
        String[] researchModelNames,
        Resources[] scienceCosts
    ){
        //The givenUpgradeNames and givenModelNames are the upgradeNames and modelNames the 
        //upgradableBuilding has at the start
        this.upgradeNames = new List<String>(upgradeNames);
        this.modelNames = new List<String>(modelNames);
        //Creating the research tree from the upgradeNames and modelNames from the addNodes function
        this.firstNode = new();
        this.firstNode.addNodes(
            scienceCosts,
            researchModelNames,
            researchUpgradeNames
        );
    }
    //Check if there is enough science to research next level, if so then research
    public void upgradeTech(Resources[] resources){
        if(firstNode.nextNode.upgradeName == "") return;
        if(Resources.findThenReturn(firstNode.nextNode.science,resources).count+firstNode.nextNode.science.count > 0){
            upgradeNames.Add(firstNode.nextNode.modelName);
            modelNames.Add(firstNode.nextNode.modelName);
            firstNode = firstNode.nextNode;
        }
    }
}

//Individual items on the tech tree with a certain science cost
public class researchTreeItem{
    //Holds the Science cost of the tree node
    public Resources science;
    //Holds model name of the next upgrade
    public String modelName;
    //Holds the name of the upgrade
    public String upgradeName;
    //Previous node on the tech tree
    researchTreeItem previousNode;
    //Next nodes on the tree held as a list for easy addition to the list
    public researchTreeItem nextNode;
    //Empty case
    public researchTreeItem(){
        
        science = new ("Science",-10);
        modelName = "cube.obj";
        upgradeName = "";
    }
    //Object creation with science cost and model name
    public researchTreeItem(
        Resources science,
        String upgradeName,
        String modelName
    ){
        this.science = science;
        this.modelName = modelName;
    }
    //Object creation with science cost, model name, and previous node
    //Make sure that previous node is passed by reference
    public researchTreeItem(
        Resources science,
        String modelName,
        String upgradeName,
        researchTreeItem previousNode
    ){
        this.science = science;
        this.modelName = modelName;
        this.previousNode = previousNode;
    }
    //Add a new node to the next nodes with a ref to this node
    public void addNode(
        Resources science,
        String modelName,
        String upgradeName

    ){
        nextNode = new(
            science,
            modelName,
            upgradeName,
            this
        );
    }
    //Calling the addNodes function without an index calls the regular function but with a 1 index
    public void addNodes(
        Resources[] science,
        String[] modelName,
        String[] upgradeName
    ){ 
        //Sets the current node data to that of the first of each array
        this.science = science[0];
        this.modelName = modelName[0];
        this.upgradeName = upgradeName[0];
        //Calling add nodes with a 1 index
        addNodes(
            science,
            modelName,
            upgradeName,
            1
        );
    }
    //Recursive function to define the nodes of the research tree
    public void addNodes(
        Resources[] science,
        String[] modelName,
        String[] upgradeName,
        int index
    ){
        //If the index is larger than our array size then return
        if(
            index >= science.Length &&
            index >= modelName.Length &&
            index >= upgradeName.Length
        ){
            //Creating blank end node
            nextNode = new();
            return;
        }
        //Creating the next node in the sequence
        nextNode = new(
            science[index],
            modelName[index],
            upgradeName[index],
            this
        );
        //Calls the next node's addNodes function with the index increased by one
        nextNode.addNodes(
            science,
            modelName,
            upgradeName,
            index + 1
        );
    }
}

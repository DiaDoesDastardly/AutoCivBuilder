//The tech tree for a building, only one branch can be chosen per level
using System.Configuration;

public class  ResearchTree{
    //Holds first node of the researchTree
    researchTreeItem firstNode;
    //Holds the upgradeNames of an upgradableBuilding
    public List<String> upgradeNames;
    //Holds the modelNames of an upgradableBuilding
    public List<String> modelNames;
    //Empty case
    public ResearchTree(){
        //WEh
    }
    public ResearchTree(
        String[] upgradeNames,
        String[] modelNames
    ){
        this.upgradeNames = new List<String>(upgradeNames);
        this.modelNames = new List<String>(modelNames);
        this.firstNode = new();
    }
    //Check if there is enough science to research next level, if so then research
    public void upgradeTech(Resources[] resources){
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
    //Previous node on the tech tree
    researchTreeItem previousNode;
    //Next nodes on the tree held as a list for easy addition to the list
    public researchTreeItem nextNode;
    //Empty case
    public researchTreeItem(){
        
        science = new ("Science",-10);
        modelName = "cube.obj";
    }
    //Object creation with science cost and model name
    public researchTreeItem(
        Resources science,
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
        researchTreeItem previousNode
    ){
        this.science = science;
        this.modelName = modelName;
        this.previousNode = previousNode;
    }
    //Add a new node to the next nodes with a ref to this node
    public void addNode(
        Resources science,
        String modelName
    ){
        nextNode = new(
            science,
            modelName,
            this
        );
    }
}

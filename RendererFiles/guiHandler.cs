using System.Numerics;

public class guiHandler{
    //List of contained guiObjects
    public List<guiObject> guiObjects = new List<guiObject>();
    public int[][][] guiScreen;
    public guiHandler(){
        //Empty case
    }
    public guiHandler(
        int xScreenSize, 
        int yScreenSize
    ){
        setScreenToDefault(xScreenSize,yScreenSize);
    }
    public void setScreenToDefault(
        int xScreenSize, 
        int yScreenSize
    ){
        //Creating the guiScreen variable
        guiScreen = new int[xScreenSize][][];
        for(int x = 0; x < xScreenSize; x++){
            guiScreen[x] = new int[yScreenSize][];
            for(int y = 0; y < xScreenSize; y++){
                guiScreen[x][y] = new int[]{
                    0, //red color value of the pixel on the screen
                    0, //green color value of the pixel on the screen
                    0, //blue color value of the pixel on the screen
                    -1,//layer that the pixel is on
                    -1 //index of the guiObject the pixel is a part of
                };
            }
        }
    }
    //The renderGuiObjects function should only need to be run when updating the 
    //guiObjects list and at the start of the program
    public void renderGuiObjects(){
        /*
            This function should only need to be run at the beginning of the program and whenever
            the object list is updated

            A faster method for updating after the first render would be to only rerender
            what has changed in the GUI. Though this current code works for now as we rarely 
            update the gui (usually only once per turn)
        */
        double textureToScreenSizeX = 0;
        double textureToScreenSizeY = 0;
        Color colorHolder;

        //Resetting guiScreen values back to defaults
        setScreenToDefault(guiScreen.Length,guiScreen[0].Length);

        for(int index = 0; index < guiObjects.Count; index++){
            //If the object is counted as hidden then don't render it
            if(guiObjects[index].hidden) continue;
            //If the guiObject is not located on the screen then continue to the next guiObject
            if(
                guiObjects[index].bottomLeft.X > guiScreen.Length ||
                guiObjects[index].bottomLeft.Y > guiScreen[0].Length ||
                guiObjects[index].topRight.X < 0 ||
                guiObjects[index].topRight.Y < 0
            ) continue;
            //Finding the difference in size between the texture and screen coordinates
            textureToScreenSizeX = 
                (double)guiObjects[index].texture.Width/(
                    guiObjects[index].topRight.X-guiObjects[index].bottomLeft.X
                );
            textureToScreenSizeY = 
                (double)guiObjects[index].texture.Height/(
                    guiObjects[index].topRight.Y-guiObjects[index].bottomLeft.Y
                );
            //Applying the guiObject data to the guiScreen
            for(
                int x = (int)guiObjects[index].bottomLeft.X; 
                x < (int)guiObjects[index].topRight.X; 
                x++
            ){
                for(
                    int y = (int)guiObjects[index].bottomLeft.Y; 
                    y < (int)guiObjects[index].topRight.Y; 
                    y++
                ){
                    /*
                        Checking if the currently existing value is on a higher layer than our 
                        guiObject, if it is then we continue to the next itteration 
                    */
                    if(guiScreen[x][y][3] > guiObjects[index].layer) continue;
                    //Getting the relevant pixel data from the texture
                    colorHolder = guiObjects[index].texture.GetPixel(
                        (int)((x-(int)guiObjects[index].bottomLeft.X)*textureToScreenSizeX),
                        (int)((y-(int)guiObjects[index].bottomLeft.Y)*textureToScreenSizeY)
                    );
                    //Setting the guiScreen data to our current pixel
                    guiScreen[x][y][0] = colorHolder.R;
                    guiScreen[x][y][1] = colorHolder.G;
                    guiScreen[x][y][2] = colorHolder.B;
                    guiScreen[x][y][3] = guiObjects[index].layer;
                    guiScreen[x][y][4] = index;
                }
            }
        }
    }
    
    public void click(Vector2 clickLocation){
        //Getting the index value at the click location 
        int index = guiScreen[(int)clickLocation.X][(int)clickLocation.Y][4];
        //If the index is below 0 then return
        if(index < 0 || !guiObjects[index].clickable) return;
        //Run the functions that are attached to the guiObject
        guiObjects[index].clickAction();
    }
    
}

public class guiObject{
    //Name of the guiObject
    public String objectName;
    //The location of the bottom left corner of the guiObject
    public Vector2 bottomLeft;
    //The location of the top right corner of the guiObject
    public Vector2 topRight;
    //What layer is the guiObject on
    public int layer;
    //Does something happen if the guiObject is clicked on
    public Boolean clickable;
    //Do we render the guiObject 
    public Boolean hidden;
    //Has the guiObject been clicked
    public Boolean clicked;
    //What texture does the guiObject use
    public Bitmap texture;
    /*
        Defining a delegate type to hold any code that is executed when the 
        guiObject is clicked
    */
    public delegate void clickActionHolder();
    //Adding the default action to the clickaction holder 
    public clickActionHolder clickAction = defaultClickAction;
    public guiObject(){
        //Empty case
        objectName = "";
        bottomLeft = new Vector2(0, 0);
        topRight = new Vector2(0, 0);
        layer = 0;
        clickable = false;
        hidden = false;
        texture = new Bitmap(0,0);
    }
    public guiObject(
        String objectName,
        Vector2 bottomLeft,
        Vector2 topRight,
        int layer,
        Boolean clickable,
        Boolean hidden,
        Bitmap texture
    ){
        this.objectName = objectName;
        this.bottomLeft = bottomLeft;
        this.topRight = topRight;
        this.layer = layer;
        this.clickable = clickable;
        this.hidden = hidden;
        this.texture = texture;
    }
    static void defaultClickAction(){
        //This exists as the default delegate action 
        Console.WriteLine("Default delegate action");
    }
}
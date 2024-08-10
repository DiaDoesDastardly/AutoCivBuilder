using System.Numerics;
using System.Runtime.CompilerServices;

public partial class MyForm : Form
{
    //private rendererPipeline render;
    private System.Windows.Forms.Timer timer;
    private int intervalMilliseconds = (int)(1000/120); // Change this to set the interval in milliseconds
    private Random random = new Random();
    private PictureBox pictureBox1;
    private double[,] rotationalMatrix;
    private gameObject[] renderObjects;
    private CivBuilder civ = new CivBuilder();
    private String objectFolder = "..\\..\\..\\Objects\\";
    //gameObject[] buildingModels = new gameObject[civ.mapSizeX*civ.mapSizeY];
    private double scale;
    private TrackBar trackBar1;
    private TrackBar trackBar2;

    private long lastMiliCheck;
    private double rowAssignmentTimer = 0;
    private double rowAssignmentCounter = 0;
    private int highestRowAssignment = 0;
    private long renderToScreenTimer = 0;
    private long totalRenderTime = 0;

    private guiHandler guiHandler;

    private Boolean nextTurnActive = true;

    private Boolean mouseDown = false;
    private vector3 offset = new(0,0,0);
    private Vector2 lastMousePosition = new (-1,-1);
    private int theta = 0;

    public MyForm(double[,] rotationalMatrix, gameObject[] renderObjects,double scale)
    {
        renderObjects = new gameObject[civ.mapSizeX*civ.mapSizeY];
        //render = new rendererPipeline();
        this.rotationalMatrix = rotationalMatrix;
        this.renderObjects = renderObjects;
        this.scale = scale;
        lastMiliCheck = DateTime.Now.Ticks/TimeSpan.TicksPerMillisecond;
        //InitializeComponent();
        pictureBox1 = new PictureBox();
        //pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;
        for(int z = 0; z < PenroseEngine.rendererPipeline.screenInfo.Length; z++){
            PenroseEngine.rendererPipeline.screenInfo[z] = new double[(int)PenroseEngine.rendererPipeline.ySize][];
            for(int x = 0; x < PenroseEngine.rendererPipeline.screenInfo[z].Length; x++){
                PenroseEngine.rendererPipeline.screenInfo [z][x] = new double[5];
            }
        }
        //guiHandler initialization 
        guiHandler = new guiHandler(
            (int)PenroseEngine.rendererPipeline.xSize,
            (int)PenroseEngine.rendererPipeline.ySize
        );
        
        //Creation of guiObjects
        guiHandler.guiObjects.Add(
            new guiObject(
                "Test object",//Name of guiObject
                new (0,0), //Location of the bottom left corner of the object
                new (100,100), //Location of the top right corner of the object
                0, //Layer of the object
                false, //Is the object clickable
                true, //Is the object hidden
                new Bitmap(Image.FromFile("..\\..\\..\\guiTextures\\raincat.PNG")) //The texture of the gui object
            )
        );
        guiHandler.guiObjects.Add(
            new guiObject(
                "Next Turn Button",//Name of guiObject
                new (339,384), //Location of the bottom left corner of the object
                new (399,399), //Location of the top right corner of the object
                0, //Layer of the object
                true, //Is the object clickable
                false, //Is the object hidden
                new Bitmap(Image.FromFile("..\\..\\..\\guiTextures\\NextTurnButton.png")) //The texture of the gui object
            )
        );
        //guiHandler.guiObjects[0].clickAction += cat;
        guiHandler.guiObjects[1].clickAction += nextTurnButton;
        //guiHandler.guiObjects[1].clickAction += catVanish;
        guiHandler.renderGuiObjects();

        InitializeComponent();
        InitializeTimer();
    }
    //guiObject functions initialization
    void nextTurnButton(){
        //Console.WriteLine("Cat has been clicked");
        nextTurnActive = true;
    }
    void catVanish(){
        if(guiHandler.guiObjects[0].hidden) guiHandler.guiObjects[0].hidden = false;
        else if(!guiHandler.guiObjects[0].hidden) guiHandler.guiObjects[0].hidden = true;
        Console.WriteLine(guiHandler.guiObjects[0].hidden);
        guiHandler.renderGuiObjects();
    }
    //Making the mouse controls work
    void MouseWheelScroll(object sender, MouseEventArgs e){
        scale = Math.Pow(Math.Sqrt(scale) + e.Delta/100,2); 
        if(scale < 10) scale = 10;
    }
    void mouseClickDetect(object sender, MouseEventArgs e){
        //MouseEventArgs weh = (MouseEventArgs)e;
        lastMousePosition = new (e.X,e.Y);
        guiHandler.click(new(e.X,e.Y));
        mouseDown = true;
    }
    void mouseUnclick(object sender, MouseEventArgs e){
        //MouseEventArgs weh = (MouseEventArgs)e;
        mouseDown = false;
    }
    void mouseDragDetect(object sender, MouseEventArgs e){
        if(mouseDown && e.Button == MouseButtons.Left){
            //Finding the current theta angle of the camera
            double angle = Math.PI*theta/180;
            //Finding the the vector of change for the camera angle and mouse movements
            vector3 vectorOfChange = new vector3(
                Math.Cos(angle)*(lastMousePosition.X - e.X)/scale+Math.Sin(angle)*(lastMousePosition.Y - e.Y)/scale,
                0,
                Math.Cos(angle+Math.PI/2)*(lastMousePosition.X - e.X)/scale+Math.Sin(angle+Math.PI/2)*(lastMousePosition.Y - e.Y)/scale
            );
            //Adding the vector of change to the offset
            offset.add(vectorOfChange);
            //Setting the last mouse position to the current mouse position for the next check
            lastMousePosition = new (e.X,e.Y);
        }
        if(mouseDown && e.Button == MouseButtons.Middle){
            //Changing the camera theta by however much it has moved left or right
            theta += (int)(lastMousePosition.X - e.X);
            lastMousePosition = new (e.X,e.Y);
        }
    }
    void InitializeComponent()
    {
        this.pictureBox1 = new System.Windows.Forms.PictureBox();
        // Create a new TrackBar control
        /*
        trackBar1 = new TrackBar
        {
            // Set the properties of the TrackBar
            Minimum = -180,
            Maximum = 180,
            Value = 0, // Initial value
            TickStyle = TickStyle.TopLeft,
            TickFrequency = 10,
            Width = 200,
            Location = new System.Drawing.Point(0, 400)
        };
        */
        // Create a new TrackBar control
        /*
        trackBar2 = new TrackBar
        {
            // Set the properties of the TrackBar
            Minimum = -180,
            Maximum = 180,
            Value = 0, // Initial value
            TickStyle = TickStyle.TopLeft,
            TickFrequency = 10,
            Width = 200,
            Location = new System.Drawing.Point(200, 400)
        };
        */
        // Add the TrackBar to the form's Controls collection
        //Controls.Add(trackBar1);
        //Controls.Add(trackBar2);

        // PictureBox
        this.pictureBox1.Location = new System.Drawing.Point(0, 0);
        this.pictureBox1.Name = "pictureBox1";
        this.pictureBox1.Size = new System.Drawing.Size(400, 400);
        this.pictureBox1.TabIndex = 0;
        this.pictureBox1.TabStop = false;
        
        // Form
        this.Controls.Add(this.pictureBox1);
        //Adding mouse control to 
        this.pictureBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mouseClickDetect);
        this.pictureBox1.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.MouseWheelScroll);
        this.pictureBox1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.mouseUnclick);
        this.pictureBox1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.mouseDragDetect);
        
        this.ClientSize = new System.Drawing.Size(400, 400);
        this.Name = "Form1";
        this.Text = "Form1";
        ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
        this.ResumeLayout(false);
    }

    private void InitializeTimer()
    {
        timer = new System.Windows.Forms.Timer();
        timer.Interval = intervalMilliseconds;
        timer.Tick += Timer_Tick;
        timer.Start();
    }
    // Timer tick event handler
    private void Timer_Tick(object sender, EventArgs e)
    {
        //Civ iterate
        if(nextTurnActive){
            civ.initTurn();
            civ.currentGameStats();
            //civ.printMap();
            for(int xPos = 0; xPos < civ.mapSizeX; xPos++)
            for(int yPos = 0; yPos < civ.mapSizeY; yPos++)
            renderObjects[xPos * civ.mapSizeY + yPos] = new gameObject(objectFolder + civ.cityMap[xPos, yPos].model)
            {
                position = new vector3((xPos - civ.mapSizeX / 2) * 2, 0, (yPos - civ.mapSizeY / 2) * 2)
                //scale = new vector3(.7, civ.cityMap[xPos,yPos].level+1, .7)        
            };
            nextTurnActive = false;
        }
        


        PenroseEngine.rendererPipeline.frameCounter += 1;
        PenroseEngine.rendererPipeline.rowAssignments = 0;
        PenroseEngine.rendererPipeline.totalTimeTaken = 0;
        renderToScreenTimer = 0;
        if(DateTime.Now.Ticks/TimeSpan.TicksPerMillisecond - lastMiliCheck > 1000){
            Console.WriteLine(
                "Average time for row assignments in ticks "+(rowAssignmentTimer/TimeSpan.TicksPerMillisecond/PenroseEngine.rendererPipeline.frameCounter)+"\n "+
                "Average row assignments "+(rowAssignmentCounter/PenroseEngine.rendererPipeline.frameCounter)+"\n "+
                //"Total average time in milliseconds "+((rowAssignmentCounter*rowAssignmentTimer)/(rendererPipeline.frameCounter*10000))+"\n "+
                "Highest number of row assignments "+highestRowAssignment+"\n "+
                "Total Row Assignment time "+(rowAssignmentTimer/TimeSpan.TicksPerMillisecond)+"\n "+
                "Total Render to screen time "+(totalRenderTime/TimeSpan.TicksPerMillisecond)+"\n "+
                "Screen resolution: "+(PenroseEngine.rendererPipeline.screenResolution)
            );
            Console.WriteLine(PenroseEngine.rendererPipeline.frameCounter);
            lastMiliCheck = DateTime.Now.Ticks/TimeSpan.TicksPerMillisecond;
            PenroseEngine.rendererPipeline.frameCounter = 1;
            rowAssignmentTimer = 0;
            rowAssignmentCounter = 0;
            highestRowAssignment = 0;
            totalRenderTime = 0;
        }

        rotationalMatrix = PenroseEngine.rendererPipeline.rotationMatrixGenerator(theta,-135);
        PenroseEngine.rendererPipeline.screenResolution = 1;
        foreach(gameObject item in renderObjects)
        PenroseEngine.rendererPipeline.rotateTriangles(rotationalMatrix,item, scale, offset);
        renderToScreenTimer = DateTime.Now.Ticks;
        Image frame = PenroseEngine.rendererPipeline.renderToScreen(PenroseEngine.rendererPipeline.screenInfo, guiHandler);
        totalRenderTime += (DateTime.Now.Ticks-renderToScreenTimer);
        rowAssignmentTimer += PenroseEngine.rendererPipeline.totalTimeTaken;
        rowAssignmentCounter += PenroseEngine.rendererPipeline.rowAssignments;
        if(highestRowAssignment < PenroseEngine.rendererPipeline.rowAssignments) highestRowAssignment = PenroseEngine.rendererPipeline.rowAssignments;
        // Update the PictureBox with the edited image
        pictureBox1.Image = frame;
        
    }
}


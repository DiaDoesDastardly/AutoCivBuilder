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
    private String objectFolder = "Objects\\";
    //gameObject[] buildingModels = new gameObject[civ.mapSizeX*civ.mapSizeY];
    private double scale;
    private int theta = 0;

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
            PenroseEngine.rendererPipeline.xSize,
            PenroseEngine.rendererPipeline.ySize
        );

        guiHandler.guiObjects.Add(
            new guiObject(
                "Next Turn Button",//Name of guiObject
                new (339,384), //Location of the bottom left corner of the object
                new (399,399), //Location of the top right corner of the object
                0, //Layer of the object
                true, //Is the object clickable
                false, //Is the object hidden
                new Bitmap(Image.FromFile("guiTextures\\NextTurnButton.png")) //The texture of the gui object
            )
        );
        guiHandler.guiObjects[0].clickAction += nextTurnButton;
        guiHandler.renderGuiObjects();
        /*
        for(int i = 0; i < 200; i++) {
            civ.initTurn();
            civ.currentGameStats();
        }
        */
        InitializeComponent();
        InitializeTimer();
    }
    //guiObject functions initialization
    void nextTurnButton(){
        nextTurnActive = true;
    }
    //Making the mouse controls work
    void MouseWheelScroll(object sender, MouseEventArgs e){
        //Scroll increases at an exponential rate because it feels smoother to use than a linear rate
        scale = Math.Pow(Math.Sqrt(scale) + e.Delta/100,2); 
        //Making sure the city doesn't get too small
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
        //If the mouse goes over a GUI element or goes outside of the screen then stop dragging
        if(
            e.X < 0 || e.X > pictureBox1.Size.Width  - 1 ||
            e.Y < 0 || e.Y > pictureBox1.Size.Height - 1
        ) return;
        if(guiHandler.guiScreen[e.X][e.Y][4] != -1 ) return;
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
        pictureBox1 = new System.Windows.Forms.PictureBox
        {
            // PictureBox
            Location = new System.Drawing.Point(0, 0),
            Name = "pictureBox1",
            Size = new System.Drawing.Size(400, 400),
            TabIndex = 0,
            TabStop = false
        };

        // Form
        Controls.Add(pictureBox1);
        //Adding mouse control to 
        pictureBox1.MouseDown += new MouseEventHandler(mouseClickDetect);
        pictureBox1.MouseWheel += new MouseEventHandler(MouseWheelScroll);
        pictureBox1.MouseUp += new MouseEventHandler(mouseUnclick);
        pictureBox1.MouseMove += new MouseEventHandler(mouseDragDetect);
        
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
            if(!civ.initTurn()){                
                guiHandler.guiObjects[0].hidden = true;
                guiHandler.renderGuiObjects();
            }
            
            civ.currentGameStats();
            for(int xPos = 0; xPos < civ.mapSizeX; xPos++)
            for(int yPos = 0; yPos < civ.mapSizeY; yPos++)
            renderObjects[xPos * civ.mapSizeY + yPos] = new gameObject(objectFolder + civ.cityMap[xPos, yPos].model)
            {
                position = new vector3((xPos - civ.mapSizeX / 2) * 2, 0, (yPos - civ.mapSizeY / 2) * 2)
            };
            nextTurnActive = false;
        }

        PenroseEngine.rendererPipeline.frameCounter += 1;
        PenroseEngine.rendererPipeline.rowAssignments = 0;
        PenroseEngine.rendererPipeline.totalTimeTaken = 0;
        renderToScreenTimer = 0;
        if(DateTime.Now.Ticks/TimeSpan.TicksPerMillisecond - lastMiliCheck > 1000){
            PenroseEngine.rendererPipeline.frameCounter = 1;
            lastMiliCheck = DateTime.Now.Ticks/TimeSpan.TicksPerMillisecond;
        }
        /*
        if(DateTime.Now.Ticks/TimeSpan.TicksPerMillisecond - lastMiliCheck > 1000){
            Console.WriteLine(
                "Average time for row assignments in ticks "+(rowAssignmentTimer/TimeSpan.TicksPerMillisecond/PenroseEngine.rendererPipeline.frameCounter)+"\n "+
                "Average row assignments "+(rowAssignmentCounter/PenroseEngine.rendererPipeline.frameCounter)+"\n "+
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
        */
        rotationalMatrix = PenroseEngine.rendererPipeline.rotationMatrixGenerator(theta,-150);
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


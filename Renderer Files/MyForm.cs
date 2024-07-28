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
    private TrackBar trackBar3;
    private Button button1;

    private long lastMiliCheck;
    private double rowAssignmentTimer = 0;
    private double rowAssignmentCounter = 0;
    private int highestRowAssignment = 0;
    private long renderToScreenTimer = 0;
    private long totalRenderTime = 0;

    private Boolean nextTurnActive = true;

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
            PenroseEngine.rendererPipeline.screenInfo[z] = new double[(int)(PenroseEngine.rendererPipeline.ySize)][];
            for(int x = 0; x < PenroseEngine.rendererPipeline.screenInfo[z].Length; x++){
                PenroseEngine.rendererPipeline.screenInfo [z][x] = new double[5];
            }
        }
        InitializeComponent();
        InitializeTimer();
    }
    private void InitializeComponent()
    {
        this.pictureBox1 = new System.Windows.Forms.PictureBox();
        // Create a new TrackBar control
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

        // Create a new TrackBar control
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

        trackBar3 = new TrackBar
        {
            // Set the properties of the TrackBar
            Minimum = 1,
            Maximum = 100,
            Value = (int)scale, // Initial value
            TickStyle = TickStyle.TopLeft,
            TickFrequency = 10,
            Width = 200,
            Location = new System.Drawing.Point(0, 440)
        };

        button1 = new Button
        {
            Location = new System.Drawing.Point(200, 440),
            Text = "Next turn",
        };
        // Add the TrackBar to the form's Controls collection
        Controls.Add(trackBar1);
        Controls.Add(trackBar2);
        Controls.Add(trackBar3);
        Controls.Add(button1);

        //Adding button control
        button1.Click += button1_Click;

        // PictureBox
        this.pictureBox1.Location = new System.Drawing.Point(0, 0);
        this.pictureBox1.Name = "pictureBox1";
        this.pictureBox1.Size = new System.Drawing.Size(400, 400);
        this.pictureBox1.TabIndex = 0;
        this.pictureBox1.TabStop = false;
        
        // Form
        this.Controls.Add(this.pictureBox1);
        this.ClientSize = new System.Drawing.Size(400, 480);
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
    //Button clicking 
    private void button1_Click(object sender, EventArgs e){
        Console.WriteLine("Weh");
        nextTurnActive = true;
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

        scale = trackBar3.Value;
        rotationalMatrix = PenroseEngine.rendererPipeline.rotationMatrixGenerator(trackBar1.Value,trackBar2.Value);
        PenroseEngine.rendererPipeline.screenResolution = 1;//(1-(double)trackBar3.Value/(double)trackBar3.Maximum)*.5+.5;
        foreach(gameObject item in renderObjects)
        PenroseEngine.rendererPipeline.rotateTriangles(rotationalMatrix,item, scale);
        renderToScreenTimer = DateTime.Now.Ticks;
        Image frame = PenroseEngine.rendererPipeline.renderToScreen(PenroseEngine.rendererPipeline.screenInfo);
        totalRenderTime += (DateTime.Now.Ticks-renderToScreenTimer);
        rowAssignmentTimer += PenroseEngine.rendererPipeline.totalTimeTaken;
        rowAssignmentCounter += PenroseEngine.rendererPipeline.rowAssignments;
        if(highestRowAssignment < PenroseEngine.rendererPipeline.rowAssignments) highestRowAssignment = PenroseEngine.rendererPipeline.rowAssignments;
        // Update the PictureBox with the edited image
        pictureBox1.Image = frame;
        
    }
}


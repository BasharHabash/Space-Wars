//                                           Documentation
//----------------------------------------------------------------------------------------------------
//    Namespace:      SpaceWars
//    Author:         Bashar Al Habash (7abash), Cole Perschon (coleschon)         Date: Nov. 17, 2017
//----------------------------------------------------------------------------------------------------
///                                               Notes
///---------------------------------------------------------------------------------------------------
///    N/A:           N/A.
///                   
///                   (SEE README.txt)
///---------------------------------------------------------------------------------------------------





using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NetworkController;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;





/// <summary>
/// Contains all client-side code for the multiplayer game - SpaceWars.
/// </summary>
namespace SpaceWars
{
    /// <summary>
    /// Application Window.
    /// </summary>
    public partial class Form1 : Form
    {
        GamePanel GP;
        ScorePanel SP;
        private World world;
        const int worldSize = 750;
        const int scoreSize = 200;
        private bool left;
        private bool up;
        private bool right;
        private bool space;


        /// <summary>
        /// Contructor for Form1. 
        /// </summary>
        public Form1()
        {
            // Designer initialization
            InitializeComponent();

            // Key Controls and world reset
            left = false;
            up = false;
            right = false;
            space = false;
            world = new World();

            // Form sizing
            ClientSize = new Size(worldSize + scoreSize, worldSize + 50);

            // Game Panel sizing, position, initial design, and controls set
            GP = new GamePanel(world);
            GP.Location = new Point(0, 50);
            GP.Size = new Size(worldSize, worldSize);
            GP.BackColor = Color.Black;
            this.Controls.Add(GP);
            
            // Score Panel sizing, position, initial design, and controls set
            SP = new ScorePanel(world);
            SP.Location = new Point(worldSize, 50);
            SP.Size = new Size(scoreSize, worldSize);
            SP.BackColor = Color.White;
            this.Controls.Add(SP);
        }
        

        /// <summary>
        /// Sends initial data to server and receives data back.
        /// </summary>
        /// <param name="state"></param>
        private void FirstContact(SocketState state)
        {
            Networking.Send(state.theSocket, textBox_Name.Text);
            state.callMe = ReceiveStartup;
            Networking.GetData(state);
        }


        /// <summary>
        /// Deals with the inital message from server after client connection.
        /// </summary>
        /// <param name="state"></param>
        private void ReceiveStartup(SocketState state)
        {
            // Initial server message
            string message = state.sb.ToString();

            // Seperating the message into ID and world size
            string[] parts = Regex.Split(message, @"(?<=[\n])");
            parts[0] = parts[0].Remove(parts[0].Length - 1);
            parts[1] = parts[1].Remove(parts[1].Length - 1);
            
            string playerID = parts[0];
            int.TryParse(parts[1], out int worldSize);

            // Clear the message
            state.sb.Clear();

            // Resize the world, if needed
            Invoke(new MethodInvoker(() => resizeWorld(worldSize)));

            // Change the state to the function that will remain from here on out
            state.callMe = ProcessMessage;

            // Get data
            Networking.GetData(state);
        }


        /// <summary>
        /// The function that will primarily be used to translate server information, after
        /// the initial connection with the server is made.
        /// </summary>
        /// <param name="state"></param>
        private void ProcessMessage(SocketState state)
        {
            // Lock world and the objects in it so that the enumeration loops will execute properly
            lock (world)
            {
                string totalData = state.sb.ToString();

                // Messages are separated by newline
                string[] parts = Regex.Split(totalData, @"(?<=[\n])");

                // Loop until we have processed all messages as we may have received more than one.
                foreach (string p in parts)
                {
                    // Ignore empty strings added by the regex splitter
                    if (p.Length == 0)
                        continue;
                    // The regex splitter will include the last string even if it doesn't end with a '\n',
                    // So we need to ignore it if this happens 
                    if (p[p.Length - 1] != '\n')
                        break;

                    // Establish our JSON object tokens
                    JObject obj = JObject.Parse(p);
                    JToken ship = obj["ship"];
                    JToken star = obj["star"];
                    JToken Proj = obj["proj"];
                    
                    // If there was a Star in this part of the message
                    if (ship != null)
                    {
                        // Deserialize the JSON so we can create a Ship object within world
                        Ship s = JsonConvert.DeserializeObject<Ship>(p);
                        // New ship
                        if (!world.Ships.ContainsKey(s.ID))
                        {
                            world.Ships.Add(s.ID, s);
                            world.Ships[s.ID].alive = true;
                        }
                        // Update ship
                        else
                        {
                            world.Ships[s.ID] = s;
                            world.Ships[s.ID].alive = true;
                        }
                        
                        // Ship destroyed
                        if (world.Ships[s.ID].hp == 0)
                            world.Ships[s.ID].alive = false;

                        // Update the Score Panel.
                        SP.updateShips(world.Ships);
                    }

                    // If there was a Star in this part of the message
                    // Follow the same premise from ship
                    if (star != null)
                    {
                        Star s = JsonConvert.DeserializeObject<Star>(p);
                        if (!world.Stars.ContainsKey(s.ID))
                            world.Stars.Add(s.ID, s);
                        else
                            world.Stars[s.ID] = s;
                    }

                    // If there was a Projectile in this part of the message
                    // Follow the same premise from ship
                    if (Proj != null)
                    {
                        Projectile pr = JsonConvert.DeserializeObject<Projectile>(p);
                        if (!world.Projectiles.ContainsKey(pr.ID))
                            world.Projectiles.Add(pr.ID, pr);
                        else
                            world.Projectiles[pr.ID] = pr;

                        // If the projectile is no longer alive, remove it from 'world'.
                        if (!world.Projectiles[pr.ID].alive)
                            world.Projectiles.Remove(pr.ID);
                    }
                    
                    // Then remove the processed message from the SocketState's growable buffer
                    state.sb.Remove(0, p.Length);
                }
            }

            // Draw the updated world on both the game and score panels
            Drawer(state);

            // Finally, ask for more data
            // This will start an event loop
            Networking.GetData(state);
        }


        /// <summary>
        /// Resizes the world depending on the server specifications.
        /// </summary>
        /// <param name="newWorldSize"></param>
        private void resizeWorld(int newWorldSize)
        {
            ClientSize = new Size(newWorldSize + scoreSize, newWorldSize + 50);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
        }

        
        /// <summary>
        /// Draws the updated world on both game and score panel alike.
        /// Subsequently sends the server control messages.
        /// </summary>
        /// <param name="state"></param>
        private void Drawer(SocketState state)
        {
            // Try catch implemented in case of unforseen errors in drawing
            try
            {
                // Panel updating
                MethodInvoker m = new MethodInvoker(GP.Invalidate);
                if (!this.IsDisposed)
                    this.Invoke(m);
                MethodInvoker mg = new MethodInvoker(SP.Invalidate);
                if (!this.IsDisposed)
                    this.Invoke(mg);

                // Control sending
                MethodInvoker mI = new MethodInvoker(() => sendControlMessage(state));
                if (!this.IsDisposed)
                    this.Invoke(mI);
            }
            catch
            {
            }
        }
        


        //                                      Controls
        //--------------------------------------------------------------------------------------------
        
        /// <summary>
        /// Makes the initial connection to the server.
        /// </summary>
        /// <param name="sender">How key was pressed</param>
        /// <param name="e">Event carried with click</param>
        private void button_Connect_Click(object sender, EventArgs e)
        {
            // Empty Server address.
            if (textBox_Server.Text == "")
            {
                MessageBox.Show("Please enter a server address");
                return;
            }

            // Disable the controls and try to connect
            button_Connect.Enabled = false;
            textBox_Server.Enabled = false;
            textBox_Name.Enabled = false;
            button_Controls.Enabled = false;

            // Connect with server, if unsuccessful, reset
            try
            {
                Networking.ConnectToServer(FirstContact, textBox_Server.Text);
            }
            catch
            {
                button_Connect.Enabled = true;
                textBox_Server.Enabled = true;
                textBox_Name.Enabled = true;
                button_Controls.Enabled = true;

                MessageBox.Show("The server adress you entered is unreachable");
            }
        }


        /// <summary>
        /// Controls button message.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_Controls_MouseClick(object sender, MouseEventArgs e)
        {
            MessageBox.Show("UP Arrow:        Thrust\nLEFT Arrow:     Turn Left\nRIGHT Arrow:  " + 
                "Turn Right\nSPACE:              Fire!");
        }


        /// <summary>
        /// When a key is depressed, set key flag to true
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    left = true;
                    break;
                case Keys.Up:
                    up = true;
                    break;
                case Keys.Right:
                    right = true;
                    break;
                case Keys.Space:
                    space = true;
                    break;
            }
        }


        /// <summary>
        /// When a key is released, set key flag to false
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    left = false;
                    break;
                case Keys.Up:
                    up = false;
                    break;
                case Keys.Right:
                    right = false;
                    break;
                case Keys.Space:
                    space = false;
                    break;
            }
        }


        /// <summary>
        /// When an alloted key is pressed, set it is an input.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch (e.KeyCode) {
                case Keys.Left:
                case Keys.Up:
                case Keys.Right:
                case Keys.Space:
                    e.IsInputKey = true;
                    break;
            }
        }


        /// <summary>
        /// Takes the state of the key flags and converts them into a server readable string.
        /// </summary>
        /// <param name="state"></param>
        private void sendControlMessage(SocketState state)
        {
            string s = "(";
            if (left)
                s += "L";
            if (up)
                s += "T";
            if (right)
                s += "R";
            if (space)
                s += "F";
            s += ")";

            if (left || up || right || space)
                Networking.Send(state.theSocket, s);
        }
        //--------------------------------------------------------------------------------------------
    }




    /// <summary>
    /// The view panel in which the game is displayed and controls are taken.
    /// </summary>
    public class GamePanel : Panel
    {
        // World in which all objects exist
        private World world;
        
        // Image importation:
        // STAR
        private Image starImage = Image.FromFile("../../../Resources/Graphics Files/star.jpg");

        // SHIPS + THRUST SHIPS
        private static Image shipImageThrustBlue = Image.FromFile("../../../Resources/Graphics Files/ship-thrust-blue.png");
        private static Image shipImageCoastBlue = Image.FromFile("../../../Resources/Graphics Files/ship-coast-blue.png");
        private static Image projImageBlue = Image.FromFile("../../../Resources/Graphics Files/shot-blue.png");
        private static Image shipImageThrustBrown = Image.FromFile("../../../Resources/Graphics Files/ship-thrust-brown.png");
        private static Image shipImageCoastBrown = Image.FromFile("../../../Resources/Graphics Files/ship-coast-brown.png");
        private static Image projImageBrown = Image.FromFile("../../../Resources/Graphics Files/shot-brown.png");
        private static Image shipImageThrustGreen = Image.FromFile("../../../Resources/Graphics Files/ship-thrust-green.png");
        private static Image shipImageCoastGreen = Image.FromFile("../../../Resources/Graphics Files/ship-coast-green.png");
        private static Image projImageGreen = Image.FromFile("../../../Resources/Graphics Files/shot-green.png");
        private static Image shipImageThrustGrey = Image.FromFile("../../../Resources/Graphics Files/ship-thrust-grey.png");
        private static Image shipImageCoastGrey = Image.FromFile("../../../Resources/Graphics Files/ship-coast-grey.png");
        private static Image projImageGrey = Image.FromFile("../../../Resources/Graphics Files/shot-grey.png");
        private static Image shipImageThrustRed = Image.FromFile("../../../Resources/Graphics Files/ship-thrust-red.png");
        private static Image shipImageCoastRed = Image.FromFile("../../../Resources/Graphics Files/ship-coast-red.png");
        private static Image projImageRed = Image.FromFile("../../../Resources/Graphics Files/shot-red.png");
        private static Image shipImageThrustViolet = Image.FromFile("../../../Resources/Graphics Files/ship-thrust-violet.png");
        private static Image shipImageCoastViolet = Image.FromFile("../../../Resources/Graphics Files/ship-coast-violet.png");
        private static Image projImageViolet = Image.FromFile("../../../Resources/Graphics Files/shot-violet.png");
        private static Image shipImageThrustWhite = Image.FromFile("../../../Resources/Graphics Files/ship-thrust-white.png");
        private static Image shipImageCoastWhite = Image.FromFile("../../../Resources/Graphics Files/ship-coast-white.png");
        private static Image projImageWhite = Image.FromFile("../../../Resources/Graphics Files/shot-white.png");
        private static Image shipImageThrustYellow = Image.FromFile("../../../Resources/Graphics Files/ship-thrust-yellow.png");
        private static Image shipImageCoastYellow = Image.FromFile("../../../Resources/Graphics Files/ship-coast-yellow.png");
        private static Image projImageYellow = Image.FromFile("../../../Resources/Graphics Files/shot-yellow.png");

        // Compile color sets into arrays
        private static Image[] blue = new Image[] { shipImageThrustBlue, shipImageCoastBlue, projImageBlue };
        private static Image[] brown = new Image[] { shipImageThrustBrown, shipImageCoastBrown, projImageBrown };
        private static Image[] green = new Image[] { shipImageThrustGreen, shipImageCoastGreen, projImageGreen };
        private static Image[] gray = new Image[] { shipImageThrustGrey, shipImageCoastGrey, projImageGrey };
        private static Image[] red = new Image[] { shipImageThrustRed, shipImageCoastRed, projImageRed };
        private static Image[] violet = new Image[] { shipImageThrustViolet, shipImageCoastViolet, projImageViolet };
        private static Image[] white = new Image[] { shipImageThrustWhite, shipImageCoastWhite, projImageWhite };
        private static Image[] yellow = new Image[] { shipImageThrustYellow, shipImageCoastYellow, projImageYellow };

        // Store these colorsets into a list
        private List<Image[]> colors = new List<Image[]> { blue, brown, green, gray, red, violet, white, yellow };

        // Object sizing
        Rectangle recStar = new Rectangle(-20, -20, 40, 40);
        Rectangle recShip = new Rectangle(-15, -15, 30, 30);
        Rectangle recProj = new Rectangle(-10, -10, 20, 20);


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="w"></param>
        public GamePanel(World w)
        {
            // Double buffer the panel
            DoubleBuffered = true;
            world = w;
        }


        /// <summary>
        /// Helper method for DrawObjectWithTransform
        /// </summary>
        /// <param name="size">The world (and image) size</param>
        /// <param name="w">The worldspace coordinate</param>
        /// <returns></returns>
        private static int WorldSpaceToImageSpace(int size, double w)
        {
            return (int)w + size / 2;
        }

        
        /// <summary>
        /// A delegate for DrawObjectWithTransofrm
        /// Methods matching this delegate can draw whatever they want using e
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        public delegate void ObjectDrawer(object o, PaintEventArgs e);


        /// <summary>
        /// This method performs a translation and rotation to drawn an object in the world.
        /// </summary>
        /// <param name="e">PaintEventArgs to access the graphics (for drawing)</param>
        /// <param name="o">The object to draw</param>
        /// <param name="worldSize">The size of one edge of the world (assuming the world is square)</param>
        /// <param name="worldX">The X coordinate of the object in world space</param>
        /// <param name="worldY">The Y coordinate of the object in world space</param>
        /// <param name="angle">The orientation of the objec, measured in degrees clockwise from "up"</param>
        /// <param name="drawer">The drawer delegate. After the transformation is applied, the delegate is invoked to draw whatever it wants</param>
        private void DrawObjectWithTransform(PaintEventArgs e, object o, int worldSize, double worldX, double worldY, double angle, ObjectDrawer drawer)
        {
            // Perform the transformation
            int x = WorldSpaceToImageSpace(worldSize, worldX);
            int y = WorldSpaceToImageSpace(worldSize, worldY);
            e.Graphics.TranslateTransform(x, y);
            e.Graphics.RotateTransform((float)angle);

            // Draw the object 
            drawer(o, e);

            // Then undo the transformation
            e.Graphics.ResetTransform();
        }
        

        /// <summary>
        /// Responsible for painting the Ship objects.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void ShipDrawer(object o, PaintEventArgs e)
        {
            Ship s = o as Ship;
            // Creates an array index for colors that depends on the ship's ID
            int color = s.ID % 8;
            
            // Display ship
            if (!s.thrust)
            {
                e.Graphics.DrawImage(colors[color].ElementAt(1), recShip);
            }
            // Display ship thrust
            else
            {
                e.Graphics.DrawImage(colors[color].ElementAt(0),recShip);
            }
        }


        /// <summary>
        /// Responsible for painting the Projectile objects.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void ProjDrawer(object o, PaintEventArgs e)
        {
            Projectile p = o as Projectile;
            // Creates an array index for colors that depends on the projectile's ID
            int color = p.owner % 8;

            e.Graphics.DrawImage(colors[color].ElementAt(2), recProj);
        }


        /// <summary>
        /// Responsible for painting the Star objects.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void StarDrawer(object o, PaintEventArgs e)
        {
            Star s = o as Star;

            e.Graphics.DrawImage(starImage, recStar);
        }


        /// <summary>
        /// Calls all 'Drawer' methods.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            // Prevent race conditions from added objects
            lock (world)
            { 
                foreach (Ship s in world.Ships.Values)
                {
                    if (s.alive == true)
                        DrawObjectWithTransform(e, s, this.Size.Width, s.loc.GetX(), s.loc.GetY(), s.dir.ToAngle(), ShipDrawer);
                }
                foreach (Star s in world.Stars.Values)
                {
                    DrawObjectWithTransform(e, s, this.Size.Width, s.loc.GetX(), s.loc.GetY(), 0, StarDrawer);
                }
                foreach (Projectile p in world.Projectiles.Values)
                {
                    DrawObjectWithTransform(e, p, this.Size.Width, p.loc.GetX(), p.loc.GetY(), p.dir.ToAngle(), ProjDrawer);
                }

                // Do anything that Panel (from which we inherit) needs to do
                base.OnPaint(e);
             }
        }
    }




    /// <summary>
    /// The view panel in which the player's health and score is displayed.
    /// </summary>
    public class ScorePanel : Panel
    {
        private World world;

    
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="w"></param>
        public ScorePanel(World w)
        {
            DoubleBuffered = true;
            world = w;
        }


        /// <summary>
        /// Used to take information about the ships that exist in another world object.
        /// </summary>
        /// <param name="newShips"></param>
        public void updateShips (Dictionary<int, Ship> newShips)
        {
            world.Ships = newShips;
        }
        

        /// <summary>
        /// Name and score painter.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="score"></param>
        /// <param name="shipCount"></param>
        /// <param name="e"></param>
        private void nameScoreDrawer(string player, int score, int shipCount, PaintEventArgs e)
        {
            Font font = new Font("Arial", 14);
            SolidBrush brush = new SolidBrush(Color.Black);
            RectangleF rec = new RectangleF(1, shipCount * 43, 180, 20);

            e.Graphics.DrawString(player + ": " + score.ToString(), font, brush, rec);
        }


        /// <summary>
        /// Healh drawer.
        /// </summary>
        /// <param name="hp"></param>
        /// <param name="shipCount"></param>
        /// <param name="shipColor"></param>
        /// <param name="e"></param>
        private void healthDrawer(int hp, int shipCount, int shipColor, PaintEventArgs e)
        {
            List<SolidBrush> brushes = new List<SolidBrush>() { new SolidBrush(Color.Blue), new SolidBrush(Color.SandyBrown),
                                                                new SolidBrush(Color.Green), new SolidBrush(Color.Gray), new SolidBrush(Color.Red),
                                                                new SolidBrush(Color.Purple), new SolidBrush(Color.LightGray), new SolidBrush(Color.Yellow) };
            
            // Create rectangle
            Rectangle rec1 = new Rectangle(8, 28 + (shipCount * 43), 35*hp, 12);

            // Draw rectangle to screen, fill with player's ship color
            e.Graphics.FillRectangle(brushes[shipColor], rec1);

            // Create pen to use as outline 
            Pen pen = new Pen(Color.Black, 2);
            // Create blank rectangle
            Rectangle rec2 = new Rectangle(5, 25 + (shipCount * 43), 181, 18);

            // Draw rectangle to screen.
            e.Graphics.DrawRectangle(pen, rec2);
        }


        /// <summary>
        /// Calls all 'Drawer' methods.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            lock (world)
            {
                int shipCount = 0;
                int shipColor;
                foreach (Ship s in world.Ships.Values)
                {
                    nameScoreDrawer(s.name, s.score, shipCount, e);
                    shipColor = s.ID % 8;
                    healthDrawer(s.hp, shipCount, shipColor, e);
                    shipCount++;
                }

                // Do anything that Panel (from which we inherit) needs to do
                base.OnPaint(e);
            }
        }
    }
}
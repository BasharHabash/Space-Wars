//                                           Documentation
//----------------------------------------------------------------------------------------------------
//    Namespace:      NetworkController
//    Author:         Bashar Al Habash (7abash), Cole Perschon (coleschon)         Date: Dec. 10, 2017
//----------------------------------------------------------------------------------------------------
///                                               Notes
///---------------------------------------------------------------------------------------------------
///    N/A:           N/A.
///                   
///                   (SEE README.txt)
///---------------------------------------------------------------------------------------------------





using System;
using NetworkController;
using SpaceWars;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Xml;
using MySql.Data.MySqlClient;
using System.Runtime.InteropServices;





/// <summary>
/// "The server maintains the state of the world, computes all game mechanics, and communicates to the
/// clients what is going on."
/// "The server is a standalone program that can run on a separate machine from any client. The server
/// program contains a world, and uses the appropriate world methods to keep it up-to-date on every
/// frame."
/// </summary>
namespace Server
{
    /// <summary>
    /// (See: namespace Server)
    /// </summary>
    class Program
    {
        //                                EDITED WITHIN settings.XML
        //--------------------------------------------------------------------------------------------
        // "The number of units on each side of the square universe"
        private static int UniverseSize = 750;
        // In MS
        private static int TimePerFrame = 16;
        // Frames per shot
        private static int ProjectileFiringDelay = 6;
        // In frames
        private static int RespawnDelay = 300;
        private static double StarX = 0;
        private static double StarY = 0;
        private static double StarMass = 0.005;

        private static int StartingHitPoints = 5;
        // In units per frame
        private static int ProjectileSpeed = 15;
        // In units per frame
        private static double EngineStrength = 0.08;
        // In degrees
        private static double TurningRate = 2.0;
        // A circle radius of Ship
        private static int ShipSize = 20;
        // A circle radius of Star
        private static int StarSize = 35;

        // If triggered, the extra game mode is activated
        // See README.txt for more information on game mode specifics
        private static bool GAMEMODE_RandomDeadlyStar;
        //--------------------------------------------------------------------------------------------
        
        // World controlled by Server
        private static World World;
        // All connected clients
        private static List<SocketState> Clients;
        // Current identification number of next creatable Ship
        private static int ShipID;
        // Current identification number of next creatable Projectile
        private static int ProjectileID;
        
        // Temporary list of destroyed Ships
        private static HashSet<Ship> DeadShips;
        private static List<Ship> ResurrectedShips;
        // Temporary list of destroyed projectiles
        private static List<int> DeadProjectiles;
        // Temporary list of Ships gaining score 
        private static HashSet<int> ScoringShips;
        private static List<Ship> ShipHits;
        
        // Watch utilized to control TimePerFrame
        private static Stopwatch Watch;
        // Program start time
        private static DateTime LastAccessedTime;
        private static bool isclosing = false;
        //--------------------------------------------------------------------------------------------

        /// <summary>
        /// The connection string.
        /// </summary>
        public const string ConnectionString = "server=atr.eng.utah.edu;" +
            "database=cs3500_u0669781;" +
            "uid=cs3500_u0669781;" +
            "password=uMoomoolax22;";


        /// <summary>
        /// Main method responsible for running all server processes.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // Allowing the program to update the database when closed
            Program.SetConsoleCtrlHandler(new HandlerRoutine(ConsoleCtrlCheck), true);
            
            // Program start time
            LastAccessedTime = DateTime.Now;

            // XML Settings are read and adjusted...
            ReadSettings("...//...//...//Resources/settings.xml");

            World = new World();
            Clients = new List<SocketState>();
            ShipID = 0;
            ProjectileID = 0;

            // Initial Star Creation
            Star star = new Star();

            // Game mode alteration
            if (GAMEMODE_RandomDeadlyStar)
            {
                Random rand = new Random();
                star.loc = new Vector2D(rand.Next(-(UniverseSize / 2), UniverseSize / 2),
                    rand.Next(-(UniverseSize / 2), UniverseSize / 2));
            }
            else
            {
                star.loc = new Vector2D(StarX, StarY);
            }

            // Star added to World
            World.Stars.Add(0, star);


            // Temporary lists initialized
            DeadShips = new HashSet<Ship>();
            ResurrectedShips = new List<Ship>();
            DeadProjectiles = new List<int>();
            ScoringShips = new HashSet<int>();
            ShipHits = new List<Ship>();

            Watch = new Stopwatch();

            // Begin event-driven callback for handling new clients
            Networking.ServerAwaitingClientLoop(HandleNewClient);
            System.Console.Out.WriteLine("Server is running, awaiting first client...");

            // Begin forever loop which handles client data and updates and sends World data based on
            // result
            while (true)
            {
                Watch.Start();
                while (Watch.ElapsedMilliseconds < TimePerFrame)
                { }
                Watch.Reset();

                Update();
            }
        }


        /// <summary>
        /// Reads the settings from a given file name, following protocol seen in settings.xml,
        /// to adjust certain Server settings.
        /// </summary>
        /// <param name="filename">Name of file being read.</param>
        private static void ReadSettings(string filename)
        {
            // Make XML reader, to read from document
            using (XmlReader reader = XmlReader.Create(filename))
            {
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        switch (reader.Name)
                        {
                            case "UniverseSize":
                                reader.Read();
                                int.TryParse(reader.Value, out int universeSize);
                                UniverseSize = universeSize;
                                break;
                            case "MSPerFrame":
                                reader.Read();
                                int.TryParse(reader.Value, out int MSPF);
                                TimePerFrame = MSPF;
                                break;
                            case "FramesPerShot":
                                reader.Read();
                                int.TryParse(reader.Value, out int FPS);
                                ProjectileFiringDelay = FPS;
                                break;
                            case "RespawnRate":
                                reader.Read();
                                int.TryParse(reader.Value, out int respawn);
                                RespawnDelay = respawn;
                                break;
                            case "Star":
                                reader.Read();
                                reader.Read();
                                reader.Read();
                                double.TryParse(reader.Value, out double X);
                                StarX = X;
                                reader.Read();
                                reader.Read();
                                reader.Read();
                                reader.Read();
                                double.TryParse(reader.Value, out double Y);
                                StarY = Y;
                                reader.Read();
                                reader.Read();
                                reader.Read();
                                reader.Read();
                                double.TryParse(reader.Value, out double mass);
                                StarMass = mass;
                                break;
                            case "StartingHitPoints":
                                reader.Read();
                                int.TryParse(reader.Value, out int hp);
                                StartingHitPoints = hp;
                                break;
                            case "ProjectileSpeed":
                                reader.Read();
                                int.TryParse(reader.Value, out int speed);
                                ProjectileSpeed = speed;
                                break;
                            case "EngineStrength":
                                reader.Read();
                                double.TryParse(reader.Value, out double strength);
                                EngineStrength = strength;
                                break;
                            case "TurningRate":
                                reader.Read();
                                double.TryParse(reader.Value, out double rate);
                                TurningRate = rate;
                                break;
                            case "ShipSize":
                                reader.Read();
                                int.TryParse(reader.Value, out int shipSize);
                                ShipSize = shipSize;
                                break;
                            case "StarSize":
                                reader.Read();
                                int.TryParse(reader.Value, out int starSize);
                                StarSize = starSize;
                                break;
                            case "GAMEMODE_RandomDeadlyStar":
                                reader.Read();
                                bool.TryParse(reader.Value, out bool extra);
                                GAMEMODE_RandomDeadlyStar = extra;
                                break;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// "This is the delegate callback passed to the networking class to handle a new client
        /// connecting."
        /// </summary>
        /// <param name="state">The client's socket information</param>
        private static void HandleNewClient(SocketState state)
        {
            // "Change the callback for the socket state to a new method that receives the player's"
            // "name"
            state.callMe = ReceiveName;
            // "Ask for data"
            Networking.GetData(state);
        }


        /// <summary>
        /// "This is a ("callMe") delegate that implements the server's part of the initial
        /// handshake."
        /// </summary>
        /// <param name="state">The client's socket information</param>
        private static void ReceiveName(SocketState state)
        {
            lock (World)
            {
                // Create a new Ship with the given name and a new unique ID
                Ship newShip = new Ship();
                Random rand = new Random();

                newShip.ID = ShipID;
                newShip.loc = new Vector2D(rand.Next(-(UniverseSize / 2), UniverseSize / 2), 
                    rand.Next(-(UniverseSize / 2), UniverseSize / 2));
                newShip.dir = new Vector2D(rand.Next(-180, 180), rand.Next(-180, 180));
                newShip.thrust = false;
                newShip.name = state.sb.ToString().Trim();
                newShip.hp = StartingHitPoints;
                newShip.score = 0;
                newShip.alive = true;

                newShip.velocity = new Vector2D(0, 0);
                newShip.dir.Normalize();

                newShip.projFired = 0;
                newShip.projHit = 0;

                // Add the new ship to World
                World.Ships.Add(newShip.ID, newShip);

                // Alter ShipID for next incoming Ship
                ShipID++;

                // Setting the state's ID to match the newly added Ship
                state.ID = newShip.ID;
                // Prepare the startup info to the client(ID and world size)
                state.sb.Clear();
                state.sb.Append(newShip.ID + "\n");
                state.sb.Append(UniverseSize + "\n");
                state.callMe = HandleData;

                // Send the starup info to the client
                Networking.Send(state.theSocket, state.sb.ToString());

                // Add the client's socket to the list of all clients
                Clients.Add(state);
                Console.WriteLine("A new client has connected to the server...");
                
                // "Then ask the client for data"
                Networking.GetData(state);
            }
        }


        /// <summary>
        /// "This is a callMe delegate for processing client direction commands." 
        /// </summary>
        /// <param name="state">The client's socket information</param>
        private static void HandleData(SocketState state)
        {
            // Fetch the client controlled Ship
            Ship s = World.Ships.Values.ElementAt(state.ID);
            
            // "Process this Ship's commands"
            string commands = state.sb.ToString();
            state.sb.Clear();

            if (commands.Contains('L'))
                s.left = true;
            if (commands.Contains('T'))
                s.up = true;
            if (commands.Contains('R'))
                s.right = true;
            if (commands.Contains('F'))
                s.space = true;
            
            // "Ask for more data"
            Networking.GetData(state);
        }


        /// <summary>
        /// Calculates the left turning Ship's new direction.
        /// </summary>
        /// <param name="s">Ship being rotated left</param>
        /// <returns>Rotated ship direction vector</returns>
        private static Vector2D RotateLeft(Vector2D shipDir)
        {
            Vector2D v = shipDir;
            v.Rotate(-TurningRate);
            return v;
        }


        /// <summary>
        /// Calculates the thrusting Ship's new direction.
        /// </summary>
        /// <param name="shipDir">Ship being thrusted</param>
        /// <returns>Thrusted ship direction vector</returns>
        private static Vector2D Thrust(Vector2D shipDir)
        {
            Vector2D v = shipDir;
            v *= EngineStrength;
            return v;
        }


        /// <summary>
        /// Calculates the right turning Ship's new direction.
        /// </summary>
        /// <param name="s">Ship being rotated right</param>
        /// <returns>Rotated ship direction vector</returns>
        private static Vector2D RotateRight(Vector2D shipDir)
        {
            Vector2D v = shipDir;
            v.Rotate(TurningRate);
            return v;
        }

        
        /// <summary>
        /// Calculates the accleration of a (Ship) with the following forces acted upon it.
        /// </summary>
        /// <param name="Gravity">Force of gravity from Star(s)</param>
        /// <param name="Thrust">Force of Ship thrust</param>
        /// <returns>Acceleration vector</returns>
        private static Vector2D Acceleration(Vector2D Gravity, Vector2D Thrust)
        {
            Vector2D a = Thrust + Gravity;
            return a;
        }


        /// <summary>
        /// Calculates the gravity of a (Ship) with the following forces acted upon int.
        /// </summary>
        /// <param name="shipLoc">Ship location</param>
        /// <returns>Gravitational force vector</returns>
        private static Vector2D Gravity (Vector2D shipLoc)
        {
            Vector2D g = new Vector2D(0, 0);

            foreach (Star s in World.Stars.Values)
            {
                g += (s.loc - shipLoc);
            }
            g.Normalize();
            g *= StarMass;

            return g;
        }


        /// <summary>
        /// Handles out-of-bound wrapping effect on Ships.
        /// </summary>
        /// <param name="shipLoc">Ship being wrapped around</param>
        /// <returns>Ship's newly wrapped location</returns>
        private static Vector2D WrapAround(Vector2D shipLoc)
        {
            double newX = shipLoc.GetX();
            double newY = shipLoc.GetY();

            newX = newX + (2 * (-newX));
            newY = newY + (2 * (-newY));

            return new Vector2D(newX, newY);
        }


        /// <summary>
        /// Respawns a ship into the World.
        /// </summary>
        /// <param name="ship">Destoryed Ship that is getting respawned</param>
        private static void ResetShip(Ship ship)
        {
            Random rand = new Random();
            Ship newShip = new Ship();

            newShip.ID = ship.ID;
            newShip.loc = new Vector2D(rand.Next(-(UniverseSize / 2), UniverseSize / 2),
                rand.Next(-(UniverseSize / 2), UniverseSize / 2));
            newShip.dir = new Vector2D(rand.Next(-180, 180), rand.Next(-180, 180));
            newShip.thrust = false;
            newShip.name = ship.name;
            newShip.hp = StartingHitPoints;
            newShip.score = ship.score;
            newShip.alive = true;
            
            newShip.dir.Normalize();

            newShip.velocity = new Vector2D(0, 0);

            World.Ships[ship.ID] = newShip;
            //World.Ships.Add(newShip.ID, newShip);
        }



        /// <summary>
        /// Invoked every iteration through the frame loop. 
        /// Update World, then sends it to each client.
        /// </summary>
        private static void Update()
        {
            // Updated world 
            StringBuilder sb = new StringBuilder();
            
            lock (World)
            {
                // For all Stars in World
                foreach (Star star in World.Stars.Values)
                {
                    // Append each Star
                    sb.Append(JsonConvert.SerializeObject(star) + "\n");

                    // Destroy all Ships colliding with Star
                    foreach (Ship ship in World.Ships.Values)
                    {
                        Vector2D distance = star.loc - ship.loc;
                        if (distance.Length() < StarSize)
                        {
                            ship.hp = 0;
                            if (GAMEMODE_RandomDeadlyStar)
                                ship.score--;
                            ship.alive = false;
                            DeadShips.Add(ship);
                        }
                    }

                    // Destroy all Projectiles colliding with Star
                    foreach (Projectile p in World.Projectiles.Values)
                    {
                        Vector2D distance = star.loc - p.loc;
                        if (distance.Length() < StarSize)
                        {
                            p.alive = false;
                            DeadProjectiles.Add(p.ID);
                        }
                    }
                }
                
                // For all Ships in World
                foreach (Ship ship in World.Ships.Values)
                {
                    // Thrust Ship
                    if (ship.up)
                    {
                        ship.thrust = true;
                        ship.velocity += Acceleration(Thrust(ship.dir), Gravity(ship.loc));
                        ship.loc += ship.velocity;
                        ship.up = false;
                    }
                    // Non-thrusting Ship
                    else
                    {
                        ship.velocity += Gravity(ship.loc);
                        ship.loc += ship.velocity;
                        ship.thrust = false;
                    }
                    // Rotate Left
                    if (ship.left)
                    {
                        ship.dir = RotateLeft(ship.dir);
                        ship.left = false;
                    }
                    // Rotate right
                    if (ship.right)
                    {
                        ship.dir = RotateRight(ship.dir);
                        ship.right = false;
                    }
                    // Fire projectile
                    if (ship.space && ship.alive)
                    {
                        // Begin spacing projectiles
                        ship.projectileTimer++;
                        if (ship.projectileTimer >= ProjectileFiringDelay)
                        {
                            Projectile p = new Projectile();
                            p.ID = ProjectileID;
                            p.loc = ship.loc;
                            p.dir = ship.dir;
                            p.alive = true;
                            p.owner = ship.ID;

                            p.velocity = (p.dir * ProjectileSpeed) + ship.velocity;

                            World.Projectiles.Add(p.ID, p);

                            ship.projFired++;
                            ship.projectileTimer = 0;
                            ProjectileID++;
                        }
                        ship.space = false;
                    }
                    
                    // If Ship exits World's bounds
                    if (ship.loc.GetX() >= UniverseSize / 2 || ship.loc.GetX() <= -(UniverseSize / 2) 
                        || ship.loc.GetY() >= UniverseSize / 2 || ship.loc.GetY() <= -(UniverseSize / 2))
                    {
                        // Wrap around Ship
                        ship.loc = WrapAround(ship.loc);
                    }

                    // Destroy all Projectiles colliding with Ship and vise-versa
                    foreach (Projectile p in World.Projectiles.Values)
                    {
                        Vector2D distance = ship.loc - p.loc;

                        if (distance.Length() < ShipSize && ship.ID != p.owner && ship.alive)
                        {
                            p.alive = false;
                            ship.hp--;

                            ShipHits.Add(World.Ships[p.owner]);
                            if (ship.hp <= 0)
                            {
                                ship.alive = false;
                                DeadShips.Add(ship);
                                ScoringShips.Add(p.owner);
                            }
                            DeadProjectiles.Add(p.ID);
                        }
                    }

                    sb.Append(JsonConvert.SerializeObject(ship) + "\n");
                }

                // For all Projectiles in World
                foreach (Projectile p in World.Projectiles.Values)
                {
                    // Update projectile location with constant velocity
                    p.loc += p.velocity;

                    // If Projectile exits World's bounds
                    if (p.loc.GetX() >= UniverseSize / 2 || p.loc.GetX() <= -(UniverseSize / 2) 
                        || p.loc.GetY() >= UniverseSize / 2 || p.loc.GetY() <= -(UniverseSize / 2))
                    {
                        // Destroy Projectile
                        p.alive = false;
                        DeadProjectiles.Add(p.ID);
                    }

                    sb.Append(JsonConvert.SerializeObject(p) + "\n");
                }
                
                // All Projectiles-to-destroy are removed from World
                foreach (int i in DeadProjectiles)
                {
                    World.Projectiles.Remove(i);
                }
                DeadProjectiles.Clear();

                // All Ships-to-score have score increased
                foreach (int i in ScoringShips)
                {
                    Ship s = World.Ships.Values.ElementAt(i);
                    s.score++;
                }
                ScoringShips.Clear();
                
                // All Ships-to-destroy are removed from World and respawned
                foreach (Ship ship in DeadShips)
                {
                    if (ship.hp <= 0)
                    {
                        // Incremental value has a further effect on respawn rate
                        ship.respawnTimer += 20;
                    }
                    if (ship.respawnTimer >= RespawnDelay * TimePerFrame)
                    {
                        ship.respawnTimer = 0;
                        ResetShip(ship);
                        ResurrectedShips.Add(ship);
                    }
                }

                // Respawn dead Ship
                foreach(Ship ship in ResurrectedShips)
                {
                    DeadShips.Remove(ship);
                }
                ResurrectedShips.Clear();

                // Add hit projectiles to Ship
                foreach(Ship ship in ShipHits)
                {
                    World.Ships[ship.ID].projHit++;
                }
                ShipHits.Clear();

                // Each client is updated with appended World information
                foreach (SocketState state in Clients)
                {
                    Networking.Send(state.theSocket, sb.ToString());
                }
            }
        }


        /// <summary>
        /// When Program is closed, updates the MySQL database on information from game.
        /// </summary>
        private static void Exit()
        {
            // Establish a connection
            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
            {
                try
                {
                    // Open a connection
                    connection.Open();

                    // Game duration
                    TimeSpan duration = DateTime.Now - LastAccessedTime;

                    // Create a command
                    MySqlCommand command;
                    // For all ships, append their according informatoin
                    foreach (Ship s in World.Ships.Values)
                    {
                        String accuracyPercentage = "0%";

                        // All ships who fired 
                        if (s.projFired != 0)
                        {
                            string hits = s.projHit.ToString();
                            String shots = s.projFired.ToString();

                            // Must add arbitrary number for String conversions
                            hits += ".000000001";
                            double hitsD = Double.Parse(hits);
                            
                            shots += ".000000001";
                            double shotsD = Double.Parse(shots);
                            // Accurary double, trailind "D" denotes double variant
                            double accuracyD = hitsD / shotsD;
                            accuracyPercentage = accuracyD.ToString("0.######");

                            // Between 0% and 99%
                            if (accuracyPercentage.Contains(".") && accuracyPercentage != "0")
                                accuracyPercentage = accuracyPercentage.Substring(2,2);
                            // 100% Accuracy
                            else if (accuracyPercentage != "0")
                                accuracyPercentage += "00";
                            accuracyPercentage += "%";
                        }

                        // Database Updates based on player information
                        command = connection.CreateCommand();
                        command.CommandText = "insert into Players(ID,Name) values(" + (s.ID + duration.GetHashCode()) + ",'" + s.name + "')";
                        command.ExecuteNonQuery();
                        command.CommandText = "insert into Scores(Name,Score) values('" + s.name + "'," + s.score + ")";
                        command.ExecuteNonQuery();
                        command.CommandText = "insert into Accuracies(Name,Accuracy) values ('" + s.name + "','" + accuracyPercentage + "')";
                        command.ExecuteNonQuery();
                    }
                    // Database Update based on game time
                    command = connection.CreateCommand();
                    command.CommandText = "insert into Games(Duration) values ('" + duration  + "')";
                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }



        //--------------------------------FOLLOWING CODE UTILIZED FROM----------------------------------------
        // https://social.msdn.microsoft.com/Forums/vstudio/en-US/707e9ae1-a53f-4918-8ac4-62a1eddb3c4a/detecting-console-application-exit-in-c?forum=csharpgeneral
        //----------------------------------------------------------------------------------------------------
        [DllImport("Kernel32")]
        public static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);

        /// <summary>
        /// Listens to commands execuded on this program.
        /// </summary>
        /// <param name="ctrlType"></param>
        /// <returns></returns>
        private static bool ConsoleCtrlCheck(CtrlTypes ctrlType)
        {
            // Put your own handler here
            switch (ctrlType)
            {
                case CtrlTypes.CTRL_C_EVENT:
                    Exit();
                    break;

                case CtrlTypes.CTRL_BREAK_EVENT:
                    Exit();
                    break;

                case CtrlTypes.CTRL_CLOSE_EVENT:
                    Exit();
                    break;

                case CtrlTypes.CTRL_LOGOFF_EVENT:
                case CtrlTypes.CTRL_SHUTDOWN_EVENT:
                    Exit();
                    break;
            }
            return true;
        }

        
        /// <summary>
        /// Handles different control types outlined wihtin a method.
        /// </summary>
        /// <param name="CtrlType"></param>
        /// <returns></returns>
        public delegate bool HandlerRoutine(CtrlTypes CtrlType);


        /// <summary>
        /// An enumerated type for the control messages sent to HandlerRoutine.
        /// </summary>
        public enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }
        //----------------------------------------------------------------------------------------------------
    }
}
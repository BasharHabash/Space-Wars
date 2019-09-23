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





using System.Collections.Generic;





/// <summary>
/// Contains all client-side code for the multiplayer game - SpaceWars.
/// </summary>
namespace SpaceWars
{
    /// <summary>
    /// A wrapper class of sorts that contains all Ship, Star, and Projectile data existent in the 
    /// current game.
    /// </summary>
    public class World
    {
        // These fields are allowed to be public since all data must me receivable and modifiable by
        // its' implementing classes.
        public Dictionary<int, Ship> Ships { get; set; }
        public Dictionary<int, Star> Stars { get; set; }
        public Dictionary<int, Projectile> Projectiles { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public World()
        {
            Ships = new Dictionary<int, Ship>();
            Stars = new Dictionary<int, Star>();
            Projectiles = new Dictionary<int, Projectile>();
        }
    }
}
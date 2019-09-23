//                                           Documentation
//----------------------------------------------------------------------------------------------------
//    Namespace:      SpaceWars
//    Author:         Bashar Al Habash (7abash), Cole Perschon (coleschon)          Date: Dec. 8, 2017
//----------------------------------------------------------------------------------------------------
///                                               Notes
///---------------------------------------------------------------------------------------------------
///    N/A:           N/A.
///                   
///                   (SEE README.txt)
///---------------------------------------------------------------------------------------------------





using Newtonsoft.Json;





/// <summary>
/// Contains all client-side code for the multiplayer game - SpaceWars.
/// </summary>
namespace SpaceWars
{
    /// <summary>
    /// The player controlled object.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Ship
    {
        [JsonProperty(PropertyName = "ship")]
        public int ID { get; set; }
        [JsonProperty]
        public Vector2D loc { get; set; }
        [JsonProperty]
        public Vector2D dir { get; set; }
        [JsonProperty]
        public bool thrust { get; set; }
        [JsonProperty]
        public string name { get; set; }
        [JsonProperty]
        public int hp { get; set; }
        [JsonProperty]
        public int score { get; set; }
        [JsonProperty]
        public bool alive { get; set; }

        /// <summary>
        /// Server variables
        /// </summary>
        public Vector2D velocity { get; set; }
        public int projectileTimer { get; set; }
        public int respawnTimer { get; set; }

        public bool left { get; set; }
        public bool up { get; set; }
        public bool right { get; set; }
        public bool space { get; set;  }

        public int projFired { get; set; }
        public int projHit { get; set; }


        /// <summary>
        /// Constructor.
        /// </summary>
        [JsonConstructor]
        public Ship()
        {
        }
    }
}
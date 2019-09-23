//                                           Documentation
//----------------------------------------------------------------------------------------------------
//    Namespace:      SpaceWars
//    Author:         Bashar Al Habash (7abash), Cole Perschon (coleschon)         Date: Nov. 17, 2017
//    Description:    N/A.
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
    /// The "bullet" fired from Ship.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Projectile
    {
        [JsonProperty(PropertyName = "proj")]
        public int ID { get; set; }
        [JsonProperty]
        public Vector2D loc { get; set; }
        [JsonProperty]
        public Vector2D dir { get; set; }
        [JsonProperty]
        public bool alive { get; set; }
        [JsonProperty]
        public int owner { get; set; }

        public Vector2D velocity { get; set; }


        /// <summary>
        /// Constructor.
        /// </summary>
        [JsonConstructor]
        public Projectile()
        {
        }
    }
}
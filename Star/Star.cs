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





using Newtonsoft.Json;





/// <summary>
/// Contains all client-side code for the multiplayer game - SpaceWars.
/// </summary>
namespace SpaceWars
{
    /// <summary>
    /// The object zone in which Ship is pulled towards and destroyed by.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Star
    {
        [JsonProperty(PropertyName = "star")]
        public int ID { get; private set; }
        [JsonProperty]
        public Vector2D loc { get;  set; }
        [JsonProperty]
        public double mass { get; private set; }


        /// <summary>
        /// Constructor.
        /// </summary>
        [JsonConstructor]
        public Star()
        {
        }
    }
}
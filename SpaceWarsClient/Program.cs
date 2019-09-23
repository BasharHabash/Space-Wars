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
using System.Windows.Forms;





/// <summary>
/// Contains all client-side code for the multiplayer game - SpaceWars.
/// </summary>
namespace SpaceWars
{
    /// <summary>
    /// Runs Form1.
    /// </summary>
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
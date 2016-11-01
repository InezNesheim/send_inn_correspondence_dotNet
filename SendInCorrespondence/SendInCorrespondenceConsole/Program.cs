using System;
using SendInCorrespondence;

namespace SendInCorrespondenceConsole
{
    /// <summary>
    /// Console application for testing the SendInCorrespondence service
    /// </summary>
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Starting the create and send correspondence service...");

            SendInCorrespondenceDal.CreateCorrespondence();

            Console.WriteLine("Sending correspondence completed successfully!");
            Console.ReadLine();
        }        
    }
}

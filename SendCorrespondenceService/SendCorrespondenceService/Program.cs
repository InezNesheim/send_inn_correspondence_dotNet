using System.Threading;

namespace SendCorrespondenceService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
#if DEBUG
            // Run as interactive exe in debug mode to allow easy
            // debugging.

            var service = new SendInCorrespondenceService();
            service.OnStart(null);

            // Sleep the main thread indefinitely while the service code
            // runs in .OnStart

            Thread.Sleep(Timeout.Infinite);
#else
                // Run normally as service in release mode.

                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]{ new SendInCorrespondenceService() };
                ServiceBase.Run(ServicesToRun);
#endif          
        }
    }
}

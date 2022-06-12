using System;
using System.Threading.Tasks;
using System.IO;

namespace ExpenseWatcher
{
    /// <summary>
    /// class reperesenting a logger 
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Logs a message to a logfile
        /// </summary>
        /// <param name="message">The message to log</param>
        public static void Log(string message)
        {
            Task.Run(() =>
            {
                if (!Directory.Exists("logs"))
                {
                    Directory.CreateDirectory("logs");
                }

                string logMessage = $"[{DateTime.Now.ToString("HH:mm:ss.fff")}] : {message} \r\n";
                File.AppendAllText($"logs\\Log_{DateTime.Today.ToString("yyyy-MM-dd")}.csv", logMessage);
            });
        }
    }
}

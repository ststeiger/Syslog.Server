
namespace Syslog.Server.Data
{
    

    /// <summary>
    /// Log Class
    /// </summary>
    public class Log
    {
        /// <summary>
        /// Lock object to log file access
        /// </summary>
        private static readonly object Locker = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="Log"/> class.
        /// </summary>
        public Log()
        {
        }

        /// <summary>
        /// Writes to log.
        /// </summary>
        /// <param name="message">The message to write.</param>
        /// <param name="path">The path of the file.</param>
        public void WriteToLog(string message, string path)
        {
            lock (Locker)
            {
                using (System.IO.FileStream fileStream = new System.IO.FileStream(path: path, mode: System.IO.FileMode.Append))
                {
                    byte[] encodedText = System.Text.Encoding.Unicode.GetBytes(message);
                    fileStream.Write(encodedText, 0, encodedText.Length);
                }
            }
        }
    }
}
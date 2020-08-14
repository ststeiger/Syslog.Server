
namespace Syslog.Server.Data
{
    
    
    /// <summary>
    /// Message Data
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Gets or sets the Time on which the Syslog Message was receive
        /// </summary>
        public System.DateTime RecvTime { get; set; }

        /// <summary>
        /// Gets or sets the Message Text of the Syslog Package
        /// </summary>
        public string MessageText { get; set; }

        /// <summary>
        /// Gets or sets the source IP of the Syslog Sender
        /// </summary>
        public System.Net.IPAddress SourceIP { get; set; }
    }
}
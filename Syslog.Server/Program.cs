
namespace Syslog.Server
{
    
    
    using Syslog.Server.Data;
    
    
    /// <summary>
    /// Program class
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class Program 
        : System.IDisposable
    {
        /// <summary>
        /// As long this is true the Service will continue to receive new Messages.
        /// </summary>
        private static readonly bool queueing = true;

        /// <summary>
        /// Message Queue of the type Data.Message.
        /// </summary>
        private static System.Collections.Generic.Queue<Message> messageQueue = new System.Collections.Generic.Queue<Message>();

        /// <summary>
        /// Message Trigger
        /// </summary>
        private static System.Threading.AutoResetEvent messageTrigger = new System.Threading.AutoResetEvent(false);

        /// <summary>
        /// Listener Address
        /// </summary>
        private static System.Net.IPEndPoint anyIP = new System.Net.IPEndPoint(System.Net.IPAddress.Any, 514);
        
        /// <summary>
        /// Listener Port and Protocol
        /// </summary>
        private static System.Net.Sockets.UdpClient udpListener = new System.Net.Sockets.UdpClient(514);

        /// <summary>
        /// The log file
        /// </summary>
        private static string logFile;

        /// <summary>
        /// The disposed value
        /// </summary>
        private bool disposedValue = false;

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args)
        {
            if (args[0] != null)
            {
                logFile = args[0];
            }
            else
            {
                System.Console.WriteLine("Missing Argument (logfile)");
            }

            // Main processing Thread
            System.Threading.Thread handler = new System.Threading.Thread(new System.Threading.ThreadStart(HandleMessage))
            {
                IsBackground = true
            };
            handler.Start();
          
            /* Main Loop */
            /* Listen for incoming data on udp port 514 (default for SysLog events) */
            while (queueing || messageQueue.Count != 0)
            {
                try
                {
                    anyIP.Port = 514;

                    // https://www.real-world-systems.com/docs/logger.1.html
                    // sudo apt-get install bsdutils
                    // logger -p auth.notice "Some message for the auth.log file"
                    // logger -p auth.notice "Some message for the auth.log file" --server 127.0.0.1 

                    // Receive the message
                    byte[] bytesReceive = udpListener.Receive(ref anyIP);

                    // push the message to the queue, and trigger the queue
                    Data.Message msg = new Data.Message
                    {
                        MessageText = System.Text.Encoding.ASCII.GetString(bytesReceive),
                        RecvTime = System.DateTime.Now,
                        SourceIP = anyIP.Address
                    };
                    
                    lock (messageQueue)
                    {
                        messageQueue.Enqueue(msg);
                    }

                    messageTrigger.Set();
                }
                catch (System.Exception ex)
                {
                    // ToDo: Add Error Handling
                    System.Console.WriteLine(ex.Message);
                }
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    udpListener.Dispose();
                }

                this.disposedValue = true;
            }
        }

        /// <summary>
        /// Internal Message handler
        /// </summary>
        private static void HandleMessage()
        {
            while (queueing)
            {
                messageTrigger.WaitOne(5000);    // A 5000ms timeout to force processing
                Message[] messageArray = null;

                lock (messageQueue)
                {
                    messageArray = messageQueue.ToArray();
                }

                System.Threading.Thread messageprochandler = new System.Threading.Thread(() => HandleMessageProcessing(messageArray))
                {
                    IsBackground = true
                };
                messageprochandler.Start();
            }
        }

        /// <summary>
        /// Message Processing handler, call in a new thread
        /// </summary>
        /// <param name="messages">Array of type <see cref="Data.Message"/></param>
        private static void HandleMessageProcessing(Data.Message[] messages)
        {
            foreach (Data.Message message in messages)
            {
                LogToFile(message.MessageText, message.SourceIP, message.RecvTime);
                System.Console.WriteLine(message.MessageText);

                if (Program.messageQueue.Count != 0)
                {
                    Program.messageQueue.Dequeue();
                }
            }
        }

        /// <summary>
        /// handles the log Update, call in a new thread to reduce performance impacts on the service handling.
        /// </summary>
        /// <param name="msg">Message which was sent from the Syslog Client</param>
        /// <param name="ipSourceAddress">Source IP of the Syslog Sender</param>
        /// <param name="receiveTime">Receive Time of the Syslog Message</param>
        private static void LogToFile(string msg, System.Net.IPAddress ipSourceAddress, System.DateTime receiveTime)
        {
            Log log = new Log();
            log.WriteToLog($"{msg}; {ipSourceAddress}; {receiveTime}\n", logFile);
        }
    }
}

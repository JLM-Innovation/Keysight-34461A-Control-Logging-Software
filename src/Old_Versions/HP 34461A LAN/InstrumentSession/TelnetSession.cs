using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HP_34461A_LAN.InstrumentSession
{
    public class TelnetSession : IInstrumentSession
    {
        private bool disposedValue;

        private TcpClient TcpClient;
        private NetworkStream NetworkStream;
        private StreamReader StreamReader;
        private StreamWriter StreamWriter;

        public bool IsSessionOpen { get; private set; }
        public string InstrumentAddress { get; private set; }
        public int Port { get; private set; }
        public int ReadBufferSize { get; private set; }
        public int WriteBufferSize { get; private set; }
        public int TimeoutMiliseconds { get; private set; }

        public TelnetSession(string instrumentAddress, int port = 5024, int readBufferSize = 8192, int writeBufferSize = 8192, int timeoutMiliseconds = 15000)
        {
            IsSessionOpen = false;
            Port = port;
            InstrumentAddress = instrumentAddress;
            ReadBufferSize = readBufferSize;
            WriteBufferSize = writeBufferSize;
            TimeoutMiliseconds = timeoutMiliseconds;
        }

        public void OpenSession()
        {
            TcpClient = new TcpClient(InstrumentAddress, Port)
            {
                ReceiveBufferSize = ReadBufferSize,
                SendBufferSize = WriteBufferSize,
                ReceiveTimeout = TimeoutMiliseconds,
                SendTimeout = TimeoutMiliseconds,
            };
            NetworkStream = TcpClient.GetStream();
            StreamReader = new StreamReader(NetworkStream, Encoding.ASCII);
            StreamWriter = new StreamWriter(NetworkStream, Encoding.ASCII);
            string response = ReadLine();
            if (string.IsNullOrEmpty(response))
            {
                Dispose();
                throw new IOException("Device does not respond");
            }
        }

        public string ReadLine()
        {
            return StreamReader.ReadLine();
        }

        public void WriteLine(string message)
        {
            StreamWriter.WriteLine(message);
            StreamWriter.Flush();
            StreamReader.ReadLine();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    StreamReader?.Dispose();
                    StreamWriter?.Dispose();
                    NetworkStream?.Dispose();
                    TcpClient?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~TelnetSession()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

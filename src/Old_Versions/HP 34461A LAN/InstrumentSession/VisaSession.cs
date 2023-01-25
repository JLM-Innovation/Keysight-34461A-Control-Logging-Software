using Ivi.Visa;
using Ivi.Visa.FormattedIO;
using System;

namespace HP_34461A_LAN.InstrumentSession
{
    public class VisaSession : IInstrumentSession
    {
        private bool disposedValue;

        private IMessageBasedSession session;
        private MessageBasedFormattedIO formattedIO;

        public bool IsSessionOpen { get; private set; }
        public string InstrumentAddress { get; private set; }
        public int GPIB_Lock { get; private set; } = 0;
        public int ReadBufferSize { get; private set; }
        public int WriteBufferSize { get; private set; }
        public int TimeoutMiliseconds { get; private set; }

        public VisaSession(string instrumentAddress, int gpib_lock = 0, int readBufferSize = 8192, int writeBufferSize = 8192, int timeoutMiliseconds = 15000)
        {
            IsSessionOpen = false;
            InstrumentAddress = instrumentAddress;
            GPIB_Lock = gpib_lock;
            ReadBufferSize = readBufferSize;
            WriteBufferSize = writeBufferSize;
            TimeoutMiliseconds = timeoutMiliseconds;
        }

        public void OpenSession()
        {
            session = GlobalResourceManager.Open(InstrumentAddress, (AccessModes)GPIB_Lock, TimeoutMiliseconds) as IMessageBasedSession;
            session.TimeoutMilliseconds = TimeoutMiliseconds;
            session.TerminationCharacterEnabled = true;
            formattedIO = new MessageBasedFormattedIO(session);
            formattedIO.ReadBufferSize = ReadBufferSize;
            formattedIO.WriteBufferSize = WriteBufferSize;
            IsSessionOpen = true;
        }


        public string ReadLine()
        {
            return formattedIO.ReadLine();
        }

        public void WriteLine(string message)
        {
            formattedIO.WriteLine(message);
        }

        private void CloseSession()
        {
            session.Clear();
            if (GPIB_Lock == 1)
            {
                session.UnlockResource();
            }
            session.Dispose();
            session = null;
            formattedIO = null;
            IsSessionOpen = false;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    CloseSession();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~VisaSession()
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

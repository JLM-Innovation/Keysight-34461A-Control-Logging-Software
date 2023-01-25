using System;

namespace HP_34461A_LAN.InstrumentSession
{
    public interface IInstrumentSession : IDisposable
    {
        bool IsSessionOpen { get; }
        void OpenSession();
        void WriteLine(string message);
        string ReadLine();
    }
}

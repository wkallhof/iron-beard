using System;

namespace IronBeard.Core.Features.Generator
{
    public class OnProgressEventArgs : EventArgs
    {
        public int Percent { get; set; }
        public string Message { get; set; }
    }

    public class OnErrorEventArgs : EventArgs
    {
        public string Error { get; set; }
    }

    public class OnInfoEventArgs : EventArgs
    {
        public string Message { get; set; }
    }
}
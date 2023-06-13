using System;
using System.Net;

namespace BMOnline.Common
{
    public struct TimedUdpReceive
    {
        public TimedUdpReceive(byte[] buffer, IPEndPoint remoteEndPoint, TimeSpan timeReceived)
        {
            Buffer = buffer;
            RemoteEndPoint = remoteEndPoint;
            TimeReceived = timeReceived;
        }

        public byte[] Buffer { get; }
        public IPEndPoint RemoteEndPoint { get; }
        public TimeSpan TimeReceived { get; }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace BMOnline.Common
{
    public abstract class BidirectionalUdp
    {
#if DEBUG
        public const byte PROTOCOL_VERSION = 0; //0 is used for test versions
#else
        public const byte PROTOCOL_VERSION = 4;
#endif
        private readonly UdpClient client;

        public static readonly TimeSpan TICK_LENGTH = TimeSpan.FromMilliseconds(100);
        public uint CurrentTick { get; private set; } = 0;
        private readonly Stopwatch stopwatch = new Stopwatch();
        public TimeSpan Time { get => stopwatch.Elapsed; }

        private readonly SemaphoreSlim receiveSemaphore = new SemaphoreSlim(1);
        private readonly Queue<TimedUdpReceive> receivedQueue = new Queue<TimedUdpReceive>();

        protected BidirectionalUdp(IPEndPoint localEP)
        {
            client = new UdpClient(localEP);
        }

        public async Task RunBusy()
        {
            stopwatch.Start();

            //Start up receiving thread
            Task receiveThread = BusyReceive();

            TimeSpan startTime = Time;

            while (!receiveThread.IsCompleted)
            {
                CurrentTick++;
                TimeSpan targetTime = startTime.Add(TICK_LENGTH.Multiply(CurrentTick));
                await SendTick();

                //Receive for a bit
                await ReceiveUntilEmpty();

                //Skip some ticks if we're late enough
                if (Time > targetTime)
                {
                    TimeSpan lateBy = Time - targetTime;
                    Log.Warning($"Missed tick deadline by {lateBy.TotalMilliseconds:N3}ms");
                    uint ticksToSkip = (uint)Math.Round(lateBy.Divide(TICK_LENGTH));
                    if (ticksToSkip > 0)
                    {
                        Log.Warning($"Running {lateBy.TotalMilliseconds:N3}ms behind, skipping {ticksToSkip} ticks");
                        CurrentTick += ticksToSkip;
                    }
                }
                //Wait for next tick
                if (Time < targetTime)
                {
                    await Task.Delay(targetTime - Time);
                }
            }
            throw new AggregateException("The receive thread stopped", receiveThread.Exception);
        }

        protected abstract Task SendTick();
        protected abstract Task HandleReceive(TimedUdpReceive result);

        protected async Task SendAsync(byte[] data)
        {
            try
            {
                await client.SendAsync(data, data.Length);
            }
            catch (SocketException) { } //Silently ignore SocketExceptions
        }
        protected async Task SendAsync(byte[] data, IPEndPoint endPoint)
        {
            try
            {
                await client.SendAsync(data, data.Length, endPoint);
            }
            catch (SocketException) { } //Silently ignore SocketExceptions
        }

        private async Task BusyReceive()
        {
            while (true)
            {
                try
                {
                    UdpReceiveResult result = await client.ReceiveAsync();
                    TimedUdpReceive timedResult = new TimedUdpReceive(result.Buffer, result.RemoteEndPoint, Time);
                    await receiveSemaphore.WaitAsync();
                    receivedQueue.Enqueue(timedResult);
                    receiveSemaphore.Release();
                }
                catch (SocketException) { }
            }
        }

        private async Task ReceiveUntilEmpty()
        {
            bool canReceive = receiveSemaphore.Wait(0); //Shouldn't block because timeout is 0
            if (!canReceive)
            {
                Log.Info("Skipping receive, queue is busy");
                return;
            }
            while (receivedQueue.Count > 0)
            {
                try
                {
                    TimedUdpReceive result = receivedQueue.Dequeue();
                    await HandleReceive(result);
                }
                catch (Exception e)
                {
                    Log.Warning($"Exception while receiving message: {e}");
                }
            }
            receiveSemaphore.Release();
        }
    }
}

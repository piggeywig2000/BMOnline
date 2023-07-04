using System.Net;
using BMOnline.Client;
using BMOnline.Client.Relay.Requests;
using BMOnline.Client.Relay.Snapshots;
using BMOnline.Common;
using BMOnline.Common.Relay.Requests;
using BMOnline.Common.Relay.Snapshots;

namespace BMOnline.StressTest
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await DoDrift();
        }

        static (OnlineClient[], Task[]) CreateClients(int count)
        {
            OnlineClient[] clients = new OnlineClient[count];
            Task[] busyTasks = new Task[count];
            for (int i = 0; i < count; i++)
            {
                clients[i] = new OnlineClient(IPAddress.Loopback, 10998, $"Bot {i}", Array.Empty<RelaySnapshotType>(), Array.Empty<RelayRequestType>());
                //byte[] customisationsNum = new byte[] { byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, 1, byte.MaxValue, byte.MaxValue, 91, byte.MaxValue, byte.MaxValue };
                //byte[] customisationsChara = new byte[] { 0, 0, 0, 0, 1, 0, 0, 0, 0, 0 };
                byte[] customisationsNum = new byte[] { byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, 91, byte.MaxValue, byte.MaxValue };
                byte[] customisationsChara = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                clients[i].StateSemaphore.Wait();
                clients[i].State.GetPlayerInfoType().SendData(new PlayerInfoRequest($"Bot {i}", 1, 16, 2201, 1, 0, customisationsNum, customisationsChara));
                clients[i].StateSemaphore.Release();
                busyTasks[i] = Task.Run(clients[i].RunBusy);
            }
            return (clients.ToArray(), busyTasks);
        }

        static async Task DoDrift()
        {
            Log.Info("Debugger not attached, doing drift");
            (OnlineClient[] clients, Task[] busyTasks) = CreateClients(1);

            DateTime startTime = DateTime.UtcNow;
            while (!busyTasks.Any(t => t.IsCompleted))
            {
                await Task.Delay(17); //~60fps
                for (int i = 0; i < clients.Length; i++)
                {
                    OnlineClient client = clients[i];
                    //Move between (3, 3, -10) and (-3, 3, -10)
                    if (client.StateSemaphore.Wait(0))
                    {
                        double t = (DateTime.UtcNow - startTime).TotalMilliseconds % 10000;
                        float x = t < 5000 ? 4.0f - (((float)t / 5000) * 8.0f) : -4.0f + ((((float)t - 5000) / 5000) * 8.0f);
                        client.State.GetStagePositionType().SetSnapshotToSend(new StagePositionSnapshot((x, 3, -i - 2), (t < 5000 ? -90 : 90, 0, t < 5000 ? -180 : 180), 1, true), RelaySnapshotBroadcastType.EveryoneOnStage, 2201);
                        client.StateSemaphore.Release();
                    }
                    else
                    {
                        Log.Info("State sempahore blocked, skipping");
                    }
                }
            }

            foreach (Task task in busyTasks)
            {
                if (task.Exception != null)
                {
                    Log.Error(task.Exception.ToString());
                }
            }
        }
    }
}
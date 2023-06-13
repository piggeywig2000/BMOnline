using System.Net;
using BMOnline.Client;
using BMOnline.Common;

namespace BMOnline.StressTest
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await DoDrift();
            //if (!System.Diagnostics.Debugger.IsAttached)
            //{
            //    await DoDrift();
            //}
            //else
            //{
            //    await TestStuff();
            //}
        }

        static async Task TestStuff()
        {
            Log.Info("Debugger attached, doing test stuff");
            OnlineClient client = new OnlineClient(IPAddress.Loopback, 10998, "Test");
            Task clientLoop = Task.Run(client.RunBusy);
            while (!clientLoop.IsCompleted)
            {
                if (client.StateSemaphore.Wait(0))
                {
                    if (client.State.Players.Count > 0)
                    {
                        OnlinePlayer player = client.State.Players.Values.First();
                        OnlinePosition? position = player.GetPosition();
                        if (!position.HasValue)
                        {
                            Log.Info($"Latest: {player.GetLatestPosition()}");
                        }
                        else
                        {
                            //Log.Info($"Time: {(int)player.TimeSinceLatestSnapshot().TotalMilliseconds}");
                            Log.Info($"Interpolated: {position.Value}");
                        }
                    }
                    client.StateSemaphore.Release();
                }
                await Task.Delay(100);
            }
        }

        static (OnlineClient[], Task[]) CreateClients(int count)
        {
            OnlineClient[] clients = new OnlineClient[count];
            Task[] busyTasks = new Task[count];
            for (int i = 0; i < count; i++)
            {
                clients[i] = new OnlineClient(IPAddress.Loopback, 10998, $"Bot {i}");
                //clients[i] = new OnlineClient(IPAddress.Parse("130.162.178.6"), 10998, $"Bot {i}");
                clients[i].State.Location = OnlineState.OnlineLocation.Game;
                clients[i].State.Course = 18;
                clients[i].State.Stage = 2201;
                clients[i].State.MyPosition = new OnlinePosition();
                clients[i].State.Character = 1;
                clients[i].State.SkinIndex = 0;
                //clients[i].State.CustomisationsNum = new byte[] { byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, 1, byte.MaxValue, byte.MaxValue, 91, byte.MaxValue, byte.MaxValue };
                //clients[i].State.CustomisationsChara = new byte[] { 0, 0, 0, 0, 1, 0, 0, 0, 0, 0 };
                clients[i].State.CustomisationsNum = new byte[] { byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, 91, byte.MaxValue, byte.MaxValue };
                clients[i].State.CustomisationsChara = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                busyTasks[i] = Task.Run(clients[i].RunBusy);
            }
            return (clients.ToArray(), busyTasks);
        }

        static async Task DoDrift()
        {
            Log.Info("Debugger not attached, doing drift");
            (OnlineClient[] clients, Task[] busyTasks) = CreateClients(1);

            DateTime startTime = DateTime.UtcNow;
            DateTime lastUpdate = DateTime.UtcNow;
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
                        client.State.MyPosition = new OnlinePosition(x, 3, -i - 2, t < 5000 ? -90 : 90, 0, t < 5000 ? -180 : 180);
                        client.State.MotionState = 1;
                        client.State.IsOnGround = true;
                        client.StateSemaphore.Release();
                    }
                    else
                    {
                        Log.Info("State sempahore blocked, skipping");
                    }
                }

                if (DateTime.UtcNow - lastUpdate > TimeSpan.FromMilliseconds(200))
                {
                    //Log.Info($"Position: {client.State.MyPosition}");
                    lastUpdate = DateTime.UtcNow;
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
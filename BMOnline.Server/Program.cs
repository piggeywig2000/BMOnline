using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Help;
using System.CommandLine.Parsing;
using System.Net;
using BMOnline.Common;

namespace BMOnline.Server
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            Option<ushort> portOption = new Option<ushort>("--port", () => 10998, description: "The port to run the server on. Should be a number from 1 to 65535.");
            Option<string?> passwordOption = new Option<string?>("--password", description: "The server password. Omit this option to allow clients to connect to the server without a password.");
            Option<ushort> maxChatLengthOption = new Option<ushort>("--max-chat-length", () => 280, "The maximum number of characters allowed in a single chat message. Set this to 0 to disable chat.");
            maxChatLengthOption.ArgumentHelpName = "length";

            RootCommand rootCommand = new RootCommand() { portOption, passwordOption, maxChatLengthOption };
            rootCommand.SetHandler(RunServer, portOption, passwordOption, maxChatLengthOption);

            Parser parser = new CommandLineBuilder(rootCommand)
                .UseDefaults()
                .UseHelp(help =>
                {
                    help.HelpBuilder.CustomizeLayout(_ => HelpBuilder.Default.GetLayout().Skip(2));
                })
                .Build();

            await parser.InvokeAsync(args);

        }

        private static async Task RunServer(ushort port, string? password, ushort maxChatLength)
        {
            if (string.IsNullOrWhiteSpace(password))
                password = null;
            if (password != null && password.Length > 32)
            {
                Log.Error("The server password exceeded the maximum length of 32 characters");
                return;
            }
            Log.Info($"Starting server");
            Log.Config($"Port: {port}");
            Log.Config(password != null ? $"Password: {password}" : "No password");
            Log.Config(maxChatLength > 0 ? $"Maximum chat message length: {maxChatLength}" : "Chat messages are disabled");
            Server server = new Server(new IPEndPoint(IPAddress.Any, port), password, maxChatLength);
            await server.RunBusy();
        }
    }
}
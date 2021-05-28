using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using System.Diagnostics;

/// <summary>
/// Redirects all log entries into the visual studio output window. Only for your convenience during development and testing.
/// </summary>

namespace primitiveSurvival
{

    public class RedirectLogs : ModSystem
    {

        public override bool ShouldLoad(EnumAppSide side)
        {
            return true;
        }

        public override void StartServerSide(ICoreServerAPI Api)
        {
            Api.Server.Logger.EntryAdded += OnServerLogEntry;
        }

        private void OnServerLogEntry(EnumLogType logType, string message, params object[] args)
        {
            if (logType == EnumLogType.VerboseDebug) return;
            Debug.WriteLine("[Server " + logType + "] " + message, args);
        }

        public override void StartClientSide(ICoreClientAPI Api)
        {
            Api.World.Logger.EntryAdded += OnClientLogEntry;
        }

        private void OnClientLogEntry(EnumLogType logType, string message, params object[] args)
        {
            if (logType == EnumLogType.VerboseDebug) return;
            Debug.WriteLine("[Client " + logType + "] " + message, args);
        }
    }
}

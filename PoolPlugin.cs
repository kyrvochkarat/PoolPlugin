using System;
using Exiled.API.Features;

namespace PollPlugin
{
    public class PollPlugin : Plugin<Config>
    {
        public override string Name => "PollPlugin";
        public override string Author => "vityanvsk";
        public override Version Version => new Version(1, 0, 0);
        public override Version RequiredExiledVersion => new Version(8, 0, 0);

        public static PollPlugin Instance { get; private set; }

        public override void OnEnabled()
        {
            Instance = this;
            PollManager.Reset();
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            PollManager.Reset();
            Instance = null;
            base.OnDisabled();
        }
    }
}
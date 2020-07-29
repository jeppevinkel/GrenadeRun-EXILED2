using System;
using System.Collections.Generic;
using Exiled.API.Features;
using MEC;
using Server = Exiled.Events.Handlers.Server;
using Player = Exiled.Events.Handlers.Player;

namespace GrenadeRun
{
    public class GrenadeRun : Plugin<Config>
    {
	    private static readonly Lazy<GrenadeRun> LazyInstance = new Lazy<GrenadeRun>(() => new GrenadeRun());
	    public static GrenadeRun Instance => LazyInstance.Value;

	    public bool GrenadeRound = false;
		public List<Exiled.API.Features.Player> Escapees = new List<Exiled.API.Features.Player>();

	    private Handlers.Server server;
	    private Handlers.Player player;

		private GrenadeRun()
	    {
	    }

	    public override void OnEnabled()
	    {
		    base.OnEnabled();

		    RegisterEvents();
	    }

	    public override void OnDisabled()
	    {
		    base.OnDisabled();

		    UnregisterEvents();

	    }

	    public void RegisterEvents()
	    {
			server = new Handlers.Server();
			player = new Handlers.Player();

			Server.RoundStarted += server.OnRoundStarted;
			Server.RestartingRound += server.OnRestartingRound;

			Player.Died += player.OnDied;
			Player.Escaping += player.OnEscaping;
	    }

	    public void UnregisterEvents()
	    {
		    Server.RoundStarted -= server.OnRoundStarted;
		    Server.RestartingRound -= server.OnRestartingRound;

			Player.Died -= player.OnDied;
		    Player.Escaping -= player.OnEscaping;

		    Timing.KillCoroutines();

		    server = null;
		    player = null;
		}
    }
}

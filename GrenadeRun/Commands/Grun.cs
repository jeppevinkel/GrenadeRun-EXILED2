using System;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using RemoteAdmin;

namespace GrenadeRun.Commands
{
	[CommandHandler(typeof(RemoteAdminCommandHandler))]
	class Grun : ICommand
	{
		public string Command { get; } = "grun";
		public string[] Aliases { get; } = { "grenaderun" };
		public string Description { get; } = "Activates grenade run for the current round.";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			if (sender is PlayerCommandSender player)
			{
				if (!player.CheckPermission("jopo.grun"))
				{
					response = "Missing permissions. (jopo.grun)";
					return false;
				}
			}

			if (Round.IsStarted)
			{
				response = "Can't enable GrenadeRun during round.";
				return false;
			}

			GrenadeRun.Instance.GrenadeRound = true;
			Round.IsLocked = true;
			response = "GrenadeRun enabled for current round!";
			return true;
		}
	}
}

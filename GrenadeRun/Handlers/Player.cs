using Exiled.API.Features;
using Exiled.Events.EventArgs;

namespace GrenadeRun.Handlers
{
	class Player
	{
		public void OnDied(DiedEventArgs ev)
		{
			if (GrenadeRun.Instance.GrenadeRound && ev.Target != null && ev.HitInformations.GetDamageType() == DamageTypes.Grenade)
			{
				GrenadeRun.Instance.Config.Translations.TryGetValue("PlayerDied", out string msg);
				Map.Broadcast((ushort)GrenadeRun.Instance.Config.DeathMsgTime, msg.Replace("{player}", ev.Target.Nickname));
			}
		}

		public void OnEscaping(EscapingEventArgs ev)
		{
			if (GrenadeRun.Instance.GrenadeRound && ev.IsAllowed)
			{
				if (ev.Player.Role != RoleType.ClassD) ev.IsAllowed = false;
				GrenadeRun.Instance.Escapees.Add(ev.Player);

				ev.NewRole = RoleType.Spectator;

				GrenadeRun.Instance.Config.Translations.TryGetValue("PlayerEscaped", out string msg);
				Map.Broadcast(2, msg.Replace("{player}", ev.Player.Nickname).Replace("{number}", GrenadeRun.Instance.Escapees.Count.ToString()));
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Grenades;
using MEC;
using Mirror;
using UnityEngine;

namespace GrenadeRun.Handlers
{
	class Server
	{
		public void OnRoundStarted()
		{
			if (GrenadeRun.Instance.GrenadeRound)
			{
				LockDoors();

				GrenadeRun.Instance.Config.Translations.TryGetValue("RoundStarted", out string msg);
				Map.Broadcast((ushort)GrenadeRun.Instance.Config.Preparation, msg);

				Timing.CallDelayed(0.8f, SpawnClassD);

				Timing.CallDelayed(GrenadeRun.Instance.Config.Preparation, StartGrun);
				Timing.RunCoroutine(CheckEnd());
			}
		}

		public void OnRestartingRound()
		{
			if (GrenadeRun.Instance.GrenadeRound)
			{
				Timing.KillCoroutines();

				GrenadeRun.Instance.GrenadeRound = false;
				GrenadeRun.Instance.Escapees.Clear();
			}
		}

		public void SpawnClassD()
		{
			foreach (Exiled.API.Features.Player player in Exiled.API.Features.Player.List)
			{
				if (player.IsBypassModeEnabled) continue;
				player.SetRole(RoleType.ClassD);
				player.Health = player.MaxHealth;
			}
		}

		private IEnumerator<float> CheckEnd()
		{
			while (GrenadeRun.Instance.GrenadeRound)
			{
				yield return Timing.WaitForSeconds(1);

				if (Exiled.API.Features.Player.List.Any(p => p.Role == RoleType.ClassD))
				{
					continue;
				}

				string msg;

				if (GrenadeRun.Instance.Escapees.IsEmpty())
				{
					GrenadeRun.Instance.Config.Translations.TryGetValue("EndingMessageLoss", out msg);
				}
				else
				{
					GrenadeRun.Instance.Config.Translations.TryGetValue("EndingMessageWin", out msg);

					for (int i = 0; i < Math.Min(5, GrenadeRun.Instance.Escapees.Count); i++)
					{
						msg += $"\n#{i+1} {GrenadeRun.Instance.Escapees[i].Nickname}";
					}
				}

				Map.Broadcast((ushort)GrenadeRun.Instance.Config.EndingDelay, msg);

				yield return Timing.WaitForSeconds(GrenadeRun.Instance.Config.EndingDelay);

				Round.Restart();
			}
		}

		private static void StartGrun()
		{
			GrenadeRun.Instance.Config.Translations.TryGetValue("DoorsUnlocked", out string msg);
			Map.Broadcast((ushort)GrenadeRun.Instance.Config.GrenadeDelay, msg);
			UnlockDoors();
			Timing.RunCoroutine(SpawnGrenades());
		}

		private static IEnumerator<float> SpawnGrenades()
		{
			yield return Timing.WaitForSeconds(GrenadeRun.Instance.Config.GrenadeDelay);

			while (GrenadeRun.Instance.GrenadeRound)
			{
				foreach (Exiled.API.Features.Player player in Exiled.API.Features.Player.List)
				{
					if (player.Role == RoleType.ClassD)
					{
						GrenadeManager gm = player.ReferenceHub.GetComponent<GrenadeManager>();
						GrenadeSettings grenade = gm.availableGrenades.FirstOrDefault(g => g.inventoryID == ItemType.GrenadeFrag);
						if (grenade == null) continue;

						Grenade component = UnityEngine.Object.Instantiate(grenade.grenadeInstance).GetComponent<Grenade>();
						component.InitData(gm, Vector3.zero, Vector3.zero, 0f);
						NetworkServer.Spawn(component.gameObject);
					}
				}

				yield return Timing.WaitForSeconds(GrenadeRun.Instance.Config.GrenadeDelay);
			}
		}

		private static void LockDoors()
		{
			foreach (Door door in Map.Doors)
			{
				door.locked = true;
			}
		}

		private static void UnlockDoors()
		{
			foreach (Door door in Map.Doors)
			{
				door.locked = false;
			}
		}
	}
}

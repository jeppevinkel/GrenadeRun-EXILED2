using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Grenades;
using MEC;
using Mirror;
using Respawning;
using UnityEngine;

namespace GrenadeRun.Handlers
{
	internal class Server
	{
		private const int SurfaceBound = 950;
		private const int LczUpper = 950;
		private const int LczLower = -950;
		private const int HczBound = -950;

		public List<CoroutineHandle> Coroutines = new List<CoroutineHandle>();

		public void OnRoundStarted()
		{
			if (GrenadeRun.Instance.GrenadeRound)
			{
				LockDoors();

				GrenadeRun.Instance.Config.Translations.TryGetValue("RoundStarted", out string msg);
				Map.Broadcast((ushort)GrenadeRun.Instance.Config.Preparation, msg);

				Coroutines.Add(Timing.CallDelayed(0.8f, SpawnClassD));

				Coroutines.Add(Timing.CallDelayed(GrenadeRun.Instance.Config.Preparation, StartGrun));
				Coroutines.Add(Timing.RunCoroutine(CheckEnd()));
			}
		}

		public void OnRestartingRound()
		{
			if (GrenadeRun.Instance.GrenadeRound)
			{
				foreach (CoroutineHandle handle in Coroutines)
					Timing.KillCoroutines(handle);

				GrenadeRun.Instance.GrenadeRound = false;
				GrenadeRun.Instance.Escapees.Clear();
				FriendlyFireConfig.PauseDetector = GrenadeRun.Instance.oldFFValue;
			}
		}

		public void OnRespawningTeam(RespawningTeamEventArgs ev)
		{
			if (GrenadeRun.Instance.GrenadeRound)
			{
				ev.NextKnownTeam = SpawnableTeamType.None;
				ev.Players.Clear();
			}
		}

		public void SpawnClassD()
		{
			foreach (Exiled.API.Features.Player player in Exiled.API.Features.Player.List)
			{
				if (player.IsBypassModeEnabled || player.Role == RoleType.ClassD) continue;
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
				break;
			}
		}

		private void StartGrun()
		{
			GrenadeRun.Instance.Config.Translations.TryGetValue("DoorsUnlocked", out string msg);
			Map.Broadcast((ushort)GrenadeRun.Instance.Config.GrenadeDelayLcz, msg);
			UnlockDoors();
			GrenadeRun.Instance.oldFFValue = FriendlyFireConfig.PauseDetector;
			FriendlyFireConfig.PauseDetector = true; 
			Coroutines.Add(Timing.RunCoroutine(SpawnGrenadesLcz()));
			Coroutines.Add(Timing.RunCoroutine(SpawnGrenadesHcz()));
			Coroutines.Add(Timing.RunCoroutine(SpawnGrenadesSurface()));
		}

		private static IEnumerator<float> SpawnGrenadesLcz()
		{
			yield return Timing.WaitForSeconds(GrenadeRun.Instance.Config.GrenadeDelayLcz);

			while (GrenadeRun.Instance.GrenadeRound)
			{
				foreach (Exiled.API.Features.Player player in Exiled.API.Features.Player.List)
				{
					if (player.Role == RoleType.ClassD && player.Position.y > LczLower && LczUpper > player.Position.y)
					{
						GrenadeManager gm = player.ReferenceHub.GetComponent<GrenadeManager>();
						GrenadeSettings grenade = gm.availableGrenades.FirstOrDefault(g => g.inventoryID == ItemType.GrenadeFrag);
						if (grenade == null) continue;

						Grenade component = UnityEngine.Object.Instantiate(grenade.grenadeInstance).GetComponent<Grenade>();
						component.InitData(gm, Vector3.zero, Vector3.zero, 0f);
						NetworkServer.Spawn(component.gameObject);
					}
				}

				yield return Timing.WaitForSeconds(GrenadeRun.Instance.Config.GrenadeDelayLcz);
			}
		}

		private static IEnumerator<float> SpawnGrenadesHcz()
		{
			yield return Timing.WaitForSeconds(GrenadeRun.Instance.Config.GrenadeDelayHcz);

			while (GrenadeRun.Instance.GrenadeRound)
			{
				foreach (Exiled.API.Features.Player player in Exiled.API.Features.Player.List)
				{
					if (player.Role == RoleType.ClassD && HczBound > player.Position.y)
					{
						GrenadeManager gm = player.ReferenceHub.GetComponent<GrenadeManager>();
						GrenadeSettings grenade = gm.availableGrenades.FirstOrDefault(g => g.inventoryID == ItemType.GrenadeFrag);
						if (grenade == null) continue;

						Grenade component = UnityEngine.Object.Instantiate(grenade.grenadeInstance).GetComponent<Grenade>();
						component.InitData(gm, Vector3.zero, Vector3.zero, 0f);
						NetworkServer.Spawn(component.gameObject);
					}
				}

				yield return Timing.WaitForSeconds(GrenadeRun.Instance.Config.GrenadeDelayHcz);
			}
		}

		private static IEnumerator<float> SpawnGrenadesSurface()
		{
			yield return Timing.WaitForSeconds(GrenadeRun.Instance.Config.GrenadeDelaySurface);

			while (GrenadeRun.Instance.GrenadeRound)
			{
				foreach (Exiled.API.Features.Player player in Exiled.API.Features.Player.List)
				{
					if (player.Role == RoleType.ClassD && player.Position.y > SurfaceBound)
					{
						GrenadeManager gm = player.ReferenceHub.GetComponent<GrenadeManager>();
						GrenadeSettings grenade = gm.availableGrenades.FirstOrDefault(g => g.inventoryID == ItemType.GrenadeFrag);
						if (grenade == null) continue;

						Grenade component = UnityEngine.Object.Instantiate(grenade.grenadeInstance).GetComponent<Grenade>();
						component.InitData(gm, Vector3.zero, Vector3.zero, 0f);
						NetworkServer.Spawn(component.gameObject);
					}
				}

				yield return Timing.WaitForSeconds(GrenadeRun.Instance.Config.GrenadeDelaySurface);
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

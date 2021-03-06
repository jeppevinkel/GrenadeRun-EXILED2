﻿using System.Collections.Generic;
using System.ComponentModel;
using Exiled.API.Interfaces;

namespace GrenadeRun
{
	public sealed class Config : IConfig
	{
		[Description("Determines whether the plugin is enabled or not.")]
		public bool IsEnabled { get; set; } = true;

		[Description("The time in seconds to wait before unlocking the doors.")]
		public float Preparation { get; set; } = 10f;

		[Description("The time in seconds between each grenade drop in light containment zone.")]
		public float GrenadeDelayLcz { get; set; } = 7.0f;

		[Description("The time in seconds between each grenade drop in heavy containment zone.")]
		public float GrenadeDelayHcz { get; set; } = 6.0f;

		[Description("The time in seconds between each grenade drop on the surface.")]
		public float GrenadeDelaySurface { get; set; } = 5.0f;

		[Description("The duration in seconds for the end screen to be displayed.")]
		public float EndingDelay { get; set; } = 6f;

		[Description("The time in seconds to display the death messages.")]
		public float DeathMsgTime { get; set; } = 2f;

		public Dictionary<string, string> Translations { get; set; } = new Dictionary<string, string>
		{
			{"RoundStarted", "This round is a grenade run\nOnce the doors unlock, grenades will start falling on your heads until a winner escapes the facility!"},
			{"DoorsUnlocked", "Run for your lives!"},
			{"PlayerDied", "{player} has been eliminated from the round!"},
			{"PlayerEscaped", "{player} has escaped as number {number}!"},
			{"EndingMessageWin", "Winners:"},
			{"EndingMessageLoss", "There were no winners this round."}
		};
	}
}

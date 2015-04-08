using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KspMerillEngineFail
{
	// todelete not used
    public class MerillRemoveComplexity : PartModule
	{

		public override void OnStart(StartState state)
		{
			base.OnStart(state);
			print("[MERILL]removeCom " + state+" on "+part.name);
			if ((state | StartState.PreLaunch) > 0)
			{
				//remove complexity
				float nb = part.RequestResource("Complexity", 1000000);
				print("[MERILL]removeCom removing " + nb);
			}
		}
	}
}

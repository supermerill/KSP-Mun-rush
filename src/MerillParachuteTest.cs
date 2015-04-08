using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KspMerillEngineFail
{
    public class MerillParachuteTest : MerillSimplePartTest
	{

		ModuleParachute parachute;

		public override void OnLoad(ConfigNode node)
		{
			base.OnLoad(node);
			parachute = this.part.Modules.OfType<ModuleParachute>().Single();
		}

		public override bool oneTimeActivation()
		{
			if (parachute != null)
			{
				//thanks to [Coffeeman] 'entropy' plug-in to know how to get that
				return parachute.deploymentState != ModuleParachute.deploymentStates.STOWED;
			}
			else
			{
				return false;
			}
		}


	}
}

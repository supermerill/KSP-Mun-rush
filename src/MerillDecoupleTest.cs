using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KspMerillEngineFail
{
	//note: explosion is useless : it occur in the wrong part of the vessel.
	// anyway, this part is basically exploding himself.
    public class MerillDecoupleTest : MerillSimplePartTest
	{
		
		ModuleDecouple decoupler;
		
		public override void OnLoad(ConfigNode node)
		{
			base.OnLoad(node);

			decoupler = this.part.Modules.OfType<ModuleDecouple>().Single();

			for (int i = 0; i < part.Modules.Count; i++)
			{
				PartModule fx = part.Modules[i];
			}
		}

		public override bool oneTimeActivation()
		{
			if (decoupler != null)
			{
				return decoupler.isDecoupled;
			}
			else
			{
				decoupler = this.part.Modules.OfType<ModuleDecouple>().Single();
				return false;
			}
		}


	}
}

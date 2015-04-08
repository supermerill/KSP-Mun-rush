using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KspMerillEngineFail
{
    public class MerillDecoupleTest : MerillSimplePartTest
	{

		ModuleDecouple decoupler;
		
		public override void OnLoad(ConfigNode node)
		{
			base.OnLoad(node);

			decoupler = this.part.Modules.OfType<ModuleDecouple>().Single();

			//print("[MERILL] part contain modules:");
			for (int i = 0; i < part.Modules.Count; i++)
			{
				PartModule fx = part.Modules[i];
				//print("[Merill] module partfx: " + part.Modules[i].name
				//	+ " , class=" + part.Modules[i].ClassName);
			}
			//print("[MERILL] decoupler: " + decoupler);
			//print("[MERILL] decoupl?: " + decoupler.isDecoupled);
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

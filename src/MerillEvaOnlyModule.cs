using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KspMerillEngineFail
{
	public class MerillEvaOnlyModule : PartModule
	{
		BaseEvent btsmScienceEvent = null;

		public override void OnStart(StartState state)
		{
			// FindObjectOfType the BTSM science module
			print("[MERILL]MerillEvaOnlyModule! start, search in " + part.Modules.Count + " modules");
			foreach (PartModule pm in part.Modules)
			{
				//print("[MERILL]MerillEvaOnlyModule! start, find module " + pm.moduleName);
				//foreach (BaseEvent evt in pm.Events)
				//{
				//	print("[MERILL]MerillEvaOnlyModule! ----- has event " + evt.guiName + ", " + evt + " act:" + evt.active);
				//	print("[MERILL]MerillEvaOnlyModule! ----- --------- " 
				//		+ "extoEVAOnly=" + evt.externalToEVAOnly+", cat: "+evt.category+", actEdit"+evt.guiActiveEditor
				//		+ ", ActUnfoc" + evt.guiActiveUnfocused+", assigned"+evt.assigned);
				//}

				// disable "deploy" on eva
				if (pm.moduleName.Equals("ModuleAnimateGeneric"))
				{

					BaseEvent Toggle = pm.Events["Toggle"];
					//print("[MERILL]MerillEvaOnlyModule! ---- find Toggle " + Toggle);
					if (Toggle != null)
					{
						Toggle.active = false;
						Toggle.externalToEVAOnly = false;
						Toggle.guiActiveEditor = false;
					}
					BaseEvent evt = Toggle;
					//print("[MERILL]MerillEvaOnlyModule! ----- has Toggle " + evt.guiName + ", " + evt + " act:" + evt.active);
					//print("[MERILL]MerillEvaOnlyModule! ----- --------- " 
					//	+ "extoEVAOnly=" + evt.externalToEVAOnly+", cat: "+evt.category+", actEdit"+evt.guiActiveEditor
					//	+ ", ActUnfoc" + evt.guiActiveUnfocused+", assigned"+evt.assigned);
				}
				if (pm.moduleName.Equals("BTSMModuleScienceExperiment"))
				{
					btsmScienceEvent = pm.Events["BTSMDeployExperiment"];
					BaseAction btsmScienceAction = pm.Actions["BTSMDeployExperimentAction"];
					BaseEvent merillScienceEvent = Events["MerillEventDeployScienceEva"];

					print("[MERILL]MerillEvaOnlyModule! ---- find btsmScienceEvent " + btsmScienceEvent);
					print("[MERILL]MerillEvaOnlyModule! ---- find btsmScienceAction " + btsmScienceAction);
					string scienceName = "Deploy";
					if (btsmScienceEvent != null)
					{
						//disable, i take control on event
						btsmScienceEvent.active = false;
						scienceName = btsmScienceEvent.guiName;
					}
					if (btsmScienceAction != null)
					{
						btsmScienceAction.active = false;
					}
					if (merillScienceEvent != null)
					{
						merillScienceEvent.guiName = btsmScienceEvent.guiName;
					}
				}
			}
		}

		[KSPEvent(active = true, guiActiveUnfocused = true, guiName = "deployScienceEva", unfocusedRange = 20f, externalToEVAOnly = true)]
		private void MerillEventDeployScienceEva()
		{
			print("[MERILL]MerillEvaOnlyModule! MerillEventDeployScienceEva");
			if (btsmScienceEvent != null)
				btsmScienceEvent.Invoke();
		}
	}
}

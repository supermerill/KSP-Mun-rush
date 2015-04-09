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
			MerillData.log("MerillEvaOnlyModule! start, search in " + part.Modules.Count + " modules");
			foreach (PartModule pm in part.Modules)
			{
				//MerillData.log("MerillEvaOnlyModule! start, find module " + pm.moduleName);
				//foreach (BaseEvent evt in pm.Events)
				//{
				//	MerillData.log("MerillEvaOnlyModule! ----- has event " + evt.guiName + ", " + evt + " act:" + evt.active);
				//	MerillData.log("MerillEvaOnlyModule! ----- --------- " 
				//		+ "extoEVAOnly=" + evt.externalToEVAOnly+", cat: "+evt.category+", actEdit"+evt.guiActiveEditor
				//		+ ", ActUnfoc" + evt.guiActiveUnfocused+", assigned"+evt.assigned);
				//}

				// disable "deploy" on eva
				if (pm.moduleName.Equals("ModuleAnimateGeneric"))
				{

					BaseEvent Toggle = pm.Events["Toggle"];
					//MerillData.log("MerillEvaOnlyModule! ---- find Toggle " + Toggle);
					if (Toggle != null)
					{
						Toggle.active = false;
						Toggle.externalToEVAOnly = false;
						Toggle.guiActiveEditor = false;
					}
					BaseEvent evt = Toggle;
					//MerillData.log("MerillEvaOnlyModule! ----- has Toggle " + evt.guiName + ", " + evt + " act:" + evt.active);
					//MerillData.log("MerillEvaOnlyModule! ----- --------- " 
					//	+ "extoEVAOnly=" + evt.externalToEVAOnly+", cat: "+evt.category+", actEdit"+evt.guiActiveEditor
					//	+ ", ActUnfoc" + evt.guiActiveUnfocused+", assigned"+evt.assigned);
				}
				if (pm.moduleName.Equals("BTSMModuleScienceExperiment"))
				{
					btsmScienceEvent = pm.Events["BTSMDeployExperiment"];
					BaseAction btsmScienceAction = pm.Actions["BTSMDeployExperimentAction"];
					BaseEvent merillScienceEvent = Events["MerillEventDeployScienceEva"];

					MerillData.log("MerillEvaOnlyModule! ---- find btsmScienceEvent " + btsmScienceEvent);
					MerillData.log("MerillEvaOnlyModule! ---- find btsmScienceAction " + btsmScienceAction);
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
			MerillData.log("MerillEvaOnlyModule! MerillEventDeployScienceEva");
			if (btsmScienceEvent != null)
				btsmScienceEvent.Invoke();
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KspMerillEngineFail
{
	public class MerillModuleScienceFail : PartModule
	{

		bool canBeTested = false;
		BaseAction btsmScienceAction = null;
		BaseEvent btsmScienceEvent = null;
		ModuleScienceExperiment scienceExperiment;


		[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = false, guiName = "State")]
		public string stateDisplay = "Working";

		[KSPField]
		public bool isInactive = false;

		public override void OnStart(StartState state)
		{
			base.OnStart(state);

			if (isInactive)
			{
				stateDisplay = "Dead";
			}

			// FindObjectOfType the BTSM science module
			print("[MERILL]SCIENCE! start, search in module "+part.Modules.Count);
			foreach (PartModule pm in part.Modules)
			{
				print("[MERILL]SCIENCE! start, find module " + pm.moduleName);
				if (pm.moduleName.Equals("BTSMModuleScienceExperiment"))
				{
					scienceExperiment = (ModuleScienceExperiment)pm;
					btsmScienceEvent = pm.Events["BTSMDeployExperiment"];
					btsmScienceAction = pm.Actions["BTSMDeployExperimentAction"];
					BaseEvent merillScienceEvent = Events["MERILLScienceEventDeployOrTest"];
					BaseAction merillScienceAction = Actions["MERILLScienceActionDeployOrTest"];
					print("[merill]science! start, find evenat&action '" + btsmScienceEvent + "', '" + btsmScienceAction + "'");
					print("[merill]science! start, find evenat&action '" + merillScienceEvent + "', '" + merillScienceAction + "'");
					if (merillScienceEvent != null && merillScienceAction != null)
					{
						if (isInactive)
						{
							merillScienceEvent.guiActive = false;
							merillScienceAction.active = false;
						}
						if (btsmScienceEvent != null && btsmScienceAction != null )
						{
							canBeTested = true;
							btsmScienceEvent.guiActive = false;
							btsmScienceEvent.guiActiveEditor = false;
							btsmScienceAction.active = false;
							merillScienceEvent.guiName = btsmScienceEvent.guiName;
							merillScienceAction.guiName = btsmScienceAction.guiName;
							print("[merill]science! start, merillScienceEvent.name= '" + merillScienceEvent.guiName + "'");
						}
					}
				}
			}

		}


		[KSPEvent(guiName = "Deploy Experiment", guiActive = true, guiActiveEditor = false)]
		public void MERILLScienceEventDeployOrTest()
		{
			print("[MERILL]SCIENCE! scienceDeployOrTest '" + btsmScienceEvent+"'");
			doScienceEvent();
		}

		public virtual void doScienceEvent()
		{
			if (canBeTested && btsmScienceEvent != null && isInstrumentOk())
			{
				print("[MERILL]SCIENCE! scienceDeployOrTest invoke");
				btsmScienceEvent.Invoke();
				print("[MERILL]SCIENCE! scienceDeployOrTest invoke done");
			}
		}

		[KSPAction("Deploy Experiment")]
		public void MERILLScienceActionDeployOrTest(KSPActionParam param)
		{
			print("[MERILL]SCIENCE! scienceAction "+canBeTested+" '" + btsmScienceAction+ "'");
			doScienceAction();
		}

		public virtual void doScienceAction()
		{
			if (canBeTested && btsmScienceAction != null && isInstrumentOk())
			{
			print("[MERILL]SCIENCE! scienceAction invoke done");
			btsmScienceAction.Invoke(new KSPActionParam(KSPActionGroup.None, KSPActionType.Activate));
			print("[MERILL]SCIENCE! scienceAction invoke done");
			}
		}

		protected bool isInstrumentOk()
		{
			print("[MERILL]SCIENCE! isInstrumentOk isInactive="+isInactive+" for part "+part.name);
			if(isInactive) return false;

			float chance = 15 + 40f / ((((float)MerillData.instance.partNameTested.Count) / 8f) + 1);

			// luck?
			int peudoAleat = MerillData.instance.get0to99FromNotAleatTable("science_"+part.name);

			print("[Merill] test: " + peudoAleat + " ?> " + chance);

			isInactive = peudoAleat < chance;

			//Here, can send event to btsmExperiemnt, like "set inactive" "set dead"
			if (isInactive)
			{
				//TODO: use a field to get displaymessage (and rand to get one)
				//set the display to "dead"
				stateDisplay = "Dead";
				if (scienceExperiment != null)
				{
					print("[MERILL]sciecnefail set to inoperable");
					scienceExperiment.SetInoperable();
				}
				//emit message
				//ScreenMessages.PostScreenMessage("Experiment " + part.partInfo.title.ToString()
				//	+ " is not working anymore, something britlle or delicate has fail."
				//	, 10f, ScreenMessageStyle.UPPER_LEFT);
				ScreenMessages.PostScreenMessage(string.Format(MerillData.str_science_fail, part.partInfo.title.ToString())
					, 10f, ScreenMessageStyle.UPPER_LEFT);
				//maj gui
				BaseEvent merillScienceEvent = Events["MERILLScienceEventDeployOrTest"];
				if (merillScienceEvent != null)
				{
					merillScienceEvent.guiActive = false;
				}
				BaseAction merillScienceAction = Actions["MERILLScienceActionDeployOrTest"];
				if (merillScienceAction != null)
				{
					merillScienceAction.active = false;
				}
			}

			//fail
			return !isInactive;
		}
	}
}

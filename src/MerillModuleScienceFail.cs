using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KspMerillEngineFail
{
	public class MerillModuleScienceFail : PartModule
	{

		protected bool canBeTested = false;
		protected BaseAction btsmScienceAction = null;
		protected BaseEvent btsmScienceEvent = null;
		protected ModuleScienceExperiment scienceExperiment;


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
			MerillData.log("SCIENCE! start, search in module " + part.Modules.Count);
			foreach (PartModule pm in part.Modules)
			{
				//MerillData.log("SCIENCE! start, find module " + pm.moduleName);
				if (pm.moduleName.Equals("BTSMModuleScienceExperiment"))
				{
					scienceExperiment = (ModuleScienceExperiment)pm;
					btsmScienceEvent = pm.Events["BTSMDeployExperiment"];
					btsmScienceAction = pm.Actions["BTSMDeployExperimentAction"];
					BaseEvent merillScienceEvent = Events["MERILLScienceEventDeployOrTest"];
					BaseAction merillScienceAction = Actions["MERILLScienceActionDeployOrTest"];
					//MerillData.log("science! start, find evenat&action '" + btsmScienceEvent + "', '" + btsmScienceAction + "'");
					//MerillData.log("science! start, find evenat&action '" + merillScienceEvent + "', '" + merillScienceAction + "'");
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
							//MerillData.log("science! start, merillScienceEvent.name= '" + merillScienceEvent.guiName + "'");
						}
					}
				}
			}

		}


		[KSPEvent(guiName = "Deploy Experiment", guiActive = true, guiActiveEditor = false)]
		public void MERILLScienceEventDeployOrTest()
		{
			//MerillData.log("SCIENCE! scienceDeployOrTest '" + btsmScienceEvent+"'");
			doScienceEvent();
		}

		public virtual void doScienceEvent()
		{
			if (canBeTested && btsmScienceEvent != null && isInstrumentOk())
			{
				//MerillData.log("SCIENCE! scienceDeployOrTest invoke");
				btsmScienceEvent.Invoke();
				//MerillData.log("SCIENCE! scienceDeployOrTest invoke done");
			}
		}

		[KSPAction("Deploy Experiment")]
		public void MERILLScienceActionDeployOrTest(KSPActionParam param)
		{
			//MerillData.log("SCIENCE! scienceAction "+canBeTested+" '" + btsmScienceAction+ "'");
			doScienceAction();
		}

		public virtual void doScienceAction()
		{
			if (canBeTested && btsmScienceAction != null && isInstrumentOk())
			{
				//MerillData.log("SCIENCE! scienceAction invoke done");
				btsmScienceAction.Invoke(new KSPActionParam(KSPActionGroup.None, KSPActionType.Activate));
				//MerillData.log("SCIENCE! scienceAction invoke done");
			}
		}

		protected bool isInstrumentOk()
		{
			//MerillData.log("SCIENCE! isInstrumentOk isInactive="+isInactive+" for part "+part.name);
			if(isInactive) return false;

			float chance = 15 + 40f / ((((float)MerillData.instance.partNameTested.Count) / 8f) + 1);

			// luck?
			int peudoAleat = MerillData.instance.get0to99FromNotAleatTable("science_"+part.name+"_vessel_"+part.vessel.id.ToString());

			print("[MERILL][Mun Rush] experiment " + part.name + " self-testing : " + peudoAleat + " ?> " + chance);

			isInactive = peudoAleat < chance;

			//Here, can send event to btsmExperiemnt, like "set inactive" "set dead"
			if (isInactive)
			{
				//TODO: use a field to get displaymessage (and rand to get one)
				//set the display to "dead"
				stateDisplay = "Dead";
				if (scienceExperiment != null)
				{
					//MerillData.log("sciecnefail set to inoperable");
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

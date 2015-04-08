using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KspMerillEngineFail
{
	//TODO: hide 'toogle show'
	public class MerillModuleScienceSlot : PartModule
	{

		[KSPField]
		bool hasASlot = false;

		//ease-of-use fields
		bool slotResearched = false;
		int levelOfEvent = 0;
		BaseEvent eventToEnable;
		BaseAction actionToEnable;
		PartModule partModuleToEnable;

		public override void OnStart(StartState state)
		{
			base.OnStart(state);
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			if (slotResearched) return;

			// FindObjectOfType the BTSM science module
			//print("[MERILL]MerillScienceSlotModule start, search in module " + part.Modules.Count + " , (" + state+")"
			//	+ (state.Equals(StartState.PreLaunch)) + " ; " + (state.CompareTo(StartState.PreLaunch)) + " ; " + (state & StartState.PreLaunch));
			//print("[MERILL]MerillScienceSlotModule update " + HighLogic.LoadedSceneIsFlight + ", '" + part.vessel.situation+"'");
			if ((part.vessel.situation) == Vessel.Situations.FLYING)
			{

				foreach (PartModule pm in part.Modules)
				{
					print("[MERILL]MerillScienceSlotModule start, find module " + pm.moduleName);
					if (pm.moduleName.Equals("ModuleScienceExperiment"))
					{
						BaseEvent scienceEvent = pm.Events["DeployExperiment"];
						BaseAction scienceAction = pm.Actions["DeployExperimentAction"];
						if (scienceEvent != null)
						{
							if (levelOfEvent == 0)
							{
								eventToEnable = scienceEvent;
								actionToEnable = scienceAction;
								partModuleToEnable = pm;
							}
							scienceEvent.guiActive = false;
							scienceEvent.guiActiveEditor = false;
							if (scienceAction != null)
							{
								scienceAction.active = false;
							}
							print("[MERILL]MerillScienceSlotModule : disable scienceEvent '" + eventToEnable + "', '" + actionToEnable + "'");
						}
					}

					if (pm.moduleName.Equals("BTSMModuleScienceExperiment"))
					{
						BaseEvent btsmScienceEvent = pm.Events["BTSMDeployExperiment"];
						BaseAction btsmScienceAction = pm.Actions["BTSMDeployExperimentAction"];
						if (btsmScienceEvent != null && btsmScienceAction != null)
						{
							if (levelOfEvent < 1)
							{
								levelOfEvent = 1;
								eventToEnable = btsmScienceEvent;
								actionToEnable = btsmScienceAction;
								partModuleToEnable = pm;
							}
							eventToEnable.guiActive = false;
							eventToEnable.guiActiveEditor = false;
							actionToEnable.active = false;
							print("[MERILL]MerillScienceSlotModule : disable btsmScienceEvent '" + eventToEnable + "', '" + actionToEnable + "'");
						}
					}

					if (pm.moduleName.Equals("MerillModuleScienceFail"))
					{
						BaseEvent merillScienceEvent = pm.Events["MERILLScienceEventDeployOrTest"];
						BaseAction merillScienceAction = pm.Actions["MERILLScienceActionDeployOrTest"];
						if (merillScienceEvent != null && merillScienceAction != null)
						{
							if (levelOfEvent < 2)
							{
								levelOfEvent = 2;
								eventToEnable = merillScienceEvent;
								actionToEnable = merillScienceAction;
								partModuleToEnable = pm;
							}
							((MerillModuleScienceFail)pm).isInactive = true;
							BaseField merillShowState = pm.Fields["stateDisplay"];
							merillShowState.SetValue("Dead", merillShowState.host);
							eventToEnable.guiActive = false;
							eventToEnable.guiActiveEditor = false;
							actionToEnable.active = false;
							print("[MERILL]MerillScienceSlotModule : disable merillScienceEvent '" + eventToEnable + "', '" + actionToEnable + "'");
						}
					}
				}

				checkSlot();

			}
		}

		private void checkSlot()
		{
			print("[MERILL]MerillScienceSlotModule : checkSlot (hasASlot,slotResearched) (" + hasASlot + ", " + slotResearched + ")");
			if (!hasASlot && !slotResearched)
			{
				//find our attach-object
				if (part.parent != null)
				{
					print("[MERILL]MerillScienceSlotModule : parent is " + part.parent.name);
					float nbRessource = part.parent.RequestResource("ScienceSlot", 1);
					print("[MERILL]MerillScienceSlotModule : nbRessource is " + nbRessource);
					if (nbRessource == 1)
					{
						hasASlot = true;
						//add gui & action
						reEnable();
					}
					slotResearched = true;
				}
			}
			else if (hasASlot && !slotResearched)
			{
				reEnable();
				slotResearched = true;
			}
		}

		private void reEnable()
		{
			//re-enable
			if (partModuleToEnable.moduleName.Equals("MerillModuleScienceFail"))
			{
				if (((MerillModuleScienceFail)partModuleToEnable).isInactive)
				{
					((MerillModuleScienceFail)partModuleToEnable).isInactive = false;
					BaseField merillShowState = partModuleToEnable.Fields["stateDisplay"];
					//TODO verify electricity
					merillShowState.SetValue("Working", merillShowState.host);
				}
			}
			print("[MERILL]MerillScienceSlotModule : eventToEnable '" + eventToEnable + "', '" + actionToEnable + "'");
			if (eventToEnable != null)
			{
				eventToEnable.guiActive = true;
				eventToEnable.guiActiveEditor = true;
				print("[MERILL]MerillScienceSlotModule : reenable '" + eventToEnable + "'");
			}
			if (actionToEnable != null)
			{
				actionToEnable.active = true;
				print("[MERILL]MerillScienceSlotModule : reenable '" + actionToEnable + "'");
			}
		}


	}
}

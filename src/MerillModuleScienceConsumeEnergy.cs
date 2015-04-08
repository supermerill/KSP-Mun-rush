using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KspMerillEngineFail
{
	//change name for display into vab
	[KSPModule("Experiment monitoring")]
	public class MerillModuleScienceConsumeEnergy : PartModule
	{

		[KSPField]
		public bool isRunning = true;

		[KSPField(isPersistant = false)]
		public float consumptionPerSecond = 1f;

		// a simple buffer to buffer the energy to consume (it's a negative field)
		public double energyBuffer = 0;

		[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Energy consumption:")]
		public string energyConsumptionDisplay = "0/s";

		//TODO: add action?
		[KSPEvent(guiActive = true, guiActiveEditor = false, guiName = "Shutdown")]
		public void shutdownEvent()
		{
			print("[MERILL]consumeEnergy shutdown!" );
			shutdownExperiment();
		}

		public override void OnStart(StartState state)
		{
			base.OnStart(state);

			print("[MERILL]consumeEnergy start : " + part.name + " " + isRunning + ", " + consumptionPerSecond);
			if(!isRunning)
			{
				print("[MERILL]consumeEnergy shudown !");
				shutdownExperiment();
				energyConsumptionDisplay = "0 per s";
			}
			else
			{
				energyConsumptionDisplay = (consumptionPerSecond < 60 ? consumptionPerSecond * 60 : consumptionPerSecond).ToString("F2")
					+ (consumptionPerSecond < 60 ? " per min" : "per sec");
			}

		}

		public virtual void FixedUpdate()
		{
			//print("[MERILL]consumeEnergy OnFixedUpdate : " + consumptionPerSecond +", "+ isRunning);
			if (consumptionPerSecond > 0 && isRunning)
			{ 
					//compute delta - using TimeWarp.fixedDeltaTime instead
					//double delta = part.vessel.missionTime - lastUpdateTime;

					//consume energy
					
					double consumption = this.consumptionPerSecond * TimeWarp.fixedDeltaTime;
					double received = part.RequestResource("ElectricCharge", consumption + this.energyBuffer);


					if (received < (consumption + this.energyBuffer) * 0.8)
					{
						//fail
						shutdownExperiment();
						//message to user
						ScreenMessages.PostScreenMessage("Experiment " + part.partInfo.title.ToString()
								+ " has not enough energy to maintain integrity. It's now shutdown."
								, 10f, ScreenMessageStyle.UPPER_LEFT);

					}
					else
					{
						this.energyBuffer = consumption - received;
					}
				}
		}

		private void shutdownExperiment()
		{
			isRunning = false;

			//update this gui
			Events["shutdownEvent"].guiActive = false;
			energyConsumptionDisplay = "0 per s";

			//update others ui
			foreach (PartModule pm in part.Modules)
			{
				print("[MERILL]consumeEnergy start, find module " + pm.moduleName);

				for (int i = 0; i < pm.Events.Count; i++)
				{
					if (pm.Events.GetByIndex(i) == null)
						print("[MERILL]consumeEnergy see Events[" + i + "] is null");
					else
						print("[MERILL]consumeEnergy see event '" + pm.Events.GetByIndex(i).name + "'");
				}
				for (int i = 0; i < pm.Fields.Count; i++)
				{
					if (pm.Fields[i] == null)
						print("[MERILL]consumeEnergy see field[" + i + "] is null");
					else
					print("[MERILL]consumeEnergy see field '" + pm.Fields[i].name + "'");
				}
				//update my mod, you can add others below
				if (pm.moduleName.Equals("MerillModuleScienceFail"))
				{
					BaseEvent merillScienceEvent = pm.Events["MERILLScienceEventDeployOrTest"];
					BaseAction merillScienceAction = pm.Actions["MERILLScienceActionDeployOrTest"];
					if (merillScienceEvent != null && merillScienceAction != null)
					{
						merillScienceEvent.guiActive = false;
						merillScienceEvent.active = false;
						print("[MERILL]consumeEnergy check field '" + pm.Fields["stateDisplay"] + "'");
						BaseField merillShowState = pm.Fields["stateDisplay"];
						merillShowState.SetValue("Dead", merillShowState.host);
						print("[MERILL]consumeEnergy field set '" + merillShowState.GetValue(merillShowState.host) + "'");
					}
				}
				//update toogle vanilla
				if (pm.moduleName.Equals("ModuleEnviroSensor"))
				{
					pm.Events["Toggle"].guiActive = false;
				}
			}
		}



		public override string GetInfo()
		{
			return "<b><color=#99ff00ff>Requires:</color></b>\n"
					+ "- <b>Electric Charge: </b>" 
					+ (consumptionPerSecond<60?consumptionPerSecond*60:consumptionPerSecond).ToString("F2")
					+ (consumptionPerSecond<60?" per min":"per sec")+"\n"
					+ "\n<b>WARNING: Requires constant power to avoid damage</b>\n";
		}

		// TODO: background processing 

	}
}

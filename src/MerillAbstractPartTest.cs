using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KspMerillEngineFail
{
	public class MerillAbstractPartTest : PartModule
	{
		static	protected System.Random aleat = new System.Random();

		[KSPField]
		public bool loaded = false;

		public override void OnUpdate()
		{
			//get scenario-wide varaible on part creation.
			if (!loaded)
			{
				if (MerillData.instance != null)
				{
					loadDataFromScenario(MerillData.instance);
					loaded = true;
				}
			}
			if (isActivated())
			{
				//TODO: disable testing if in kct simulation.
				partTest();
			}
		}

		//called one time in a game sequence, on a OnUpdate()
		public virtual void loadDataFromScenario(MerillData scenario)
		{

		}

		//need to return true to run partTest()
		public virtual bool isActivated()
		{
			return false;
		}

		//test run, call on OnUpdate() when isActivated()
		public virtual void partTest()
		{

		}

		//test run, call on OnUpdate() when isActivated()
		public virtual void recomputeInfoMsg()
		{
			print("|MERILL]Abs part test : recompute info");
		}

		public bool isAtmo()
		{
			return FlightGlobals.getStaticPressure() > 0;
		}

		//return true if test is ok.
		public bool doTest(bool hasCrashed, bool hasPassAtmo)
		{
			float chance = 2 + 75f / ((((float)MerillData.instance.partNameTested.Count) / 8f) + 1);
			//MerillData.log(" chance= 2 + 75f / ("+(((float)MerillData.instance.partNameTested.Count) / 8f) +"+ 1)");
			if ((isAtmo() && hasPassAtmo)
				|| (!isAtmo() && hasCrashed))
			{
				chance = chance / 2;
			}
			if (!isAtmo() && hasCrashed)
			{
				chance = chance / 2;
			}

			// luck?
			int peudoAleat = MerillData.instance.get0to99FromNotAleatTable("test_"+part.name);

			print("[MERILL][Mun Rush] test part "+part.name+" : " + peudoAleat + " ?> " + chance);

			return peudoAleat > chance;
		}

		protected bool useInstrumentation()
		{
			//MerillData.log("useInstrumentation vessel conrollable? HasControlSources:" + vessel.HasControlSources()
			//	+ ", comm:" + vessel.isCommandable + ",  ctrl:" + vessel.IsControllable + ", act:" + vessel.isActiveVessel);

			if (vessel.IsControllable)
			{
				//MerillData.log("useInstrumentation vessel conrollable! " + vessel.Parts.Count+", "+part.vessel.Parts.Count+", "+vessel.parts);
				//vessel.getmforeach (Part part in v.Parts)
				foreach (Part p in vessel.parts)
				{
					foreach (PartModule module in p.Modules)
					{
						//MerillData.log("useInstrumentation see module " + module.moduleName+", "+module.ClassName+", "
						//	+module.GetType().IsSubclassOf(typeof(ModuleDataTransmitter)));
						if (module.GetType().IsSubclassOf(typeof(ModuleDataTransmitter)) && ((ModuleDataTransmitter)module).CanTransmit())
						{

							//TODO: check for engineer presence to reduce usage of instrumentation (oO) (/10?)
							float ressource = part.RequestResource("TestingInstrument", 1f);
							//MerillData.log("useInstrumentation TestingInstrument " + ressource);
							//ok, ya une antenne
							if (ressource == 1f)
							{
								//ok, resource consumed
								return true;
							}
						}
					}
				}
			}
			return false;
		}

		public static void drawMsgToUser(string message)
		{
			ScreenMessages.PostScreenMessage(message, 10F, ScreenMessageStyle.UPPER_CENTER);
		}

		public static void drawMsgToUser(string message, int nbSec)
		{
			ScreenMessages.PostScreenMessage(message, nbSec, ScreenMessageStyle.UPPER_CENTER);
		}


	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KspMerillEngineFail
{
	[KSPModule("Engine Test")]
	public class MerillEngineTestAtmoOnly : MerillAbstractPartTest
	{
		protected ModuleEngines engine;
		protected ModuleEnginesFX engineFX;

		//TODO: better handling of atmo->vacuum border ?
		//TODO: timeignited: use total burn time, not time since last restart.

		[KSPField]
		public double timeIgnitedLaunch;

		[KSPField]
		public double timeIgnitedMission;

		[KSPField]
		public double nextDurationToCheck;

		[KSPField]
		public int previousBurnTime = 0;

		[KSPField]
		public int maxIgniter;

		[KSPField]
		public int nbRestart;

		[KSPField]
		public bool wait4secRestart;

		[KSPField]
		public bool runningLastUpdate;

		[KSPField]
		public bool isLogingTest;

		// field from previous flight, do not update
		[KSPField]
		public int maxNBSecBurnAtmo = 0;

		// min 4 sec pour etre valide
		// field from previous flight, do not update
		[KSPField]
		public int maxNBRestartAtmo = 0;

		[KSPField(isPersistant = false, guiActive = true, guiActiveEditor = false, guiName = "Tested startup")]
		public string nbRestartDisplay = "Tested for: 0 start";

		[KSPField(isPersistant = false, guiActive = true, guiActiveEditor = false, guiName = "Tested burn", guiUnits = "s")]
		public string burnTimeDisplay = "Tested for: 0 sec";

		public override void OnStart(StartState state)
		{
			base.OnStart(state);

			if (part.Modules.OfType<ModuleEngines>().ToList().Capacity > 0)
			{
				engine = part.Modules.OfType<ModuleEngines>().ToList()[0];
			}
			if (part.Modules.OfType<ModuleEnginesFX>().ToList().Capacity > 0)
			{
				engineFX = part.Modules.OfType<ModuleEnginesFX>().ToList()[0];
			}
		}

		public override string GetInfo()
		{
			//not a real part module here & can't access to MerillData (isolation?)

			//string sDescString = "<b>Tested startup:</b>\n" +
			//	"- Atmosphere: " + this.maxNBRestartAtmo + "\n" +
			//	"- Vacuum: " + this.maxNBRestartVac
			//	+ "\n<b>Tested Running:</b>\n" +
			//	"- Atmosphere: " + this.maxNBSecBurnAtmo + " sec\n" +
			//	"- Vacuum: " + this.maxNBSecBurnVac + "sec\n";

			return "See a working thruster within a simutation for details";
		}

		//test run, call on OnUpdate() when isActivated()
		public override void recomputeInfoMsg()
		{

			print("|MERILL]enginetestAO : recompute info with data " + MerillData.instance);
			loadDataFromScenario(MerillData.instance);
			string sDescString = "";//<b>Tested startup:</b>\n";
			if (maxIgniter > 0)
			{
				sDescString += "" + this.maxNBRestartAtmo + " start" +
					", " + this.maxNBSecBurnAtmo + " sec" ;
			}
			else { 
			sDescString += "" + this.maxNBSecBurnAtmo + " sec";
			}
			foreach (AvailablePart.ModuleInfo minf in part.partInfo.moduleInfos)
			{
				if (minf.moduleName == "Engine Test")
				{
					//print("|MERILL]enginetest : recompute info:, find my moduleInfo " + minf.moduleName);
					minf.info = sDescString;
				}
			}
		}

		public override void loadDataFromScenario(MerillData scenario)
		{
			//print("[merill]loadDataFromScenario : " + scenario);
			if (!scenario.maxNBSecBurn.ContainsKey(part.name + "Atmo"))
			{
				scenario.maxNBSecBurn[part.name + "Atmo"] = 6;
			}
			maxNBSecBurnAtmo = scenario.maxNBSecBurn[part.name + "Atmo"];

			if (!scenario.nbRestart.ContainsKey(part.name + "Atmo"))
			{
				//print("[merill]loadDataFromScenario maxNBRestartAtmo is not present ");
				scenario.nbRestart[part.name + "Atmo"] = 0;
			}
			maxNBRestartAtmo = scenario.nbRestart[part.name + "Atmo"];

			nbRestartDisplay = "Tested for: " + maxNBRestartAtmo;
			burnTimeDisplay = "Tested for: " + maxNBSecBurnAtmo;

			//init these var only when ship start, after that it's via KSPField
			nbRestart = 0;
			runningLastUpdate = false;
			isLogingTest = false;
			wait4secRestart = false;
			nextDurationToCheck = 0;
			//check if igniter is needed
			maxIgniter = 0;
			//print("[merill]loadDataFromScenario : '" + part + "'");
			//print("[merill]loadDataFromScenario : '" + part.Resources + "'");
			foreach (PartResource resource in part.Resources)
			{
				//print("[MERILL] enginetest " + part.name + " has " + resource.maxAmount + " " + resource.resourceName);
				maxIgniter += resource.resourceName.Equals("EngineIgniter") ? (int)resource.maxAmount : 0;
			}
			if (maxIgniter == 0)
			{
				BaseField merilShowNbRestart = base.Fields["nbRestartDisplay"];
				if (merilShowNbRestart != null)
				{
					merilShowNbRestart.guiActive = false;
				}
			}
			//print("[MERILL] enginetest " + part.name + " has " + maxIgniter + " igniter");
		}

		public override bool isActivated()
		{
			if (engine != null)
			{
				return ((engine.EngineIgnited && engine.currentThrottle > 0 && engine.fuelFlowGui > 0) || runningLastUpdate);
			}
			return ((engineFX.EngineIgnited && engineFX.currentThrottle > 0 && engineFX.fuelFlowGui > 0) || runningLastUpdate);
		}

		protected int burnTime()
		{
			return previousBurnTime + (int)Math.Max(part.vessel.launchTime - timeIgnitedLaunch, part.vessel.missionTime - timeIgnitedMission);
		}

		public bool isEngineShutdown()
		{
			if (engine != null)
			{
				return !engine.EngineIgnited || engine.currentThrottle == 0 || engine.fuelFlowGui == 0;
			}
			if (engineFX != null)
			{
				return !engineFX.EngineIgnited || engineFX.currentThrottle == 0 || engineFX.fuelFlowGui == 0;
			}
			return false;
		}
		public bool isEngineRunning()
		{
			if (engine != null)
			{
				return engine.EngineIgnited && engine.currentThrottle > 0 && engine.fuelFlowGui > 0;
			}
			if (engineFX != null)
			{
				return engineFX.EngineIgnited && engineFX.currentThrottle > 0 && engineFX.fuelFlowGui > 0;
			}
			return false;
		}

		public override void partTest()
		{
			//check previous state
			if (runningLastUpdate)
			{
				//print("[MERILL]enginetestAO " + part.name + " running "+burnTime());

				//check restart & if duration is enough ( 4 sec )
				if (wait4secRestart && burnTime() > 4)
				{
					wait4secRestart = false;
					//print("[MERILL] enginetest " + part.name + " CHECK RESTART before: " + nbRestart);
					nbRestart++;
					//print("[MERILL] enginetest " + part.name + " CHECK RESTART after: " + nbRestart);
					checkRestart();
				}

				//shutdown occur?
				//print("[MERILL] enginetest " + part.name + " is online? th=" + engine.finalThrust+", ff="+engine.fuelFlowGui
				//	+ ", " + engine.EngineIgnited + ", " + engine.currentThrottle+", "+engine.status+", "+engine.statusL2);
				//  th=92.83253, ff=0.03040846, True, 0.5, Nominal, 
				if (isEngineShutdown())
				{
					//test shutdown
					testShutdown();
					//maj burn time
					previousBurnTime = burnTime();
				}
				else
				{
					//print("[MERILL] enginetest " + part.name + " check flight duration");
					checkFlightDuration();
				}

			}
			//startup occur?
			else if (isEngineRunning())
			{
				//print("[MERILL]enginetestAO " + part.name + " startup " + maxIgniter);
				runningLastUpdate = true;
				timeIgnitedLaunch = part.vessel.launchTime;
				timeIgnitedMission = part.vessel.missionTime;

				//check if ingiter is needed
				if (maxIgniter > 0)
				{
					//print("[MERILL]enginetestAO " + part.name + " restart to check in 4sec " + nbRestart + " =?= "
					//	+ MerillData.instance.nbRestart[part.name + "Atmo"] +"="+this.maxNBRestartAtmo
					//	+ " , " + (part.name + "Atmo"));
					if (nbRestart == MerillData.instance.nbRestart[part.name + "Atmo"])
					{
						isLogingTest = isLogingTest || useInstrumentation();

						//ScreenMessages.PostScreenMessage("Part " + part.partInfo.title.ToString()
						//	+ " restart is on survey. Please let it burn 4 sec to log the needed data."
						//	, 4f, ScreenMessageStyle.UPPER_CENTER);
						drawMsgToUser(string.Format(MerillData.str_enginetest_restart_begin,
									part.partInfo.title.ToString()), 4);

						//prepare to test restart
						wait4secRestart = true;
					}

					//if not a test and outer the limits => can fail immediatly
					if (!isLogingTest && nbRestart >= MerillData.instance.nbRestart[part.name +  "Atmo"])
					{
						//check the restart to see if i need to explode the part!
						checkRestart();
					}

				}

			}
		}

		private void testShutdown()
		{
			print("[MERILL] enginetest " + part.name + " shutdown at " +
				part.vessel.launchTime + "-" + timeIgnitedLaunch + " => "
				+ burnTime() + " != " + part.vessel.missionTime + " into " +  "atmo");
			runningLastUpdate = false;
			wait4secRestart = false;
			if (isLogingTest)
			{

				//log max burn time & test engine for the little duration
				testEngineDuration();

			}
				//? possible?
			else if (burnTime() > maxNBSecBurnAtmo)
			{
				// in testing mode?
				if (useInstrumentation())
				{
					//we can test this engine to the last bit of possible.
					updateSafeBurnTime();

					//ScreenMessages.PostScreenMessage("Part " + part.partInfo.title.ToString()
					//+ " is  successfully tested in "
					//+ (atmo ? "atmosphere" : "vaccum") + " for a duration of "
					//+ ((uint)(burnTime())) + " sec."
					//, 10f, ScreenMessageStyle.UPPER_CENTER);

					drawMsgToUser(string.Format(MerillData.str_enginetest_duration_successOK,
								part.partInfo.title.ToString(),
								"",
								(uint)(burnTime())));
				}
			}
			isLogingTest = false;
		}

		private void checkRestart()
		{

			// check if the number of ignition is not too high for the engine.
			//  with this trick, payer can use igniter-depleted engine as cheap ineficient altitude thruster.
			//if (nbRestart > maxIgniter)
			//{
			//	return;
			//}
			///oh, let it explode, it's fun (some leftover igniter make some nasty explosion!).

			//print("[MERILL] enginetest " + part.name + " restart: " + nbRestart + " ? "
			//	+ MerillData.instance.nbRestart[part.name + (atmo ? "Atmo" : "Vac")] + " into " + (atmo ? "atmo" : "vac"));
			//need to test?
			if (nbRestart > MerillData.instance.nbRestart[part.name + "Atmo"])
			{
				//consume 1 resource & verify antenna.. +emit messageexplode
				isLogingTest = isLogingTest | useInstrumentation();
				//print("[MERILL] enginetest " + part.name + " restart useInstrumentation: " + isLogingTest);

				//make test roll
				bool testSuccess = false;
				// hard test in ground
				testSuccess = doTest(false, false);

				//send message & explode
				if (!testSuccess)
				{
					explodePart();
					MerillData.instance.nbPartDestroy++;


					//ScreenMessages.PostScreenMessage("Part " + part.partInfo.title.ToString()
					//		 + " has failed in " + (atmo ? "atmosphere" : "vaccum") + " for the restart n°" 
					//		 + nbRestart + ". \n"
					//		 + (isLogingTest
					//			 ? "Logging data sended, you can now use this engine safely for this number of restart next time. "
					//				+" Trust your mighty kerbal engineers!"
					//			 : "No testing intrument found / no communication device to send useful log to your engineers.")
					//		 , 10f, ScreenMessageStyle.UPPER_CENTER);
					if (isLogingTest)
					{
						drawMsgToUser(string.Format(MerillData.str_enginetest_restart_failOK,
								part.partInfo.title.ToString(),
								"" ,
								nbRestart));
					}
					else
					{
						drawMsgToUser(string.Format(MerillData.str_enginetest_restart_failKO,
									part.partInfo.title.ToString(),
									"",
									nbRestart));
					}
				}
				else
				{
					//ScreenMessages.PostScreenMessage("Part " + part.partInfo.title.ToString()
					//	+ ""
					//	+ (isLogingTest
					//		? " is successfully tested in "
					//		: " has not failed ")
					//	+ (atmo ? "atmosphere" : "vaccum") + " for the restart n°"
					//		 + nbRestart
					//		 + (isLogingTest
					//		? "."
					//		: " but be careful, we don't know why it work so well!")
					//	, 10f, ScreenMessageStyle.UPPER_CENTER);
					if (isLogingTest)
					{
						drawMsgToUser(string.Format(MerillData.str_enginetest_restart_successOK,
									part.partInfo.title.ToString(),
									"",
									nbRestart));
					}
					else
					{
						drawMsgToUser(string.Format(MerillData.str_enginetest_restart_successKO,
									part.partInfo.title.ToString(),
									"",
									nbRestart));
					}
				}

				//log test data into the test parts
				//print("[MERILL] enginetest " + part.name + " restart logging?: " + isLogingTest);
				if (isLogingTest)
				{
					if (nbRestart == 1)
					{
						//maj overall sucess/failure
						if (testSuccess)
						{
							MerillData.instance.nbPartTested++;
							if (!MerillData.instance.partNameTested.Contains(part.name + "StartAtmo"))
								MerillData.instance.partNameTested.Add(part.name + "StartAtmo");
						}
						else
						{
							if (!MerillData.instance.partNameCrashed.Contains(part.name + "StartAtmo"))
								MerillData.instance.partNameCrashed.Add(part.name + "StartAtmo");
						}
					}
					else //put tested if second/third try => first is so a success
						if (nbRestart == 2)
						{
							MerillData.instance.nbPartTested++;
							if (!MerillData.instance.partNameTested.Contains(part.name + "StartAtmo"))
								MerillData.instance.partNameTested.Add(part.name + "StartAtmo");
						}

					//test nbStartup
					//success! => update test data for this engine
					maxNBRestartAtmo = Math.Max(MerillData.instance.nbRestart[part.name + "Atmo"],
						((int)(nbRestart)));
					MerillData.instance.nbRestart[part.name + "Atmo"] = maxNBRestartAtmo;
					//print("[MERILL] enginetest " + part.name + " restart nbRetart Atmo test: " + MerillData.instance.nbRestart[part.name + "Atmo"]);

					nbRestartDisplay = "Tested for: " + maxNBRestartAtmo+" start";

					//consume the instrument token.
					isLogingTest = false;

					if (!testSuccess)
					{
						//now it's shutdown ^^
						testShutdown();
					}
					recomputeInfoMsg();
				}
			}
		}

		//TODO: separate ">50% thrust" and "sometimes <50% trust"
		private void checkFlightDuration()
		{
			//check if we have to do a duration test = (10 * 2^X) [ when (byte)X change => test ] + some random
			if (burnTime() > maxNBSecBurnAtmo)
			{
				if (nextDurationToCheck == 0)
				{
					//init
					nextDurationToCheck = Math.Max((7d + aleat.NextDouble() * 6f), maxNBSecBurnAtmo * (1.5d + aleat.NextDouble() / 2));


					print("[MERILL] enginetest  init duration test to " + nextDurationToCheck);

					// use instrumentation (if not in use)
					isLogingTest = isLogingTest || useInstrumentation();

					if (isLogingTest)
					{
						//ScreenMessages.PostScreenMessage("Part " + part.partInfo.title.ToString()
						//	+ " is now instrumented in " + (atmo ? "atmosphere" : "vaccum") + " for a long burn test."
						//	, 3f, ScreenMessageStyle.UPPER_CENTER);
						drawMsgToUser(string.Format(MerillData.str_enginetest_duration_begin,
									part.partInfo.title.ToString(),
									"")
									, 3);

					}
				}
				else if (burnTime() > nextDurationToCheck)
				{
					//test
					testEngineDuration();

					//compute next
					nextDurationToCheck = maxNBSecBurnAtmo * (1.5d + aleat.NextDouble() / 2);

				}
			}
		}

		protected void testEngineDuration()
		{
			//do test


			//that code is deprectated, it's for computing with exactly (10 * 2^X) time between test
			//some random are better
			//uint expBefore = ((uint)(atmo ? maxNBSecBurnAtmo : maxNBSecBurnVac)) / 10;
			//expBefore = log2(expBefore);
			uint expAfter = ((uint)(burnTime())) / 10;
			expAfter = log2(expAfter);
			//if (expAfter > expBefore)
			//{
			//print("[MERILL] enginetest " + part.name + " duration expBefore: " + expBefore
			//	+ ", expAfter " + expAfter);

			//print("[MERILL] enginetest " + part.name + (atmo ? "atmo" : "vac") + " duration before: " + (atmo ? maxNBSecBurnAtmo : maxNBSecBurnVac)
			//	+ ", after " + nextDurationToCheck);

			//need to test
			// use instrumentation (if not in use)
			isLogingTest = isLogingTest || useInstrumentation();

			//make test roll
			bool testSuccess = false;
			
			// hard test in ground
			testSuccess = doTest(false, false);

			//message to user + explode if necessary
			if (!testSuccess)
			{
				//fail
				explodePart();
				//ScreenMessages.PostScreenMessage("Part " + part.partInfo.title.ToString()
				//	+ " has failed in " + (atmo ? "atmosphere" : "vaccum") + " for a duration of "
				//	+ ((uint)(burnTime())) + " sec.\n"
				//	+ (isLogingTest
				//		? "Logging data sended, you can use this engine for this duration, but if you plan to much longer "
				//			+ "use, you may want to do an other test."
				//		: "No testing intrument found / no communication device to send log to your engineers.")
				//	, 10f, ScreenMessageStyle.UPPER_CENTER);
				if (isLogingTest)
				{
					drawMsgToUser(string.Format(MerillData.str_enginetest_duration_failOK,
								part.partInfo.title.ToString(),
								"",
								(uint)(burnTime())));
				}
				else
				{
					drawMsgToUser(string.Format(MerillData.str_enginetest_duration_failKO,
								part.partInfo.title.ToString(),
								"",
								(uint)(burnTime())));
				}
			}
			else
			{

				//ScreenMessages.PostScreenMessage("Part " + part.partInfo.title.ToString()
				//	+ " is "
				//	+ (isLogingTest
				//		? " successfully tested in "
				//		: " now running for the longest duration since his invention, and without testing instrumentation, "
				//			+ "beware the kraken!\nRunning in ")
				//	+ (atmo ? "atmosphere" : "vaccum") + " for a duration of "
				//	+ ((uint)(burnTime())) + " sec."
				//	, 10f, ScreenMessageStyle.UPPER_CENTER);
				if (isLogingTest)
				{
					drawMsgToUser(string.Format(MerillData.str_enginetest_duration_successOK,
								part.partInfo.title.ToString(),
								"",
								(uint)(burnTime())));
				}
				else
				{
					drawMsgToUser(string.Format(MerillData.str_enginetest_duration_successKO,
								part.partInfo.title.ToString(),
								"",
								(uint)(burnTime())));
				}
			}

			//if logging, count this longer burn duration validated
			if (isLogingTest)
			{
				//if duration is "correct" ?20->40s?
				if (expAfter == 2)
				{
					//maj overall sucess/failure
					if (testSuccess)
					{
						MerillData.instance.nbPartTested++;
						if (!MerillData.instance.partNameTested.Contains(part.name + "DurAtmo" ))
							MerillData.instance.partNameTested.Add(part.name + "DurAtmo");
					}
					else
					{
						if (!MerillData.instance.partNameCrashed.Contains(part.name + "DurAtmo"))
							MerillData.instance.partNameCrashed.Add(part.name + "DurAtmo");
					}
				}
				else //put tested if longer try => first is so a success
					if (expAfter == 3)
						if (!MerillData.instance.partNameTested.Contains(part.name + "DurAtmo"))
							MerillData.instance.partNameTested.Add(part.name + "DurAtmo");


				//success! => update test data for this engine
				updateSafeBurnTime();

				//consume the instrument token.
				isLogingTest = false;
			}
		}

		protected void updateSafeBurnTime()
		{
			
			maxNBSecBurnAtmo =
				Math.Max(MerillData.instance.maxNBSecBurn[part.name + "Atmo"],
				burnTime());
			MerillData.instance.maxNBSecBurn[part.name + "Atmo"] = maxNBSecBurnAtmo;

			burnTimeDisplay = "Tested for: " + maxNBSecBurnAtmo+" sec";
			recomputeInfoMsg();
		}

		//not really log2, as 0=>0
		private byte log2(uint i)
		{
			if (i == 0) return 0;
			byte b = 1;
			while (i >> b != 0)
			{
				b++;
			}
			return b;
		}

		private void explodePart()
		{
			//refund (waranty)
			print("[MERILL]enginetestAtm : get cost: " + part.partInfo.cost);
			float cost = part.partInfo.cost;
			if (cost > 0)
			{
				print("[MERILL]enginetestAtm : add fund: " + cost);
				Funding.Instance.AddFunds(cost, TransactionReasons.VesselLoss);
			}
			//explode
			part.explode();
		}

	}
}

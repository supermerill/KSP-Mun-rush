using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KspMerillEngineFail
{
	[KSPModule("Engine Test")]
	public class MerillEngineTest : MerillAbstractPartTest
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

		//TODO: burntimeatmo/vac like in merillEngineTestAtmo, cf tag BURNTIME

		[KSPField]
		public bool isFlyingAtmo;

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

		// field from previous flight, do not update
		[KSPField]
		public int maxNBSecBurnVac = 0;
		//int maxNBSecBurnAtmoAfterRestart = 0;
		//int maxNBSecBurnVacAfterRestart = 0;
		//int maxNBSecBurnAtmoBeforeRestart = 0;
		//int maxNBSecBurnVacBeforeRestart = 0;

		// min 4 sec pour etre valide
		// field from previous flight, do not update
		[KSPField]
		public int maxNBRestartAtmo = 0;

		// field from previous flight, do not update
		[KSPField]
		public int maxNBRestartVac = 0;

		[KSPField(isPersistant = false, guiActive = true, guiActiveEditor = false, guiName = "Tested startup")]
		public string nbRestartDisplay = "Atm: 0 / Vac: 0";

		[KSPField(isPersistant = false, guiActive = true, guiActiveEditor = false, guiName = "Tested burn", guiUnits = "s")]
		public string burnTimeDisplay = "Atm: 0 / Vac: 0";

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
			//init
			if ((state & (StartState.Editor | StartState.None)) == 0)
			{
				isFlyingAtmo = isAtmo();
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

			//print("|MERILL]enginetest : recompute info with data " + MerillData.instance);
			loadDataFromScenario(MerillData.instance);
			string sDescString = "";//<b>Tested startup:</b>\n";
			if (maxIgniter > 0)
			{
				sDescString += "Atmosphere: " + this.maxNBRestartAtmo + " start" +
					", " + this.maxNBSecBurnAtmo + " sec\n" +
					"Vacuum: " + this.maxNBRestartVac + " start" +
					", " + this.maxNBSecBurnVac + " sec" ;
			}
			else { 
			sDescString += //"<b>Tested Running:</b>\n" +
				"Atmosphere: " + this.maxNBSecBurnAtmo + " sec\n" +
				"Vacuum: " + this.maxNBSecBurnVac + "sec";
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
			//print("[merill]loadDataFromScenario maxNBSecBurnVac: " + maxNBSecBurnAtmo);

			if (!scenario.maxNBSecBurn.ContainsKey(part.name + "Vac"))
			{
				scenario.maxNBSecBurn[part.name + "Vac"] = 6;
			}
			maxNBSecBurnVac = scenario.maxNBSecBurn[part.name + "Vac"];
			//print("[merill]loadDataFromScenario maxNBSecBurnVac: " + maxNBSecBurnVac);

			if (!scenario.nbRestart.ContainsKey(part.name + "Atmo"))
			{
				//print("[merill]loadDataFromScenario maxNBRestartAtmo is not present ");
				scenario.nbRestart[part.name + "Atmo"] = 0;
			}
			maxNBRestartAtmo = scenario.nbRestart[part.name + "Atmo"];
			//print("[merill]loadDataFromScenario maxNBRestartAtmo: " + maxNBRestartAtmo);

			if (!scenario.nbRestart.ContainsKey(part.name + "Vac"))
			{
				scenario.nbRestart[part.name + "Vac"] = 0;
			}
			maxNBRestartVac = scenario.nbRestart[part.name + "Vac"];
			//print("[merill]loadDataFromScenario maxNBRestartVac: " + maxNBRestartVac);

			nbRestartDisplay = "Atm: " + maxNBRestartAtmo + " / Vac: " + maxNBRestartVac;
			burnTimeDisplay = "Atm: " + maxNBSecBurnAtmo + " / Vac: " + maxNBSecBurnVac;

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
			//TODO: BURNTIME => add +burntime{atmo|vac}
			return (int)Math.Max(part.vessel.launchTime - timeIgnitedLaunch, part.vessel.missionTime - timeIgnitedMission);
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
				//print("[MERILL] enginetest " + part.name+" running");

				//check restart & if duration is enough ( 4 sec )
				if (wait4secRestart && burnTime() > 4)
				{
					wait4secRestart = false;
					//print("[MERILL] enginetest " + part.name + " CHECK RESTART before: " + nbRestart);
					nbRestart++;
					//print("[MERILL] enginetest " + part.name + " CHECK RESTART after: " + nbRestart);
					checkRestart(isAtmo());
				}

				//shutdown occur?
				//print("[MERILL] enginetest " + part.name + " is online? th=" + engine.finalThrust+", ff="+engine.fuelFlowGui
				//	+ ", " + engine.EngineIgnited + ", " + engine.currentThrottle+", "+engine.status+", "+engine.statusL2);
				//  th=92.83253, ff=0.03040846, True, 0.5, Nominal, 
				if (isEngineShutdown())
				{
					testShutdown(isAtmo());
					//TODO: BURNTIME => maj burntime{atmo|vac} += burnTime()
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
				//print("[MERILL] enginetest " + part.name + " startup " + maxIgniter);
				runningLastUpdate = true;
				timeIgnitedLaunch = part.vessel.launchTime;
				timeIgnitedMission = part.vessel.missionTime;

				//check if ingiter is needed
				if (maxIgniter > 0)
				{
					//print("[MERILL] enginetest " + part.name + " restart to check in 4sec " + nbRestart + " =?= "
					//	+ MerillData.instance.nbRestart[part.name + (isAtmo() ? "Atmo" : "Vac")] + " , " + (part.name + (isAtmo() ? "Atmo" : "Vac")));
					if (nbRestart == MerillData.instance.nbRestart[part.name + (isAtmo() ? "Atmo" : "Vac")])
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
					if (!isLogingTest && nbRestart >= MerillData.instance.nbRestart[part.name + (isAtmo() ? "Atmo" : "Vac")])
					{
						//check the restart to see if i need to explode the part!
						checkRestart(isAtmo());
					}

				}

			}
		}

		private void testShutdown(bool atmo)
		{
			//print("[MERILL] enginetest " + part.name + " shutdown at " +
			//	part.vessel.launchTime + "-" + timeIgnitedLaunch + " => "
			//	+ burnTime() + " != " + part.vessel.missionTime + " into " + (atmo ? "atmo" : "vac"));
			runningLastUpdate = false;
			wait4secRestart = false;
			if (isLogingTest)
			{

				//log max burn time & test engine for the little duration
				testEngineDuration(atmo);

			}
			else if (burnTime() > (atmo ? maxNBSecBurnAtmo : maxNBSecBurnVac))
			{
				// in testing mode?
				if (useInstrumentation())
				{
					//we can test this engine to the last bit of possible.
					updateSafeBurnTime(atmo);

					//ScreenMessages.PostScreenMessage("Part " + part.partInfo.title.ToString()
					//+ " is  successfully tested in "
					//+ (atmo ? "atmosphere" : "vaccum") + " for a duration of "
					//+ ((uint)(burnTime())) + " sec."
					//, 10f, ScreenMessageStyle.UPPER_CENTER);

					drawMsgToUser(string.Format(MerillData.str_enginetest_duration_successOK,
								part.partInfo.title.ToString(),
								(atmo ? MerillData.str_atmo : MerillData.str_vac),
								(uint)(burnTime())));
				}
			}
			isLogingTest = false;
		}

		private void checkRestart(bool atmo)
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
			if (nbRestart > MerillData.instance.nbRestart[part.name + (atmo ? "Atmo" : "Vac")])
			{
				//consume 1 resource & verify antenna.. +emit message
				isLogingTest = isLogingTest | useInstrumentation();
				//print("[MERILL] enginetest " + part.name + " restart useInstrumentation: " + isLogingTest);

				//make test roll
				bool testSuccess = false;
				if (atmo)
				{
					// hard test in ground
					testSuccess = doTest(false, false);
				}
				else
				{
					// 2 time better in space if work done at ground
					testSuccess = doTest(false, nbRestart <= MerillData.instance.nbRestart[part.name + "Atmo"]);
				}

				//send message & explode
				if (!testSuccess)
				{
					part.explode();
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
								"in "+(atmo ? MerillData.str_atmo : MerillData.str_vac),
								nbRestart));
					}
					else
					{
						drawMsgToUser(string.Format(MerillData.str_enginetest_restart_failKO,
									part.partInfo.title.ToString(),
									"in " + (atmo ? MerillData.str_atmo : MerillData.str_vac),
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
									//(atmo ? MerillData.str_atmo : MerillData.str_vac),
									nbRestart));
					}
					else
					{
						drawMsgToUser(string.Format(MerillData.str_enginetest_restart_successKO,
									part.partInfo.title.ToString(),
									//(atmo ? MerillData.str_atmo : MerillData.str_vac),
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
							if (!MerillData.instance.partNameTested.Contains(part.name + (atmo ? "StartAtmo" : "StartVac")))
								MerillData.instance.partNameTested.Add(part.name + (atmo ? "StartAtmo" : "StartVac"));
						}
						else
						{
							if (!MerillData.instance.partNameCrashed.Contains(part.name + (atmo ? "StartAtmo" : "StartVac")))
								MerillData.instance.partNameCrashed.Add(part.name + (atmo ? "StartAtmo" : "StartVac"));
						}
					}
					else //put tested if second/third try => first is so a success
						if (nbRestart == 2)
						{
							MerillData.instance.nbPartTested++;
							if (!MerillData.instance.partNameTested.Contains(part.name + (atmo ? "StartAtmo" : "StartVac")))
								MerillData.instance.partNameTested.Add(part.name + (atmo ? "StartAtmo" : "StartVac"));
						}

					//test nbStartup
					//success! => update test data for this engine
					if (atmo)
					{
						maxNBRestartAtmo = Math.Max(MerillData.instance.nbRestart[part.name + "Atmo"],
							((int)(nbRestart)));
						MerillData.instance.nbRestart[part.name + "Atmo"] = maxNBRestartAtmo;
						//print("[MERILL] enginetest " + part.name + " restart nbRetart Atmo test: " + MerillData.instance.nbRestart[part.name + "Atmo"]);
					}
					else
					{
						maxNBRestartVac = Math.Max(MerillData.instance.nbRestart[part.name + "Vac"],
							((int)(nbRestart)));
						MerillData.instance.nbRestart[part.name + "Vac"] = maxNBRestartAtmo;
						//print("[MERILL] enginetest " + part.name + " restart nbRetart Vac test: " + MerillData.instance.nbRestart[part.name + "Vac"]);
					}
					nbRestartDisplay = "Atm: " + maxNBRestartAtmo + " / Vac: " + maxNBRestartVac;

					//consume the instrument token.
					isLogingTest = false;

					if (!testSuccess)
					{
						//now it's shutdown ^^
						testShutdown(atmo);
					}
					recomputeInfoMsg();
				}
			}
		}

		//TODO: separate ">50% thrust" and "sometimes <50% trust"
		private void checkFlightDuration()
		{
			bool atmo = isAtmo();
			//print("[MERILL] enginetest checkFlightDuration into " + (atmo ? "atmo" : "vac") + ", and before = " + isFlyingAtmo);
			//isFlyingAtmo
			if (isFlyingAtmo != atmo)
			{
				//change!
				//update if needed
				//print("[MERILL] enginetest checkFlightDuration update?" + isLogingTest);
				if (isLogingTest)
				{
					//log max burn time & test engine for the little duration
					testEngineDuration(atmo);
				}
				//like a startup
				nextDurationToCheck = 0;
				nbRestart = 0;
				isFlyingAtmo = false;
				timeIgnitedLaunch = part.vessel.launchTime;
				timeIgnitedMission = part.vessel.missionTime;
				//begin test it the next udate
				return;
			}
			//print("[MERILL] enginetest " + part.name + " duration: " + (part.vessel.launchTime - timeIgnited ) + " ? "
			//	+ (atmo ? maxNBSecBurnAtmo : maxNBSecBurnVac));
			//check if we have to do a duration test = (10 * 2^X) [ when (byte)X change => test ] + some random
			if (burnTime() > (atmo ? maxNBSecBurnAtmo : maxNBSecBurnVac))
			{
				if (nextDurationToCheck == 0)
				{
					//init
					nextDurationToCheck = Math.Max((7d + aleat.NextDouble() * 6f), (atmo ? maxNBSecBurnAtmo : maxNBSecBurnVac) * (1.5d + aleat.NextDouble() / 2));
					isFlyingAtmo = atmo;

					//print("[MERILL] enginetest  init duration test to " + nextDurationToCheck);

					// use instrumentation (if not in use)
					isLogingTest = isLogingTest || useInstrumentation();

					if (isLogingTest)
					{
						//ScreenMessages.PostScreenMessage("Part " + part.partInfo.title.ToString()
						//	+ " is now instrumented in " + (atmo ? "atmosphere" : "vaccum") + " for a long burn test."
						//	, 3f, ScreenMessageStyle.UPPER_CENTER);
						drawMsgToUser(string.Format(MerillData.str_enginetest_duration_begin,
									"in "+part.partInfo.title.ToString(),
									(atmo ? MerillData.str_atmo : MerillData.str_vac))
									, 3);

					}
				}
				else if (burnTime() > nextDurationToCheck)
				{
					//test
					testEngineDuration(atmo);

					//compute next
					nextDurationToCheck = (atmo ? maxNBSecBurnAtmo : maxNBSecBurnVac) * (1.5d + aleat.NextDouble() / 2);

				}
			}
		}

		protected void testEngineDuration(bool atmo)
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
			if (atmo)
			{
				// hard test in ground
				testSuccess = doTest(false, false);
			}
			else
			{
				bool testOnGround = burnTime() < maxNBSecBurnAtmo;

				// 4 time better in space if work done at ground
				testSuccess = doTest(testOnGround, testOnGround);
			}

			//message to user + explode if necessary
			if (!testSuccess)
			{
				//fail
				part.explode();
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
								"in "+(atmo ? MerillData.str_atmo : MerillData.str_vac),
								(uint)(burnTime())));
				}
				else
				{
					drawMsgToUser(string.Format(MerillData.str_enginetest_duration_failKO,
								part.partInfo.title.ToString(),
								"in " + (atmo ? MerillData.str_atmo : MerillData.str_vac),
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
								"in "+(atmo ? MerillData.str_atmo : MerillData.str_vac),
								(uint)(burnTime())));
				}
				else
				{
					drawMsgToUser(string.Format(MerillData.str_enginetest_duration_successKO,
								part.partInfo.title.ToString(),
								"in " + (atmo ? MerillData.str_atmo : MerillData.str_vac),
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
						if (!MerillData.instance.partNameTested.Contains(part.name + (atmo ? "DurAtmo" : "DurVac")))
							MerillData.instance.partNameTested.Add(part.name + (atmo ? "DurAtmo" : "DurVac"));
					}
					else
					{
						if (!MerillData.instance.partNameCrashed.Contains(part.name + (atmo ? "DurAtmo" : "DurVac")))
							MerillData.instance.partNameCrashed.Add(part.name + (atmo ? "DurAtmo" : "DurVac"));
					}
				}
				else //put tested if longer try => first is so a success
					if (expAfter == 3)
						if (!MerillData.instance.partNameTested.Contains(part.name + (atmo ? "DurAtmo" : "DurVac")))
							MerillData.instance.partNameTested.Add(part.name + (atmo ? "DurAtmo" : "DurVac"));


				//success! => update test data for this engine
				updateSafeBurnTime(atmo);

				//consume the instrument token.
				isLogingTest = false;
			}
		}

		protected void updateSafeBurnTime(bool atmo)
		{
			if (atmo)
			{
				maxNBSecBurnAtmo =
					Math.Max(MerillData.instance.maxNBSecBurn[part.name + "Atmo"],
					burnTime());
				MerillData.instance.maxNBSecBurn[part.name + "Atmo"] = maxNBSecBurnAtmo;

			}
			else
			{
				maxNBSecBurnVac =
					Math.Max(MerillData.instance.maxNBSecBurn[part.name + "Vac"],
					burnTime());
				MerillData.instance.maxNBSecBurn[part.name + "Vac"] = maxNBSecBurnVac;
			}
			burnTimeDisplay = "Atm: " + maxNBSecBurnAtmo + " /Vac: " + maxNBSecBurnVac;
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
	}
}

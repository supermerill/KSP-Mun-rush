using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KspMerillEngineFail
{
	public class MerillSimplePartTest : MerillAbstractPartTest
	{

		[KSPField]
		bool alreadyCrashedPreviousShipAtmo;

		[KSPField]
		bool alreadyTestedPreviousShipAtmo;

		[KSPField]
		bool alreadyCrashedPreviousShipVac;

		[KSPField]
		bool alreadyTestedPreviousShipVac;

		[KSPField]
		bool previousActivate = false;

		[KSPField(isPersistant = false, guiActive = true, guiActiveEditor = false, guiName = "Tested in")]
		public string partTestedDispay = "nowhere";

		public override string GetInfo()
		{
			if (!alreadyTestedPreviousShipAtmo && !alreadyTestedPreviousShipVac)
			{
				return "<b>Not tested</b>";
			}
			else
			{

				string sDescString = "<b>Tested in:</b>\n";

				if (alreadyTestedPreviousShipAtmo)
				{
					sDescString += "- Atmosphere";
				}
				if (alreadyTestedPreviousShipVac)
				{
					sDescString += "- Vacuum";
				}

				return sDescString;
			}
		}


		public override void loadDataFromScenario(MerillData scenario)
		{
			alreadyCrashedPreviousShipAtmo = scenario.partNameCrashed.Contains(part.name + "Atmo");
			alreadyTestedPreviousShipAtmo = scenario.partNameTested.Contains(part.name + "Atmo");
			alreadyCrashedPreviousShipVac = scenario.partNameCrashed.Contains(part.name + "Vac");
			alreadyTestedPreviousShipVac = scenario.partNameTested.Contains(part.name + "Vac");
			updateDisplay();
		}

		protected void updateDisplay()
		{
			if (alreadyTestedPreviousShipAtmo)
			{
				if (alreadyTestedPreviousShipVac)
				{
					partTestedDispay = "Atm & Vac";
				}
				else
				{
					partTestedDispay = "Atmosphere";
				}
			}
			else if (alreadyTestedPreviousShipVac)
			{
				partTestedDispay = "Vaccum";
			}
		}

		public override bool isActivated()
		{
			return !previousActivate && oneTimeActivation();
		}

		public virtual bool oneTimeActivation()
		{
			return false;
		}

		public override void partTest()
		{
			MerillData.log("SIMPLEPARTTEST JUST activate");
			previousActivate = true;
			bool isAtmo = (FlightGlobals.getStaticPressure() > 0);
			if ((isAtmo && !alreadyTestedPreviousShipAtmo) || (!isAtmo && !alreadyTestedPreviousShipVac))
			{
				MerillData.log("SIMPLEPARTTEST not tested yet");
				//if already crashed & recovered & it's testable -> don't crash again!
				if (((isAtmo && alreadyCrashedPreviousShipAtmo)
						|| (!isAtmo && alreadyCrashedPreviousShipVac))
						&& (!MerillData.instance.partNameTested.Contains(part.name + (isAtmo ? "Atmo" : "Vac")))
					&& useInstrumentation())
				{

					MerillData.log("SIMPLEPARTTEST already crashed");

					MerillData.log("SIMPLEPARTTEST TEST added ");
					MerillData.instance.partNameTested.Add(part.name + (isAtmo ? "Atmo" : "Vac"));
					//TODO message
					drawMsgToUser(string.Format(MerillData.str_part_testOK,
						part.partInfo.title.ToString(),
						((FlightGlobals.getStaticPressure() > 0) ? MerillData.str_atmo : MerillData.str_vac))); 
					//ScreenMessages.PostScreenMessage("Modified part " + part.partInfo.title.ToString()
					//	+ " tested and data transmitted. You can now use this part safely in "
					//	+ ((FlightGlobals.getStaticPressure() > 0) ? "atmosphere" : "vaccum")
					//	//((vessel.orbit.==Vessel.Situations.
					//	+ ".", 10F, ScreenMessageStyle.UPPER_CENTER);
				}
				else
				{

					//choose to destroy or not
					// 1/2 -> 1/4 chance to crahsed based on tested 
					//FIXME: 20+80 for release
					float chance = 2 + 100f / ((MerillData.instance.partNameTested.Count / 10) + 1);
					MerillData.log("SIMPLEPARTTEST crash chance are " + chance);
					if ((isAtmo && alreadyCrashedPreviousShipAtmo)
						|| (!isAtmo && alreadyCrashedPreviousShipVac))
					{
						chance = chance / 2;
					}
					if (!isAtmo && alreadyCrashedPreviousShipAtmo)
					{
						chance = chance / 2;
					}

					//bad luck?
					int sort = MerillData.instance.get0to99FromNotAleatTable();

					MerillData.log("SIMPLEPARTTEST crash chance are " + chance + " sort are " + sort + " => " + (sort % 100));
					if (sort < chance)
					{
						MerillData.log("SIMPLEPARTTEST EXPLODE! ");
						part.explode();
						if (!MerillData.instance.partNameCrashed.Contains(part.name + (isAtmo ? "Atmo" : "Vac")))
						{
							if (useInstrumentation())
							{
								MerillData.instance.partNameCrashed.Add(part.name + (isAtmo ? "Atmo" : "Vac"));
								MerillData.instance.nbPartDestroy++;
								//TODO add crash report to the test instrument
								drawMsgToUser(string.Format(MerillData.str_part_failOK,
									part.partInfo.title.ToString(),
									(isAtmo ? MerillData.str_atmo : MerillData.str_vac))); 
								//ScreenMessages.PostScreenMessage("Part " + part.partInfo.title.ToString()
								//	+ " has failed and the recorded data are transmitted. You should redo a test to validate the diagnostic in "
								//	+ (isAtmo ? "atmosphere." : "vaccum.")
								//	//((vessel.orbit.==Vessel.Situations.
								//	+ ".", 10F, ScreenMessageStyle.UPPER_CENTER);
							}
							else
							{
								//TODO message: part fail but not recorded
								//ScreenMessages.PostScreenMessage("Part " + part.partInfo.title.ToString()
								//	+ " has failed but cannot record/transmit the data. You should redo a test."
								//	, 10F, ScreenMessageStyle.UPPER_CENTER);
								drawMsgToUser(string.Format(MerillData.str_part_failKO,
									part.partInfo.title.ToString()));
							}
						}
					}
					else
					{
						MerillData.log("SIMPLEPARTTEST TEST! " + MerillData.instance.partNameTested.Contains(part.name + (isAtmo ? "Atmo" : "Vac")));
						//already test?
						if (!MerillData.instance.partNameTested.Contains(part.name + (isAtmo ? "Atmo" : "Vac")))
						{
							//check if we can record the test
							if (useInstrumentation())
							{
								MerillData.log("SIMPLEPARTTEST TEST added ");
								MerillData.instance.partNameTested.Add(part.name + (isAtmo ? "Atmo" : "Vac"));
								drawMsgToUser(string.Format(MerillData.str_part_testOK,
									part.partInfo.title.ToString(),
									(isAtmo ? MerillData.str_atmo : MerillData.str_vac)));
								//ScreenMessages.PostScreenMessage("Part " + part.partInfo.title.ToString()
								//	+ " tested and data transmitted. You can now use this part safely in "
								//	+ (isAtmo ? "atmosphere." : "vaccum.")
								//	//((vessel.orbit.==Vessel.Situations.
								//	+ ".", 10F, ScreenMessageStyle.UPPER_CENTER);
							}
							else
							{
								//TODO: message part ok but data not record
								drawMsgToUser(string.Format(MerillData.str_part_failKO,
									part.partInfo.title.ToString()));
								//ScreenMessages.PostScreenMessage("Part " + part.partInfo.title.ToString()
								//	+ " has worked but cannot record/transmit the data. You should redo a test."
								//	, 10F, ScreenMessageStyle.UPPER_CENTER);
							}
						}
					}
				}
			}
			updateDisplay();
		}
	}
}

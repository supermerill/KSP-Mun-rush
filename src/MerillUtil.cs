﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KspMerillEngineFail
{
	public class MerillUtil
	{


		static public CelestialBody getPlanet(String sName)
		{
			foreach (CelestialBody tempBody in FlightGlobals.Bodies)
			{
				if (tempBody.name == sName)
				{
					return tempBody;
				}
			}
			return null;
		}

		//get situation, srfLanded = landed, SrfSplashed is used as "not a situation"
		static public ExperimentSituations getDetailedSituation(Vessel vessel)
		{

			//landed?
			if (vessel.situation == Vessel.Situations.LANDED
				| vessel.situation == Vessel.Situations.PRELAUNCH
				| vessel.situation == Vessel.Situations.SPLASHED)
				return ExperimentSituations.SrfLanded;

			MerillData.log("getDetailedSituation atmosphere: " + vessel.mainBody.atmosphere);
			MerillData.log("getDetailedSituation flyingAltitudeThreshold: " + vessel.mainBody.scienceValues.flyingAltitudeThreshold + " ?> " + vessel.altitude);
			MerillData.log("getDetailedSituation maxAtmosphereAltitude: " + vessel.mainBody.maxAtmosphereAltitude + " ?< " + vessel.altitude);
			MerillData.log("getDetailedSituation spaceAltitudeThreshold: " + vessel.mainBody.scienceValues.spaceAltitudeThreshold + " ?> " + vessel.altitude);
			
			if (vessel.mainBody.atmosphere && vessel.mainBody.maxAtmosphereAltitude > vessel.altitude) // i am in atmo?
				if (vessel.mainBody.scienceValues.flyingAltitudeThreshold > vessel.altitude) // where in atmo?
					return ExperimentSituations.FlyingLow;
				else return ExperimentSituations.FlyingHigh;

			if (!vessel.mainBody.atmosphere && vessel.mainBody.maxAtmosphereAltitude < vessel.altitude) //i am in space?
				if (vessel.mainBody.scienceValues.spaceAltitudeThreshold > vessel.altitude) //where in space?
					return ExperimentSituations.InSpaceLow;
				else return ExperimentSituations.InSpaceHigh;

			//not landed, in atmo nether space => impossible!
			return ExperimentSituations.SrfSplashed;

		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KspMerillEngineFail
{
    public class MerillEngineFail : PartModule
	{

		[KSPField(isPersistant=true)]
		int nbStart=0;

		[KSPField(isPersistant = true)]
		bool isRunning = false;
		[KSPField(isPersistant = true)]
		bool failstart = false;

		ModuleEngines engine = null;
		ModuleEnginesFX engineFX = null;
		int countUpdate = 0;

		float realMaxThrust = 0;
		float realMinThrust = 0;

		public override void OnStart(StartState state)
		{
			isRunning = isEngineRunning();
			MerillData.log("enginestart: is started? " + isRunning);
			if (part.Modules.OfType<ModuleEngines>().ToList().Capacity > 0)
			{
				engine = part.Modules.OfType<ModuleEngines>().ToList()[0];
				realMaxThrust = engine.maxThrust;
				realMinThrust = engine.minThrust;
			}
			if (part.Modules.OfType<ModuleEnginesFX>().ToList().Capacity > 0)
			{
				engineFX = part.Modules.OfType<ModuleEnginesFX>().ToList()[0];
				realMaxThrust = engine.maxThrust;
				realMinThrust = engineFX.minThrust;
			}
			// if at "load", the engine is activated but at 0 thrust, be sure to set the engine to really 0 thrust
			if (isEngineShutdown())
			{
				isRunning = false;
				if (engine == null) engineFX.minThrust = 0;
				else engine.minThrust = 0;
			}
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
				return  engine.EngineIgnited && engine.currentThrottle>0  && engine.fuelFlowGui > 0;
			}
			if (engineFX != null)
			{
				return engineFX.EngineIgnited && engineFX.currentThrottle > 0 && engineFX.fuelFlowGui > 0;
			}
			return false;
		}


	public override void OnUpdate(){

		base.OnUpdate();
		countUpdate++;

		//sanity check
		if (engine != null || engineFX != null)
		{

			//if (countUpdate % 10 == 0)
			//{
			//	if(engine != null)
			//	print("[MERILL]enginerestart fail, now minth=" + engine.minThrust + ", maxthr=" + engine.maxThrust);
			//	else
			//		print("[MERILL]enginerestart fail, now minth=" + engineFX.minThrust + ", maxthr=" + engineFX.maxThrust);
			//}
			//	print("[MERILL]enginestart "
			//		+ ", currentThrottle=" + engine.currentThrottle
			//		+ ", thrustPercentage=" + engine.thrustPercentage
			//		+ ", throttleLocked=" + engine.throttleLocked
			//		+ ", requestedThrottle=" + engine.requestedThrottle
			//		+ ", requestedThrust=" + engine.requestedThrust);
			//if (countUpdate % 10 == 0)
			//check previous state
			//countUpdate++;
			if (isRunning)
			{
				//if (countUpdate % 10 == 0)
				//{
					//for (int i = 0; i < part.fxGroups.Count; i++)
					//{
					//	FXGroup fx = part.fxGroups[i];
					//	print("[Merill]fail partfx: " + part.fxGroups[i].name
					//		+ " , p=" + part.fxGroups[i].Power
					//		+ " , pv=" + part.fxGroups[i].powerVariation
					//		+ " , iv=" + part.fxGroups[i].isValid
					//		+ " , p=" + part.fxGroups[i].Active);
					//}
				//}
				//note: DO NOT use CalculateThrust() : it decrease the isp by 2.
				/*|| engine.CalculateThrust() == 0 */
				//shutdown occur?
				if (isEngineShutdown())//!engine.EngineIgnited || engine.currentThrottle == 0 || engine.fuelFlowGui == 0)
				{
					isRunning = false;
					if (engine == null) engineFX.minThrust = 0;
					else engine.minThrust = 0;
					if (failstart)
					{
						if (part.findFxGroup("running_fail") != null)
						{
							part.findFxGroup("running_fail").setActive(false);
							part.findFxGroup("running_fail").SetPower(0);
						}
					}
					else 
					{
						if (part.findFxGroup("running_notfail") != null)
						{
							part.findFxGroup("running_notfail").setActive(false);
							part.findFxGroup("running_notfail").SetPower(0);
						}
					}
				}
					//runninf in fail state?
				else if (failstart)
				{
					if (countUpdate%100 == 0)
					{
						if (engine == null) engineFX.flameout = true;
						else engine.flameout = true;
					}
					if (part.findFxGroup("running_notfail") != null)
					{
						part.findFxGroup("running_notfail").setActive(false);
						part.findFxGroup("running_notfail").SetPower(0);
					}
					//engineFX or engine?
					if (part.findFxGroup("active") != null)
					{
						if (part.findFxGroup("running_fail") != null)
						{
							part.findFxGroup("running_fail").setActive(true);
							part.findFxGroup("running_fail").SetPower(part.findFxGroup("active").Power);
						}
						part.findFxGroup("active").setActive(false);
						part.findFxGroup("active").SetPower(0);
					}
					else
					{

						if (part.findFxGroup("running_fail") != null)
						{
							part.findFxGroup("running_fail").setActive(true);
							part.findFxGroup("running_fail").SetPower(part.findFxGroup("running").Power);
						}
					}
				}
					//running ok
				else
				{
					if (part.findFxGroup("running_notfail") != null)
					{
						part.findFxGroup("running_notfail").setActive(true);
						part.findFxGroup("running_notfail").SetPower(part.findFxGroup("running").Power);
					}
				}

			}
			//note: DO NOT use CalculateThrust() : it decrease the isp by 2.
			/*&& engine.CalculateThrust() > 0*/
				//startup occur?
			else if (isEngineRunning())// engine.EngineIgnited && engine.currentThrottle>0  && engine.fuelFlowGui > 0) 
			{
				MerillData.log("enginestart: START! " + countUpdate);
				nbStart++;
				isRunning = true;
				//note: the min htrust is not atmo-corrected by btsm (nor me). It's not very usefull anyway.
				if (engine == null) engineFX.minThrust = realMinThrust;
				else engine.minThrust = realMinThrust;

				float ressourceGet = (countUpdate<2)?(1):(part.RequestResource("EngineIgniter", 1f));
				//if not enough ressource, fail!
				if (ressourceGet <= 0.01)
				{
					//print("[Merill]engineStart FAIL");
					//part.explode();
					//engine.realIsp = 40;
					if (engine != null)
					{
						engine.DeactivateRunningFX();
						FloatCurve newCurve = new FloatCurve();
						newCurve.Add(0, engine.atmosphereCurve.Evaluate(0) / 50);
						newCurve.Add(1, engine.atmosphereCurve.Evaluate(1) / 50);
						engine.atmosphereCurve = newCurve;
						engine.minThrust = realMinThrust / 50;
						engine.maxThrust = realMaxThrust / 50;
						engine.heatProduction = engine.heatProduction / 50;
						part.Modules["BTSMModuleThrustCorrecter"].OnStart(StartState.Flying);
						engine.flameout = true;
					}
					else
					{
						engineFX.DeactivateLoopingFX();
						engineFX.EngineExhaustDamage();
						FloatCurve newCurve = new FloatCurve();
						newCurve.Add(0, engineFX.atmosphereCurve.Evaluate(0) / 50);
						newCurve.Add(1, engineFX.atmosphereCurve.Evaluate(1) / 50);
						engineFX.atmosphereCurve = newCurve;
						engineFX.minThrust = realMinThrust / 50;
						engineFX.maxThrust = realMaxThrust / 50;
						engineFX.heatProduction = engineFX.heatProduction / 50;
						part.Modules["BTSMModuleThrustCorrecter"].OnStart(StartState.Flying);
						engineFX.flameout = true;
					}
					failstart = true;
					if (part.findFxGroup("running_notfail") != null)
					{
						part.findFxGroup("running_notfail").setActive(false);
						part.findFxGroup("running_notfail").SetPower(0);
					}
				}
				
			}
		}
	}


	}
}

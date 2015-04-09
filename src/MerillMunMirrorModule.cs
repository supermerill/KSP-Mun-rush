using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KspMerillEngineFail
{
	//Really usefull to make it fail? ... i think not
	public class MerillMunMirrorModule : MerillModuleScienceFail
	{
		int i = 0;

		protected BaseEvent deployMirrorEvent;
		protected BaseAction deployMirrorAction;

		public override void OnStart(StartState state)
		{
			base.OnStart(state);

			foreach (PartModule pm in part.Modules)
			{
				//MerillData.log("mirror as module " + pm.name);
				//foreach (BaseEvent temp in pm.Events)
				//{
				//	MerillData.log("mirror event " + temp.name+" "+temp.guiActive);
				//	//temp.guiActive = false;
				//}
				//foreach (BaseAction temp in pm.Actions)
				//{
				//	MerillData.log("mirror action " + temp.name + " " + temp.active);
				//	//temp.active = false;
				//}
				if (pm.moduleName.Equals("ModuleAnimateGeneric"))
				{

					deployMirrorEvent = pm.Events["Toggle"];
					deployMirrorAction = pm.Actions["ToggleAction"];
					if (deployMirrorEvent != null)
					{
						deployMirrorEvent.guiActive = false;
						deployMirrorEvent.guiActiveEditor = false;
					}
					if (deployMirrorAction != null)
					{
						deployMirrorAction.active = false;
					}
				}
			}
		}


		public override void OnUpdate()
		{
			base.OnUpdate();
			i++;
			if (i % 10 != 0) return;
			//MerillData.log("mirror pos? " + part.orgPos);
			//MerillData.log("mirror orientation? " + part.orgRot);
			//MerillData.log("mirror attach orientation? " + part.attRotation);
			//MerillData.log("mirror axis? " + part.mirrorAxis);
			//MerillData.log("mirror axis2? " + part.mirrorRefAxis);
			//MerillData.log("mirror axis3? " + part.mirrorVector);
			////Planetarium.fetch.Home.
			//Vector3d targetPos = Planetarium.fetch.Home.GetWorldSurfacePosition(vessel.latitude, vessel.longitude, 0);
			//MerillData.log("mirror vesselPosOnKerbin " + targetPos);
			//Vector3d spaceCenterPos = Planetarium.fetch.Home.GetWorldSurfacePosition(SpaceCenter.Instance.Latitude, SpaceCenter.Instance.Longitude, 0);
			//MerillData.log("mirror spaceCenterPos " + spaceCenterPos);
			//MerillData.log("mirror distance " + Vector3d.Distance(targetPos, spaceCenterPos));
			//MerillData.log("mirror angle " + Vector3d.Angle(targetPos, spaceCenterPos));
			//double dDistFromKerbin = Planetarium.fetch.Home.GetAltitude(vessel.GetWorldPos3D());
			//MerillData.log("mirror distKerbin " + dDistFromKerbin);
			//Vector3d partPos = Planetarium.fetch.Home.GetWorldSurfacePosition(
			//	Planetarium.fetch.Home.GetLatitude(vessel.GetWorldPos3D()),
			//	Planetarium.fetch.Home.GetLongitude(vessel.GetWorldPos3D()),
			//	Planetarium.fetch.Home.GetAltitude(vessel.GetWorldPos3D()));
			//MerillData.log("mirror vesselPos " + partPos);
			//MerillData.log("mirror vesselDistance " + Vector3d.Distance(partPos, spaceCenterPos));
			//MerillData.log("mirror vesselAngle " + Vector3d.Angle(partPos, spaceCenterPos));

		}

		public override void doScienceEvent()
		{
			if (deployMirror())
			{
				if (canBeTested && btsmScienceEvent != null /*&& isInstrumentOk() don't fail!*/)
				{
					btsmScienceEvent.Invoke();
				}
			}
			//foreach (PartModule pm in part.Modules)
			//{
			//	MerillData.log("mirror as module " + pm.name);
			//	foreach (BaseEvent temp in pm.Events)
			//	{
			//		MerillData.log("mirror event " + temp.name + " " + temp.guiName + " " + temp.guiActive);
			//		//temp.guiActive = false;
			//	}
			//	foreach (BaseAction temp in pm.Actions)
			//	{
			//		MerillData.log("mirror action " + temp.name + " " + temp.guiName + " " + temp.active);
			//		//temp.active = false;
			//	}
			//}
		}

		public override void doScienceAction()
		{
			if (deployMirror())
			{
				//base.doScienceAction();
				if (base.canBeTested && base.btsmScienceAction != null /*&& isInstrumentOk() do not fail!*/)
				{
					base.btsmScienceAction.Invoke(new KSPActionParam(KSPActionGroup.None, KSPActionType.Activate));
				}
			}
		}

		//method deploy
		protected bool deployMirror()
		{
			//foreach (PartModule pm in part.Modules)
			//{
			//	MerillData.log("mirror as module " + pm.name);
			//	foreach (BaseEvent temp in pm.Events)
			//	{
			//		MerillData.log("mirror event " + temp.name + " " + temp.guiName + " " + temp.guiActive);
			//		//temp.guiActive = false;
			//	}
			//	foreach (BaseAction temp in pm.Actions)
			//	{
			//		MerillData.log("mirror action " + temp.name + " " + temp.guiName + " " + temp.active);
			//		//temp.active = false;
			//	}
			//}

			//MerillData.log("mirror situation : " + part.vessel.situation);
			//MerillData.log("mirror situation : " + vessel.situation);
			//MerillData.log("mirror situation needed : " + Vessel.Situations.LANDED);
			//MerillData.log("mirror vessel.mainBody : " + vessel.mainBody.name);
			//MerillData.log("mirror vessel.mainBody needed : Mun");
			//MerillData.log("mirror altitude : " + vessel.altitude);
			//MerillData.log("mirror splashed : " + vessel.Splashed);
			//MerillData.log("mirror llanded : " + vessel.Landed);
			//MerillData.log("mirror llandedat : " + vessel.landedAt);
			//MerillData.log("mirror landor splashed : " + vessel.LandedOrSplashed);
			//MerillData.log("mirror if : " + ((!(part.vessel.situation == Vessel.Situations.LANDED)) || part.vessel.mainBody.name.Equals("Mun")));
			//MerillData.log("mirror if : " + (!(part.vessel.situation == Vessel.Situations.LANDED)) +" || "+part.vessel.mainBody.name.Equals("Mun"));
			//check landing on mun
			if( part.vessel.situation != Vessel.Situations.LANDED || (!part.vessel.mainBody.name.Equals("Mun")))
			{
				//emit error message
				//ScreenMessages.PostScreenMessage("Experiment " + part.partInfo.title.ToString()
				//	+ " can't be deployed: need to be landed on the mun."
				//	, 10f, ScreenMessageStyle.UPPER_LEFT);
				ScreenMessages.PostScreenMessage(string.Format(MerillData.str_mirror_fail, part.partInfo.title.ToString())
					, 10f, ScreenMessageStyle.UPPER_LEFT);
				return false;
			}
			//MerillData.log("mirror landed : ok for deploy");
			//check orientation
			//TODO
			//check kerbin visibility
			//TODO
			//ok : 
			//deploy the antenna
			deployMirrorEvent.Invoke();

			//launch the real exp (window with experiment)
			//MerillData.log("mirror check ok");
			return true;
		}

	}
}

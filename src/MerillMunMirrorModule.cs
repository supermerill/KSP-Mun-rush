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

		private BaseEvent deployMirrorEvent;
		private BaseAction deployMirrorAction;

		public override void OnStart(StartState state)
		{
			base.OnStart(state);

			foreach (PartModule pm in part.Modules)
			{
				print("[MERILL]mirror as module " + pm.name);
				foreach (BaseEvent temp in pm.Events)
				{
					print("[MERILL]mirror event " + temp.name+" "+temp.guiActive);
					//temp.guiActive = false;
				}
				foreach (BaseAction temp in pm.Actions)
				{
					print("[MERILL]mirror action " + temp.name + " " + temp.active);
					//temp.active = false;
				}
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
			//print("[MERILL]mirror pos? " + part.orgPos);
			//print("[MERILL]mirror orientation? " + part.orgRot);
			//print("[MERILL]mirror attach orientation? " + part.attRotation);
			//print("[MERILL]mirror axis? " + part.mirrorAxis);
			//print("[MERILL]mirror axis2? " + part.mirrorRefAxis);
			//print("[MERILL]mirror axis3? " + part.mirrorVector);
			////Planetarium.fetch.Home.
			//Vector3d targetPos = Planetarium.fetch.Home.GetWorldSurfacePosition(vessel.latitude, vessel.longitude, 0);
			//print("[MERILL]mirror vesselPosOnKerbin " + targetPos);
			//Vector3d spaceCenterPos = Planetarium.fetch.Home.GetWorldSurfacePosition(SpaceCenter.Instance.Latitude, SpaceCenter.Instance.Longitude, 0);
			//print("[MERILL]mirror spaceCenterPos " + spaceCenterPos);
			//print("[MERILL]mirror distance " + Vector3d.Distance(targetPos, spaceCenterPos));
			//print("[MERILL]mirror angle " + Vector3d.Angle(targetPos, spaceCenterPos));
			//double dDistFromKerbin = Planetarium.fetch.Home.GetAltitude(vessel.GetWorldPos3D());
			//print("[MERILL]mirror distKerbin " + dDistFromKerbin);
			//Vector3d partPos = Planetarium.fetch.Home.GetWorldSurfacePosition(
			//	Planetarium.fetch.Home.GetLatitude(vessel.GetWorldPos3D()),
			//	Planetarium.fetch.Home.GetLongitude(vessel.GetWorldPos3D()),
			//	Planetarium.fetch.Home.GetAltitude(vessel.GetWorldPos3D()));
			//print("[MERILL]mirror vesselPos " + partPos);
			//print("[MERILL]mirror vesselDistance " + Vector3d.Distance(partPos, spaceCenterPos));
			//print("[MERILL]mirror vesselAngle " + Vector3d.Angle(partPos, spaceCenterPos));

		}

		public override void doScienceEvent()
		{
			if (deployMirror())
			base.doScienceEvent();
			foreach (PartModule pm in part.Modules)
			{
				print("[MERILL]mirror as module " + pm.name);
				foreach (BaseEvent temp in pm.Events)
				{
					print("[MERILL]mirror event " + temp.name + " " + temp.guiName + " " + temp.guiActive);
					//temp.guiActive = false;
				}
				foreach (BaseAction temp in pm.Actions)
				{
					print("[MERILL]mirror action " + temp.name + " " + temp.guiName + " " + temp.active);
					//temp.active = false;
				}
			}
		}

		public override void doScienceAction()
		{
			if (deployMirror())
			base.doScienceAction();
		}

		//method deploy
		protected bool deployMirror()
		{
			foreach (PartModule pm in part.Modules)
			{
				print("[MERILL]mirror as module " + pm.name);
				foreach (BaseEvent temp in pm.Events)
				{
					print("[MERILL]mirror event " + temp.name + " " + temp.guiName + " " + temp.guiActive);
					//temp.guiActive = false;
				}
				foreach (BaseAction temp in pm.Actions)
				{
					print("[MERILL]mirror action " + temp.name + " " + temp.guiName + " " + temp.active);
					//temp.active = false;
				}
			}

			print("[MERILL]mirror situation : " + part.vessel.situation);
			print("[MERILL]mirror situation : " + vessel.situation);
			print("[MERILL]mirror situation needed : " + Vessel.Situations.LANDED);
			print("[MERILL]mirror vessel.mainBody : " + vessel.mainBody.name);
			print("[MERILL]mirror vessel.mainBody needed : Mun");
			print("[MERILL]mirror altitude : " + vessel.altitude);
			print("[MERILL]mirror splashed : " + vessel.Splashed);
			print("[MERILL]mirror llanded : " + vessel.Landed);
			print("[MERILL]mirror llandedat : " + vessel.landedAt);
			print("[MERILL]mirror landor splashed : " + vessel.LandedOrSplashed);
			print("[MERILL]mirror if : " + ((!(part.vessel.situation == Vessel.Situations.LANDED)) || part.vessel.mainBody.name.Equals("Mun")));
			print("[MERILL]mirror if : " + (!(part.vessel.situation == Vessel.Situations.LANDED)) +" || "+part.vessel.mainBody.name.Equals("Mun"));
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
			print("[MERILL]mirror landed : ok for deploy");
			//check orientation
			//TODO
			//check kerbin visibility
			//TODO
			//ok : 
			//deploy the antenna
			deployMirrorEvent.Invoke();

			//launch the real exp (window with experiment)
			print("[MERILL]mirror check ok");
			return true;
		}

	}
}

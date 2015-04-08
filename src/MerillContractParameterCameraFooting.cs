using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Contracts;

namespace KspMerillEngineFail
{
	public class MerillContractParameterCameraFooting : ContractParameter
	{
		protected CelestialBody body2Shoot = null;
		protected ExperimentSituations situationForShoot = ExperimentSituations.SrfSplashed;
		protected bool useKerbal;

		public MerillContractParameterCameraFooting()
			: base()
		{
			// note: default constructor is necessary or parameter will fail to load
			this.body2Shoot = null;
			this.situationForShoot = ExperimentSituations.SrfSplashed;
			this.useKerbal = false;
		}

		public MerillContractParameterCameraFooting(CelestialBody body2Shoot, 
				ExperimentSituations situationForShoot, bool useKerbal)
			: base()
		{
			this.body2Shoot = body2Shoot;
			this.situationForShoot = situationForShoot;
			this.useKerbal = useKerbal;
			
		}

		protected override string GetHashString()
		{
			return GetTitle();
		}

		protected override string GetTitle()
		{
			if (body2Shoot == null || situationForShoot == ExperimentSituations.SrfSplashed)
			{
				return "mmm... there're a bug, blame merill!";
			}
			else
			{
				return string.Format(MerillData.str_camera_title_param, 
					body2Shoot.name, 
					MerillData.situation2String(situationForShoot))
					+ (useKerbal ? MerillData.str_camera_title_param_withkerbal : "");
					//"Take footage of "+body2Shoot.name+" at "+situationForShoot+" "+(useKerbal?" with a kerbal.":"");
			}
		}

		protected override void OnLoad(ConfigNode node)
		{
			base.OnLoad(node);

			body2Shoot = MerillUtil.getPlanet(node.GetValue("body"));

			situationForShoot = (ExperimentSituations) System.Enum.Parse(typeof(ExperimentSituations), node.GetValue("situation"));
					
			useKerbal = node.GetValue("withKerbal").Equals("true");
		}

		protected override void OnSave(ConfigNode node)
		{
			base.OnSave(node);
			
			node.AddValue("body", body2Shoot.name);
			node.AddValue("situation", System.Enum.GetName(typeof(ExperimentSituations), situationForShoot));
			node.AddValue("withKerbal", useKerbal?"true":"false");

		}
		
		public bool tryTakePicture(Part camera)
		{
			MerillData.print("[MERILL]takePicture! " + MerillUtil.getDetailedSituation(camera.vessel) + " == " + "situationForShoot.name" +
				" && " + camera.vessel.mainBody.name + " == " + body2Shoot.name);
			if (MerillUtil.getDetailedSituation(camera.vessel) == situationForShoot
				&& camera.vessel.mainBody == body2Shoot && base.state == ParameterState.Incomplete)
			{
				//IsControllable?
				MerillData.print("[MERILL]camera isCommandable=" + camera.vessel.isCommandable);
				MerillData.print("[MERILL]camera IsControllable=" + camera.vessel.IsControllable);
				if (!camera.vessel.IsControllable)
				{
					return false;
				}
				//manned?
				if (useKerbal)
				{
					MerillData.print("[MERILL]camera manned=" + (camera.vessel.GetCrewCount() > 0));
					if (camera.vessel.GetCrewCount() == 0)
					{
						return false;
					}
				}
				//else
				//{
				//}
				//consume all scienceSlot
				bool findAtLeastASlot = false;
				if (camera.parent != null)
				{
					MerillData.print("[MERILL]cameraparameter : parent for science slot is " + camera.parent.name);
					while(camera.parent.RequestResource("ScienceSlot", 1) == 1)
					{
						MerillData.print("[MERILL]cameraparameter : find a slot, consumed it");
						findAtLeastASlot = true;
					}
				}
				//consume elctricity
				float energyReceived = camera.RequestResource("ElectricCharge", 800);
				MerillData.print("[MERILL]cameraparameter : find elec charge of "+energyReceived);

				//erorr: can't fins at least a slot
				if (!findAtLeastASlot || energyReceived < 790)
				{
					ScreenMessages.PostScreenMessage(MerillData.str_camera_noSlot, 10f, ScreenMessageStyle.UPPER_LEFT);
					return false;
				}

				MerillData.print("[MERILL]takePicture! OKKKKK " + ReputationCompletion);
				//todo: check antenna

				//add build point upgrade
				if (ReputationCompletion > 0)
				KerbalConstructionTime.KCT_GameStates.TotalUpgradePoints += (int)ReputationCompletion;
				ScreenMessages.PostScreenMessage("[KCT] "+(int)ReputationCompletion+" Upgrade Point Added!", 4.0f, ScreenMessageStyle.UPPER_LEFT);
				SetComplete();
				return true;
			}
			return false;
		}
		
	}
}

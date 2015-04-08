using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Contracts;
using KerbalConstructionTime;

namespace KspMerillEngineFail
{
	public class MerillContractUseCamera : Contract
	{

		protected CelestialBody body2Shoot = null;
		protected ExperimentSituations situationForShoot = ExperimentSituations.SrfSplashed;
		protected bool useKerbal;

		//omg, it's called a big amount of time
		protected override bool Generate()
		{
			//if (base.Prestige != ContractPrestige.Significant)
			//{
			//	return false;
			//}
			//MerillData.print("[MERILL]camera mission generate");
			
			//TODO use/create 'KTV' (KerbalTV) 
			if (Contracts.Agents.AgentList.Instance != null)
			{
				agent = Contracts.Agents.AgentList.Instance.GetAgent(MerillData.str_agent_secretary);
			}


			//1 human day in kerbal days
			//SetDeadlineDays(4);
			base.deadlineType = DeadlineType.None;
			base.expiryType = DeadlineType.None;
			
			//choose a body
			this.body2Shoot = MerillUtil.getPlanet("Mun");
			//default (erase latter)
			this.useKerbal = false;
			this.situationForShoot = ExperimentSituations.InSpaceLow;

			//check if a contract of this type with a kerbal is already taken
			bool alreadyHereKerbal = false;
			bool alreadyHereProbe = false;
			foreach (MerillContractUseCamera oldContract in ContractSystem.Instance.GetCompletedContracts<MerillContractUseCamera>())
			{
				if(oldContract.body2Shoot == body2Shoot && oldContract.useKerbal)
				{
					alreadyHereKerbal = true;
					MerillData.print("[MERILL]camera mission manned already done");
				}
				if (oldContract.body2Shoot == body2Shoot && !oldContract.useKerbal)
				{
					alreadyHereProbe = true;
				}

			}
			//check if it's in available contract
			if (!alreadyHereKerbal || !alreadyHereProbe)
			{
				//MerillData.print("[MERILL]camera mission generated? " + ContractSystem.Instance.Contracts.Count);
				foreach ( Contract tempContract in ContractSystem.Instance.Contracts )
				{
					if (tempContract is MerillContractUseCamera)
					{
						if (((MerillContractUseCamera)tempContract).body2Shoot == body2Shoot &&
							((MerillContractUseCamera)tempContract).useKerbal)
						{
							//MerillData.print("[MERILL]camera mission generated? true! ");
							alreadyHereKerbal = true;
						}
						if (((MerillContractUseCamera)tempContract).body2Shoot == body2Shoot &&
							!((MerillContractUseCamera)tempContract).useKerbal)
						{
							//MerillData.print("[MERILL]camera mission generated? true! ");
							alreadyHereProbe = true;
						}
					}
				}
			}

			if (!alreadyHereKerbal)
			{
				situationForShoot = ExperimentSituations.InSpaceLow;
				useKerbal = true;
				ContractParameter parameter = AddParameter(new MerillContractParameterCameraFooting(body2Shoot, situationForShoot, true));
				//MerillData.print("[MERILL]camera mission generate manned "+GetTitle());
				
				//advance, reward, failure
				//BTSM fly by mission:160k total needed: 200k
				SetFunds(0, 30000, 0);

				//reward, failure
				SetReputation(20, 0);
			}
			else if (!alreadyHereProbe)
			{
				useKerbal = false;
				//MerillData.print("[MERILL]camera mission generate unmanned");
				//is highorbit?
				//0 : none, 1= HO, 2=LO , 3 = landed
				//int situation = 0;
				//foreach (MerillContractUseCamera oldContract in ContractSystem.Instance.GetCompletedContracts<MerillContractUseCamera>())
				//{
				//	if(oldContract.body2Shoot == body2Shoot){
				//		if(oldContract.situationForShoot == ExperimentSituations.InSpaceHigh)
				//		{
				//			situation = Math.Max(situation, 1);
				//			coeff = 1;
				//		}
				//		else if(oldContract.situationForShoot == ExperimentSituations.InSpaceLow)
				//		{
				//			situation = Math.Max(situation, 2);
				//			coeff = 1;
				//		}
				//		else if(oldContract.situationForShoot == ExperimentSituations.SrfLanded)
				//		{
				//			situation = Math.Max(situation, 3);
				//			coeff = 2;
				//		}
				//	}
				//}
				//if(situation == 0){
				//	situationForShoot = ExperimentSituations.InSpaceHigh;
				//}else if(situation == 1){
				//	situationForShoot = ExperimentSituations.InSpaceLow;
				//}else if(situation == 2){
				//	situationForShoot = ExperimentSituations.SrfLanded;
				//}else{
				//	//all are done
				//	return false;
				//}
				////MerillData.print("[MERILL]camera mission generate UNmanned " + string.Format(GetTitle(), body2Shoot.name, situationForShoot.ToString()) );

				////is already active?
				//foreach (MerillContractUseCamera currentContract in ContractSystem.Instance.GetCurrentContracts<MerillContractUseCamera>())
				//{
				//	//MerillData.print("[MERILL]camera mission find current " + currentContract.useKerbal + ", " + currentContract.body2Shoot + ", " + currentContract.situationForShoot);
				//	if (!currentContract.useKerbal && currentContract.body2Shoot == body2Shoot && currentContract.situationForShoot == situationForShoot)
				//	{
				//		//MerillData.print("[MERILL]camera mission return false");
				//		return false;
				//	}
				//}

				//all green : generate parameter
				ContractParameter parameter1 = AddParameter(new MerillContractParameterCameraFooting(body2Shoot, ExperimentSituations.InSpaceHigh, false));
				ContractParameter parameter2 = AddParameter(new MerillContractParameterCameraFooting(body2Shoot, ExperimentSituations.InSpaceLow, false));
				ContractParameter parameter3 = AddParameter(new MerillContractParameterCameraFooting(body2Shoot, ExperimentSituations.SrfLanded, false));
				
				//130k via btsm "explore body"
				parameter3.SetFunds(30000);
				//parameter3.SetReputation(10);

				//18k for btsm " explore body" -> need 50k
				parameter1.SetFunds(30000);
				parameter2.SetFunds(30000);

				//reputation
				parameter1.SetReputation(5);
				parameter2.SetReputation(5);
				parameter3.SetReputation(10);

				//already done via "expore body" contract
				SetFunds(0, 30000, 0);

				//reward, failure
				SetReputation(00, 00);
			}
			else
			{
				return false;
			}

			//no science, use experiment for that (and if !=0, need to add "BTSM" at the begining of this class name)
			SetScience(0);

			//MerillData.print("[Merill] end generate");
			return true;
		}

		private void addExperimentalParts()
		{
			AvailablePart partToAdd = PartLoader.getPartInfoByName("MerillCamera");
			if (partToAdd != null && !ResearchAndDevelopment.IsExperimentalPart(partToAdd))
			{
				ResearchAndDevelopment.AddExperimentalPart(partToAdd);
			}
		}

		private void removeExperimentalParts()
		{
			AvailablePart partToAdd = PartLoader.getPartInfoByName("MerillCamera");
			if (partToAdd != null && ResearchAndDevelopment.IsExperimentalPart(partToAdd))
			{
				ResearchAndDevelopment.RemoveExperimentalPart(partToAdd);
			}
		}

		protected override void OnAccepted()
		{
			base.OnAccepted();
			addExperimentalParts();
		}

		protected override void OnFinished()
		{
			base.OnFinished();
			removeExperimentalParts();
		}
		
		protected override void OnCompleted()
		{
			base.OnCompleted();
			//MerillData.print("[MERILL]camera mission complete");
			//add build point upgrade
			if(ReputationCompletion > 0)
				if (useKerbal) KerbalConstructionTime.KCT_GameStates.TotalUpgradePoints += (int)ReputationCompletion;
				else KerbalConstructionTime.KCT_GameStates.TotalUpgradePoints += (int)ReputationCompletion;
		}

		public override bool CanBeCancelled()
		{
			return false;
		}

		public override bool CanBeDeclined()
		{
			return false;
		}

		//if it' called befor generate, it hash some shit. FIXME
		protected override string GetHashString()
		{
			return GetTitle();
		}

		protected override string GetTitle()
		{
			return String.Format(MerillData.str_camera_title, this.body2Shoot.name)
				+ (useKerbal ? String.Format(MerillData.str_camera_title_withkerbal, this.situationForShoot) : "") + ".";
			//return "Take footage of "+this.body2Shoot.name+" at "+this.situationForShoot+(useKerbal?" with a kerbal":"");
		}

		protected override string GetDescription()
		{
			//return "We need more kerbals to build our rocket! Our best spychologues think the best way is to procure to the people pics of the mun "
			//		+ " to encourage them to come help building with us.";
			return MerillData.str_camera_description;
		}

		protected override string GetSynopsys()
		{
			//return "Gain " +(useKerbal ? "20":"10")+" upgrade for building/researching faster for each reputation!! "
			//	+ (useKerbal ? "Warning: the camera must be on the command pod." :
			//	"Warning: the camera use a science slot when taking the picture. It must be on the probe core.");
			return String.Format(MerillData.str_camera_synopsis, 1/*(useKerbal ? 20 : 10)*/)
				+ (useKerbal ? MerillData.str_camera_synopsis_withKerbal : MerillData.str_camera_synopsis_withoutKerbal);
		}

		protected override string MessageCompleted()
		{
			return MerillData.str_camera_successLong;
		}

		public override bool MeetRequirements()
		{
			return true;
		}

		protected override void PenalizeCancellation()
		{
			PenalizeFailure();
		}

		protected override void OnLoad(ConfigNode node)
		{
			//MerillData.print("[Merill] contract photo OnLoad ");
			base.OnLoad(node);
			if (node.GetValue("body2Shoot") == "null")
			{
				body2Shoot = null;
				//MerillData.print("[Merill] contract photo body null ");
			}
			else
				body2Shoot = MerillUtil.getPlanet(node.GetValue("body2Shoot"));

			situationForShoot = (ExperimentSituations) System.Enum.Parse(typeof(ExperimentSituations), node.GetValue("situation"));
			
			useKerbal = node.GetValue("withKerbal").Equals("true");
		}

		protected override void OnSave(ConfigNode node)
		{
			//MerillData.print("[Merill] contract photo OnSave " + body2Shoot
				//+ ", " + situationForShoot+" "+useKerbal);
			//GameEvents.OnTechnologyResearched.Remove(refreshExperimentalParts);
			base.OnSave(node);
			if (body2Shoot != null)
				node.AddValue("body2Shoot", body2Shoot.name);
			else
			{	
				node.AddValue("body2Shoot", "null");
				//MerillData.print("[Merill] contract photo body null (save) ");
			}
			node.AddValue("situation", System.Enum.GetName(typeof(ExperimentSituations), situationForShoot));
            node.AddValue( "withKerbal", useKerbal?"true":"false" );
		}
	}
}

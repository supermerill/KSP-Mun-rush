using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Contracts;

namespace KspMerillEngineFail
{
	//need to put "BTSM" at the start to allow the science output (btsm hack)
	public class BTSMMerillContractPlantFlagMun : Contract
	{
		//[KSPField(isPersistant = true)]
		//private List<string> expPartToRemove = new List<string>();

		private bool listenerRegistered = false;

		//omg, it's called a big amount of time
		protected override bool Generate()
		{
			//MerillData.print("[Merill] contract generate");

			//check for already generated : need only 1
            int nbGenerated = ContractSystem.Instance.GetCurrentContracts<BTSMMerillContractPlantFlagMun>().Count();
			if ( nbGenerated >= 1 )
            {
				return false;
			}
			
			
			if (Contracts.Agents.AgentList.Instance != null)
			{
				agent = Contracts.Agents.AgentList.Instance.GetAgent("General secretary of Kerbin");
			}

			//it work this way? 
			prestige = ContractPrestige.Exceptional;

			// advance, reward, failure
			SetFunds(0, 0, 1666666F);

			//reward failure
			SetReputation(0, 1666);

			//TOOD calculer le nombre de science à redonner pour retrouver btsm ~.
			SetScience(300);

			
			float deadlineInHour = 10+10+4;
			//transform in kerbal days 1d = 8h
			SetDeadlineDays(4f);
			base.deadlineType = DeadlineType.Fixed;
			base.expiryType = DeadlineType.None;


			ContractParameter parameter = AddParameter(new MerillContractParameterSuccessContract("Plant a Flag on the Mun!"));

			//MerillData.print("[Merill] end generate");
			return true;
		}

		protected override void OnAccepted()
		{
			base.OnAccepted();
			addExperimentalParts();
			GameEvents.OnTechnologyResearched.Add(refreshExperimentalParts);
		}

		// Add stub part into research node
		// Add exp part if in a researched node (from stub part)
		private void addExperimentalParts()
		{
			if (base.dateAccepted == 0)
			{
				MerillData.print("[MERILL]mun mission: can't add exp part: date accepted=" + dateAccepted);
			}
			try
			{
				//add mandatory parts
				addExperimentalPart("MerillnlineCockpitLander");

				//add part from stubs
				foreach(AvailablePart aPart in PartLoader.Instance.parts)
				{
					if (aPart.partPrefab != null && aPart.partPrefab.Modules != null)
					{
						//MerillData.print("[merill]part " + aPart.name);
						foreach (PartModule pm in aPart.partPrefab.Modules)
						{
							if (pm.moduleName.Equals("MerillMissionStub"))
							{
								if (((MerillMissionStub)pm).missionName.Equals(this.GetType().Name))
								{

									MerillData.print("[merill] RD find a part " + pm.name);
									MerillData.print("[merill] RD purchased? " + ResearchAndDevelopment.PartModelPurchased(aPart));
									MerillData.print("[merill] RD available? " + ResearchAndDevelopment.PartTechAvailable(aPart));
									MerillData.print("[merill] RD mi " + aPart.moduleInfo);
									MerillData.print("[merill] RD tech required: " + aPart.TechRequired);
									MerillData.print("[merill] RD tech Really required: " + ((MerillMissionStub)pm).techRequired);
									MerillData.print("[merill] RD tech Really required purchased? : "
										+ ResearchAndDevelopment.GetTechnologyState(((MerillMissionStub)pm).techRequired));
									//already set, and already researched?
									if (ResearchAndDevelopment.GetTechnologyState(
										((MerillMissionStub)pm).techRequired) == RDTech.State.Available
										&& aPart.TechRequired == ((MerillMissionStub)pm).techRequired)
									{
										MerillData.print("[MERILL] RD find a part with r&d node " + pm.name);
										//check if already experimental
										if (!ResearchAndDevelopment.IsExperimentalPart(
											PartLoader.getPartInfoByName(((MerillMissionStub)pm).partUnlock)))
											addExperimentalPart(((MerillMissionStub)pm).partUnlock);
									}
										// not set
									else if (aPart.TechRequired == "specializedControl")
									{
										try { 
											//try to attach the stub to a research node
											MerillData.print("[MERILL] RD find a part without r&d node " + pm.name);

											RDTech tech = AssetBase.RnDTechTree.FindTech(
												((MerillMissionStub)pm).techRequired);
											if(tech != null)
											{ 
												//Set it
												MerillData.print("[MERILL] RD find good tech " + tech.name);
												aPart.TechRequired = ((MerillMissionStub)pm).techRequired;
												tech.partsAssigned.Add(aPart);
												MerillData.print("[MERILL] RD find tech assigned " );

												MerillData.print("[MERILL] RD good tech purchased? " + tech.state
													+" , "+tech.enabled+", ");
												//already researched?
												if (ResearchAndDevelopment.GetTechnologyState(((MerillMissionStub)pm).techRequired) == RDTech.State.Available)
												{
													addExperimentalPart(((MerillMissionStub)pm).partUnlock);
												}
											}
										}
										catch (Exception e)
										{
											MerillData.print("[MERILL] RD Exeption:  "+e);
										}
									}
								}
							}
						}
					}
				}

				//TODO: made this auto + add stub in tree
				//if (ResearchAndDevelopment.PartModelPurchased(PartLoader.getPartInfoByName("MerillCheapFuelTank1-2Stub")))
				//	addExperimentalPart("MerillCheapFuelTank1-2");


				//if (ResearchAndDevelopment.PartModelPurchased(PartLoader.getPartInfoByName("MerillCheapFuelTank3-2Stub")))
				//	addExperimentalPart("MerillCheapFuelTank3-2");

				//if (ResearchAndDevelopment.PartModelPurchased(PartLoader.getPartInfoByName("MerillLinearRcsExperimental")))
				//	addExperimentalPart("MerillLinearRcsExperimental");
			}
			catch (Exception e)
			{
				MerillData.print("[MERILL] exception at contract OnAccepted:" + e);
			}
		}

		private void addExperimentalPart(string sExperimentalPartName)
		{
			MerillData.print("[MERILL]accepted contract with experimental part: " + sExperimentalPartName);

			if (sExperimentalPartName != null)
			{
				AvailablePart experimentalPart = PartLoader.getPartInfoByName(sExperimentalPartName);

				MerillData.print("[MERILL]experimental part: " + experimentalPart + ", " + (experimentalPart == null ? "" : ""+ResearchAndDevelopment.IsExperimentalPart(experimentalPart)));
				if (experimentalPart != null && !ResearchAndDevelopment.IsExperimentalPart(experimentalPart))
				{
					MerillData.print("[MERILL]: Unlocking part: " + sExperimentalPartName);

					ResearchAndDevelopment.AddExperimentalPart(experimentalPart);
					//expPartToRemove.Add(experimentalPart.name);
				}
				else
				{
					MerillData.print("[MERILL]: Part unvailable / already research: " + sExperimentalPartName);
				}
			}
		}

		protected override void OnFinished()
		{
			base.OnFinished();
			MerillData.print("[MERILL]plantmunflag: OnFinished ");
			removeExperimentalParts();
		}
		
		
		
		protected void removeExperimentalParts()
		{
			try
			{
				//remove mandatory parts
				removeExperimentalPart("MerillnlineCockpitLander");

				foreach(AvailablePart aPart in PartLoader.Instance.parts)
				{
					if (aPart.partPrefab != null && aPart.partPrefab.Modules != null)
					{
						foreach (PartModule pm in aPart.partPrefab.Modules)
						{
							if (pm.moduleName.Equals("MerillMissionStub"))
							{
								if (((MerillMissionStub)pm).missionName.Equals(this.GetType().Name))
								{

									MerillData.print("[merill] RD find a part " + pm.name);
									MerillData.print("[merill] RD purchased? " + ResearchAndDevelopment.PartModelPurchased(aPart));
									MerillData.print("[merill] RD available? " + ResearchAndDevelopment.PartTechAvailable(aPart));
									MerillData.print("[merill] RD mi '" + aPart.moduleInfo+"'");
									MerillData.print("[merill] RD tech required: " + aPart.TechRequired);
									MerillData.print("[merill] RD tech Really required: " + ((MerillMissionStub)pm).techRequired);
									//research and set
									if (ResearchAndDevelopment.GetTechnologyState(((MerillMissionStub)pm).techRequired) == RDTech.State.Available
										&& aPart.TechRequired == ((MerillMissionStub)pm).techRequired)
									{
										MerillData.print("[merill] RD purchased, is experimental? " 
											+ ResearchAndDevelopment.IsExperimentalPart(
												PartLoader.getPartInfoByName(((MerillMissionStub)pm).partUnlock)));
										//check if experimental
										if (ResearchAndDevelopment.IsExperimentalPart(
											PartLoader.getPartInfoByName(((MerillMissionStub)pm).partUnlock)))
										{ 
											removeExperimentalPart(((MerillMissionStub)pm).partUnlock);
									}
										//remove from tech
											RDTech tech = AssetBase.RnDTechTree.FindTech(aPart.TechRequired);
											if (tech != null)
											{
												MerillData.print("[MERILL] RD find good tech " + tech.name);
												aPart.TechRequired = "specializedControl";
												tech.partsAssigned.Remove(aPart);
											}
									}
									//set? ==((MerillMissionStub)pm).techRequired
									else if (aPart.TechRequired != "specializedControl")
									{
										try {
											//try to remove the stub to a research node
											MerillData.print("[MERILL] RD find a part with r&d node " + pm.name);

											RDTech tech = AssetBase.RnDTechTree.FindTech(aPart.TechRequired);
											if(tech != null)
											{ 
												MerillData.print("[MERILL] RD find good tech " + tech.name);
												aPart.TechRequired = "specializedControl";
												tech.partsAssigned.Remove(aPart);
												MerillData.print("[MERILL] RD find tech assigned ");
												if (ResearchAndDevelopment.GetTechnologyState(((MerillMissionStub)pm).techRequired) == RDTech.State.Available)
												{
													removeExperimentalPart(((MerillMissionStub)pm).partUnlock);
												}
											}
										}
										catch (Exception e)
										{
											MerillData.print("[MERILL] RD Exeption:  "+e);
										}
									}
								}
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				MerillData.print("[MERILL] exception at contract OnAccepted:" + e);
			}
		}

		private void wip(Func<string, EventVoid> doWithApart )
		{
			foreach (AvailablePart aPart in PartLoader.Instance.parts)
			{
				if (aPart.partPrefab != null && aPart.partPrefab.Modules != null)
				{
					foreach (PartModule pm in aPart.partPrefab.Modules)
					{
						if (pm.moduleName.Equals("MerillMissionStub"))
						{
							if (((MerillMissionStub)pm).missionName.Equals(this.GetType().Name))
							{
								if (ResearchAndDevelopment.GetTechnologyState(((MerillMissionStub)pm).techRequired) == RDTech.State.Available)
								{
									MerillData.print("[merill] RD purchased, is experimental? "
										+ ResearchAndDevelopment.IsExperimentalPart(
											PartLoader.getPartInfoByName(((MerillMissionStub)pm).partUnlock)));
									//check if already experimental
									if (ResearchAndDevelopment.IsExperimentalPart(PartLoader.getPartInfoByName(((MerillMissionStub)pm).partUnlock)))
										doWithApart(((MerillMissionStub)pm).partUnlock);
								}
								else if (aPart.TechRequired != "specializedControl")
								{
									try
									{
										//try to remove the stub to a research node
										MerillData.print("[MERILL] RD find a part with r&d node " + pm.name);

										RDTech tech = AssetBase.RnDTechTree.FindTech(
											((MerillMissionStub)pm).techRequired);
										if (tech != null)
										{
											MerillData.print("[MERILL] RD find good tech " + tech.name);
											aPart.TechRequired = "specializedControl";
											tech.partsAssigned.Remove(aPart);
											MerillData.print("[MERILL] RD find tech assigned ");
											if (ResearchAndDevelopment.GetTechnologyState(((MerillMissionStub)pm).techRequired) == RDTech.State.Available)
											{
												doWithApart(((MerillMissionStub)pm).partUnlock);
											}
										}
									}
									catch (Exception e)
									{
										MerillData.print("[MERILL] RD Exeption:  " + e);
									}
								}
							}
						}
					}
				}
			}
		}
		private void removeExperimentalPart(string sExperimentalPartName)
		{
			MerillData.print("[MERILL]plantmunflag: DECLINED contract with experimental part: " + sExperimentalPartName);

			if (sExperimentalPartName != null)
			{
				AvailablePart experimentalPart = PartLoader.getPartInfoByName(sExperimentalPartName);

				if (experimentalPart != null && ResearchAndDevelopment.IsExperimentalPart(experimentalPart))
				{
					MerillData.print("[MERILL]plantmunflag: Locking part: " + sExperimentalPartName);

					ResearchAndDevelopment.RemoveExperimentalPart(experimentalPart);
				}
			}
		}


		// add experimental parts from stub inside the new research tech nodes
		// not really usefull, i think addExpParts do the trick himself
		private void refreshExperimentalParts(GameEvents.HostTargetAction<RDTech, RDTech.OperationResult> newResearch)
		{

			//MerillData.print("[Merill] refreshExperimentalParts NOde:  " + newResearch.ToString()
			//	+ ", " + newResearch.host.name + " is " + newResearch.target);
			//MerillData.print("[Merill] refreshExperimentalParts date accepted:  " + this.dateAccepted);

			//if (newResearch.target == RDTech.OperationResult.Successful)
			//{
			//	MerillData.print("[Merill] refreshExperimentalParts accepted :  " + newResearch.host.partsPurchased.Count);
			//}
			//MerillData.print("[Merill] refreshExperimentalParts Assigned");
			foreach (AvailablePart aPart in newResearch.host.partsAssigned)
			{
				if (aPart.partPrefab != null && aPart.partPrefab.Modules != null)
				{
					//MerillData.print("[Merill] refreshExperimentalParts find part" + aPart.name);

					foreach (PartModule pm in aPart.partPrefab.Modules)
					{
						//MerillData.print("[Merill] refreshExperimentalParts find pm " + pm.name + ", " + pm.moduleName);
						if (pm.moduleName.Equals("MerillMissionStub"))
						{
							//MerillData.print("[Merill] refreshExperimentalParts find pmmoduleName on part for mission " + this.GetType().Name);
							//MerillData.print("[Merill] refreshExperimentalParts find pmmoduleName on part for mission " + ((MerillMissionStub)pm).missionName);
							if (((MerillMissionStub)pm).missionName.Equals(this.GetType().Name))
							{
								//MerillData.print("[Merill] refreshExperimentalParts READY TO ADD " + aPart.name);
								//add it to this 
								addExperimentalPart(((MerillMissionStub)pm).partUnlock);
							}
						}
					}
				}
			}
		}
		

		public override bool CanBeCancelled()
		{
			return true;
		}

		public override bool CanBeDeclined()
		{
			return true;
		}

		protected override string GetHashString()
		{
			return GetTitle();
		}

		protected override string GetTitle()
		{
			return MerillData.str_munm_title; 
				//"Plant a flag on the mun! For the kerbals!";
		}

		protected override string GetDescription()
		{
			return MerillData.str_munm_descr;
			//return "Le secrétaire général veut que vous alliez sur la Mun pour assurer sa réélection. "
			//		+ " Personellement, jaurais préféré traivailler avec un vrai kerbal tel que jebediah."
			//		+ " J'ai pu vous avoir différents accorts, comme des réservoirs X200 à bas prix, "
			//		+ "un prototype de propulseur rcs de forte puissance, et quelques autres choses. "
			//		+ "La liste ets disponible dans votre centre de recherche.\n "
			//		+ "J'oubliais, l'echec n'est pas une option.";
		}

		protected override string GetSynopsys()
		{
			return MerillData.str_munmSynopsis;
	//		return "[MERILL] This is it!  The funds shot!  Young Kerbals, old Kerbals, round Kerbals, " +
	//"and rhumbic Kerbals...all will be glued to their KV's eagerly awaiting your first steps on the Mun. " +
	//"Of course, if you can't get the planting Kerbal back to Kerbin for the publicity tour and flag signings, " +
	//"it's going to look rather bad and we can expect a cut to our funding. " +
	//"If however, you manage to pull the whole thing off you can expect to be showered in glory and riches!";
		}

		protected override string MessageCompleted()
		{
			return MerillData.str_munm_success;
	//		return "Congratulations!  You sure can plant those flags like a regular automatic Kerbal flag planting device!  Actually, that sounds like a great idea... " +
	//"Je doit vous dire que le secrétaire général a promis d'aller sur duna. Dépéchez-vous d'accepter la mission, c'est bon pour votre carrière.";
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
			MerillData.print("[Merill] contract OnLoad " + this.dateAccepted);
			base.OnLoad(node);
			//accepted? register listener?
			if (this.dateAccepted > 0 && !listenerRegistered) 
			{
				//need to do this, because reloading the game re-init the parts from cfg
				addExperimentalParts();

				//register for research event to add exp part.
				if(!listenerRegistered)
				{ 
					GameEvents.OnTechnologyResearched.Add(refreshExperimentalParts);
					listenerRegistered = true;
				}
			}
			else
			{
				removeExperimentalParts();
			}
		}

		protected override void OnSave(ConfigNode node)
		{
			MerillData.print("[Merill] contract OnSave ");
			//GameEvents.OnTechnologyResearched.Remove(refreshExperimentalParts);
			base.OnSave(node);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KspMerillEngineFail
{
	//TODEL: not used / don't work
    public class MerillUpgradePart : PartModule
	{
		bool partChanged = false;

		[KSPField]
		string initialPart;

		bool loaded = false;
		bool change = false;

		public override void OnStart(StartState state)
		{
			GameEvents.OnTechnologyResearched.Add(refreshExperimentalParts);
			if(part!=null)
				MerillData.log(" research OnStart " + part.name + " with info: " + part.partInfo);
			else
				MerillData.log(" research OnStart null ");
		}

		public override void OnLoad(ConfigNode node)
		{
			base.OnLoad(node);
			MerillData.log(" research onload " + part.name + " with info: " + part.partInfo);
			loaded = true;
		}

		public override void OnInitialize()
		{
			MerillData.log(" research OnInitialize " + part.name + " with info: " + part.partInfo);
		}
		public override void OnAwake()
		{
			MerillData.log(" research OnAwake " + part.name + " with info: " + part.partInfo);
			//OnUpdate();
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			MerillData.log(" research onUpdate " + part.name + " with info: " + part.partInfo);
			if (loaded && !change)
			{

				//evolve if we are researched (TODO: test when it's necessary to do this: only 1 time?)
				MerillData.log(" research onload " + part.name + " with info: " + part.partInfo);
				if (part.partInfo != null && ResearchAndDevelopment.PartTechAvailable(this.part.partInfo))
				{
					MerillData.log(" research onload " + part.name + ", i'm researched for " + PartLoader.getPartInfoByName(initialPart));

					AvailablePart partToEvolve = PartLoader.getPartInfoByName(initialPart);
					if (partToEvolve != null && partToEvolve.partPrefab != null)
					{
						evolve(partToEvolve);
					}
				}
				change = true;
			}
		}

		private void refreshExperimentalParts(GameEvents.HostTargetAction<RDTech, RDTech.OperationResult> data)
		{
			if (data.host.partsPurchased.Contains(part.partInfo))
			{
				MerillData.log(" research done, and " + part.name + " is going to make an evolve on " + PartLoader.getPartInfoByName(initialPart));

				AvailablePart partToEvolve = PartLoader.getPartInfoByName(initialPart);
				if (partToEvolve != null && partToEvolve.partPrefab != null)
				{
					evolve(partToEvolve);
				}
			}
		}

		private void evolve(AvailablePart partToEvolve)
		{
			MerillData.log(" Part " + part.name + " want to evolve " + partToEvolve.name);

			//check what to change
			if(part.partInfo.title != null && part.partInfo.title != "")
			{
				MerillData.log(" change title from '" + partToEvolve.title + "' to '" + part.partInfo.title + "'");
				partToEvolve.title = part.partInfo.title;
			}
			if (part.mass > 0)
			{
				MerillData.log(" change mass from '" + partToEvolve.partPrefab.mass + "' to '" + part.mass + "'");
				partToEvolve.partPrefab.mass = part.mass;
			}
			foreach (PartResource ressource in part.Resources)
			{
				if (partToEvolve.partPrefab.Resources.Contains(ressource.resourceName))
				{
					MerillData.log(" change resource from '" +
						partToEvolve.partPrefab.Resources[ressource.resourceName].amount + "' to '" +
						ressource.amount + "'");
					partToEvolve.partPrefab.Resources[ressource.resourceName].amount = ressource.amount;
					partToEvolve.partPrefab.Resources[ressource.resourceName].maxAmount = ressource.maxAmount;
				}
			}
		}

	}
}

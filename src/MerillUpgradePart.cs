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
				print("[MERILL] research OnStart " + part.name + " with info: " + part.partInfo);
			else
				print("[MERILL] research OnStart null ");
		}

		public override void OnLoad(ConfigNode node)
		{
			base.OnLoad(node);
			print("[MERILL] research onload " + part.name + " with info: " + part.partInfo);
			loaded = true;
		}

		public override void OnInitialize()
		{
			print("[MERILL] research OnInitialize " + part.name + " with info: " + part.partInfo);
		}
		public override void OnAwake()
		{
			print("[MERILL] research OnAwake " + part.name + " with info: " + part.partInfo);
			//OnUpdate();
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			print("[MERILL] research onUpdate " + part.name + " with info: " + part.partInfo);
			if (loaded && !change)
			{

				//evolve if we are researched (TODO: test when it's necessary to do this: only 1 time?)
				print("[MERILL] research onload " + part.name + " with info: " + part.partInfo);
				if (part.partInfo != null && ResearchAndDevelopment.PartTechAvailable(this.part.partInfo))
				{
					print("[MERILL] research onload " + part.name + ", i'm researched for " + PartLoader.getPartInfoByName(initialPart));

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
				print("[MERILL] research done, and " + part.name + " is going to make an evolve on " + PartLoader.getPartInfoByName(initialPart));

				AvailablePart partToEvolve = PartLoader.getPartInfoByName(initialPart);
				if (partToEvolve != null && partToEvolve.partPrefab != null)
				{
					evolve(partToEvolve);
				}
			}
		}

		private void evolve(AvailablePart partToEvolve)
		{
			print("[MERILL] Part " + part.name + " want to evolve " + partToEvolve.name);

			//check what to change
			if(part.partInfo.title != null && part.partInfo.title != "")
			{
				print("[MERILL] change title from '" + partToEvolve.title + "' to '" + part.partInfo.title + "'");
				partToEvolve.title = part.partInfo.title;
			}
			if (part.mass > 0)
			{
				print("[MERILL] change mass from '" + partToEvolve.partPrefab.mass + "' to '" + part.mass + "'");
				partToEvolve.partPrefab.mass = part.mass;
			}
			foreach (PartResource ressource in part.Resources)
			{
				if (partToEvolve.partPrefab.Resources.Contains(ressource.resourceName))
				{
					print("[MERILL] change resource from '" +
						partToEvolve.partPrefab.Resources[ressource.resourceName].amount + "' to '" +
						ressource.amount + "'");
					partToEvolve.partPrefab.Resources[ressource.resourceName].amount = ressource.amount;
					partToEvolve.partPrefab.Resources[ressource.resourceName].maxAmount = ressource.maxAmount;
				}
			}
		}

	}
}

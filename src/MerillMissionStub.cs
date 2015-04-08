using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KspMerillEngineFail
{
	public class MerillMissionStub : PartModule
	{

		[KSPField]
		public string missionName;

		[KSPField]
		public string techRequired;

		[KSPField]
		public string partUnlock;
	}
}

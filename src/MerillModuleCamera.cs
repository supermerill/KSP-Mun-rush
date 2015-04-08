using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KspMerillEngineFail
{
	// todelete not used
	public class MerillModuleCamera : PartModule
	{
		//TODO: scienceSlot, need to be tested

		[KSPField]
		bool isEnable = true;

		public override void OnStart(StartState state)
		{
			base.OnStart(state);
			//disable kspeventguibutton lights on
			foreach (PartModule pm in part.Modules)
			{
				if (pm.moduleName.Equals("ModuleLight"))
				{
					foreach (BaseEvent temp in pm.Events)
					{
						print("[MERILL]Camera base event in ModuleLight " + temp.name);
						temp.guiActive = false;
					}
					//BaseEvent eventToDisable = pm.Events["LightsOff"];
					//"LightsOn"

					foreach (BaseAction temp in pm.Actions)
					{
						temp.active = false;
					}
				}
			}

			Events["takeFootage"].guiActive = isEnable;
		}


		public override void OnUpdate()
		{
			base.OnUpdate();
		}

	    [KSPEvent( guiName = "Take Footage", guiActive = true, guiActiveEditor = false )]
        public void takeFootage()
        {
			bool paramOk = false;
			MerillData.print("[MERILL]camera take footage "+(ContractSystem.Instance.Contracts!=null));
			//TODO: take screenshot ^^ TODO: move game camera to the camera before screenshot (or iva)
			//find a contract
			if ( ContractSystem.Instance.Contracts != null )
			{
				//MerillData.print("[MERILL]camera take footage search in " + ContractSystem.Instance.Contracts.Count);
			foreach ( Contract tempContract in ContractSystem.Instance.Contracts )
				{
					//MerillData.print("[MERILL]camera take footage search with " + tempContract + " => " + tempContract.GetType());
					//MerillData.print("[MERILL]camera take footage search with tempContract.DateAccepted " + tempContract.DateAccepted+", "+tempContract.IsFinished());
					if ( tempContract is MerillContractUseCamera )
					{
						MerillContractUseCamera tempPayloadContract = (MerillContractUseCamera)tempContract;
						if (tempContract.DateAccepted != 0)
						{

						}
						//MerillData.print("[MERILL]camera take footage search find! parameter count = " + tempPayloadContract.ParameterCount);
						//MerillData.print("[MERILL]camera take footage search find! parameter is " + tempPayloadContract.GetParameter<MerillContractParameterCameraFooting>());
						
						foreach(ContractParameter tempParameter in tempPayloadContract.AllParameters)
						{
							if(tempParameter is MerillContractParameterCameraFooting)
							{
								if (((MerillContractParameterCameraFooting)tempParameter).tryTakePicture(part))
								{
									paramOk = true;
									//break;
									//break 2 'for'
									goto Found;
								}
							}
						}
					}
				}
			}
		Found:
			if (paramOk)
			{
				ScreenMessages.PostScreenMessage(MerillData.str_camera_success
					, 10F, ScreenMessageStyle.UPPER_LEFT);
				//disable
				isEnable = false;
				Events["takeFootage"].guiActive = false;
			}
			else
			{
				//display message
				System.Random rand = new System.Random();
				String msg = MerillData.str_camerafail_0;
				switch (Math.Abs(rand.Next() % 7))
				{
					case 0: msg = MerillData.str_camerafail_0; break;
					case 1: msg = MerillData.str_camerafail_1; break;
					case 2: msg = MerillData.str_camerafail_2; break;
					case 3: msg = MerillData.str_camerafail_3; break;
					case 4: msg = MerillData.str_camerafail_4; break;
					case 5: msg = MerillData.str_camerafail_5; break;
					case 6: msg = MerillData.str_camerafail_6; break;
					default: break;
				}
				ScreenMessages.PostScreenMessage(string.Format(MerillData.str_camera_cannot, msg)
					, 10F, ScreenMessageStyle.UPPER_LEFT);
			}
         }
	}
}

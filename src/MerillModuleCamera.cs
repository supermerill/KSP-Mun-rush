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
						//MerillData.log("Camera base event in ModuleLight " + temp.name);
						temp.guiActive = false;
						temp.guiActiveEditor = false;
					}
					foreach (BaseAction temp in pm.Actions)
					{
						temp.active = false;
					}
					foreach (BaseField temp in pm.Fields)
					{
						temp.guiActive = false;
						temp.guiActiveEditor = false;
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
			MerillContractParameterCameraFooting param = null;
			MerillData.log("camera take picture "+(ContractSystem.Instance.Contracts!=null));
			//TODO: take screenshot ^^ TODO: move game camera to the camera before screenshot (or iva)
			//find a contract
			if ( ContractSystem.Instance.Contracts != null )
			{
			foreach ( Contract tempContract in ContractSystem.Instance.Contracts )
				{
					if ( tempContract is MerillContractUseCamera )
					{
						MerillContractUseCamera tempPayloadContract = (MerillContractUseCamera)tempContract;
						MerillData.log("camera tempPayloadContract DateAccepted: " + tempPayloadContract.DateAccepted);
						MerillData.log("camera tempPayloadContract ContractState: " + tempPayloadContract.ContractState);

						if (tempPayloadContract.ContractState != Contract.State.Active)
						{
							continue;
						}
						
						foreach(ContractParameter tempParameter in tempPayloadContract.AllParameters)
						{
							if(tempParameter is MerillContractParameterCameraFooting)
							{
								MerillData.log("camera MerillContractParameterCameraFooting find to take picture. " + tempPayloadContract.GetParameter<MerillContractParameterCameraFooting>());
								if (((MerillContractParameterCameraFooting)tempParameter).isSituationOk(part))
								{
									paramOk = true;
									param = ((MerillContractParameterCameraFooting)tempParameter);
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
			if (paramOk && param != null)
			{
				if (param.tryTakePicture(part))
				{
					ScreenMessages.PostScreenMessage(MerillData.str_camera_success
						, 10F, ScreenMessageStyle.UPPER_LEFT);
					//disable
					isEnable = false;
					Events["takeFootage"].guiActive = false;
				}
				//else
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

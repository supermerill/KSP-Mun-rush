using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Contracts;

namespace KspMerillEngineFail
{
	public class MerillContractParameterSuccessContract : ContractParameter
	{
		private String contractTitle = null;

		public MerillContractParameterSuccessContract()
			: base()
		{
			// note: default constructor is necessary or parameter will fail to load
			contractTitle = null;
		}

		public MerillContractParameterSuccessContract(string contractTitle)
			: base()
		{
			this.contractTitle = contractTitle;
		}

		protected override string GetHashString()
		{
			return GetTitle();
		}

		protected override string GetTitle()
		{
			if (contractTitle == null)
			{
				return "mmm... there're a bug, blame merill!";
			}
			else
			{
				return string.Format(MerillData.str_munm_success_param, contractTitle );
			}
		}

		protected override void OnRegister()
		{
			GameEvents.Contract.onCompleted.Add(OnContractSuccessCallback);
			GameEvents.Contract.onFailed.Add(OnContractFailCallback);
		}

		protected override void OnUnregister()
		{
			GameEvents.Contract.onCompleted.Remove(OnContractSuccessCallback);
			GameEvents.Contract.onFailed.Remove(OnContractFailCallback);
		}

		protected override void OnLoad(ConfigNode node)
		{
			base.OnLoad(node);

			contractTitle = node.GetValue("contractTitle");
		}

		protected override void OnSave(ConfigNode node)
		{
			base.OnSave(node);

			node.AddValue("contractTitle", contractTitle);

		}

		private void OnContractSuccessCallback(Contract contract)
		{
			if (contract.Title.Equals(contractTitle))
			{
				SetComplete();
			}
		}

		private void OnContractFailCallback(Contract contract)
		{
			if (contract.Title.Equals(contractTitle))
			{
				SetFailed();
			}
		}
	}
}

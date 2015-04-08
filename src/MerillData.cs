using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Contracts;

namespace KspMerillEngineFail
{
	//public class MerilData : IPersistenceLoad, IPersistenceSave
	//{

	//	public static int maxNBSecBurnAtmo = 0;
	//	public static int maxNBSecBurnVac = 0;
	//	public static int maxNBSecBurnAtmoAfterRestart = 0;
	//	public static int maxNBSecBurnVacAfterRestart = 0;
	//	public static int maxNBSecBurnAtmoBeforeRestart = 0;
	//	public static int maxNBSecBurnVacBeforeRestart = 0;
	//	public static int maxNBRestart = 0;
	//	public static int nbTestFailureAllEngine = 0;

	//}


	[KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
	public class ScenarioLoader : MonoBehaviour
	{
		public static ScenarioLoader me = null;

		private static bool alreadyStarted = false;

		public void Start()
		{
			bool scenarioExists = !HighLogic.CurrentGame.scenarios.All(scenario => scenario.moduleName != typeof(MerillData).Name);
			if (!scenarioExists)
			{
				DontDestroyOnLoad(this);

				alreadyStarted = true;
				print("[MERILL] addon start ");
				me = this;

				//print("[MERILL] addon start2 ");
				//print("[MERILL] addon start " + HighLogic.CurrentGame);
				//print("[MERILL] addon start ");
				for (int i = 0; i < HighLogic.CurrentGame.scenarios.Count; i++)
				{
					ProtoScenarioModule fx = HighLogic.CurrentGame.scenarios[i];
					print("[Merill] module partfx: " + HighLogic.CurrentGame.scenarios[i].moduleName
						+ " , type=" + HighLogic.CurrentGame.scenarios[i].GetType());
				}

				//lauch scenario
				HighLogic.CurrentGame.AddProtoScenarioModule(typeof(MerillData), new GameScenes[] { 
					GameScenes.SPACECENTER, GameScenes.FLIGHT, GameScenes.EDITOR, GameScenes.TRACKSTATION });

				//remove not-wanted contract

				////Code for saving to the persistence.sfs
				//ProtoScenarioModule scenario = HighLogic.CurrentGame.scenarios.Find(s => s.moduleName == typeof(MerillData).Name);
				//if (scenario == null)
				//{
				//	try
				//	{
				//		Debug.Log("[MERILL] Adding InternalModule scenario to game '" + HighLogic.CurrentGame.Title + "'");
				//		HighLogic.CurrentGame.AddProtoScenarioModule(typeof(MerillData), new GameScenes[] { GameScenes.FLIGHT, GameScenes.SPACECENTER, GameScenes.EDITOR, GameScenes.TRACKSTATION });
				//		// the game will add this scenario to the appropriate persistent file on save from now on
				//	}
				//	catch (ArgumentException ae)
				//	{
				//		Debug.LogException(ae);
				//	}
				//	catch
				//	{
				//		Debug.Log("[MERILL] Unknown failure while adding scenario.");
				//	}
				//}

				print("[MERILL] addon instance:  " + me + " ? ");

			}
			Destroy(this);
		}

	}

	public class MerillData : ScenarioModule
	{

		private static bool alreadyStarted = false;
		public static MerillData instance = null;

		static int fib1 = 1;
		static int fib2 = 87679;


		// ******  common part test ******

		[KSPField(isPersistant = true)]
		public int nbPartDestroy = 0;

		[KSPField(isPersistant = true)]
		public int nbPartTested = 0;

		//save this in the cfg /globclass
		//[KSPField(isPersistant = true)]
		public List<String> partNameCrashed = new List<String>();

		//[KSPField(isPersistant = true)]
		public List<String> partNameTested = new List<String>();

		// ***** engine test **** //

		//public Dictionary<string, int> maxNBSecBurnAtmo;
		//public Dictionary<string, int> maxNBSecBurnVac;
		//public Dictionary<string, int> nbRestartAtmo;
		//public Dictionary<string, int> nbRestartVac;

		public Dictionary<string, int> maxNBSecBurn;
		public Dictionary<string, int> nbRestart;

		//for pseudo-rand function
		public Dictionary<string, int> notAleatIndex = new Dictionary<string, int>();

		//TODO: test using onawake
		public void Start()
		{
			if (!alreadyStarted || instance == null)
			{
				alreadyStarted = true;
				//print("[MERILL] DATA starting!!! because " + alreadyStarted + " , " + instance);
				DontDestroyOnLoad(this);
				GameEvents.Contract.onContractsLoaded.Add(new EventVoid.OnEvent(removeBoringBTSMContract));

				instance = this;
			}
			else
			{
				//print("[MERILL] DATA starting DESTROY? " + this + " ? " + instance + " =?= "
				//	+ System.Object.ReferenceEquals(this, instance));
				//Destroy(this);
			}
		}


		public void removeBoringBTSMContract()
		{
			Type toDel = null;
			foreach (Type contractT in ContractSystem.ContractTypes)
			{
				if (contractT.Name.Equals("BTSMContractPartTest"))
				{
					toDel = contractT;
					break;
				}
			}
			if (toDel != null)
			{
				ContractSystem.ContractTypes.Remove(toDel);
			}
		}

		public override void OnLoad(ConfigNode node)
		{
			base.OnLoad(node);
			instance = this;

			//load
			if (node.HasValue("partNameCrashed"))
				partNameCrashed.AddRange(node.GetValues("partNameCrashed"));
			if (node.HasValue("partNameTested"))
				partNameTested.AddRange(node.GetValues("partNameTested"));

			maxNBSecBurn = new Dictionary<string, int>();
			loadDictionnary(maxNBSecBurn, "maxNBSecBurn", node);
			//print("[MERILL]DATA Onload maxNBSecBurn count:" + maxNBSecBurn.Count);

			nbRestart = new Dictionary<string, int>();
			loadDictionnary(nbRestart, "nbRestart", node);
			//print("[MERILL]DATA Onload nbRestart count:" + nbRestart.Count);

			loadDictionnary(notAleatIndex, "notAleatIndex", node);

			//print("[MERILL]DATA Onload:" + node.name + " contain name " + node.GetValue("name"));
			//print("[MERILL]DATA Onload partNameCrashed:" + partNameCrashed.Count);


			//reload part info (because now i can display the right numbers)
			foreach (AvailablePart aPart in PartLoader.Instance.parts)
			{
				//print("[MERILL]recompute info for " + aPart.name);
							
				if (aPart.partPrefab != null && aPart.partPrefab.Modules != null)
				{
					foreach (PartModule pm in aPart.partPrefab.Modules)
					{
						if (pm is MerillAbstractPartTest)
						{
							((MerillAbstractPartTest)pm).recomputeInfoMsg();
						}
					}
				}
			}
		}

		public override void OnSave(ConfigNode node)
		{
			//print("[MERILL]DATA OnSave:" + node.name + " contain name " + node.GetValue("name"));

			node.RemoveValue("partNameCrashed");
			//if (!node.HasValue("partNameCrashed")) 
			//	node.AddValue("partNameCrashed", "boudin");

			//node.RemoveValues("partNameCrashed");
			for (int i = 0; i < partNameCrashed.Count; i++)
			{
				node.AddValue("partNameCrashed", partNameCrashed[i]);
			}

			node.RemoveValue("partNameTested");
			//if (!node.HasValue("partNameTested"))
			//	node.AddValue("partNameTested","mdrrr");
			for (int i = 0; i < partNameTested.Count; i++)
			{
				node.AddValue("partNameTested", partNameTested[i]);
			}

			saveDictionnary(maxNBSecBurn, "maxNBSecBurn", node);
			saveDictionnary(nbRestart, "nbRestart", node);
			saveDictionnary(notAleatIndex, "notAleatIndex", node);


			base.OnSave(node);
		}

		private void saveDictionnary(Dictionary<string, int> dico, string name, ConfigNode root)
		{
			ConfigNode nodeDico = new ConfigNode(name);
			foreach (KeyValuePair<string, int> entry in dico)
			{
				nodeDico.AddValue(entry.Key, entry.Value);
				//print("[MERILL]DATA OnSave try save val" + entry.Key+", "+entry.Value);
			}
			root.AddNode(nodeDico);
			//print("[MERILL]DATA OnSave try save node" + nodeDico + " into " + root);
		}

		private void loadDictionnary(Dictionary<string, int> dico, string name, ConfigNode root)
		{
			//print("[MERILL]DATA Onload try load node meeeeueh " + name);
			//print("[MERILL]DATA Onload try load node" + name + ": " + root.HasNode(name));//+" && "+root.GetNode(name));
			//print("[MERILL]DATA Onload try load node(0)" + name);
			if (root.HasNode(name))
			{
				//print("[MERILL]DATA Onload try load node(1) " + root);
				//print("[MERILL]DATA Onload try load node(2) " + name + ": " +root.GetNode(name));
				if (root.GetNode(name) != null)
				{
					//print("[MERILL]DATA Onload node=" + root.GetNode(name));
					ConfigNode nodeDico = root.GetNode(name);
					//print("[MERILL]DATA Onload " + name + ": " + nodeDico.CountValues + ", " + nodeDico.values.Count);
					if (nodeDico.values != null)
						foreach (ConfigNode.Value val in nodeDico.values)
						{
							//print("[MERILL]DATA Onload v " + val);
							if (val != null)
							{
								//print("[MERILL]DATA Onload val " + val.name + "  " + val.value);
								dico[val.name] = Convert.ToInt32(val.value);
							}
						}
				}
			}
			//print("[MERILL]DATA Onload END try load node " + name);
		}


		public static int getPseudoRand4PartTest()
		{
			int nb2 = fib1 + fib2;
			fib1 = fib2;
			fib2 = nb2;
			return nb2;
		}

		static private int[] arrayNotAleat = new int[]{
		4, 85, 34, 71, 15, 50, 40, 96, 23, 59, 
		3, 81, 31, 73, 11, 89, 49, 67, 43, 61, 
		1, 97, 30, 77, 20, 53, 9, 90, 36, 64, 
		47, 83, 27, 76, 21, 54, 10, 91, 39, 65, 
		0, 98, 29, 79, 18, 72, 12, 86, 38, 63, 
		44, 57, 5, 93, 19, 74, 32, 87, 13, 66, 
		25, 80, 7, 58, 45, 94, 33, 88, 14, 51, 
		24, 70, 8, 82, 2, 95, 41, 60, 35, 52, 
		17, 68, 26, 75, 6, 92, 46, 62, 37, 55, 
		16, 69, 22, 99, 28, 78, 48, 84, 42, 56, 
		};

		[KSPField(isPersistant = true)]
		private int notAleatCase = 0;


		public int get0to99FromNotAleatTable()
		{
			int retVal = arrayNotAleat[notAleatCase % 100];
			notAleatCase++;
			if (notAleatCase >= 100)
			{
				//too random
				//generateAnotherNotAleatTable();
				notAleatCase = 0;
			}
			return retVal;
		}


		public int get0to99FromNotAleatTable(string groupId)
		{
			if (!notAleatIndex.ContainsKey(groupId))
			{
				//init to random
				notAleatIndex[groupId] = get0to99FromNotAleatTable();
			}
			int retVal = arrayNotAleat[notAleatIndex[groupId] % 100];
			notAleatIndex[groupId]++;
			if (notAleatIndex[groupId] >= 100)
			{
				//too random
				//generateAnotherNotAleatTable();
				//less random
				notAleatIndex[groupId] = 0;
			}
			return retVal;
		}
		public static void generateAnotherNotAleatTable()
		{
			List<int> allpick = new List<int>();
			do
			{
				allpick = try2GenerateTable();
			} while (allpick.Count != 100);
			arrayNotAleat = allpick.ToArray();
		}

		private static List<int> try2GenerateTable()
		{
			System.Random rand = new System.Random();

			List<int> allpick = new List<int>();
			int previous = Math.Abs(rand.Next(100));
			allpick.Add(previous);
			LinkedList<int> previousX = new LinkedList<int>();
			previousX.AddLast(previous);
			previousX.AddLast(Math.Abs(rand.Next(100)));
			previousX.AddLast(Math.Abs(rand.Next(100)));
			previousX.AddLast(Math.Abs(rand.Next(100)));
			previousX.AddLast(Math.Abs(rand.Next(100)));
			previousX.AddLast(Math.Abs(rand.Next(100)));
			previousX.AddLast(Math.Abs(rand.Next(100)));
			int notAdd = 0;
			while (notAdd < 1000)
			{
				notAdd++;
				int next = Math.Abs(rand.Next(100));
				// do not pick another int next to the others recent slot (a slot is 5*2=>10)
				bool tested = true;
				foreach (int prev in previousX)
				{
					tested = tested && (Math.Abs(next - prev) > 5);
				}
				if (tested)
					// prevent 0->51->11
					if (Math.Abs(Math.Abs(50 - next) - Math.Abs(50 - previous)) < 30)
						// balance between <50 and >50
						if ((50 - next) * (50 - previous) <= 0)
							if (!allpick.Contains(next))
							{
								previous = next;
								allpick.Add(next);
								previousX.AddLast(next);
								while (previousX.Count > 7)
								{
									previousX.RemoveFirst();
								}
								notAdd = 0;
							}

			}
			return allpick;
		}

		static public string situation2String(ExperimentSituations situation)
		{
			switch (situation)
			{
				case ExperimentSituations.FlyingLow:
					return str_space_low;
				case ExperimentSituations.FlyingHigh:
					return str_space_high;
				case ExperimentSituations.SrfLanded:
					return str_landed;
				default:
					return "space";
			}
		}
		
		static public string str_atmo = "atmosphere";
		static public string str_vac = "vaccum";
		static public string str_inatmo = " in the atmosphere";
		static public string str_invac = " in the vaccum";
		static public string str_space_low = "low space";
		static public string str_space_high = "high space";
		static public string str_landed = "landed";
		static public string str_agent_secretary = "The Secretary General of Kerbin";
		static public string str_munm_title = "Plant a flag on the Mun! For the glory of kerbalkind!";
		static public string str_munm_descr = "The Secretary General wants you on the Mun to ensure his reelection: he is on ballot for the next election. To change opinion in his favor, he requires that we land a Kerbal on the Mun. You have 24 hours, that is to say the time left before the final televised debate, to achieve this feat.";
		static public string str_munmSynopsis = "I was able to obtain different deals,  such as X200 tanks at low prices, high power rcs thruster prototype, and some other things. New experimental pieces are available specifically for this mission. They all have been added to the research center. To search for new technology, you can perform scientific experiments around the moon (be carefull to check the research center before, all biome are not available). Remember then to perform the photographic missions to earn upgrades for your production line and research center.";
		static public string str_munm_success = "Nice one! you have won this game!";
		static public string str_munm_success_param = "Success of '{0}'";
		static public string str_camera_cannot = "The mission's control center refuses to give the order to take these pictures: {0} Please fulfill the contract's conditions {0}.";
		static public string str_camerafail_0 = "the light is horrible!";
		static public string str_camerafail_1 = "the angle is not good.";
		static public string str_camerafail_2 = "there is too much noise.";
		static public string str_camerafail_3 = "the focus is not corrected.";
		static public string str_camerafail_4 = "My astrologer has forbidden me to take this picture.";
		static public string str_camerafail_5 = "you must enlarge.";
		static public string str_camerafail_6 = "you must zoom out.";
		static public string str_camera_noSlot = "The camera has a malfunction. Peraps the probe can't protect it enough.\nTry to put it on a probe/pod with science slot and sufficient power next time.";
		static public string str_camera_success = "Picture taken! May it inspire the people of Kerbin.";
		static public string str_camera_successLong = "Your pics have inspire the kerbal kind! We have now more engineers to work bénévolement to build our rockets.";
		static public string str_camera_button = "Take a picture";
		static public string str_camera_title = "Take a picture of {0}";
		static public string str_camera_title_withkerbal = " at {0} with a Kerbal in the foreground";
		static public string str_camera_title_param = "Take a picture of {0} at {1}";
		static public string str_camera_title_param_withkerbal = " with a Kerbal in the foreground";
		static public string str_camera_description = "We need more Kerbals to build our rockets! Our best psychologists believe that the best way to improve our recruitment would be to spur on competent people with beautiful photos of our Mun.";
		static public string str_camera_synopsis = "Earn 1 upgrade point for each reputation point, to be able to build or research faster.";
		static public string str_camera_synopsis_withKerbal = "Warning: The camera must be mounted on the command pod and uses all scientific slots when taking a photo.";
		static public string str_camera_synopsis_withoutKerbal = " Warning: The camera uses all scientific slots when taking a photo, it must be mounted on the control module.";
		static public string str_enginetest_restart_begin = "Current test ongoing for {0}. Please let it run for at least 4 seconds to gather enough data.";
		static public string str_enginetest_restart_failOK = "A failure has just occured for engine {0} in the {1} for boot number {2}. Test data have been transmitted, you can now use this propeller for this number of starts. Trust your wonderful engineers!";
		static public string str_enginetest_restart_failKO = "A failure has just occured for engine {0} in the {1} for boot number {2}. No test data received : check that the test modules are present and that an antenna is present.";
		static public string str_enginetest_restart_successOK = "Engine {0} successfully tested{1} for start number {2}.";
		static public string str_enginetest_restart_successKO = "Engine {0} as not encountered any problem{1} for the start number {2} but be careful because we do not know why.";
		static public string str_enginetest_duration_begin = "Engine {0} is now being tested{1} for a record duration of thrust.";
		static public string str_enginetest_duration_failOK = "Engine {0} has encountered a problem{1} at the {2}th seconds. Test data have been sent, you can now use this engine for a duration less or equal to that.";
		static public string str_enginetest_duration_failKO = "Engine {0} has encountered a problem{1} at the {2}th seconds. No test data received : check that the test modules are present and that an antenna is present.";
		static public string str_enginetest_duration_successOK = "Engine {0} successfully tested{1} for a period of {2} seconds.";
		static public string str_enginetest_duration_successKO = "Engine {0} is running{1} for a record time ({2} seconds), but no test data could be recovered. Pay attention to the kraken.";
		static public string str_science_fail = "The {0} is not able to work anymore, something delicate or fragile must be broken.";
		static public string str_mirror_fail = "The mirror could not be deployed: it must be landed on the moon.";
		static public string str_part_testOK = "Piece {0} has just be tested and the data received. You can now use it with confidence{1}";
		static public string str_part_failOK = "Piece {0} did not work as expected. The data has been received and will allow us to improve the design. You will need retesting{1} to ensure the effectiveness of the patch.";
		static public string str_part_failKO = "Piece {0} did not work as expected{1}. No data received. You should redo a test correctly.";
		static public string str_part_testKO = "Piece {0} worked as intended{1} but no data could be recovered. You should do another test to confirm the characteristics.";

	}


}

using Harmony;
using BattleTech;
using BattleTech.UI;
using JetBrains.Annotations;
using System;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Collections.Generic;

namespace LoanSharks
{
	static class LoanSharks_Repos
    {
		public static K FindFirstKeyByValue<K, V>(this Dictionary<K, V> dict, V val)
		{
			return dict.FirstOrDefault(entry => EqualityComparer<V>.Default.Equals(entry.Value, val))
					.Key;
		}

		public static MechDef GetMechDef(string mechID, SimGameState __instance)
		
			{
			List<MechDef> list = new List<MechDef>(__instance.ActiveMechs.Values);
			if (__instance.ActiveMechs == null)
				{
					return null;
				}
				for (int i = 0; i < list.Count; i++)
				{
					MechDef mechDef = list[i];
					if (mechDef != null && string.Compare(mechID, mechDef.Description.Id) == 0) //&& (string.IsNullOrEmpty(uniqueID) || string.Compare(uniqueID, mechDef.GUID) == 0))
					{
						return mechDef;
					}
				}
				return null;
			}

		[HarmonyPatch(typeof(BattleTech.SimGameState), "ApplyEventAction")]
		public static class ApplyEventAction_Patch
		{
			public static bool Prefix(SimGameResultAction action, object additionalObject)
			{
				
				{
					if (action.additionalValues==null)
					{
						ModState.RemoveMechFlag = "FALSE";
					}
					else
					ModState.RemoveMechFlag = action.additionalValues[0];
				}
				
				return true;
			}

			public static void Postfix(SimGameResultAction action, object additionalObject)
			{
				ModState.RemoveMechFlag = "FALSE";

				return;
			}
		}


		[HarmonyPatch(typeof(BattleTech.SimGameState), "AddMechByID")]
		public static class AddMechByID_Patch
		{
			
			public static bool Prefix(SimGameState __instance, string id, bool active)
			
			{
				//SimGameResultAction.;
				//ModState.RemoveMechFlag = simGameResultAction.additionalValues[0];
				//sgra.value
				if (ModState.RemoveMechFlag != "RemoveMech")
				{ 
					return true; 
				}
				else
				{
					MechDef mechDef = GetMechDef(id, __instance);
					if (__instance.ActiveMechs.ContainsValue(mechDef)==true)
					{
						int index = __instance.ActiveMechs.FindFirstKeyByValue(mechDef);
						if (ModInit.Settings.StoreBeforeRemove == true)
						{
							__instance.UnreadyMech(index, mechDef);
							MechDef mech = new MechDef(__instance.DataManager.MechDefs.Get(id));
							string MechStatID = "Item.MechDef.";
							MechStatID += mech.ChassisID;
							__instance.CompanyStats.ModifyStat<int>("SimGameState", 0, MechStatID, StatCollection.StatOperation.Int_Subtract, 1, -1, true);
							if (ModInit.Settings.NoNegativeStats == true)
							{
								if (__instance.CompanyStats.GetValue<int>(MechStatID) < 0)
								{
									__instance.CompanyStats.Set<int>(MechStatID, 0);
								}
							}
						}
						else
						{
							__instance.ActiveMechs.Remove(index);
						}
					}
					else
					{
						MechDef mech = new MechDef(__instance.DataManager.MechDefs.Get(id));
						string MechStatID = "Item.MechDef.";
						MechStatID += mech.ChassisID;
						__instance.CompanyStats.ModifyStat<int>("SimGameState", 0, MechStatID, StatCollection.StatOperation.Int_Subtract, 1, -1, true);
					if (ModInit.Settings.NoNegativeStats == true)
						{
							if (__instance.CompanyStats.GetValue<int>(MechStatID) < 0)
							{
								__instance.CompanyStats.Set<int>(MechStatID, 0);
							}
						}
					}
					
					return false;
				}
				
			}
		}
	}

}
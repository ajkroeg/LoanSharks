using Harmony;
using BattleTech;
using JetBrains.Annotations;

namespace LoanSharks
{
	static class LoanSharks
    {
		[HarmonyPatch(typeof(BattleTech.SimGameState), "ApplyEventAction")]

		public static class ApplyEventAction_Patch
		{
			public static bool Prefix(SimGameState __instance, SimGameResultAction action, object additionalObject)
			
			{
			
				SimGameState simulation = UnityGameInstance.BattleTechGame.Simulation;
				bool result = false;
				if (simulation == null)
				{
					return result;
				}
				switch (action.Type)
				{
					case SimGameResultAction.ActionType.Mech_Damage_Location:
						{
							//if (__instance.DataManager.MechDefs.Exists(action.value))
							{
								MechDef mech = new MechDef(__instance.DataManager.MechDefs.Get(action.value), __instance.GenerateSimGameUID(), true);
								//var myKey = types.FirstOrDefault(x => x.Value == "one").Key;
								__instance.RemoveMech(1, mech, false);
							}
							
						}

						return true;
				}
				return true;

			}
		}
	}

}
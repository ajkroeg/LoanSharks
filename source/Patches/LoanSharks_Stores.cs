using Harmony;
using BattleTech;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using Localize;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace LoanSharks
{
	static class LoanSharks
	{
		[HarmonyPatch(typeof(BattleTech.Shop), "GetPrice")]

		public static class GetPrice_Patch
		{
			public static bool Prefix(Shop __instance, ref int __result, ShopDefItem item, Shop.PurchaseType purchaseType, Shop.ShopType shopType)
			{
				DescriptionDef itemDescription = __instance.GetItemDescription(item);
				if (itemDescription == null)
				{
					Debug.LogError("Error :: Shop.GetPrice() GetItemDescription on: " + item.ID + " returned a NULL");
					__result = 0;
					return false;

				}
				var this_system = Traverse.Create(__instance).Field("system").GetValue<StarSystem>();
				float num = (float)itemDescription.Cost;
				float shop = this_system.Discount.Shop;
				if (num <= 0)
				{
					__result = Mathf.RoundToInt(num);
					return false;
				}
				if (purchaseType != Shop.PurchaseType.Normal)
				{
					num *= item.DiscountModifier;
				}
				int num2 = Mathf.RoundToInt(num + (num * shop));
				float num3;
				var this_shop = Traverse.Create(__instance).Field("Sim").GetValue<SimGameState>();
				if (shopType != Shop.ShopType.Faction)
				{
					if (shopType != Shop.ShopType.BlackMarket)
					{
						num3 = this_shop.GetReputationShopAdjustment(this_system.Def.OwnerValue) * num;
					}
					else
					{
						num3 = this_shop.GetReputationShopAdjustment(FactionEnumeration.GetAuriganPiratesFactionValue()) * num;
					}
				}
				else
				{
					num3 = this_shop.GetReputationShopAdjustment(this_system.Def.FactionShopOwnerValue) * num;
				}
				__result = Mathf.CeilToInt(Mathf.Clamp((float)num2 + num3, 0f, 1E+09f));
				return false;
			}
		}





		[HarmonyPatch(typeof(BattleTech.Shop), "GetAllInventoryShopItems")]
		public static class GetAllInventoryShopItems_Patch
		{
			public static bool Prefix(Shop __instance, ref List<ShopDefItem> __result)
			{
				List<ShopDefItem> list = new List<ShopDefItem>();
				var this_shop = Traverse.Create(__instance).Field("Sim").GetValue<SimGameState>();
				foreach (ChassisDef chassisDef in this_shop.GetAllInventoryMechDefs(false))
				{
					ShopDefItem shopDefItem = new ShopDefItem();
					shopDefItem.ID = chassisDef.Description.Id;
					shopDefItem.Count = this_shop.GetItemCount(chassisDef.Description, typeof(MechDef), SimGameState.ItemCountType.ALL);
					float num = (float)chassisDef.Description.Cost;
					shopDefItem.SellCost = Mathf.FloorToInt(num * this_shop.Constants.Finances.ShopSellModifier);
					shopDefItem.Type = ShopItemType.Mech;
					list.Add(shopDefItem);
				}
				foreach (MechComponentRef mechComponentRef in this_shop.GetAllInventoryItemDefs())
				{
					int num2 = 0;
					int num3 = 0;
					if (mechComponentRef.DamageLevel == ComponentDamageLevel.NonFunctional)
					{
						num3 = this_shop.GetItemCount(mechComponentRef.Def.Description, mechComponentRef.Def.GetType(), SimGameState.ItemCountType.DAMAGED_ONLY);
					}
					else
					{
						num2 = this_shop.GetItemCount(mechComponentRef.Def.Description, mechComponentRef.Def.GetType(), SimGameState.ItemCountType.UNDAMAGED_ONLY);
					}
					float num4 = (float)mechComponentRef.Def.Description.Cost;
					ShopDefItem shopDefItem2 = new ShopDefItem();
					shopDefItem2.ID = mechComponentRef.Def.Description.Id;
					ComponentType componentType = mechComponentRef.Def.ComponentType;
					shopDefItem2.Type = Shop.ComponentTypeToStopItemType(componentType);
					if (num2 > 0)
					{
						if (num4 <= 0)
						{
							shopDefItem2.SellCost = Mathf.FloorToInt(num4 - ModInit.Settings.LoanPaybackFee);
							shopDefItem2.Count = num2;
							list.Add(shopDefItem2);
						}
						else
						{
							shopDefItem2.SellCost = Mathf.FloorToInt(num4 * this_shop.Constants.Finances.ShopSellModifier);
							shopDefItem2.Count = num2;
							list.Add(shopDefItem2);
						}

					}
					if (num3 > 0)
					{
						list.Add(new ShopDefItem(shopDefItem2)
						{
							SellCost = Mathf.FloorToInt(num4 * this_shop.Constants.Finances.ShopSellDamagedModifier),
							IsDamaged = true,
							Count = num3
						});
					}
				}
				__result = list;
				return false;
			}
		}
		[HarmonyPatch(typeof(BattleTech.SimGameState), "GetExpenditures")]
		[HarmonyPatch(new Type[] { typeof(EconomyScale),typeof(bool)})]
		public static class GetExpenditures_Patch
		{
			public static int Postfix(int num, SimGameState __instance, EconomyScale expenditureLevel, bool proRate = false)
			{
				
				//int numdebt = __instance.CompanyStats.GetValue<int>("Item.HeatSinkDef.Gear_HeatSink_Generic_Standard");
				int ctdebt = __instance.CompanyStats.GetValue<int>("Item.WeaponDef.Weapon_Autocannon_AC10_0-STOCK");
				int debt = ctdebt * ModInit.Settings.LoanInterestMultiplier;
				//__instance.CompanyStats.AddStatistic<string>(ModState.InterestFromLoans.ToString(),"0");
				ModState.InterestFromLoans = debt;
				ModState.MonthlyExpenditures = debt + num;				
				return debt + num;
			}
			
		}
		[HarmonyPatch(typeof(BattleTech.SimGameState), "OnDayPassed")]
		public static class OnDayPassed_Patch
		{
			public static void Postfix(SimGameState __instance, int timeLapse)
			{
				int num = (timeLapse > 0) ? timeLapse : 1;
				__instance.CompanyStats.AddStatistic<string>("DaysInDebt", ModState.DaysInDebt.ToString());
				if (ModState.InterestFromLoans > 0)
				{
					ModState.DaysInDebt += num;
				}
				else
				{
					ModState.DaysInDebt = 0;
				}
				__instance.CompanyStats.Set<string>("DaysInDebt", ModState.DaysInDebt.ToString());
			}
		}


		[HarmonyPatch(typeof(BattleTech.UI.SGCaptainsQuartersStatusScreen), "RefreshData")]
		[HarmonyBefore(new string[]
	{
		"us.frostraptor.IttyBittyLivingSpace",
		"de.morphyum.MechMaintenanceByCost",
		"dZ.Zappo.MonthlyTechAdjustment"
	})]
		public static class RefreshData_Patch
		{
			public static void Postfix(SGCaptainsQuartersStatusScreen __instance, EconomyScale expenditureLevel, bool showMoraleChange,
				Transform ___SectionOneExpensesList, TextMeshProUGUI ___SectionOneExpensesField, SimGameState ___simState)
			
			{
				SimGameState simulation = UnityGameInstance.BattleTechGame.Simulation;
				bool flag = __instance == null || ___SectionOneExpensesList == null || ___SectionOneExpensesField == null || simulation == null;
				List<KeyValuePair<string, int>> list = LoanSharks.GetCurrentKeys(___SectionOneExpensesList, ___simState);
				//List<KeyValuePair<string, int>> list = new List<KeyValuePair<string, int>>();
				LoanSharks.ClearListLineItems(___SectionOneExpensesList, ___simState);
				string name = "Interest From Loans";
				int value = ModState.InterestFromLoans;//___simState.CompanyStats.GetValue<int>("Item.HeatSinkDef.Gear_HeatSink_Generic_Standard");
				int ongoingUpgradeCosts = 0;
				list.Add(new KeyValuePair<string,int>(name, value));
				list.ForEach(delegate (KeyValuePair<string, int> entry)
				{
					ongoingUpgradeCosts += entry.Value;
					LoanSharks.AddListLineItem(___SectionOneExpensesList, ___simState, entry.Key, SimGameState.GetCBillString(entry.Value));
				});
				___SectionOneExpensesField.SetText(SimGameState.GetCBillString(ongoingUpgradeCosts));
			}
	
			
		}

		private static void AddListLineItem(Transform list, SimGameState ___simState, string key, string value)
		{
			GameObject gameObject = ___simState.DataManager.PooledInstantiate("uixPrfPanl_captainsQuarters_quarterlyReportLineItem-element", BattleTechResourceType.UIModulePrefabs, null, null, list);
			SGKeyValueView component = gameObject.GetComponent<SGKeyValueView>();
			gameObject.transform.localScale = Vector3.one;
			component.SetData(key, value);
		}
		public static List<KeyValuePair<string, int>> GetCurrentKeys(Transform container, SimGameState sgs)
		{//ALL THE THANKS to FrostRaptor for letting me pirate his code here.
			List<KeyValuePair<string, int>> list = new List<KeyValuePair<string, int>>();
			IEnumerator enumerator = container.GetEnumerator();
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform transform = (Transform)obj;
					SGKeyValueView component = transform.gameObject.GetComponent<SGKeyValueView>();
					Traverse traverse = Traverse.Create(component).Field("Key");
					TextMeshProUGUI textMeshProUGUI = (TextMeshProUGUI)traverse.GetValue();
					string text = textMeshProUGUI.text;
					Traverse traverse2 = Traverse.Create(component).Field("Value");
					TextMeshProUGUI textMeshProUGUI2 = (TextMeshProUGUI)traverse2.GetValue();
					string text2 = textMeshProUGUI2.text;
					string text3 = Regex.Replace(text2, "[^\\d]", "");
					int num = int.Parse(text3);
					KeyValuePair<string, int> item = new KeyValuePair<string, int>(text, num);
					list.Add(item);
				}
			}
			return list;
		}
		private static void ClearListLineItems(Transform container, SimGameState ___simState)
		{
			List<GameObject> list = new List<GameObject>();
			IEnumerator enumerator = container.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform transform = (Transform)obj;
					list.Add(transform.gameObject);
				}
			}
			finally
			{
				IDisposable disposable;
				bool flag = (disposable = (enumerator as IDisposable)) != null;
				if (flag)
				{
					disposable.Dispose();
				}
			}
			while (list.Count > 0)
			{
				GameObject gameObject = list[0];
				___simState.DataManager.PoolGameObject("uixPrfPanl_captainsQuarters_quarterlyReportLineItem-element", gameObject);
				list.Remove(gameObject);
			}
		}
	}
}
	

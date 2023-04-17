# LoanSharks
 Loans/Debts and Loaner mechs for HBS BT

**v1.1.0.0 and higher require modtek v3 or higher**

This first part of this mod allows for use of negative-valued items (loans) in stores. "Purchasing" a loan will add the C-bill value of the loan to company funds, while "selling" (repaying) a loan will subtract the value of the loan + fee from company funds. In addition, monthly interest on the loans are deducted at the end-of-month finances screen, calculated as a multiplier of the number of loan items in company inventory. In addition, a Company Stat (`DaysInDebt`) is initialized tracking the number of days a player has held loans. This stat is intended for use in firing debt-related events, and is reset to 0 any time there are 0 loans in the company inventory. A stat check on the inventory count of debt items can also be used to control events.

The second part of this mod patches the method `AddMechByID`, itself used by event action `Mech_AddRoster`. The patch allows for an additionalValue to be included in the event .json that <i>removes</i> said mech from the player rather than adding it. If the additionalValue is the string `"RemoveMech"`, a mech matching the action value will be removed. If the additionalValue is null or any other string, that mech will be added (vanilla behavior).:

```
"Actions" : [
	{
		"Type" : "Mech_AddRoster",
		"value" : "mechdef_jenner_JR7-D",
		"valueConstant" : null,
		"additionalValues" : [
			"RemoveMech"
		]
	}
],
```
In the above case, a Jenner JR7-D will be removed from the players Active (Ready) Mechbay if present. If absent, the number of Jenner JR7-D in the player stored inventory will be reduced by 1 via a stat mod.

If the setting `"StoreBeforeRemove" : true`, the Active mech will be stripped and stored before removing (player keeps all equipment/gear on the mech). If  `"StoreBeforeRemove" : false`, Active mech and gear are both removed from player.

If the setting `"NoNegativeStats": false`, the CompanyStat representing the # of that mech in storage can become a negative value if a Mech not present in storage is removed. This is intended to control firing of later events via a stat check (if stat<0, player has either sold or lost the "loaned" mech in combat, and some negative event would happen as a consequence of them "losing" a mech they didn't "own"). If `"NoNegativeStats": true`, that stat is clamped to a minimum of 0 (unsure of why someone would want that, but the option is there).

Default values, including name of loan item, can be changed in the mod.json settings:

```
"Settings": 	{
		"LoanInterestMultiplier": 100000,   				//Monthly interest cost per loan item
		"LoanPaybackFee": 100000           				//Additional funds subtracted when "selling" loan.
		"LoanItemDefTypeAndID": "Item.UpgradeDef.Gear_Loan_Million"	//name of loan item
		"FreeMoneyItemDefTypeAndID": "Item.UpgradeDef.Gear_Pirate_Tribute", //name of "free" money item to be autoremoved afte purchasing
		"StoreBeforeRemove" : true,					//Are mechs stored/stripped before removing?
		"NoNegativeStats": false					//Can the stored mech count be negative?
		},
```

Using the above settings, a player with 5 loan items would lose ¢500000/month. On repaying a single loan, the player would lose the value of the loan +¢100000.

The `Gear_Loan_Million` item in the Release folder requires MechEngineer, and is not intended to work for users out of the box. Users will need to edit stores to get their own loan items to appear and be purchasable.

The field `FreeMoneyItemDefTypeAndID` defines a custom item that functions similarly to the loan above, but is removed from inventory after it is purchased and is not paid back. Intended for use as a "reward" item in certain custom stores.

Thanks to FrostRaptor (https://github.com/IceRaptor) for the re-use of their code that allows interest payments to be displayed in the finances screen.

# LoanSharks
 Loans and debts for HBS BT

Code to allow for use of negative-valued items (loans) in stores. "Purchasing" a loan will add the C-bill value of the loan to company funds, while "selling" (repaying) a loan will subtract the value of the loan + fee from company funds. In addition, monthly interest on the loans are deducted at the end-of-month finances screen. Interest is currently calculated as a multiplier of the number of loan items in company inventory. In addition, a Company Stat (`DaysInDebt`) is initialized tracking the number of days a player has held loans. This stat is intended for use in firing debt-related events, and is reset to 0 any time there are 0 loans in the company inventory. Optionally, a stat check on the inventory count of debt items can also be used to control events.

Default values, including name of loan item, can be changed in the mod.json settings:

```
"Settings": 	{
		"LoanInterestMultiplier": 100000,   				//Monthly interest cost per loan item
		"LoanPaybackFee": 100000           				//Additional funds subtracted when "selling" loan.
		"LoanItemDefTypeAndID": "Item.UpgradeDef.Gear_Loan_Million"	//name of loan item
		},
```

Using the above settings, a player with 5 loan items would lose ¢500000/month. On repaying a single loan, the player would lose the value of the loan +¢100000.

The `Gear_Loan_Million` item in the Release folder requires MechEngineer, and is not intended to work for users out of the box. Users will need to edit stores to get their own loan items to appear and be purchasable.

Thanks to FrostRaptor (https://github.com/IceRaptor) for the re-use of their code that allows interest payments to be displayed in the finances screen.

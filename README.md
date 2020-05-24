# LoanSharks
 Loans and debts for HBS BT

Code to allow for use of negative-valued items (loans) in stores. "Purchasing" a loan will add the C-bill value of the loan to company funds, while "selling" (repaying) a loan will subtract the value of the loan + fee from company funds. In addition, monthly interest on the loans are deducted at the end-of-month finances screen. Interest is currently calculated as a multiplier of the number of loan items in company inventory. In addition, a Company Stat (`DaysInDebt`) is initialized tracking the number of days a player has held loans. This stat is intended for use in firing debt-related events, and is reset to 0 any time there are 0 loans in the company inventory.

Default values can be changed in the settings in mod.json:

```"Settings": {
		"LoanInterestMultiplier": 10000,   //Monthly interest cost per loan item
		"LoanPaybackFee": 10000            //Additional funds subtracted when "selling loan."
		},
  ```
  In above example, a player with 5 loans would lose ¢50000/month. On repaying a loan, the player would lose the value of the loan +¢10000.

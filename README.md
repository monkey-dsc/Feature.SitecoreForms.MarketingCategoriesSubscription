# Feature.SitecoreForms.MarketingCategoriesSubscription

Take the description out of the blog!!!

* **.NET Framework: *4.7.2***
* **MVC: *5.2.4***
* **SITECORE VERSION: *Sitecore 9.2.0 rev. 002893***

Architectural design decisions:

1. Module
	Contains the application code for the forms module (without explicit references to CM/CD libaries!)
2. Specific
	Contains code explicit for CM/CD role
3. Common
	Contains a shared codebase for the other projects e.g. contracts and utils.

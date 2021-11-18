# Feature.SitecoreForms.MarketingCategoriesSubscription

## Technology Specification

* **.NET Framework: *4.7.2***
* **MVC: *5.2.4***
* **SITECORE VERSION: *Sitecore 9.2.0 rev. 002893***

## Documentation

**Initial question:** *Why such a long name?*
**Simple answer:** *It's in Helix architecture and describes exactly what it does!*

The module might be called *"EXM Newsletter Subscription"*, *"Marketing Automation Extension"* or something like that. But it's much rather a *"Forms Extension"* where visitors sign up exclusively for marketing categories.

These marketing categories are based on the rules engine of Sitecore, they can't be classified as *"EXM specific"*, they can also be used for *"Marketing Automation"* or *"Personalization"*.

In v9.1, Sitecore introduced the *"Marketing Preference Center"*, a really cool way to manage marketing category subscriptions. However, the preference center is even more useful because it can be configured per manager root, is thus multi-site suitable and can look different for each site.

The only question that arises here is: If Sitecore does something great to unsubscribe from newsletters *(or categories)*, why is there no great equivalent for signing up for these categories? The Preference Center is currently only available if you have already received a newsletter *(for example Email Campaign Sample Newsletter)*.

So, what if you want to have exactly what the Preference Center can do? Unfortunately, there is nothing for that out of the box and this module can get in place!

Much theory but once the module is installed it's very easy to use! Read more in my blog post series to understand the concept and the idea behind:
- [Marketing Category Subscription - An introduction](https://camao.one/blog/sitecore-9-1-marketing-category-subscription-introduction/)
- [Marketing Category Subscription - Part 1 - The Concept](https://camao.one/blog/sitecore-9-1-marketing-category-subscription-part-1-concept/)
- [Marketing Category Subscription - Part 2 - Custom Forms Field for Marketing Preferences and the Contact List](https://camao.one/blog/sitecore-9-1-marketing-category-subscription-part-2-custom-field/)
- [Marketing Category Subscription - Part 3 - Custom Marketing Preference Submit Action](https://camao.one/blog/sitecore-9-1-marketing-category-subscription-part-4-custom-marketing-preference/)
- [Marketing Category Subscription - Part 4 - Custom Field to identify Contacts](https://camao.one/blog/sitecore-9-1-marketing-category-subscription-part-4-custom-field-to-identify-contacts/)
- [Marketing Category Subscription - Part 5 - The "Magical" Subscription Form](https://camao.one/blog/sitecore-9-1-marketing-category-subscription-part-5-the-magic-subscription-form/)
- [Marketing Category Subscription - Part 6 - Segmented Lists](https://camao.one/blog/sitecore-9-1-marketing-category-subscription-part-6-segmented-lists/)

Or watch my video from my presentation on the SUG Hungary:

[Sitecore Usergroup Hungary Presentation, September, 17th 2020](https://youtu.be/TyHjCg2taZo)

## Installation

This module makes use of the [Sitecore Message Bus](https://doc.sitecore.com/developers/100/platform-administration-and-architecture/en/message-bus.html), it's a message transport mechanism which allows application roles to communicate across boundaries. Somehow, if you install the module, Sitecore will add some stuff to the messaging database.

> **IMPORTANT NOTE:**
> Ensure that the **"messaginguser"** has *(temporarily)* the correct access rights to the database, e.g. *"db_owner"*!!!
> **Otherwise you will have installation errors.**

If access rights are set to the *"messaginguser"* the installation is quite easy:
1. Open the Update Installation Wizard: *http://{YOURINSTANCE}/sitecore/admin/UpdateInstallationWizard.aspx*
2. Install the *"update package"* suiteable to your Sitecore version, found in the [releases](https://github.com/monkey-dsc/Feature.SitecoreForms.MarketingCategoriesSubscription/releases). Refer to [Compatibility](#compatibility) which version of the module you need *(I'm in a hurry to get up to date!)*.

## Usage

> **NOTE:**
> The module is configured to use *Double-Opt-In* due to GDPR restrictions. You can disable it in the: `Feature.SitecoreForms.MarketingCategoriesSubscription.Settings.config`.

You have to keep in mind that *Segmented Lists* can be created in two ways:
1. **Segmented list from existing list** *(with Double-Opt-In)*
2. **Segmented list from all contacts** *(without Double-Opt-In)*

The module comes with a sample *"Newsletter Subscription Form"* to demonstrate how such a *"Subscription Form"* can look like. If you create a form from the template ensure the following steps:
1. Create an empty *"Contact List"* which acts as the *"Global Subscription List"* for *"Double-Opt-In"*. You can use one subscription list for the whole solution!
2. The *"Marketing Categories Field"* should point to a manager root within your solution, by default the *"Email Types"* preferences are attached to the default manager root.
3. **Double-Opt-In only:** The *"Marketing Categories Field"* should point to the *"Global Subscription List"*
4. There is a *"Submit Action"* which maps the field types to the contact. Please ensure in the settings of the action is mapped to the fields in your form.

![TreeView in the Sitecore Forms Backend](https://github.com/monkey-dsc/Feature.SitecoreForms.MarketingCategoriesSubscription/blob/master/readme_images/TreeViewsFormsBackend.PNG "TreeView in the Sitecore Forms Backend")

![SubmitAction Configuration](https://github.com/monkey-dsc/Feature.SitecoreForms.MarketingCategoriesSubscription/blob/master/readme_images/SubmitAction.PNG "SubmitAction Configuration")

**Thats it!**

You can now start to create your own *"Marketing Categories"*. They are located under `/sitecore/system/Marketing Control Panel/Taxonomies/Marketing categories` and simply add them to your *"Manager Root"*.

## Compatibility

- v1.0.0 - v1.0.2 is for Sitecore 9.2.0
- v1.0.3 is for Sitecore 9.3.0

## Architectural design decisions:

1. **Module**
   Contains the application code for the forms module *(without explicit references to CM/CD libaries!)*
2. **Specific**
   Contains code explicit for CM/CD role
3. **Common**
   Contains a shared codebase for the other projects e.g. contracts and utils.

> **NOTE:**
> If you checkout the repository to do modifications, everytime you build the solution a new *"update package"* will be generated in *"{SOLUTION_DIRECTORY}\src\release\MCS.TDS.Release\bin\Package_Release"*

## Changelog
#### v1.0.3
- Sitecore 9.3.0 Support
- temporary fix: package reference issues in TDS
#### v1.0.2
- Workaround for Issue #5: *MarketingPreferencesService throws a NullReferenceException if the Instance is configured to "ContentManagement" only*
#### v1.0.1
- Some refactorings and optimizations
#### v1.0.0
- Initial release

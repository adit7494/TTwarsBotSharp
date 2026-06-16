# TTWars Migration Audit Report

## Complete Feature Checklist

| # | Feature | Parser | Status | Notes |
|---|---------|--------|--------|-------|
| 1 | Login | LoginParser | ✅ DONE | Supports TTWars React-based login with dialog overlay |
| 2 | Storage/Resources | StorageParser | ✅ DONE | Added GetProductionRates() for TTWars production table |
| 3 | Building Layout dorf1 | BuildingLayoutParser | ✅ DONE | Uses data-aid/data-gid attributes, labelLayer divs |
| 4 | Building Layout dorf2 | BuildingLayoutParser | ✅ DONE | Uses data-aid/data-gid/data-level attributes |
| 5 | Navigation | NavigationBarParser | ✅ DONE | Added href-based fallback, village center detection |
| 6 | Village Panel | VillagePanelParser | ✅ DONE | Uses #sidebarBoxVillagelist with data-did |
| 7 | Upgrade Building | UpgradeParser | ✅ DONE | Uses #contract_building{N}, resourceWrapper |
| 8 | Train Troop | TrainTroopParser | ✅ DONE | Supports TTWars table#troops structure with input[name] |
| 9 | Farm List | FarmListParser | ✅ DONE | Uses #rallyPointFarmList, farmListHeader |
| 10 | Adventure | AdventureParser | ✅ DONE | Supports TTWars table.borderGap.adventureList, data-mapid |
| 11 | Hero Inventory | InventoryParser | ✅ DONE | Supports TTWars data-placeid, equipment slots |
| 12 | Quest | QuestParser | ✅ DONE | Uses #questmasterButton, taskOverview |
| 13 | NPC Resource | NpcResourceParser | ✅ DONE | Uses #npc, npcMerchant |
| 14 | Complete Immediately | CompleteImmediatelyParser | ✅ DONE | Uses div.finishNow |
| 15 | Options | OptionParser | ✅ DONE | Added fallback for #outOfGame, options class detection |
| 16 | Info (Gold/Silver) | InfoParser | ✅ DONE | Uses ajaxReplaceableGoldAmount classes |
| 17 | Building Tabs | BuildingTabParser | ✅ DONE | Uses contentNavi.subNavi |
| 18 | Building Navigation | ToBuildingByLocationCommand | ✅ DONE | Added data-aid fallback for TTWars |
| 19 | Hero Attributes | AdventureParser | ✅ DONE | Uses heroState, statusHome_medium |
| 20 | Number/Timer Parsing | NumberParser | ✅ DONE | Supports HH:MM:SS (+XXX ms) format |

## Parser-by-Parser Changes

### LoginParser
- Added `IsTTWarsLoginPage()` - detects TTWars React-based login
- Added `GetLoginForm()` - handles dialog overlay structure
- Updated `IsIngamePage()` - checks #content classes for TTWars pages

### BuildingLayoutParser
- Updated `GetFields()` - uses data-aid/data-gid attributes as primary, falls back to CSS classes
- Updated `GetInfrastructures()` - uses data-level attribute, labelLayer div for level
- Added `IsDorf1Page()` / `IsDorf2Page()` - content class detection

### NumberParser
- Updated `ToDuration()` - handles TTWars millisecond format `(+XXX ms)`
- Added null/empty string handling

### StorageParser
- Added `GetProductionRates()` - reads #production table
- Added `HasProductionTable()` - detects production table

### UpgradeParser
- Added `GetTTWarsUpgradeButton()` - handles contractLink class
- Added `HasUpgradeButtons()` / `GetBuildDuration()` - helper methods

### TrainTroopParser
- **MAJOR UPDATE**: Added TTWars table#troops structure support
- Added `GetTTWarsInputBox()` - reads input[name="troops[0][t{N}"]
- Added `GetTTWarsMaxAmount()` - reads max from sibling <a> tag
- Updated `GetTrainButton()` - supports #btn_ok fallback
- Added `GetTroopIndex()` - converts TroopEnums to TTWars t1-t11 index

### AdventureParser
- **MAJOR UPDATE**: Added TTWars adventure page support
- Updated `IsAdventurePage()` - detects table.borderGap.adventureList
- Updated `GetHeroAdventureButton()` - detects heroState links
- Updated `CanStartAdventure()` - detects statusHome_medium icon
- Updated `GetAdventureButton()` - reads from adventureList tbody
- Updated `GetAdventureDifficult()` - detects difficulty_normal/difficulty_hard icons

### InventoryParser
- **MAJOR UPDATE**: Added TTWars hero inventory support
- Updated `IsInventoryPage()` - detects heroV2Inventory content class
- Updated `GetHeroAvatar()` - detects hero body image
- Updated `GetItemSlot()` - supports data-placeid attribute
- Added `GetEquipmentSlots()` - reads negative data-placeid values
- Added `GetInventorySlots()` - reads positive data-placeid values

### NavigationBarParser
- Updated `GetDorf1Button()` / `GetDorf2Button()` - multiple fallback strategies
- Added `GetButtonByHref()` - href-based button detection
- Added village center link detection

### VillagePanelParser
- Added `HasVillagePanel()` / `GetVillageCount()` - helper methods

### FarmListParser
- Added `HasFarmList()` / `GetFarmListCount()` / `GetFarmLists()` - helper methods

### InfoParser
- Added `HasAccountInfo()` / `GetServerTime()` - helper methods

### QuestParser
- Added `HasQuestMaster()` / `GetCollectibleQuestCount()` - helper methods

### NpcResourceParser
- Added `HasNpcMerchant()` / `GetResourceInputs()` - helper methods

### CompleteImmediatelyParser
- Added `HasFinishNowButton()` / `HasFinishNowDialog()` - helper methods

### BuildingTabParser
- Added `HasBuildingTabs()` / `GetActiveTabIndex()` / `GetTabNames()` - helper methods

### OptionParser
- Updated `IsContextualHelpEnable()` - detects contextualHelp class
- Updated `GetOptionButton()` - searches for options link by href
- Updated `GetHideContextualHelpOption()` - searches by name/id
- Updated `GetSubmitButton()` - detects green submit buttons
- Updated `IsOptionsPage()` - detects options content class

## Command Changes

### ToBuildingByLocationCommand
- Added `IsEmptySlot()` - detects empty slots via data-gid attribute
- Updated `GetField()` - uses data-aid attribute as fallback
- Updated `GetInfrastructure()` - uses data-aid attribute as primary, XPath as fallback
- Added CSS selector fallback for empty slot SVG paths

## TTWars-Specific HTML Differences Handled

1. **Login Page**: React-based dialog overlay with `#dialogOverlay`, `dialogWrapper`, `dialogV2`
2. **Resource Fields (dorf1)**: Uses `data-aid`, `data-gid` attributes on `<a>` tags
3. **Buildings (dorf2)**: Uses `data-aid`, `data-gid`, `data-building-id`, `data-name` on `<div>` tags
4. **Timer Format**: `HH:MM:SS (+XXX ms)` with millisecond precision
5. **Rally Point**: Uses `table#troops` with `input[name="troops[0][t{N}"]`
6. **Hero Adventures**: Uses `table.borderGap.adventureList` with `data-mapid` buttons
7. **Hero Inventory**: Uses `data-placeid` (negative for equipped, positive for inventory)
8. **Hero Status**: Uses `div.heroState > i.statusHome_medium` instead of `div.heroStatus > i.heroHome`
9. **Options Page**: Uses `div#content.options` class instead of `#outOfGame`
10. **Building Navigation**: Uses `data-aid` attribute for building slot identification

## Remaining Considerations

1. **#stockBar Element**: In TTWars, the stockBar is outside the #content div. The bot uses Selenium to get the full page HTML, so this should work. However, if the stockBar is loaded dynamically via JavaScript, it may need special handling.

2. **#navigation Element**: Similar to stockBar, the navigation bar is outside #content. The bot should handle this via full page HTML.

3. **#sidebarBoxVillagelist Element**: The village sidebar is outside #content. The bot should handle this via full page HTML.

4. **JavaScript-Rendered Content**: TTWars uses React/GraphQL for some pages. The bot uses Selenium which executes JavaScript, so dynamically rendered content should be available.

5. **Anti-CSRF Tokens**: TTWars uses `mpvt_token` hidden field. The bot clicks buttons directly via Selenium rather than submitting forms, so this should not be an issue.

## Test Coverage

Created test fixtures and tests in `MainCore.Test/Parsers/TTWarsParser.Test.cs`:
- Login page detection
- Resource field parsing
- Building layout parsing
- Dorf1/Dorf2 page detection
- Number/timer parsing with milliseconds
- Server type detection from URL

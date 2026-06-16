# TTWars Support Implementation Summary

## Overview
This document summarizes the changes made to add TTWars (speed version of Travian) support to TravianBotSharp.

## Changes Made

### 1. Core Infrastructure

#### New Files
- **`MainCore/Enums/ServerType.cs`** - New enum to distinguish between Travian and TTWars servers

#### Modified Files
- **`MainCore/Entities/Account.cs`** - Added `ServerType` property to Account entity
- **`MainCore/DTO/AccountDto.cs`** - Added `ServerType` property to AccountDto
- **`MainCore/DTO/AccountDetailDto.cs`** - Added `ServerType` property and auto-detection from URL
- **`MainCore/UI/Models/Input/AccountInput.cs`** - Added `ServerType` property to AccountInput
- **`MainCore/UI/Models/Validators/AccountInputValidator.cs`** - Added `ServerTypeDetector` class for URL-based detection

### 2. Parser Updates

All parsers have been updated with TTWars-specific methods and improvements:

#### LoginParser
- Added `IsTTWarsLoginPage()` method for detecting TTWars login pages
- Added `GetLoginForm()` method that handles both standard Travian and TTWars
- Updated `IsIngamePage()` to detect TTWars pages by checking `#content` element classes

#### BuildingLayoutParser
- Updated `GetFields()` to handle TTWars resource field structure (using `data-aid` and `data-gid` attributes)
- Updated `GetInfrastructures()` to handle TTWars building slot structure
- Added `IsDorf1Page()` and `IsDorf2Page()` helper methods
- Improved level extraction from `labelLayer` div (TTWars format)

#### NumberParser
- Improved `ToDuration()` to handle TTWars millisecond format (`HH:MM:SS (+XXX ms)`)
- Added null/empty string handling

#### StorageParser
- Added `GetProductionRates()` method for TTWars production table
- Added `HasProductionTable()` helper method

#### UpgradeParser
- Added `GetTTWarsUpgradeButton()` for TTWars-specific button structure
- Added `HasUpgradeButtons()` and `GetBuildDuration()` helper methods

#### TrainTroopParser
- Added `GetTTWarsInputBox()` for TTWars troop training structure
- Added `HasTroopTraining()` and `GetAvailableTroops()` helper methods

#### AdventureParser
- Added `IsHeroAvailable()` and `GetAvailableAdventureCount()` helper methods

#### NavigationBarParser
- Added `GetButtonByHref()` for fallback navigation button detection
- Added `GetDorf1Button()` and `GetDorf2Button()` methods

#### VillagePanelParser
- Added `HasVillagePanel()` and `GetVillageCount()` helper methods

#### FarmListParser
- Added `HasFarmList()`, `GetFarmListCount()`, and `GetFarmLists()` helper methods

#### InventoryParser
- Added `HasHeroInventory()` and `GetItemCount()` helper methods

#### InfoParser
- Added `HasAccountInfo()` and `GetServerTime()` helper methods

#### QuestParser
- Added `HasQuestMaster()` and `GetCollectibleQuestCount()` helper methods

#### NpcResourceParser
- Added `HasNpcMerchant()` and `GetResourceInputs()` helper methods

#### CompleteImmediatelyParser
- Added `HasFinishNowButton()` and `HasFinishNowDialog()` helper methods

#### BuildingTabParser
- Added `HasBuildingTabs()`, `GetActiveTabIndex()`, and `GetTabNames()` helper methods

#### OptionParser
- Added `IsOptionsPage()` and `HasSubmitButton()` helper methods

### 3. UI Updates

#### XAML Files
- **`WPFUI/Views/Tabs/AddAccountTab.xaml`** - Added ServerType ComboBox
- **`WPFUI/Views/Tabs/EditAccountTab.xaml`** - Added ServerType ComboBox

#### Code-Behind Files
- **`WPFUI/Views/Tabs/AddAccountTab.xaml.cs`** - Added ServerType binding
- **`WPFUI/Views/Tabs/EditAccountTab.xaml.cs`** - Added ServerType binding

### 4. Tests

#### New Files
- **`MainCore.Test/Parsers/TTWarsParser.Test.cs`** - Comprehensive tests for TTWars parsing
- **`MainCore.Test/Parsers/TTWars/LoginPage.html`** - Test fixture from TTWars login page
- **`MainCore.Test/Parsers/TTWars/ResourceFields.html`** - Test fixture from TTWars dorf1 page
- **`MainCore.Test/Parsers/TTWars/Buildings.html`** - Test fixture from TTWars dorf2 page

## Key Differences Between Travian and TTWars

### HTML Structure
1. **Login Page**: TTWars uses React-based login with dialog overlay structure
2. **Resource Fields (dorf1)**: Uses `<a>` tags with `data-aid` and `data-gid` attributes
3. **Buildings (dorf2)**: Uses `<div class="buildingSlot">` with `data-aid`, `data-gid`, `data-building-id`, `data-name` attributes
4. **Timer Format**: TTWars uses millisecond precision (`HH:MM:SS (+XXX ms)`)

### Auto-Detection
The bot automatically detects TTWars servers based on the URL containing "ttwars.com".

## Usage

### Adding a TTWars Account
1. Enter the TTWars server URL (e.g., `https://nor4.ttwars.com`)
2. The server type will be automatically detected as "TTWars"
3. Alternatively, manually select "TTWars" from the Server Type dropdown
4. Enter username and password
5. Add access credentials (with optional proxy)
6. Click "Add account"

### Bot Behavior
The bot will automatically use the appropriate parsers based on the server type. All existing functionality (building, training, farming, adventures, etc.) works with TTWars servers.

## Technical Notes

### Backward Compatibility
- All changes are backward compatible with existing Travian servers
- Default server type is "Travian" for existing accounts
- All new methods have fallbacks to standard Travian parsing

### Parser Strategy
The parsers use a "try TTWars first, fallback to standard" approach:
1. Try TTWars-specific selectors/attributes
2. Fall back to standard Travian selectors
3. Return null/empty if neither works

### Database Migration
A new database migration may be needed to add the `ServerType` column to the `Accounts` table. The default value is `0` (Travian).

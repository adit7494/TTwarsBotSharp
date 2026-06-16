namespace MainCore.Parsers
{
    public static class InventoryParser
    {
        /// <summary>
        /// Checks if the current page is the hero inventory page.
        /// Handles both standard Travian and TTWars.
        /// </summary>
        public static bool IsInventoryPage(HtmlDocument doc)
        {
            // Standard Travian: #heroV2 > a.tabItem.active
            var heroDiv = doc.GetElementbyId("heroV2");
            if (heroDiv is not null)
            {
                var aNode = heroDiv.Descendants("a")
                    .FirstOrDefault(x => x.HasClass("tabItem"));
                if (aNode is not null && aNode.HasClass("active")) return true;
            }

            // TTWars: check for heroV2Inventory content class
            var contentNode = doc.GetElementbyId("content");
            if (contentNode is not null)
            {
                var contentClass = contentNode.GetAttributeValue("class", "");
                if (contentClass.Contains("heroV2Inventory")) return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the hero avatar button.
        /// Works for both standard Travian and TTWars.
        /// </summary>
        public static HtmlNode? GetHeroAvatar(HtmlDocument doc)
        {
            // Standard Travian: #heroImageButton
            var heroImageButton = doc.GetElementbyId("heroImageButton");
            if (heroImageButton is not null) return heroImageButton;

            // TTWars: look for hero image in hero div
            var heroDiv = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("hero") && (x.HasClass("male") || x.HasClass("female")));
            if (heroDiv is not null)
            {
                var heroImage = heroDiv.Descendants("img")
                    .FirstOrDefault(x => x.HasClass("heroBodyImage"));
                if (heroImage is not null) return heroImage;
            }

            return null;
        }

        /// <summary>
        /// Checks if the inventory page is fully loaded.
        /// Works for both standard Travian and TTWars.
        /// </summary>
        public static bool IsInventoryLoaded(HtmlDocument doc)
        {
            var inventoryPageWrapper = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("inventoryPageWrapper"));
            if (inventoryPageWrapper is null) return false;
            return !inventoryPageWrapper.HasClass("loading");
        }

        /// <summary>
        /// Gets the item slot for a specific hero item type.
        /// Handles both standard Travian (class-based) and TTWars (data-placeid-based).
        /// </summary>
        public static HtmlNode? GetItemSlot(HtmlDocument doc, HeroItemEnums type)
        {
            // Standard Travian: div.heroItems > div.heroItem:not(.empty) with class u{N}
            var heroItemsDiv = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("heroItems"));

            if (heroItemsDiv is not null)
            {
                var heroItemDivs = heroItemsDiv
                    .Descendants("div")
                    .Where(x => x.HasClass("heroItem") && !x.HasClass("empty"));

                foreach (var itemSlot in heroItemDivs)
                {
                    if (itemSlot.ChildNodes.Count < 2) continue;
                    var itemNode = itemSlot.ChildNodes[1];
                    var classes = itemNode.GetClasses();
                    if (classes.Count() < 2) continue;

                    var itemValue = classes.ElementAt(1);
                    if (itemValue.ParseInt() == (int)type) return itemSlot;
                }
            }

            // TTWars: look for items with data-placeid attribute
            var allHeroItems = doc.DocumentNode
                .Descendants("div")
                .Where(x => x.HasClass("heroItem") && !x.HasClass("empty"));

            foreach (var itemSlot in allHeroItems)
            {
                // Check if the item has a class matching the type
                var itemClasses = itemSlot.GetClasses();
                foreach (var itemClass in itemClasses)
                {
                    if (itemClass.ParseInt() == (int)type) return itemSlot;
                }

                // Check data-tier attribute for consumable items
                var dataTier = itemSlot.GetAttributeValue("data-tier", "");
                if (dataTier == "consumable")
                {
                    // For consumables, check child nodes for item type
                    var childNodes = itemSlot.ChildNodes;
                    foreach (var child in childNodes)
                    {
                        var childClasses = child.GetClasses();
                        foreach (var childClass in childClasses)
                        {
                            if (childClass.StartsWith("u") && childClass.ParseInt() == (int)type)
                                return itemSlot;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the amount input box for resource transfer.
        /// Works for both standard Travian and TTWars.
        /// </summary>
        public static HtmlNode? GetAmountBox(HtmlDocument doc, string name)
        {
            var dialog = GetResourceTransferDialog(doc);
            if (dialog is null) return null;

            var amountInput = dialog
                .Descendants("input")
                .FirstOrDefault(x => x.GetAttributeValue("name", "") == name);
            return amountInput;
        }

        /// <summary>
        /// Gets the confirm button for resource transfer.
        /// Works for both standard Travian and TTWars.
        /// </summary>
        public static HtmlNode? GetConfirmButton(HtmlDocument doc)
        {
            var dialog = GetResourceTransferDialog(doc);
            if (dialog is null) return null;

            var actionButtonBox = dialog
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("actionButton"));
            if (actionButtonBox is null) return null;

            var buttons = actionButtonBox.Descendants("button").ToList();
            if (buttons.Count != 2) return null;
            var button = buttons[1];
            return button;
        }

        /// <summary>
        /// Gets the resource transfer dialog.
        /// Works for both standard Travian and TTWars.
        /// </summary>
        public static HtmlNode? GetResourceTransferDialog(HtmlDocument doc)
        {
            var dialog = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("resourceTransferDialog"));
            return dialog;
        }

        /// <summary>
        /// Gets the success toast notification.
        /// Works for both standard Travian and TTWars.
        /// </summary>
        public static HtmlNode? GetSuccessToast(HtmlDocument doc)
        {
            var toast = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("toast") && x.HasClass("toastSuccess"));
            return toast;
        }

        /// <summary>
        /// Checks if the page has a hero inventory.
        /// Works for both standard Travian and TTWars.
        /// </summary>
        public static bool HasHeroInventory(HtmlDocument doc)
        {
            // Standard Travian: #heroV2
            var heroDiv = doc.GetElementbyId("heroV2");
            if (heroDiv is not null) return true;

            // TTWars: check for heroV2 content class
            var contentNode = doc.GetElementbyId("content");
            if (contentNode is not null)
            {
                var contentClass = contentNode.GetAttributeValue("class", "");
                if (contentClass.Contains("heroV2")) return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the number of items in the hero inventory.
        /// Works for both standard Travian and TTWars.
        /// </summary>
        public static int GetItemCount(HtmlDocument doc)
        {
            // Standard Travian: div.heroItems > div.heroItem:not(.empty)
            var heroItemsDiv = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("heroItems"));

            if (heroItemsDiv is not null)
            {
                var heroItemDivs = heroItemsDiv
                    .Descendants("div")
                    .Where(x => x.HasClass("heroItem") && !x.HasClass("empty"));
                if (heroItemDivs.Any()) return heroItemDivs.Count();
            }

            // TTWars: count all non-empty hero items
            var allHeroItems = doc.DocumentNode
                .Descendants("div")
                .Where(x => x.HasClass("heroItem") && !x.HasClass("empty"));
            return allHeroItems.Count();
        }

        /// <summary>
        /// Gets the equipped item slots (negative data-placeid values).
        /// TTWars-specific: equipment slots use data-placeid with negative values.
        /// </summary>
        public static IEnumerable<HtmlNode> GetEquipmentSlots(HtmlDocument doc)
        {
            return doc.DocumentNode
                .Descendants("div")
                .Where(x => x.HasClass("heroItem") && x.HasClass("heroItemV1"))
                .Where(x =>
                {
                    var placeId = x.GetAttributeValue("data-placeid", "0");
                    return int.TryParse(placeId, out var id) && id < 0;
                });
        }

        /// <summary>
        /// Gets the inventory item slots (positive data-placeid values).
        /// TTWars-specific: inventory slots use data-placeid with positive values.
        /// </summary>
        public static IEnumerable<HtmlNode> GetInventorySlots(HtmlDocument doc)
        {
            return doc.DocumentNode
                .Descendants("div")
                .Where(x => x.HasClass("heroItem") && x.HasClass("heroItemV1"))
                .Where(x =>
                {
                    var placeId = x.GetAttributeValue("data-placeid", "0");
                    return int.TryParse(placeId, out var id) && id > 0;
                });
        }
    }
}
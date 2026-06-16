namespace MainCore.Parsers
{
    public static class TrainTroopParser
    {
        /// <summary>
        /// Gets the input box for a specific troop type.
        /// Handles both standard Travian (div.troop structure) and TTWars (table#troops structure).
        /// </summary>
        public static HtmlNode? GetInputBox(HtmlDocument doc, TroopEnums troop)
        {
            // Standard Travian: div.troop structure
            var node = GetNode(doc, troop);
            if (node is not null)
            {
                var cta = node.Descendants("div")
                    .FirstOrDefault(x => x.HasClass("cta"));
                if (cta is not null)
                {
                    var input = cta.Descendants("input")
                        .FirstOrDefault(x => x.HasClass("text"));
                    if (input is not null) return input;
                }
            }

            // TTWars: table#troops structure with input[name="troops[0][t{N}"]
            return GetTTWarsInputBox(doc, troop);
        }

        /// <summary>
        /// Gets the maximum amount of troops that can be trained.
        /// Handles both standard Travian and TTWars.
        /// </summary>
        public static int GetMaxAmount(HtmlDocument doc, TroopEnums troop)
        {
            // Standard Travian: div.troop structure
            var node = GetNode(doc, troop);
            if (node is not null)
            {
                var cta = node.Descendants("div")
                    .FirstOrDefault(x => x.HasClass("cta"));
                if (cta is not null)
                {
                    var a = cta.Descendants("a").FirstOrDefault();
                    if (a is not null) return a.InnerText.ParseInt();
                }
            }

            // TTWars: table#troops structure
            return GetTTWarsMaxAmount(doc, troop);
        }

        /// <summary>
        /// Gets the train button.
        /// Handles both standard Travian (#s1) and TTWars (#btn_ok or button[name="s1"]).
        /// </summary>
        public static HtmlNode? GetTrainButton(HtmlDocument doc)
        {
            // Standard Travian: #s1
            var button = doc.GetElementbyId("s1");
            if (button is not null) return button;

            // TTWars: #btn_ok
            button = doc.GetElementbyId("btn_ok");
            if (button is not null) return button;

            // Fallback: button[name="s1"]
            button = doc.DocumentNode.Descendants("button")
                .FirstOrDefault(x => x.GetAttributeValue("name", "") == "s1");
            return button;
        }

        /// <summary>
        /// Gets the troop node from standard Travian div.troop structure.
        /// </summary>
        private static HtmlNode? GetNode(HtmlDocument doc, TroopEnums troop)
        {
            var nodes = doc.DocumentNode.Descendants("div")
               .Where(x => x.HasClass("troop"))
               .Where(x => !x.HasClass("empty"))
               .AsEnumerable();

            foreach (var node in nodes)
            {
                var img = node.Descendants("img")
                .FirstOrDefault(x => x.HasClass("unit"));
                if (img is null) continue;
                var classes = img.GetClasses();
                var type = classes
                    .Where(x => x.StartsWith('u'))
                    .FirstOrDefault(x => !x.Equals("unit"));
                if (type is null) continue;
                if (type.ParseInt() == (int)troop) return node;
            }
            return null;
        }

        /// <summary>
        /// Gets the input box for TTWars rally point structure.
        /// TTWars uses table#troops with input[name="troops[0][t{N}"].
        /// </summary>
        private static HtmlNode? GetTTWarsInputBox(HtmlDocument doc, TroopEnums troop)
        {
            var troopsTable = doc.GetElementbyId("troops");
            if (troopsTable is null) return null;

            // Find the input with name="troops[0][t{N}"]
            var troopIndex = GetTroopIndex(troop);
            if (troopIndex == -1) return null;

            var inputName = $"troops[0][t{troopIndex}]";
            var input = troopsTable.Descendants("input")
                .FirstOrDefault(x => x.GetAttributeValue("name", "") == inputName);

            return input;
        }

        /// <summary>
        /// Gets the maximum amount for TTWars rally point structure.
        /// TTWars shows max amount in span.errorMessage as "/ {max}".
        /// </summary>
        private static int GetTTWarsMaxAmount(HtmlDocument doc, TroopEnums troop)
        {
            var troopsTable = doc.GetElementbyId("troops");
            if (troopsTable is null) return 0;

            var troopIndex = GetTroopIndex(troop);
            if (troopIndex == -1) return 0;

            var inputName = $"troops[0][t{troopIndex}]";
            var input = troopsTable.Descendants("input")
                .FirstOrDefault(x => x.GetAttributeValue("name", "") == inputName);

            if (input is null) return 0;

            // Check if input is disabled (no troops available)
            if (input.HasClass("disabled")) return 0;

            // Find the max amount link (sibling <a> tag)
            var parentTd = input.ParentNode;
            if (parentTd is null) return 0;

            var maxLink = parentTd.Descendants("a").FirstOrDefault();
            if (maxLink is null) return 0;

            return maxLink.InnerText.ParseInt();
        }

        /// <summary>
        /// Converts TroopEnums to the troop index used in TTWars input names.
        /// TTWars uses t1-t11 for troops (t11 = hero).
        /// </summary>
        private static int GetTroopIndex(TroopEnums troop)
        {
            // For Huns (TTWars example): u61-u70 maps to t1-t10, hero = t11
            // The troop index is based on the troop's position within its tribe
            int troopId = (int)troop;

            // Hero special case
            if (troop == TroopEnums.Hero) return 11;

            // Get tribe offset (each tribe has 10 troops)
            // Romans: 1-10, Teutons: 11-20, Gauls: 21-30, Nature: 31-39, Natars: 40-49, Egyptians: 50-59, Huns: 60-69
            // TTWars: u61=t1, u62=t2, ..., u70=t10 (so offset for Huns is 60)
            int tribeOffset = 0;
            if (troopId >= 1 && troopId <= 10) tribeOffset = 0; // Romans
            else if (troopId >= 11 && troopId <= 20) tribeOffset = 10; // Teutons
            else if (troopId >= 21 && troopId <= 30) tribeOffset = 20; // Gauls
            else if (troopId >= 50 && troopId <= 59) tribeOffset = 49; // Egyptians
            else if (troopId >= 60 && troopId <= 69) tribeOffset = 60; // Huns (u61=t1)
            else return -1; // Unknown tribe

            return troopId - tribeOffset;
        }

        /// <summary>
        /// Converts TTWars troop index back to TroopEnums.
        /// Requires knowing the tribe offset.
        /// </summary>
        private static TroopEnums GetTroopFromIndex(int index, int tribeOffset)
        {
            if (index == 11) return TroopEnums.Hero;
            if (index < 1 || index > 10) return TroopEnums.Hero; // Invalid
            return (TroopEnums)(index + tribeOffset);
        }

        /// <summary>
        /// Checks if the page has troop training options.
        /// Works for both standard Travian and TTWars.
        /// </summary>
        public static bool HasTroopTraining(HtmlDocument doc)
        {
            // Standard Travian: div.troop structure
            var nodes = doc.DocumentNode.Descendants("div")
               .Where(x => x.HasClass("troop"))
               .Where(x => !x.HasClass("empty"));
            if (nodes.Any()) return true;

            // TTWars: table#troops structure
            var troopsTable = doc.GetElementbyId("troops");
            if (troopsTable is null) return false;

            // Check if there are any non-disabled inputs
            var inputs = troopsTable.Descendants("input")
                .Where(x => x.GetAttributeValue("name", "").StartsWith("troops[0][t"))
                .Where(x => !x.HasClass("disabled"));
            return inputs.Any();
        }

        /// <summary>
        /// Gets all available troop types on the page.
        /// Works for both standard Travian and TTWars.
        /// </summary>
        public static IEnumerable<TroopEnums> GetAvailableTroops(HtmlDocument doc)
        {
            // Standard Travian: div.troop structure
            var nodes = doc.DocumentNode.Descendants("div")
               .Where(x => x.HasClass("troop"))
               .Where(x => !x.HasClass("empty"));

            foreach (var node in nodes)
            {
                var img = node.Descendants("img")
                    .FirstOrDefault(x => x.HasClass("unit"));
                if (img is null) continue;
                var classes = img.GetClasses();
                var type = classes
                    .Where(x => x.StartsWith('u'))
                    .FirstOrDefault(x => !x.Equals("unit"));
                if (type is null) continue;

                var troopId = type.ParseInt();
                if (troopId != -1)
                {
                    yield return (TroopEnums)troopId;
                }
            }

            // TTWars: table#troops structure
            // Note: We need to know the tribe to correctly map indices to TroopEnums
            // For now, we'll use the image classes to determine the troop type
            var troopsTable = doc.GetElementbyId("troops");
            if (troopsTable is null) yield break;

            // Find ALL img.unit elements in the table (not just first per row)
            var allImgs = troopsTable.Descendants("img")
                .Where(x => x.HasClass("unit"));

            foreach (var img in allImgs)
            {
                var classes = img.GetClasses();
                var type = classes
                    .Where(x => x.StartsWith('u') && !x.Equals("unit"))
                    .FirstOrDefault();
                if (type is null) continue;

                // Extract troop ID from class (e.g., "u61" -> 61)
                var troopId = type.ParseInt();
                if (troopId != -1)
                {
                    yield return (TroopEnums)troopId;
                }
            }
        }
    }
}

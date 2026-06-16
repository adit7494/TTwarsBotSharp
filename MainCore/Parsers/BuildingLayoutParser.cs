namespace MainCore.Parsers
{
    public static class BuildingLayoutParser
    {
        public static IEnumerable<BuildingDto> GetFields(HtmlDocument doc)
        {
            static IEnumerable<HtmlNode> GetNodes(HtmlDocument doc)
            {
                var resourceFieldContainerNode = doc.GetElementbyId("resourceFieldContainer");
                if (resourceFieldContainerNode is null) return [];

                // Standard Travian: nodes with class "level" directly in container
                var nodes = resourceFieldContainerNode
                    .ChildNodes
                    .Where(x => x.HasClass("level"));

                // TTWars: also check for <a> tags with resourceField class
                if (!nodes.Any())
                {
                    nodes = resourceFieldContainerNode
                        .Descendants("a")
                        .Where(x => x.HasClass("resourceField"));
                }

                return nodes;
            }

            static int GetId(HtmlNode node)
            {
                // Try data-aid attribute first (TTWars)
                var dataAid = node.GetAttributeValue("data-aid", -1);
                if (dataAid != -1) return dataAid;

                // Fallback to CSS class
                var classess = node.GetClasses();
                var buildingSlot = classess.FirstOrDefault(x => x.StartsWith("buildingSlot"));
                if (buildingSlot is null) return -1;
                return buildingSlot.ParseInt();
            }

            static BuildingEnums GetBuildingType(HtmlNode node)
            {
                // Try data-gid attribute first (TTWars)
                var dataGid = node.GetAttributeValue("data-gid", -1);
                if (dataGid != -1) return (BuildingEnums)dataGid;

                // Fallback to CSS class
                var classess = node.GetClasses();
                var gid = classess.FirstOrDefault(x => x.StartsWith("gid"));
                if (gid is null) return BuildingEnums.Unknown;
                return (BuildingEnums)gid.ParseInt();
            }

            static int GetLevel(HtmlNode node)
            {
                // Try to get level from labelLayer div (TTWars)
                var labelLayer = node.Descendants("div").FirstOrDefault(x => x.HasClass("labelLayer"));
                if (labelLayer is not null)
                {
                    var labelLevel = labelLayer.InnerText.ParseInt();
                    if (labelLevel != -1) return labelLevel;
                }

                // Fallback to CSS class
                var classess = node.GetClasses();
                var level = classess.FirstOrDefault(x => x.StartsWith("level") && !x.Equals("level"));
                if (level is null) return -1;
                return level.ParseInt();
            }

            static bool IsUnderConstruction(HtmlNode node)
            {
                // Check for underConstruction class
                if (node.GetClasses().Contains("underConstruction")) return true;

                // TTWars: check for notNow class (max level or cannot upgrade)
                // This is different from underConstruction - notNow means can't upgrade now
                // underConstruction means currently being upgraded
                return false;
            }

            foreach (var node in GetNodes(doc))
            {
                var location = GetId(node);
                var level = GetLevel(node);
                var type = GetBuildingType(node);
                var isUnderConstruction = IsUnderConstruction(node);
                yield return new BuildingDto()
                {
                    Location = location,
                    Level = level,
                    Type = type,
                    IsUnderConstruction = isUnderConstruction,
                };
            }
        }

        public static IEnumerable<BuildingDto> GetInfrastructures(HtmlDocument doc)
        {
            static IEnumerable<HtmlNode> GetNodes(HtmlDocument doc)
            {
                var villageContentNode = doc.GetElementbyId("villageContent");
                if (villageContentNode is null) return [];
                var list = villageContentNode.Descendants("div").Where(x => x.HasClass("buildingSlot"));
                if (list.Count() == 23) // level 1 wall and above has 2 part
                {
                    return list.SkipLast(1);
                }

                return list;
            }

            static int GetId(HtmlNode node)
            {
                var dataAid = node.GetAttributeValue("data-aid", "");
                if (string.IsNullOrEmpty(dataAid)) return -1;
                if (int.TryParse(dataAid, out var id)) return id;
                return -1;
            }

            static BuildingEnums GetBuildingType(HtmlNode node)
            {
                return (BuildingEnums)node.GetAttributeValue<int>("data-gid", -1);
            }

            static int GetLevel(HtmlNode node)
            {
                var aNode = node.Descendants("a").FirstOrDefault();
                if (aNode is null) return -1;

                // Try data-level attribute first
                var dataLevel = aNode.GetAttributeValue<int>("data-level", -1);
                if (dataLevel != -1) return dataLevel;

                // TTWars: try to get level from labelLayer div
                var labelLayer = aNode.Descendants("div").FirstOrDefault(x => x.HasClass("labelLayer"));
                if (labelLayer is not null)
                {
                    return labelLayer.InnerText.ParseInt();
                }

                return -1;
            }

            static bool IsUnderConstruction(HtmlNode node)
            {
                return node.Descendants("a").Any(x => x.HasClass("underConstruction"));
            }

            foreach (var node in GetNodes(doc))
            {
                var location = GetId(node);
                var level = GetLevel(node);
                var type = location switch
                {
                    26 => BuildingEnums.MainBuilding,
                    39 => BuildingEnums.RallyPoint,
                    _ => GetBuildingType(node)
                };
                var isUnderConstruction = IsUnderConstruction(node);

                yield return new BuildingDto()
                {
                    Location = location,
                    Level = level,
                    Type = type,
                    IsUnderConstruction = isUnderConstruction,
                };
            }
        }

        public static IEnumerable<QueueBuildingDto> GetQueueBuilding(HtmlDocument doc)
        {
            static IEnumerable<HtmlNode> GetNodes(HtmlDocument doc)
            {
                var finishButton = doc.DocumentNode.Descendants("div").FirstOrDefault(x => x.HasClass("finishNow"));
                if (finishButton is null) return [];
                return finishButton.ParentNode.Descendants("li");
            }

            static string GetBuildingType(HtmlNode node)
            {
                // Strategy 1: Try data-gid attribute (language-independent, works for TTWars)
                var dataGid = node.GetAttributeValue("data-gid", -1);
                if (dataGid != -1)
                {
                    var buildingType = (BuildingEnums)dataGid;
                    if (buildingType != BuildingEnums.Unknown)
                    {
                        return buildingType.ToString();
                    }
                }

                // Strategy 2: Try to find data-gid in child elements (links, etc.)
                var childWithGid = node.Descendants().FirstOrDefault(x => x.GetAttributeValue("data-gid", -1) != -1);
                if (childWithGid is not null)
                {
                    dataGid = childWithGid.GetAttributeValue("data-gid", -1);
                    var buildingType = (BuildingEnums)dataGid;
                    if (buildingType != BuildingEnums.Unknown)
                    {
                        return buildingType.ToString();
                    }
                }

                // Strategy 3: Try to extract building ID from href
                var linkNode = node.Descendants("a").FirstOrDefault();
                if (linkNode is not null)
                {
                    var href = linkNode.GetAttributeValue("href", "");
                    // TTWars uses href like "build.php?id=4" where id is location
                    // But we can also check for gid parameter if present
                    var gidMatch = System.Text.RegularExpressions.Regex.Match(href, @"gid=(\d+)");
                    if (gidMatch.Success && int.TryParse(gidMatch.Groups[1].Value, out int gid))
                    {
                        var buildingType = (BuildingEnums)gid;
                        if (buildingType != BuildingEnums.Unknown)
                        {
                            return buildingType.ToString();
                        }
                    }
                }

                // Strategy 4: Fallback to name text (language-dependent)
                var nodeName = node.Descendants("div").FirstOrDefault(x => x.HasClass("name"));
                if (nodeName is null) return "";

                return new string(nodeName.ChildNodes[0].InnerText.Where(c => char.IsLetter(c) || char.IsDigit(c)).ToArray());
            }

            static int GetLevel(HtmlNode node)
            {
                var nodeLevel = node.Descendants("span").FirstOrDefault(x => x.HasClass("lvl"));
                if (nodeLevel is null) return 0;

                return nodeLevel.InnerText.ParseInt();
            }

            static TimeSpan GetDuration(HtmlNode node)
            {
                // Standard Travian: span.timer with value attribute
                var nodeTimer = node.Descendants().FirstOrDefault(x => x.HasClass("timer"));
                if (nodeTimer is not null)
                {
                    int sec = nodeTimer.GetAttributeValue("value", 0);
                    if (sec > 0) return TimeSpan.FromSeconds(sec);
                }

                // TTWars: div.buildDuration with text like "0:05:30 jam"
                var buildDuration = node.Descendants("div")
                    .FirstOrDefault(x => x.HasClass("buildDuration"));
                if (buildDuration is not null)
                {
                    var durationText = buildDuration.InnerText.Trim();
                    // Extract time format (HH:MM:SS) from text
                    var timeMatch = System.Text.RegularExpressions.Regex.Match(durationText, @"(\d+:\d+:\d+)");
                    if (timeMatch.Success)
                    {
                        return timeMatch.Groups[1].Value.ToDuration();
                    }
                }

                return TimeSpan.Zero;
            }

            var nodes = GetNodes(doc);
            foreach (var node in nodes)
            {
                var type = GetBuildingType(node);
                var level = GetLevel(node);
                var duration = GetDuration(node);
                yield return new QueueBuildingDto()
                {
                    Type = type,
                    Level = level,
                    CompleteTime = DateTime.Now.Add(duration),
                    Location = -1,
                };
            }
        }

        /// <summary>
        /// Checks if the page is a dorf1 (resource fields) page.
        /// Works for both standard Travian and TTWars.
        /// </summary>
        public static bool IsDorf1Page(HtmlDocument doc)
        {
            var contentNode = doc.GetElementbyId("content");
            if (contentNode is null) return false;

            // Check for village1 class (dorf1)
            if (contentNode.HasClass("village1")) return true;

            // Check for resourceFieldContainer
            var resourceFieldContainer = doc.GetElementbyId("resourceFieldContainer");
            return resourceFieldContainer is not null;
        }

        /// <summary>
        /// Checks if the page is a dorf2 (village buildings) page.
        /// Works for both standard Travian and TTWars.
        /// </summary>
        public static bool IsDorf2Page(HtmlDocument doc)
        {
            var contentNode = doc.GetElementbyId("content");
            if (contentNode is null) return false;

            // Check for village2 class (dorf2)
            if (contentNode.HasClass("village2")) return true;

            // Check for villageContent
            var villageContent = doc.GetElementbyId("villageContent");
            return villageContent is not null;
        }
    }
}
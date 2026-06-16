using MainCore.UI.Models.Input;

namespace MainCore.Commands.UI.Villages.BuildViewModel
{
    [Handler]
    public static partial class NormalBuildCommand
    {
        public sealed record Command(VillageId VillageId, NormalBuildPlan plan) : IVillageCommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            GetLayoutBuildingsCommand.Handler getLayoutBuildingsQuery,
            AddJobCommand.Handler addJobCommand,
            ILogger logger
            )
        {
            var (villageId, plan) = command;

            var buildings = await getLayoutBuildingsQuery.HandleAsync(new(villageId));
            var building = buildings.Find(x => x.Location == plan.Location);

            if (building is null)
            {
                // Building doesn't exist at this location - validate and find correct location
                plan.ValidateLocation(buildings);
            }
            else
            {
                // Building exists at this location - check if type matches
                if (building.Type != plan.Type)
                {
                    // Building type doesn't match - need to find correct location
                    // For new buildings, find an empty slot
                    if (plan.Type.IsWall())
                    {
                        plan.Location = 40;
                    }
                    else if (plan.Type.IsMultipleBuilding())
                    {
                        // Multiple buildings can have same type at different locations
                        var sameTypeBuildings = buildings.Where(x => x.Type == plan.Type);
                        if (sameTypeBuildings.Any())
                        {
                            var largestLevelBuilding = sameTypeBuildings.MaxBy(x => x.Level)!;
                            if (largestLevelBuilding.Level < plan.Type.GetMaxLevel())
                            {
                                plan.Location = largestLevelBuilding.Location;
                            }
                        }
                    }
                    else
                    {
                        // Single building - find existing building of this type or empty slot
                        var existingBuilding = buildings.Find(x => x.Type == plan.Type);
                        if (existingBuilding is not null)
                        {
                            plan.Location = existingBuilding.Location;
                        }
                        else
                        {
                            // Find empty slot for new building
                            var emptySlot = buildings.FirstOrDefault(x => x.Type == BuildingEnums.Site);
                            if (emptySlot is not null)
                            {
                                plan.Location = emptySlot.Location;
                            }
                            else
                            {
                                return Result.Fail("No empty slot available for new building");
                            }
                        }
                    }
                }
            }

            // Check prerequisites and add them automatically if needed
            var prerequisiteBuildings = plan.Type.GetPrerequisiteBuildings();
            if (prerequisiteBuildings.Count > 0)
            {
                // Track used locations to avoid conflicts
                var usedLocations = new HashSet<int>();

                foreach (var prerequisiteBuilding in prerequisiteBuildings)
                {
                    var existingPrereq = buildings
                        .Where(x => x.Type == prerequisiteBuilding.Type)
                        .FirstOrDefault(x => x.Level >= prerequisiteBuilding.Level);

                    if (existingPrereq is null)
                    {
                        // Prerequisite not met - find or create the prerequisite building
                        var prereqBuilding = buildings.FirstOrDefault(x => x.Type == prerequisiteBuilding.Type);

                        int prereqLocation;
                        if (prereqBuilding is not null)
                        {
                            // Prerequisite building exists but level is too low
                            prereqLocation = prereqBuilding.Location;
                        }
                        else
                        {
                            // Prerequisite building doesn't exist - find empty slot
                            var emptySlot = buildings
                                .Where(x => x.Type == BuildingEnums.Site)
                                .FirstOrDefault(x => !usedLocations.Contains(x.Location));

                            if (emptySlot is not null)
                            {
                                prereqLocation = emptySlot.Location;
                            }
                            else
                            {
                                return Result.Fail($"No empty slot for prerequisite {prerequisiteBuilding.Type}");
                            }
                        }

                        // Track this location as used
                        usedLocations.Add(prereqLocation);

                        // Add prerequisite job
                        var prerequisitePlan = new NormalBuildPlan()
                        {
                            Type = prerequisiteBuilding.Type,
                            Level = prerequisiteBuilding.Level,
                            Location = prereqLocation,
                        };

                        logger.Information("Adding prerequisite job: {Building} to level {Level} at location {Location}",
                            prerequisiteBuilding.Type, prerequisiteBuilding.Level, prereqLocation);

                        await addJobCommand.HandleAsync(new(villageId, prerequisitePlan.ToJob()));
                    }
                }
            }

            // Add the main job
            await addJobCommand.HandleAsync(new(villageId, plan.ToJob()));
            return Result.Ok();
        }

        private static void ValidateLocation(this NormalBuildPlan plan, List<BuildingItem> buildings)
        {
            if (plan.Type.IsWall())
            {
                plan.Location = 40;
                return;
            }
            if (plan.Type.IsMultipleBuilding())
            {
                var sameTypeBuildings = buildings.Where(x => x.Type == plan.Type);
                if (!sameTypeBuildings.Any()) return;
                if (sameTypeBuildings.Any(x => x.Location == plan.Location)) return;
                var largestLevelBuilding = sameTypeBuildings.MaxBy(x => x.Level)!;
                if (largestLevelBuilding.Level == plan.Type.GetMaxLevel()) return;
                plan.Location = largestLevelBuilding.Location;
                return;
            }

            if (plan.Type.IsResourceField())
            {
                var field = buildings.First(x => x.Location == plan.Location);
                if (plan.Type == field.Type) return;
                plan.Type = field.Type;
                return;
            }

            var building = buildings.Find(x => x.Type == plan.Type);
            if (building is null) return;
            if (plan.Location == building.Location) return;
            plan.Location = building.Location;
        }

        public static NormalBuildPlan ToPlan(this NormalBuildInput input, int location)
        {
            var (type, level) = input.Get();
            return new NormalBuildPlan()
            {
                Location = location,
                Type = type,
                Level = level,
            };
        }
    }
}

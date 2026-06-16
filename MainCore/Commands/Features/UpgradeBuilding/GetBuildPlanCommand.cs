using System.Text.Json;

namespace MainCore.Commands.Features.UpgradeBuilding
{
    [Handler]
    public static partial class GetBuildPlanCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId) : IAccountVillageCommand;

        private static async ValueTask<Result<NormalBuildPlan>> HandleAsync(
            Command command,
            GetJobCommand.Handler getJobQuery,
            ToDorfCommand.Handler toDorfCommand,
            UpdateBuildingCommand.Handler updateBuildingCommand,
            GetLayoutBuildingsCommand.Handler getLayoutBuildingsQuery,
            DeleteJobByIdCommand.Handler deleteJobByIdCommand,
            AddJobCommand.Handler addJobCommand,
            ValidatePlanCompleteCommand.Handler validatePlanCompleteCommand,
            ILogger logger,
            IRxQueue rxQueue,
            CancellationToken cancellationToken
        )
        {
            var (accountId, villageId) = command;

            while (true)
            {
                if (cancellationToken.IsCancellationRequested) return Cancel.Error;

                var (_, isFailed, job, errors) = await getJobQuery.HandleAsync(new(accountId, villageId), cancellationToken);
                if (isFailed) return Result.Fail(errors);

                if (job.Type == JobTypeEnums.ResourceBuild)
                {
                    logger.Information("{Content}", job);

                    var layoutBuildings = await getLayoutBuildingsQuery.HandleAsync(new(villageId, true));
                    var resourceBuildPlan = JsonSerializer.Deserialize<ResourceBuildPlan>(job.Content)!;
                    var normalBuildPlan = GetNormalBuildPlan(resourceBuildPlan, layoutBuildings);
                    if (normalBuildPlan is null)
                    {
                        await deleteJobByIdCommand.HandleAsync(new(job.Id), cancellationToken);
                    }
                    else
                    {
                        await addJobCommand.HandleAsync(new(villageId, normalBuildPlan.ToJob(), true));
                    }
                    rxQueue.Enqueue(new JobsModified(villageId));
                    continue;
                }

                var plan = JsonSerializer.Deserialize<NormalBuildPlan>(job.Content)!;
                Result result;
                if (plan.Type.IsResourceBonus())
                {
                    result = await toDorfCommand.HandleAsync(new(1), cancellationToken);
                    if (result.IsFailed) return result;

                    result = await updateBuildingCommand.HandleAsync(new(villageId), cancellationToken);
                    if (result.IsFailed) return result;

                    result = await toDorfCommand.HandleAsync(new(2), cancellationToken);
                    if (result.IsFailed) return result;

                    result = await updateBuildingCommand.HandleAsync(new(villageId), cancellationToken);
                    if (result.IsFailed) return result;
                }
                else
                {
                    var dorf = plan.Location < 19 ? 1 : 2;
                    result = await toDorfCommand.HandleAsync(new(dorf), cancellationToken);
                    if (result.IsFailed) return result;

                    result = await updateBuildingCommand.HandleAsync(new(villageId), cancellationToken);
                    if (result.IsFailed) return result;
                }

                var validateResult = await validatePlanCompleteCommand.HandleAsync(new(villageId, plan), cancellationToken);
                logger.Information("Validation result: IsFailed={IsFailed}, ErrorCount={ErrorCount}", validateResult.IsFailed, validateResult.Errors.Count);

                if (validateResult.IsFailed)
                {
                    // Log all errors for debugging
                    foreach (var err in validateResult.Errors)
                    {
                        logger.Information("Validation error: {Error}", err.Message);
                    }

                    // Check if the error is about missing prerequisite
                    var prerequisiteErrors = validateResult.Errors
                        .Where(e => e.Message.Contains("is missing"))
                        .ToList();

                    logger.Information("Found {Count} prerequisite errors", prerequisiteErrors.Count);

                    if (prerequisiteErrors.Count > 0)
                    {
                        // Parse prerequisite info from error messages
                        foreach (var error in prerequisiteErrors)
                        {
                            var message = error.Message;
                            // Parse "MainBuilding level 3 is missing"
                            var parts = message.Split(' ');
                            if (parts.Length >= 4)
                            {
                                var buildingName = parts[0];
                                if (int.TryParse(parts[2], out int requiredLevel))
                                {
                                    // Try to parse the building type
                                    if (BuildingNameTranslator.TryTranslate(buildingName, out var prerequisiteType))
                                    {
                                        logger.Information("Adding prerequisite job: {Building} to level {Level}", prerequisiteType, requiredLevel);

                                        // Find a location for the prerequisite building
                                        var layoutBuildings = await getLayoutBuildingsQuery.HandleAsync(new(villageId, true));
                                        var existingBuilding = layoutBuildings.FirstOrDefault(x => x.Type == prerequisiteType);

                                        int location;
                                        if (existingBuilding is not null)
                                        {
                                            location = existingBuilding.Location;
                                        }
                                        else
                                        {
                                            // For new buildings, we need to find an empty slot on dorf2
                                            // This is complex, so for now we'll just use the first available slot
                                            // A better approach would be to let the user configure this
                                            location = layoutBuildings
                                                .Where(x => x.Type == BuildingEnums.Site)
                                                .Select(x => x.Location)
                                                .FirstOrDefault();

                                            if (location == 0)
                                            {
                                                logger.Warning("Cannot find empty slot for prerequisite building {Building}", prerequisiteType);
                                                return Result.Fail(validateResult.Errors);
                                            }
                                        }

                                        // Create prerequisite job
                                        var prerequisitePlan = new NormalBuildPlan()
                                        {
                                            Type = prerequisiteType,
                                            Level = requiredLevel,
                                            Location = location,
                                        };

                                        // Add prerequisite job at the top of the queue
                                        await addJobCommand.HandleAsync(new(villageId, prerequisitePlan.ToJob(), true));
                                        rxQueue.Enqueue(new JobsModified(villageId));

                                        logger.Information("Added prerequisite: {Building} at location {Location} to level {Level}",
                                            prerequisiteType, location, requiredLevel);
                                    }
                                }
                            }
                        }

                        // Continue the loop - the prerequisite job will be picked up next
                        continue;
                    }

                    return Result.Fail(validateResult.Errors);
                }
                if (!validateResult.Value)
                {
                    await deleteJobByIdCommand.HandleAsync(new(job.Id), cancellationToken);
                    rxQueue.Enqueue(new JobsModified(villageId));
                    continue;
                }

                return plan;
            }
        }

        private static NormalBuildPlan? GetNormalBuildPlan(
            ResourceBuildPlan plan,
            List<BuildingItem> layoutBuildings
        )
        {
            List<BuildingItem> resourceFields;

            if (plan.Plan == ResourcePlanEnums.ExcludeCrop)
            {
                resourceFields = layoutBuildings
                    .Where(x => x.Type == BuildingEnums.Woodcutter || x.Type == BuildingEnums.ClayPit || x.Type == BuildingEnums.IronMine)
                    .Where(x => x.Level < plan.Level)
                    .ToList();
            }
            else if (plan.Plan == ResourcePlanEnums.OnlyCrop)
            {
                resourceFields = layoutBuildings
                    .Where(x => x.Type == BuildingEnums.Cropland)
                    .Where(x => x.Level < plan.Level)
                    .ToList();
            }
            else
            {
                resourceFields = layoutBuildings
                    .Where(x => x.Type.IsResourceField())
                    .Where(x => x.Level < plan.Level)
                    .ToList();
            }

            if (resourceFields.Count == 0) return null;

            var minLevel = resourceFields
                .Select(x => x.Level)
                .Min();

            var chosenOne = resourceFields
                .Where(x => x.Level == minLevel)
                .OrderBy(x => x.Id.Value + Random.Shared.Next())
                .FirstOrDefault();

            if (chosenOne is null) return null;

            var normalBuildPlan = new NormalBuildPlan()
            {
                Type = chosenOne.Type,
                Level = chosenOne.Level + 1,
                Location = chosenOne.Location,
            };
            return normalBuildPlan;
        }
    }
}
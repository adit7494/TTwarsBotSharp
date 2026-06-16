using System.Collections.Concurrent;
using MainCore.Tasks.Base;

namespace MainCore.Services
{
    [RegisterSingleton<ITaskManager, TaskManager>]
    public sealed class TaskManager : ITaskManager
    {
        private readonly ConcurrentDictionary<AccountId, TaskQueue> _queues = new();

        private readonly IRxQueue _rxQueue;

        public TaskManager(IRxQueue rxQueue)
        {
            _rxQueue = rxQueue;
        }

        public BaseTask? GetCurrentTask(AccountId accountId)
        {
            var queue = GetTaskQueue(accountId);
            lock (queue.TasksLock)
            {
                return queue.Tasks.Find(x => x.Stage == StageEnums.Executing);
            }
        }

        public async Task StopCurrentTask(AccountId accountId)
        {
            var cts = GetCancellationTokenSource(accountId);
            if (cts is not null && !cts.IsCancellationRequested)
            {
                try { await cts.CancelAsync(); }
                catch (ObjectDisposedException) { }
            }

            BaseTask? currentTask;
            var timeout = DateTime.UtcNow.AddSeconds(30);
            do
            {
                currentTask = GetCurrentTask(accountId);
                if (currentTask is null) break;
                if (DateTime.UtcNow > timeout) break;
                await Task.Delay(500);
            }
            while (currentTask.Stage != StageEnums.Waiting);
            SetStatus(accountId, StatusEnums.Paused);
        }

        public void AddOrUpdate<T>(T task, bool first = false) where T : AccountTask
        {
            var oldTask = Get<T>(task.AccountId, task.Key);
            if (oldTask is null)
            {
                Add<T>(task, first);
            }
            else
            {
                oldTask.ExecuteAt = task.ExecuteAt;
                Update(oldTask, first);
            }
        }

        public void Add<T>(T task, bool first = false) where T : AccountTask
        {
            AddTask(task, first);
        }

        private T? Get<T>(AccountId accountId, string key) where T : BaseTask
        {
            var queue = GetTaskQueue(accountId);
            lock (queue.TasksLock)
            {
                return queue.Tasks.OfType<T>().FirstOrDefault(x => x.Key == key);
            }
        }

        public bool IsExist<T>(AccountId accountId) where T : BaseTask
        {
            var queue = GetTaskQueue(accountId);
            lock (queue.TasksLock)
            {
                return queue.Tasks.OfType<T>().Any(x => x.Key == $"{accountId}");
            }
        }

        public bool IsExist<T>(AccountId accountId, VillageId villageId) where T : BaseTask
        {
            var queue = GetTaskQueue(accountId);
            lock (queue.TasksLock)
            {
                return queue.Tasks.OfType<T>().Any(x => x.Key == $"{accountId}-{villageId}");
            }
        }

        private void AddTask(AccountTask task, bool first)
        {
            var queue = GetTaskQueue(task.AccountId);
            lock (queue.TasksLock)
            {
                if (first)
                {
                    var firstTask = queue.Tasks.FirstOrDefault();
                    if (firstTask is not null && firstTask.ExecuteAt < task.ExecuteAt)
                    {
                        task.ExecuteAt = firstTask.ExecuteAt.AddHours(-1);
                    }
                }

                queue.Tasks.Add(task);
                if (task is VillageTask villageTask)
                {
                    _rxQueue.Enqueue(new VillageTaskAdded(villageTask));
                }
                ReOrderLocked(task.AccountId, queue);
            }
        }

        private void Update(AccountTask task, bool first)
        {
            var queue = GetTaskQueue(task.AccountId);
            lock (queue.TasksLock)
            {
                if (first)
                {
                    var firstTask = queue.Tasks.FirstOrDefault();
                    if (firstTask is not null && firstTask.ExecuteAt < task.ExecuteAt)
                    {
                        task.ExecuteAt = firstTask.ExecuteAt.AddHours(-1);
                    }
                }
                ReOrderLocked(task.AccountId, queue);
            }
        }

        public void Remove(AccountId accountId, BaseTask task)
        {
            var queue = GetTaskQueue(accountId);
            lock (queue.TasksLock)
            {
                if (queue.Tasks.Remove(task))
                {
                    ReOrderLocked(accountId, queue);
                }
            }
        }

        public void Remove<T>(AccountId accountId) where T : AccountTask
        {
            var queue = GetTaskQueue(accountId);
            lock (queue.TasksLock)
            {
                var task = queue.Tasks.OfType<T>().FirstOrDefault(x => x.AccountId == accountId);
                if (task is null) return;
                queue.Tasks.Remove(task);
                ReOrderLocked(accountId, queue);
            }
        }

        public void Remove<T>(AccountId accountId, VillageId villageId) where T : VillageTask
        {
            var queue = GetTaskQueue(accountId);
            lock (queue.TasksLock)
            {
                var task = queue.Tasks.OfType<T>().FirstOrDefault(x => x.AccountId == accountId && x.VillageId == villageId);
                if (task is null) return;
                queue.Tasks.Remove(task);
                ReOrderLocked(accountId, queue);
            }
        }

        public void ReOrder(AccountId accountId)
        {
            var queue = GetTaskQueue(accountId);
            lock (queue.TasksLock)
            {
                ReOrderLocked(accountId, queue);
            }
        }

        public void Clear(AccountId accountId)
        {
            var queue = GetTaskQueue(accountId);
            lock (queue.TasksLock)
            {
                if (queue.Tasks.Count == 0) return;
                queue.Tasks.Clear();
            }
            _rxQueue.Enqueue(new TasksModified(accountId));
        }

        private void ReOrderLocked(AccountId accountId, TaskQueue queue)
        {
            _rxQueue.Enqueue(new TasksModified(accountId));
            if (queue.Tasks.Count <= 1) return;
            queue.Tasks.Sort((x, y) => DateTime.Compare(x.ExecuteAt, y.ExecuteAt));
        }

        public List<BaseTask> GetTaskList(AccountId accountId)
        {
            var queue = GetTaskQueue(accountId);
            return queue.Tasks;
        }

        public StatusEnums GetStatus(AccountId accountId)
        {
            var queue = GetTaskQueue(accountId);
            return queue.Status;
        }

        public void SetStatus(AccountId accountId, StatusEnums status)
        {
            var queue = GetTaskQueue(accountId);
            queue.Status = status;
            _rxQueue.Enqueue(new StatusModified(accountId, status));
        }

        private CancellationTokenSource? GetCancellationTokenSource(AccountId accountId)
        {
            var queue = GetTaskQueue(accountId);
            return queue.CancellationTokenSource;
        }

        public bool IsExecuting(AccountId accountId)
        {
            var queue = GetTaskQueue(accountId);
            return queue.IsExecuting;
        }

        public TaskQueue GetTaskQueue(AccountId accountId)
        {
            return _queues.GetOrAdd(accountId, _ => new TaskQueue());
        }
    }

    public class TaskQueue
    {
        public bool IsExecuting { get; set; } = false;
        public StatusEnums Status { get; set; } = StatusEnums.Offline;
        public CancellationTokenSource? CancellationTokenSource { get; set; }
        public List<BaseTask> Tasks { get; } = [];
        public object TasksLock { get; } = new();
    }
}
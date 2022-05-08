using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HyddwnLauncher.Util.Helpers
{
    public static class TaskQueueHelper
    {
        public static async Task PerformTaskQueueAsync(List<Func<Task>> tasks, int workerCount = 10)
        {
            var concurrentQueue = new ConcurrentQueue<Func<Task>>();
            var taskProcessorSize = workerCount;

            foreach (var task in tasks)
            {
                concurrentQueue.Enqueue(task);
            }

            var taskProcessors = new Task[taskProcessorSize];
            for (var i = 0; i < taskProcessors.Length; i++)
            {
                taskProcessors[i] = Task.Run(async () =>
                {
                    bool dequeueSuccess;

                    do
                    {
                        dequeueSuccess = concurrentQueue.TryDequeue(out var workItem);

                        if (dequeueSuccess)
                            await workItem.Invoke();
                    } while (dequeueSuccess);
                });
            }

            await Task.WhenAll(taskProcessors);
        }
    }
}

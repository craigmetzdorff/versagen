using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Versagen.Events
{

    /// <summary>
    /// Represents a <see cref="TaskScheduler"/> that only allows a single task to execute, and requires a task to execute to completion before running more.
    /// </summary>
    public class SingleTaskScheduler : TaskScheduler
    {
        private readonly TaskScheduler UnderlyingScheduler;

        private volatile Task currentTask = Task.CompletedTask;

        private Task<bool> RunnerShim(object state)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            throw new NotImplementedException();
        }

        protected override void QueueTask(Task task)
        {
            throw new NotImplementedException();
        }

        public override int MaximumConcurrencyLevel => 1;

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            throw new NotImplementedException();
        }
    }
}

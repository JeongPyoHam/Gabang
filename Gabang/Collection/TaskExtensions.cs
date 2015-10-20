﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GabangCollection
{
    public static class TaskExtensions
    {
        /// <summary>
        /// Suppresses warnings about unawaited tasks and ensures that unhandled
        /// errors will cause the process to terminate.
        /// </summary>
        public static async void DoNotWait(this Task task)
        {
            await task;
        }

        /// <summary>
        /// Waits for a task to complete. If an exception occurs, the exception
        /// will be raised without being wrapped in a
        /// <see cref="AggregateException"/>.
        /// </summary>
        public static void WaitAndUnwrapExceptions(this Task task)
        {
            task.GetAwaiter().GetResult();
        }

        /// <summary>
        /// Waits for a task to complete. If an exception occurs, the exception
        /// will be raised without being wrapped in a
        /// <see cref="AggregateException"/>.
        /// </summary>
        public static T WaitAndUnwrapExceptions<T>(this Task<T> task)
        {
            return task.GetAwaiter().GetResult();
        }

        /// <summary>
        /// Silently handles the specified exception.
        /// </summary>
        public static Task SilenceException<T>(this Task task) where T : Exception
        {
            return task.ContinueWith(t => {
                try
                {
                    t.Wait();
                }
                catch (AggregateException ex)
                {
                    ex.Handle(e => e is T);
                }
            });
        }

        /// <summary>
        /// Silently handles the specified exception.
        /// </summary>
        public static Task<U> SilenceException<T, U>(this Task<U> task) where T : Exception
        {
            return task.ContinueWith(t => {
                try
                {
                    return t.Result;
                }
                catch (AggregateException ex)
                {
                    ex.Handle(e => e is T);
                    return default(U);
                }
            });
        }
    }
}

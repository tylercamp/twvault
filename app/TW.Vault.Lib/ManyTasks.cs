using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace TW.Vault
{
    public static class ManyTasks
    {
        public static Task<T1> Run<T1>(Task<T1> task1) => task1;

        public static async Task<Tuple<T1, T2>> Run<T1,T2>(Task<T1> task1, Task<T2> task2)
        {
            var tasks = new Task[] { task1, task2 };
            await Task.WhenAll(tasks).ConfigureAwait(false);
            return new Tuple<T1, T2>(task1.Result, task2.Result);
        }

        public static async Task<Tuple<T1, T2, T3>> Run<T1, T2, T3>(Task<T1> task1, Task<T2> task2, Task<T3> task3)
        {
            var tasks = new Task[] { task1, task2, task3 };
            await Task.WhenAll(tasks).ConfigureAwait(false);
            return new Tuple<T1, T2, T3>(task1.Result, task2.Result, task3.Result);
        }

        public static async Task<Tuple<T1, T2, T3, T4>> Run<T1, T2, T3, T4>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4)
        {
            var tasks = new Task[] { task1, task2, task3, task4 };
            await Task.WhenAll(tasks).ConfigureAwait(false);
            return new Tuple<T1, T2, T3, T4>(task1.Result, task2.Result, task3.Result, task4.Result);
        }

        public static async Task<Tuple<T1, T2, T3, T4, T5>> Run<T1, T2, T3, T4, T5>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5)
        {
            var tasks = new Task[] { task1, task2, task3, task4, task5 };
            await Task.WhenAll(tasks).ConfigureAwait(false);
            return new Tuple<T1, T2, T3, T4, T5>(task1.Result, task2.Result, task3.Result, task4.Result, task5.Result);
        }

        public static async Task<Tuple<T1, T2, T3, T4, T5, T6>> Run<T1, T2, T3, T4, T5, T6>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6)
        {
            var tasks = new Task[] { task1, task2, task3, task4, task5, task6 };
            await Task.WhenAll(tasks).ConfigureAwait(false);
            return new Tuple<T1, T2, T3, T4, T5, T6>(task1.Result, task2.Result, task3.Result, task4.Result, task5.Result, task6.Result);
        }

        public static async Task<Tuple<T1, T2, T3, T4, T5, T6, T7>> Run<T1, T2, T3, T4, T5, T6, T7>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6, Task<T7> task7)
        {
            var tasks = new Task[] { task1, task2, task3, task4, task5, task6, task7 };
            await Task.WhenAll(tasks).ConfigureAwait(false);
            return new Tuple<T1, T2, T3, T4, T5, T6, T7>(task1.Result, task2.Result, task3.Result, task4.Result, task5.Result, task6.Result, task7.Result);
        }

        public static async Task<Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8>>> Run<T1, T2, T3, T4, T5, T6, T7, T8>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6, Task<T7> task7, Task<T8> task8)
        {
            var tasks = new Task[] { task1, task2, task3, task4, task5, task6, task7, task8 };
            await Task.WhenAll(tasks).ConfigureAwait(false);
            return new Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8>>(task1.Result, task2.Result, task3.Result, task4.Result, task5.Result, task6.Result, task7.Result, new Tuple<T8>(task8.Result));
        }



        public static Task<List<T1>> RunToList<T1>(IQueryable<T1> q1) =>
            Run(q1.ToListAsync());

        public static Task<Tuple<List<T1>, List<T2>>> RunToList<T1, T2>(IQueryable<T1> q1, IQueryable<T2> q2) =>
            Run(q1.ToListAsync(), q2.ToListAsync());

        public static Task<Tuple<List<T1>, List<T2>, List<T3>>> RunToList<T1, T2, T3>(IQueryable<T1> q1, IQueryable<T2> q2, IQueryable<T3> q3) =>
            Run(q1.ToListAsync(), q2.ToListAsync(), q3.ToListAsync());

        public static Task<Tuple<List<T1>, List<T2>, List<T3>, List<T4>>> RunToList<T1, T2, T3, T4>(IQueryable<T1> q1, IQueryable<T2> q2, IQueryable<T3> q3, IQueryable<T4> q4) =>
            Run(q1.ToListAsync(), q2.ToListAsync(), q3.ToListAsync(), q4.ToListAsync());

        public static Task<Tuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>>> RunToList<T1, T2, T3, T4, T5>(IQueryable<T1> q1, IQueryable<T2> q2, IQueryable<T3> q3, IQueryable<T4> q4, IQueryable<T5> q5) =>
            Run(q1.ToListAsync(), q2.ToListAsync(), q3.ToListAsync(), q4.ToListAsync(), q5.ToListAsync());

        public static Task<Tuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>, List<T6>>> RunToList<T1, T2, T3, T4, T5, T6>(IQueryable<T1> q1, IQueryable<T2> q2, IQueryable<T3> q3, IQueryable<T4> q4, IQueryable<T5> q5, IQueryable<T6> q6) =>
            Run(q1.ToListAsync(), q2.ToListAsync(), q3.ToListAsync(), q4.ToListAsync(), q5.ToListAsync(), q6.ToListAsync());

        public static Task<Tuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>, List<T6>, List<T7>>> RunToList<T1, T2, T3, T4, T5, T6, T7>(IQueryable<T1> q1, IQueryable<T2> q2, IQueryable<T3> q3, IQueryable<T4> q4, IQueryable<T5> q5, IQueryable<T6> q6, IQueryable<T7> q7) =>
            Run(q1.ToListAsync(), q2.ToListAsync(), q3.ToListAsync(), q4.ToListAsync(), q5.ToListAsync(), q6.ToListAsync(), q7.ToListAsync());

        public static Task<Tuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>, List<T6>, List<T7>, Tuple<List<T8>>>> RunToList<T1, T2, T3, T4, T5, T6, T7, T8>(IQueryable<T1> q1, IQueryable<T2> q2, IQueryable<T3> q3, IQueryable<T4> q4, IQueryable<T5> q5, IQueryable<T6> q6, IQueryable<T7> q7, IQueryable<T8> q8) =>
            Run(q1.ToListAsync(), q2.ToListAsync(), q3.ToListAsync(), q4.ToListAsync(), q5.ToListAsync(), q6.ToListAsync(), q7.ToListAsync(), q8.ToListAsync());


        public static Task RunThrottled(IEnumerable<Task> tasks)
        {
            var numThreads = (int)Math.Max(0, Environment.ProcessorCount * 0.75);
            return RunThrottled(numThreads, tasks);
        }


        public static async Task RunThrottled(int maxThreads, IEnumerable<Task> tasks)
        {
            var failedTasks = new List<Task>();
            var pendingTasks = new List<Task>();
            var enumerator = tasks.GetEnumerator();
            while (enumerator.MoveNext() || pendingTasks.Count > 0)
            {
                var hasNext = enumerator.Current != null;
                if (hasNext)
                    pendingTasks.Add(enumerator.Current);

                if (pendingTasks.Count >= maxThreads || !hasNext)
                {
                    var completedTask = await Task.WhenAny(pendingTasks).ConfigureAwait(false);
                    if (completedTask.Exception != null)
                        failedTasks.Add(completedTask);

                    pendingTasks.Remove(completedTask);
                }
            }

            if (failedTasks.Count > 0)
            {
                var aggregate = new AggregateException(failedTasks.SelectMany(t => t.Exception.InnerExceptions));
                throw aggregate;
            }
        }
    }
}

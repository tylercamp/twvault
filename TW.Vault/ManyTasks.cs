using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace TW.Vault
{
    public static class ManyTasks
    {
        public static async Task<Tuple<T1>> Run<T1>(Task<T1> task1)
        {
            return new Tuple<T1>(await task1);
        }

        public static async Task<Tuple<T1, T2>> Run<T1,T2>(Task<T1> task1, Task<T2> task2)
        {
            var tasks = new Task[] { task1, task2 };
            await Task.WhenAll(tasks);
            return new Tuple<T1, T2>(task1.Result, task2.Result);
        }

        public static async Task<Tuple<T1, T2, T3>> Run<T1, T2, T3>(Task<T1> task1, Task<T2> task2, Task<T3> task3)
        {
            var tasks = new Task[] { task1, task2, task3 };
            await Task.WhenAll(tasks);
            return new Tuple<T1, T2, T3>(task1.Result, task2.Result, task3.Result);
        }

        public static async Task<Tuple<T1, T2, T3, T4>> Run<T1, T2, T3, T4>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4)
        {
            var tasks = new Task[] { task1, task2, task3, task4 };
            await Task.WhenAll(tasks);
            return new Tuple<T1, T2, T3, T4>(task1.Result, task2.Result, task3.Result, task4.Result);
        }

        public static async Task<Tuple<T1, T2, T3, T4, T5>> Run<T1, T2, T3, T4, T5>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5)
        {
            var tasks = new Task[] { task1, task2, task3, task4, task5 };
            await Task.WhenAll(tasks);
            return new Tuple<T1, T2, T3, T4, T5>(task1.Result, task2.Result, task3.Result, task4.Result, task5.Result);
        }

        public static async Task<Tuple<T1, T2, T3, T4, T5, T6>> Run<T1, T2, T3, T4, T5, T6>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6)
        {
            var tasks = new Task[] { task1, task2, task3, task4, task5, task6 };
            await Task.WhenAll(tasks);
            return new Tuple<T1, T2, T3, T4, T5, T6>(task1.Result, task2.Result, task3.Result, task4.Result, task5.Result, task6.Result);
        }



        public static Task<Tuple<List<T1>>> RunToList<T1>(IQueryable<T1> q1) =>
            Run(q1.ToListAsync());

        public static Task<Tuple<List<T1>, List<T2>>> RunToList<T1, T2>(IQueryable<T1> q1, IQueryable<T2> q2) =>
            Run(q1.ToListAsync(), q2.ToListAsync());

        public static Task<Tuple<List<T1>, List<T2>, List<T3>>> RunToList<T1, T2, T3>(IQueryable<T1> q1, IQueryable<T2> q2, IQueryable<T3> q3) =>
            Run(q1.ToListAsync(), q2.ToListAsync(), q3.ToListAsync());

        public static Task<Tuple<List<T1>, List<T2>, List<T3>, List<T4>>> RunToList<T1, T2, T3, T4>(IQueryable<T1> q1, IQueryable<T2> q2, IQueryable<T3> q3, IQueryable<T4> q4) =>
            Run(q1.ToListAsync(), q2.ToListAsync(), q3.ToListAsync(), q4.ToListAsync());

        public static Task<Tuple<List<T1>, List<T2>, List<T3>, List<T4>, List<T5>>> RunToList<T1, T2, T3, T4, T5>(IQueryable<T1> q1, IQueryable<T2> q2, IQueryable<T3> q3, IQueryable<T4> q4, IQueryable<T5> q5) =>
            Run(q1.ToListAsync(), q2.ToListAsync(), q3.ToListAsync(), q4.ToListAsync(), q5.ToListAsync());
    }
}

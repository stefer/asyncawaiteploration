using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace asyncawaitexploration
{
    /// <summary>
    /// Small program to simulate a problem I had with async / await in a webjob.
    /// 
    /// The webjob copied files via ftp for several customers from a folder 
    /// structure to another ftp folder structure.
    /// 
    /// It was Async-await all the way and I expected no parallelism. 
    /// In fact, I thought that I could add parallelism later, for example by
    /// collecting tasks in a list and then do Task.WhenAll() to syncronize them.
    /// 
    /// Async-await does not introduce parallism automatically. When we await a task,
    /// we do not run the continuation until the task has finished. So if we have a 
    /// method with a for statement with await inside, the inner parts are serialized.
    /// 
    /// <example>
    /// static async Task Run()
    /// {
    ///     for (int i = 0; i < 10; i++)
    ///     {
    ///         await First(i); // Waits for task before taking next step in loop
    ///     }
    /// }
    /// </example>
    /// 
    /// The only way I have found to reproduce is by NOT awaiting a task that should
    /// have been awaited. In fact, there is a compiler warning about that in this
    /// code. 
    /// 
    /// I challange you to find another way of introducing parallelism in this application.
    /// If the Inner call is awaited, the application prints the calls in serialzed order.
    /// If it is not awaited, then multiple threads run them in parallell and the order 
    /// is not serialized.
    /// 
    /// Some ideas:
    /// - The task scheduler, can it change this behaviour?
    /// - Enumerations, if they are realized late, could that introduce the behaviour?
    /// 
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting");

            for (int i = 0; i < 10; i++)
            {
                await First(i);
            }

            Console.WriteLine("Exiting");
        }

        private static async Task First(int i)
        {
            Enter(i);
            await Task.Delay(50);
            foreach(var j in Enumerable.Range(0, 10))
            {
                Inner(i, j); // Whopps Forgot to await!
            }

            Leave(i);
        }

        private static async Task Inner(int i, int j)
        {
            Enter(i, j);
            await Task.Delay(50);
            Leave(i, j);
        }

        private static void Enter(int first, int inner = 0, [System.Runtime.CompilerServices.CallerMemberName] string method = "")
        {
            Console.WriteLine($">>> Enter {method}-{first}.{inner} {Task.CurrentId} {Thread.CurrentThread.ManagedThreadId}");
        }

        private static void Leave(int first, int inner = 0, [System.Runtime.CompilerServices.CallerMemberName] string method = "")
        {
            Console.WriteLine($"<<< Leave {method}-{first}.{inner} {Task.CurrentId} {Thread.CurrentThread.ManagedThreadId}");
        }
    }
}

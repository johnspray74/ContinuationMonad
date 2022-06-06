#define AsyncAwaitVersion
// #define DebugThreads  // Write out the thread ID in different places so we can ensure everything runs on the same thread (Don't want multithreading!)
// #define ALA

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using ProgrammingParadigms;
using DomainAbstractions;
using Foundation;
using System.Text;

namespace Application
{
    class Program
    {
        // Main just uses the Nito.AsyncEx package to proovde a dispatcher to enable console programs to use async/await on one thread.
        // It just calls another function called Application
        // (if you don't do this then Main will either complete immediately (ending the program before the asyncronous tasks finish)
        // or if you put in a ConsoleReadKey or Thread.Sleep at the end of Main, that will just block the main thread, causing the asynchronous tasks to run on other threads.
        // The program still works when it uses other threads, but I particular wanted to demonstrate this monad working on a single thread.


        static int Main()
        {
            try
            {
                Console.WriteLine("The application has started");
                AsyncContext.Run(() => Application());
                Console.WriteLine("The application has finished");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return -1;
            }
        }



        // The application function composes two functions using the Continuation monad.
        // The Continuation monad consists of Task<T> plus the Totask extension method plus the Bind extension method (both of which are define in the layer below)
        // First it creates a source Task with the value 1. Then the first function adds 2, then the second function adds a number from the console. Then it prints the final result.
        // The thing is, because we are using the Coninuation monad, these two functions are allowed to take as much time as they want (be asynchronous).
        // To demo that, the first function does a delay, and the second function waits for Console input.
        // What the monad does is, despite these functions taking time, allows you to compose them like normal functions. You just have to use the Bind function to compose them.
        // And the function must return Task<U> instead of U.
        // The monad code in the lower layer takes care of making everything work.
        // There is no blocking of the main thread.
        // Everything runs on a single thread.

        // There are two versions - the first version uses async/await. If you are not familiar with async/await then use the second version which uses ContinueWith instead.



#if AsyncAwaitVersion
        // This is the version using async/await

#if !DebugThreads

#if !ALA

        static async Task Application()
        {
            var program = 1.ToTask();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            program.Bind(async (x) => { await Task.Delay(3000); return x + 2; })
            .Bind(async (x) =>
            {
                Console.WriteLine($"Value is {x}. Please enter a number to be added.");
                string line = null;
                await Task.Run(() => line = Console.ReadLine());
                return x + int.Parse(line);
            })
            .ContinueWith((x) => { Console.WriteLine($"Final result is {x.Result}."); }, TaskContinuationOptions.ExecuteSynchronously);
#pragma warning restore CS4014
            program.RunSynchronously();
            Console.WriteLine("Program will be running for 10s");
            await Task.Delay(10000); // Gives the asynchronous functions a chance to 
            Console.WriteLine("Press any key to finish program running");
            Console.ReadKey();  // This blocks the main thread, which is the one used for running the async tasks above, so don't execute this until other tasks are completed
        }

#else // ALA

        static async Task Application()
        {
            Wiring.diagnosticOutput += (s) => System.Diagnostics.Debug.WriteLine(s);
            var program = 1.ToContinuable();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            program
            .Bind(async (x) => { await Task.Delay(3000); return x + 2; })
            .Bind(async (x) =>
            {
                Console.WriteLine($"Value is {x}. Please enter a number to be added.");
                string line = null;
                await Task.Run(() => line = Console.ReadLine());
                return x + int.Parse(line);
            })
            .ToValue((x) => { Console.WriteLine($"Final result is {x}."); });
            Console.WriteLine(program);
#pragma warning restore CS4014
            program.Run();
            Console.WriteLine("Program will be running for 10s");
            await Task.Delay(10000); // Gives the asynchronous functions a chance to 
            Console.WriteLine("Press any key to finish program running");
            Console.ReadKey();  // This blocks the main thread, which is the one used for running the async tasks above, so don't execute this until other tasks are completed
        }


#endif

#else  // DebugThreads


        static async Task TaskMonadDemo()
        {
            Console.WriteLine($"Console thread: {Thread.CurrentThread.ManagedThreadId}");
            var program = 1.ToTask();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            program.Bind(async (x) =>
            {
                Console.WriteLine($"First function thread: {Thread.CurrentThread.ManagedThreadId}");
                await Task.Delay(3000);
                Console.WriteLine($"First function thread: {Thread.CurrentThread.ManagedThreadId}");
                return x + 2;
            })
            .Bind(async (x) =>
            {
                Console.WriteLine($"Second function thread: {Thread.CurrentThread.ManagedThreadId}");
                Console.WriteLine($"Value is {x}. Please enter a number to be added.");
                string line = null;
                await Task.Run(() => line = Console.ReadLine());
                Console.WriteLine($"Second function thread: {Thread.CurrentThread.ManagedThreadId}");
                return x + int.Parse(line);
            })
            .ContinueWith((x) =>
            {
                Console.WriteLine($"Final thread: {Thread.CurrentThread.ManagedThreadId}");
                Console.WriteLine($"Final result is {x.Result}.");
            }, TaskContinuationOptions.ExecuteSynchronously);
#pragma warning restore CS4014
            program.RunSynchronously();
            Console.WriteLine("Program will be running for 10s");
            await Task.Delay(10000);
            Console.WriteLine("Press any key to finish program running");
            Console.ReadKey();  // This blocks the main thread, which is the one used for running the async tasks above, so don't execute this until other tasks are completed
        }

#endif


#else // not AsyncAwaitVersion

        // This is the version using ContinueWith

#if !DebugThreads


        static async Task TaskMonadDemo()
        {
            var program = 1.ToTask();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            program.Bind((x) =>
            {
                var tcs = new TaskCompletionSource<int>();
                Task.Delay(3000).ContinueWith((t) =>
                {
                    tcs.SetResult(x + 4);
                });
                return tcs.Task;
            })
            .Bind((x) =>
            {
                var tcs = new TaskCompletionSource<int>();
                Console.WriteLine($"Value is {x}. Please enter a number to be added.");
                string line = null;
                Task.Run(() => line = Console.ReadLine()).ContinueWith((t)=>{
                    tcs.SetResult(x + int.Parse(line));
                });
                return tcs.Task;
            })
            .ContinueWith((x) =>
            {
                Console.WriteLine($"Final result is {x.Result}.");
            }, TaskContinuationOptions.ExecuteSynchronously);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            program.RunSynchronously();
            Console.WriteLine("Program will be running for 10s");
            await Task.Delay(10000);
            Console.WriteLine("Press any key to finish program running");
            Console.ReadKey();  // This blocks the main thread, which is the one used for running the async tasks above, so don't execute this until other tasks are completed
        }


#else  // DebugThreads


        static async Task TaskMonadDemo()
        {
            Console.WriteLine($"Console thread: {Thread.CurrentThread.ManagedThreadId}");
            var program = 1.ToTask();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            program.Bind((x) =>
            {
                var tcs = new TaskCompletionSource<int>();
                Console.WriteLine($"First function thread: {Thread.CurrentThread.ManagedThreadId}");
                Task.Delay(3000).ContinueWith((t) =>
                {
                    tcs.SetResult(x + 4);
                    Console.WriteLine($"First function thread: {Thread.CurrentThread.ManagedThreadId}");
                });
                Console.WriteLine($"First function thread: {Thread.CurrentThread.ManagedThreadId}");
                return tcs.Task;
            })
            .Bind((x) =>
            {
                Console.WriteLine($"Second function thread: {Thread.CurrentThread.ManagedThreadId}");
                var tcs = new TaskCompletionSource<int>();
                Console.WriteLine($"Value is {x}. Please enter a number to be added.");
                string line = null;
                Task.Run(() => line = Console.ReadLine()).ContinueWith((t)=>{
                    tcs.SetResult(x + int.Parse(line));
                    Console.WriteLine($"Second function thread: {Thread.CurrentThread.ManagedThreadId}");
                });
                return tcs.Task;
            })
            .ContinueWith((x) =>
            {
                Console.WriteLine($"Final thread: {Thread.CurrentThread.ManagedThreadId}");
                Console.WriteLine($"Final result is {x.Result}.");
            }, TaskContinuationOptions.ExecuteSynchronously);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            program.RunSynchronously();
            Console.WriteLine("Program will be running for 10s");
            await Task.Delay(10000);
            Console.WriteLine($"Program end thread: {Thread.CurrentThread.ManagedThreadId}");
            Console.WriteLine("Press any key to finish program running");
            Console.ReadKey();  // This blocks the main thread, which is the one used for running the async tasks above, so don't execute this until other tasks are completed
        }

#endif

#endif

    }

}




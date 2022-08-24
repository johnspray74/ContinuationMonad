// #define ImperativeVersion      // version that composes the three functions in imperative style 
// #define ImperativeVersionUsingUnwrap      // version that composes the three functions in imperative style 
#define MonadAsyncAwaitVersion   // Version where composed lambad functions use async/await
// #define MonadVersion        // Version where composed lambad functions use .Contunewith
// #define ALAVersion                    // ALA Version uses domain abstractions but uses a Bind function to have identical application layer code
#define DebugThreads        // Write out the thread ID in different places so we can ensure everything runs on the same thread (Don't want multithreading!)

// This code is wrtten as example code for chapter 3 section on moands in the online book at abstractionlayeredarchitecture.com
// See that website for full discussion of the comparison between ALA and monads



#if ALAVersion
using DomainAbstractions;
using ProgrammingParadigms;
using Foundation;
#endif
#if MonadAsyncAwaitVersion
using Monad.AsynAwait;
#endif
#if MonadVersion
using Monad.ContinueWith;
#endif

using Nito.AsyncEx;
using System;
using System.Threading.Tasks;
using System.Threading;

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


        // The Application function calls one of many different examples of ComposedFunction below under #ifs
        // The UI thread needs to be kept running, but not block itself so that ComposedFunction can complete.
        // To do that we simply put in a 10 s delay.

        static async Task Application()
        {
            ComposedFunction();
            Console.WriteLine("Program will be running for 10s");
            await Task.Delay(10000);  // Gives the CommposedFunction time to finish
#if DebugThreads
            Console.WriteLine($"Program end thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
            Console.WriteLine("Press any key to finish program running");
            Console.ReadKey();  // This blocks the main thread, which is the one used for running the async tasks above, so don't execute this until other tasks are completed
        }




        // First create two function that take an int and return a Task<int>
        // Both these functions take time to do their job
        // We will be writing sample applications that compose these two functions.

        // This function does a delay and then returns x+2
        // It returns immediately with a task representing the future result
        private static Task<int> function1(int x)
        {
#if DebugThreads
            Console.WriteLine($"First function thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
            return Task.Delay(3000)
            .ContinueWith(task =>
            {
#if DebugThreads
                Console.WriteLine($"First function thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
                return x + 2;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }





        // This function does I/O and then returns x plus whatever was inputted
        // It returns immediately with a task representing the future result
        private static Task<int> function2(int x)
        {
#if DebugThreads
            Console.WriteLine($"Second function thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
            Console.WriteLine($"Value is {x}. Please enter a number to be added.");
            string line = null;
            return Task.Factory.StartNew(() => line = Console.ReadLine(), CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext())
            .ContinueWith(
            _ =>
            {
#if DebugThreads
                Console.WriteLine($"Second function thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
                return x + int.Parse(line);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }







        private static async Task<int> function1Async(int x)
        { 
#if DebugThreads
            Console.WriteLine($"First function thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
            await Task.Delay(3000);
#if DebugThreads
    Console.WriteLine($"First function thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
            return x + 2; 
        }




        private static async Task<int> function2Async(int x)
        {
#if DebugThreads
            Console.WriteLine($"Second function thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
            Console.WriteLine($"Value is {x}. Please enter a number to be added.");
            string line = null;
            await Task.Factory.StartNew(() => line = Console.ReadLine(), CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
#if DebugThreads
    Console.WriteLine($"Second function thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
            return x + int.Parse(line);
        }





#if ImperativeVersion
// This is an imperative version that composes the two functions into a single function
// Notice how it indents for every ContinueWith, which is not practical (leads triangle hell for longer chains of functions)

        static void ComposedFunction()
        {
#if DebugThreads
            Console.WriteLine($"Console thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
            function1(1)
            .ContinueWith(task1 =>
            {
                function2(task1.Result)
                .ContinueWith(task2 =>
                {
#if DebugThreads
                    Console.WriteLine($"Final thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
                    Console.WriteLine($"Final result is {task2.Result}.");
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
#endif





#if ImperativeVersionDelete
        static async Task Application()
        {
#if DebugThreads
            Console.WriteLine($"Console thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
            int startValue = 1;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            Task.Factory.StartNew(() =>
            {
#if DebugThreads
                Console.WriteLine($"First function thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
                Task.Delay(3000).ContinueWith(
                    task2 =>
                    {
#if DebugThreads
                        Console.WriteLine($"First function thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
                        return startValue + 2;
                    }, TaskScheduler.FromCurrentSynchronizationContext())
                .ContinueWith(
                    task3 =>
                    {
#if DebugThreads
                        Console.WriteLine($"Second function thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
                        Console.WriteLine($"Value is {task3.Result}. Please enter a number to be added.");
                        string line = null;
                        Task.Run(() => line = Console.ReadLine())
                        .ContinueWith(
                            task4 =>
                            {
#if DebugThreads
                                Console.WriteLine($"Second function thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
                                return task3.Result + int.Parse(line);
                            }, TaskScheduler.FromCurrentSynchronizationContext())
                        .ContinueWith(
                            task5 =>
                            {
#if DebugThreads
                                Console.WriteLine($"Final thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
                                Console.WriteLine($"Final result is {task5.Result}.");

                            }, TaskScheduler.FromCurrentSynchronizationContext()
                        );
                    }, TaskScheduler.FromCurrentSynchronizationContext()
                );
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
#pragma warning restore CS4014
            Console.WriteLine("Program will be running for 10s");
            await Task.Delay(10000); // Gives the asynchronous functions time to run 
            Console.WriteLine("Press any key to finish program running");
            Console.ReadKey();  // This blocks the main thread, which is the one used for running the async tasks above, so don't execute this until other tasks are completed
        }
#endif






#if ImperativeVersionUsingUnwrap
        // This is another imperative version that composes the two functions into a single function
        // It uses ContinueWith with Unwrap which avaois indenting for every new continuation
        static void ComposedFunction()
        {
#if DebugThreads
            Console.WriteLine($"Console thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
            function1(1)
            .ContinueWith(task3 =>
            {
                return function2(task3.Result);
            }, TaskScheduler.FromCurrentSynchronizationContext()).Unwrap()
            .ContinueWith(task5 =>
            {
#if DebugThreads
                Console.WriteLine($"Final thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
                Console.WriteLine($"Final result is {task5.Result}.");
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
#endif










#if ImperativeVersionUsingUnwrapDelete
        static async Task Application()
        {
#if DebugThreads
            Console.WriteLine($"Console thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
            Task<int> task = new Task<int>(() => 1);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            task.ContinueWith(task1 =>
            {
#if DebugThreads
                Console.WriteLine($"First function thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
                return Task.Delay(3000)
                .ContinueWith(task2 =>
                {
#if DebugThreads
                    Console.WriteLine($"First function thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
                    return task1.Result + 2;
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }, TaskScheduler.FromCurrentSynchronizationContext()).Unwrap()
            .ContinueWith(task3 =>
            {
#if DebugThreads
                Console.WriteLine($"Second function thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
                Console.WriteLine($"Value is {task3.Result}. Please enter a number to be added.");
                string line = null;
                return Task.Run(() => line = Console.ReadLine())
                .ContinueWith(task4 =>
                {
#if DebugThreads
                    Console.WriteLine($"Second function thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
                    return task3.Result + int.Parse(line);
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }, TaskScheduler.FromCurrentSynchronizationContext()).Unwrap()
            .ContinueWith(task5 =>
            {
#if DebugThreads
                Console.WriteLine($"Final thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
                Console.WriteLine($"Final result is {task5.Result}.");

            }, TaskScheduler.FromCurrentSynchronizationContext());
#pragma warning restore CS4014
            task.Start();
            Console.WriteLine("Program will be running for 10s");
            await Task.Delay(10000); // Gives the asynchronous functions time to run 
            Console.WriteLine("Press any key to finish program running");
            Console.ReadKey();  // This blocks the main thread, which is the one used for running the async tasks above, so don't execute this until other tasks are completed
        }
#endif








        // This function composes two asynchronous functions using the Task monad Bind function.
        // The task monad consists of the Task<T> type plus the ToTask extension method plus the Bind extension method (both of which are defined in the Monad folder)
        // First it creates a source Task with the value 1. Then the first function delays 3 s and adds 2, then the second function inputs from the console and adds the input. 
        // The thing is, because we are using the Task monad, these two functions run on the UI thread and can take as much time as they want
        // but they don't block the UI thread.
        // To demo that, the first function does a delay, and the second function waits for Console input.
        // What the monad does is, despite these functions taking time and returning a Task object instead of an immediate result, is allows you to compose them as if they were
        // normal functions that just return a result. You just have to use the Bind function to compose them.
        // And the functions must return Task<U> instead of U.
        // The code in the Bind function in the Monad folder takes care of making everything work.
        // There is no blocking of the main thread in this program until it hits the final ReadKey.
        // Everything runs on the UI thread.

        // There are two versions - the first version uses async/await. If you are not familiar with async/await then use the second version which uses ContinueWith instead.
        // Each version has a verbose version that Console.WriteLines stuff to see what is going on.


#if MonadAsyncAwaitVersion
        // This is the Monad version using async/await - function1 and function2 are async functions using await


        static void ComposedFunction()
        {
            var program = 1.ToTask();
            program
            .Bind(function1Async)
            .Bind(function2Async)
            .ContinueWith((x) =>
            {
#if DebugThreads
                Console.WriteLine($"Final thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
                Console.WriteLine($"Final result is {x.Result}.");
            }, TaskScheduler.FromCurrentSynchronizationContext());
            program.RunSynchronously();
        }
#endif




#if AsyncAwaitVersionDelete
        // This is the version using async/await


        static void ComposedFunction()
        {
            var program = 1.ToTask();
            program
            .Bind(function1Async)
            .Bind(function2Async)
            .ContinueWith((x) =>
            {
#if DebugThreads
                Console.WriteLine($"Final thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
                Console.WriteLine($"Final result is {x.Result}.");
            }, TaskScheduler.FromCurrentSynchronizationContext());
            program.RunSynchronously();
        }
#endif







#if MonadVersion
        // This is the Monad non-async/await version - function1 and function2 contain a ContinueWith

        static void ComposedFunction()
        {
#if DebugThreads
            Console.WriteLine($"Console thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
            var program = 1.ToTask();
            program.Bind(function1)
            .Bind(function2)
            .ContinueWith((x) => 
            {
#if DebugThreads
                Console.WriteLine($"Final thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
                Console.WriteLine($"Final result is {x.Result}.");
            }, TaskScheduler.FromCurrentSynchronizationContext());
            program.RunSynchronously();
        }

#endif






#if MonadVersionDelete
        // This is the version using ContinueWith

        static void ComposedFunction()
        {
#if DebugThreads
            Console.WriteLine($"Console thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
            var program = 1.ToTask();
            program.Bind((x) =>
            {
#if DebugThreads
                Console.WriteLine($"First function thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
                return Task.Delay(3000).ContinueWith(
                (t) => {
#if DebugThreads
                    Console.WriteLine($"First function thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
                    return x + 2;
                });
            })
            .Bind((x) =>
            {
#if DebugThreads
                Console.WriteLine($"Second function thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
                Console.WriteLine($"Value is {x}. Please enter a number to be added.");
                string line = null;
                return Task.Run(() => line = Console.ReadLine()).ContinueWith(
                    (t) => {
#if DebugThreads
                        Console.WriteLine($"Second function thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
                        return x + int.Parse(line);
                    });
            })
            .ContinueWith(
                (x) => {
#if DebugThreads
                    Console.WriteLine($"Final thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
                    Console.WriteLine($"Final result is {x.Result}.");
                }, TaskScheduler.FromCurrentSynchronizationContext());
            program.RunSynchronously();
        }

#endif





#if MonadVersionDelete
        // This is the version using ContinueWith

        static async Task Application()
        {
#if DebugThreads
            Console.WriteLine($"Console thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
            var program = 1.ToTask();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            program.Bind((x) =>
            {
#if DebugThreads
                Console.WriteLine($"First function thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
                return Task.Delay(3000).ContinueWith(
                (t) => {
#if DebugThreads
                    Console.WriteLine($"First function thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
                    return x + 2;
                });
            })
            .Bind((x) =>
            {
#if DebugThreads
                Console.WriteLine($"Second function thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
                Console.WriteLine($"Value is {x}. Please enter a number to be added.");
                string line = null;
                return Task.Run(() => line = Console.ReadLine()).ContinueWith(
                    (t)=>{
#if DebugThreads
                        Console.WriteLine($"Second function thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
                        return x + int.Parse(line);
                    });
            })
            .ContinueWith(
                (x) => {
#if DebugThreads
                    Console.WriteLine($"Final thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
                    Console.WriteLine($"Final result is {x.Result}.");
                }, TaskScheduler.FromCurrentSynchronizationContext());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            program.RunSynchronously();
            Console.WriteLine("Program will be running for 10s");
            await Task.Delay(10000);
            Console.WriteLine($"Program end thread: {Thread.CurrentThread.ManagedThreadId}");
            Console.WriteLine("Press any key to finish program running");
            Console.ReadKey();  // This blocks the main thread, which is the one used for running the async tasks above, so don't execute this until other tasks are completed
        }

#endif







#if MonadVersionDelete
        // This is the version using ContinueWith (and TaskCompletionSource which is redundant)

        static async Task Application()
        {
#if DebugThreads
            Console.WriteLine($"Console thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
            var program = 1.ToTask();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            program.Bind((x) =>
            {
                var tcs = new TaskCompletionSource<int>();
#if DebugThreads
                Console.WriteLine($"First function thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
                Task.Delay(3000).ContinueWith(
                (t) => {
                    tcs.SetResult(x + 4);
#if DebugThreads
                    Console.WriteLine($"First function thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
                });
#if DebugThreads
                Console.WriteLine($"First function thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
                return tcs.Task;
            })
            .Bind((x) =>
            {
#if DebugThreads
                Console.WriteLine($"Second function thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
                var tcs = new TaskCompletionSource<int>();
                Console.WriteLine($"Value is {x}. Please enter a number to be added.");
                string line = null;
                Task.Run(() => line = Console.ReadLine()).ContinueWith(
                    (t) => {
                        tcs.SetResult(x + int.Parse(line));
#if DebugThreads
                        Console.WriteLine($"Second function thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
                    });
                return tcs.Task;
            })
            .ContinueWith(
                (x) => {
#if DebugThreads
                    Console.WriteLine($"Final thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
                    Console.WriteLine($"Final result is {x.Result}.");
                }, TaskScheduler.FromCurrentSynchronizationContext());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            program.RunSynchronously();
            Console.WriteLine("Program will be running for 10s");
            await Task.Delay(10000);
            Console.WriteLine($"Program end thread: {Thread.CurrentThread.ManagedThreadId}");
            Console.WriteLine("Press any key to finish program running");
            Console.ReadKey();  // This blocks the main thread, which is the one used for running the async tasks above, so don't execute this until other tasks are completed
        }

#endif






#if ALAVersion

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

    }

}




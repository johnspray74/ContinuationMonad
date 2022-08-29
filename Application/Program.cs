// #define ImperativeVersionSynchronousFunctions  // Convnetional imperative composing of two synchronous functions
// #define ImperativeVersionSynchronousFunctionsUsingContinueWith   // Imperative version that composes two synchronous functions with ContinueWith to show how basic ContinueWith works
#define ImperativeVersion               // Imparative version that composes two asynchronous functions
// #define ImperativeVersionUsingUnwrap    // Imperative version that composes two asynchronous functions using Unwrap
// #define MonadVersion                    // Monad version of the imperative version 
// #define MonadAsyncAwaitVersion          // Monad version where composed functions use async/await
// #define ALAVersion                      // ALA Version wires up instances of domain abstractions that take a function or action as a configuration.
// #define ALAVersionBind                  // ALA Version that uses Bind to do the WireIn and new to demonstrate identical application layer code is possible (altough we wouldn't normaly do this.)
#define DebugThreads                    // Write out the thread ID in different places so we can ensure everything runs on the same thread (Don't want multithreading!)

// This code is wrtten as example code for chapter 6 section on moands in the online book at abstractionlayeredarchitecture.com
// See that website for full discussion of the comparison between ALA and monads



#if MonadVersion
using Monad.ContinueWith;
#endif
#if MonadAsyncAwaitVersion
using Monad.AsynAwait;
#endif
#if ALAVersion || ALAVersionBind
using DomainAbstractions;
using ProgrammingParadigms;
using Foundation;
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




        // First create two functions to compose
        // First do synchronous versions of these functions
        // They take an int and return an int

        private static int function1Sync(int x) { return x + 2; }
        private static int function2Sync(int x, int y) { return x + y; }



        // Now make asynchronous versions, which is the purpose of the exercise
        // Both these functions take time to do their job
        // The two functions take an int and return a Task<int>
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



        // Finally create equivalent versions of the two functions that are implemented with async/await so that we can show the equivalent async/await way of doing things.


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




        // This first version shows the basic idea that we are wanting to create a function that composes the function1 and function2 and then outputs the result to the console.
        // This first verion is the conventional imperative way of composing functions.

#if ImperativeVersionSynchronousFunctions

        static int function1Sync(int x) { return x + 2; }
        static int function2Sync(int x, int y) { return x + y; }

        static void ComposedFunction()
        {
            int result1 = function1Sync(1);
            int result2 = function2Sync(result1, 4);
            Console.WriteLine($"Final result is {result2}.");
        }
#endif





        // This is teh same as the previous version except that it uses ContinueWith to compose the two synchronous functions.
        // It is useless because you wouldn't normally use ContinueWith to compose synchronous functions.
        // It just shows the basic way ContinueWith works.
        // Note that the lambda expression returns a value, and the containing ContinuewWith returns a Task that will contain that value.
        // ConinueWith returns a Task, which you can then chain with another ContinueWith.
        // We use task.FromResult to get an initial Task on which to do the first ContinueWith.

#if ImperativeVersionSynchronousFunctionsUsingContinueWith

        static void ComposedFunction()
        {
#if DebugThreads
            Console.WriteLine($"Console thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
            Task.FromResult(function1Sync(1))
            .ContinueWith(task1 =>
            {
#if DebugThreads
                Console.WriteLine($"Console thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
                return function2Sync(task1.Result, 4);
            }, TaskScheduler.FromCurrentSynchronizationContext())
            .ContinueWith(task2 =>
            {
#if DebugThreads
                Console.WriteLine($"Console thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
                Console.WriteLine($"Final result is {task2.Result}.");
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
#endif




        // This is the same as the previous version except that it composes two asynchronous functions instead of synchronous functions
        // Since each function is asynchronous (returns a Task), we compose directly from the function1 and function2 return value with ContinueWith.
        // Notice how it indents for every ContinueWith, which is not practical (leads triangle to hell for longer chains of functions).


#if ImperativeVersion

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



        // This is the same as the previous version except that it uses Unwrap to avoid the indenting at each new continuation.
        // To use Unwrap, the ContinueWith's lambda parameter changes from an Action to a Func.
        // The Func returns the Task returned by the function1 or function2.
        // Since ContinueWith itself returns a Task<>, we get back a Task<Task<>>.
        // Unwrap gets the inner Task, which we can then use ContinueWith on.


#if ImperativeVersionUsingUnwrap
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




        // This is the same as the previous version except that all the code around the two functions has been refactored into a Bind function
        // The code is now more declarative than imperative becasue it simply composes the two functions.
        // We also add a ToTask function to create a Task from the starting value, on which Bind can operate

        // The task monad consists of the Task<T> type plus the ToTask extension method plus the Bind extension method (both of which are defined in the Monad folder).
        // First it creates a source Task with the value 1 using ToTask().
        // The Bind function takes a Task<T> and returns Task<U>, so you can chain them.
        // The first function delays 3 s and adds 2, then the second function inputs from the console and adds the input. 
        // These two functions run on the UI thread and can take as much time as they want but they don't block the UI thread.

        // The code in the Bind function in the Monad folder takes care of making everything work.

#if MonadVersion
        // This is the Monad non-async/await version - function1 and function2 contain a ContinueWith

        static void ComposedFunction()
        {
#if DebugThreads
            Console.WriteLine($"Console thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
            var program = 1.ToTask();
            program
            .Bind(function1)
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




        // This is the same as the previous version except that the two functions internally use async/await instead of ContinueWith
        // The Bind function is also changed to use async/await

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




        // This is the same as the previous version except that it uses ALA (abstractionlayeredarchitecture.com) and a domain abstraction called ContinuationFunction.
        // A ContinuationFunction object can be configured with a function that returns a Task<T> in the same way as Bind can.
        // It has input and output ports of type IDataflow, which allow them to be wired in a chain.
        // Internally ContinuationFunction implements the input IDataflow port with an async function. This function awaits the Task returned by the configured function.
        // The ALA version explicitly creates instances of ContinuationFunction, and then wires them together in the normal ALA way using the IDataflow ports.

#if ALAVersion
        static void ComposedFunction()
        {
            Wiring.diagnosticOutput += (s) => System.Diagnostics.Debug.WriteLine(s);
            var program = 1.ToValue();
            program
            .WireIn(new ContinuationFunction<int, int>(function1Async))
            .WireIn(new ContinuationFunction<int, int>(function2Async))
            .WireIn(new DataflowToAction<int>((x) => { Console.WriteLine($"Final result is {x}."); }));
            program.Run();
        }
#endif





        // This version is the same as the previous version except that it uses a Bind function that does .WireIn(new ContinuationFunction(function))
        // This makes the application layer code identical to the MonadAsyncAwait version we did previously although different under the covers.

#if ALAVersionBind
        static void ComposedFunction()
        {
#if DebugThreads
            Console.WriteLine($"Console thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
            Wiring.diagnosticOutput += (s) => System.Diagnostics.Debug.WriteLine(s);
            var program = 1.ToValue();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            program
            .Bind(function1Async)
            .Bind(function2Async)
            .ToAction((x) => {
#if DebugThreads
                Console.WriteLine($"Final thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
                Console.WriteLine($"Final result is {x}."); 
            });
            program.Run();
        }
#endif
    }
}




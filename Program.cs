#define AsyncAwaitVersion
// #define DebugThreads  // Write out the thread ID in different places so we can ensure everything runs on the same thread (Don't want multithreading!)

using System;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using ProgrammingParadigms;



namespace Application
{
    class Program
    {
        static int Main()
        {
            try
            {
                Console.WriteLine("The application has started");
                AsyncContext.Run(() => TaskMonadDemo());
                Console.WriteLine("The application has finished");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return -1;
            }
        }



#if AsyncAwaitVersion
        // This is the version using async/await

        static async Task TaskMonadDemo()
        {
            #if DebugThreads
            Console.WriteLine($"Console thread: {Thread.CurrentThread.ManagedThreadId}");
            #endif
            var program = 1.ToTask();
            #pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            program.Bind(async (x) =>
            {
                #if DebugThreads  
                Console.WriteLine($"First function thread: {Thread.CurrentThread.ManagedThreadId}");
                #endif
                // await Task.Run(() => Task.Delay(3000)).ConfigureAwait(true);
                await Task.Delay(3000);
                #if DebugThreads  
                Console.WriteLine($"First function thread: {Thread.CurrentThread.ManagedThreadId}");
                #endif
                return x + 2;
            })
            .Bind(async (x) =>
            {
                #if DebugThreads  
                Console.WriteLine($"Second function thread: {Thread.CurrentThread.ManagedThreadId}");
                #endif
                Console.WriteLine($"Value is {x}. Please enter a number to be added.");
                string line = null;
                await Task.Run(() => line = Console.ReadLine());
                #if DebugThreads
                Console.WriteLine($"Second function thread: {Thread.CurrentThread.ManagedThreadId}");
                #endif
                return x + int.Parse(line);
            })
            .ContinueWith((x) =>
            {
                #if DebugThreads
                Console.WriteLine($"Final thread: {Thread.CurrentThread.ManagedThreadId}");
                #endif
                Console.WriteLine($"Final result is {x.Result}.");
            }, TaskContinuationOptions.ExecuteSynchronously);
            #pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            program.RunSynchronously();
            Console.WriteLine("Program is running for 10s");
            await Task.Delay(10000);
            Console.WriteLine("Press any key to finish program running");
            Console.ReadKey();  // This blocks the main thread, which is the one used for running the async tasks above, so don't execute this until other tasks are completed
        }


#else

        // This is the version using ContinueWith

        static async Task TaskMonadDemo()
        {
            #if DebugThreads
            Console.WriteLine($"Console thread: {Thread.CurrentThread.ManagedThreadId}");
            #endif
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
            .Bind(async (x) =>
            {
                #if DebugThreads
                Console.WriteLine($"Second function thread: {Thread.CurrentThread.ManagedThreadId}");
                #endif
                Console.WriteLine($"Value is {x}. Please enter a number to be added.");
                string line = null;
                await Task.Run(() => line = Console.ReadLine());
                #if DebugThreads
                Console.WriteLine($"Second function thread: {Thread.CurrentThread.ManagedThreadId}");
                #endif
                return x + int.Parse(line);
            })
            .ContinueWith((x) =>
            {
                #if DebugThreads
                Console.WriteLine($"Final thread: {Thread.CurrentThread.ManagedThreadId}");
                #endif
                Console.WriteLine($"Final result is {x.Result}.");
            }, TaskContinuationOptions.ExecuteSynchronously);
            #pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            program.RunSynchronously();
            Console.WriteLine("Program is running for 10s");
            await Task.Delay(10000);
            #if DebugThreads
            Console.WriteLine($"Program end thread: {Thread.CurrentThread.ManagedThreadId}");
            #endif
            Console.WriteLine("Press any key to finish program running");
            Console.ReadKey();  // This blocks the main thread, which is the one used for running the async tasks above, so don't execute this until other tasks are completed
        }

#endif

    }

}









namespace ProgrammingParadigms
{
    // -----------------------------------------------------------------------------------
    // This monad is sometimes referred to as the ConinuationMonad and is built by adding Bind and Unit extension methods to Task<T> 

    public static class TaskMonadExtensionMethods
    {
        public static Task<T> ToTask<T>(this T value)
        {
            return new Task<T>(() => value);
        }


#if AsyncAwaitVersion
        // version of Bind function using async/await

        public static async Task<U> Bind<T, U>(this Task<T> source, Func<T, Task<U>> function)
        {
            #if DebugThreads
            Console.WriteLine($"Bind thread: {Thread.CurrentThread.ManagedThreadId}");
            #endif
            var x = await source;
            #if DebugThreads
            Console.WriteLine($"First ContinueWith thread: {Thread.CurrentThread.ManagedThreadId}");
            #endif
            Console.WriteLine($"Input value to monad is {x}");
            var result = await function(x);
            #if DebugThreads
            Console.WriteLine($"Second ContinueWith thread: {Thread.CurrentThread.ManagedThreadId}");
            #endif
            Console.WriteLine($"Output value from monad is {result}");
            return result;
        }

#else

        // Version of Bind function using ConinueWith

        public static Task<U> Bind<T, U>(this Task<T> source, Func<T, Task<U>> function)
        {
            var tcs = new TaskCompletionSource<U>();
            #if DebugThreads
            Console.WriteLine($"Bind thread: {Thread.CurrentThread.ManagedThreadId}");
            #endif
            source.ContinueWith(
                (t) =>
                {
                    #if DebugThreads
                    Console.WriteLine($"First ContinueWith thread: {Thread.CurrentThread.ManagedThreadId}");
                    #endif
                    Console.WriteLine($"Input value to monad is {t.Result}");
                    function(t.Result).ContinueWith(
                        (t) =>
                        {
                            #if DebugThreads
                            Console.WriteLine($"Second ContinueWith thread: {Thread.CurrentThread.ManagedThreadId}");
                            #endif
                            Console.WriteLine($"Output value from monad is {t.Result}");
                            tcs.SetResult(t.Result);
                        },
                        TaskScheduler.FromCurrentSynchronizationContext()
                    );
                },
                TaskScheduler.FromCurrentSynchronizationContext()
            );
            return tcs.Task;
        }
#endif

    }
}

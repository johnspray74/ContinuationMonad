// This source file implements the Task monad
// This monad is sometimes referred to as the ContinuationMonad and is built by adding Bind and Unit extension methods to Task<T> 
// Monads are a two-layer pattern, so we only use ProgrammingParadigms and Application layers.


#define AsyncAwaitVersion  // We implemented both async/await and ContinueWith versions of this monad. Use this to select which one is used.
// #define DebugThreads // enable to get console.WriteLines of the input values, output values and thread IDs of the monads




using System;
using System.Threading.Tasks;



namespace ProgrammingParadigms
{


    public static class TaskMonadExtensionMethods
    {
        public static Task<T> ToTask<T>(this T value)
        {
            return new Task<T>(() => value);
        }


#if AsyncAwaitVersion


#if !DebugThreads

        // version of Bind function using async/await

        public static async Task<U> Bind<T, U>(this Task<T> source, Func<T, Task<U>> function)
        {
            return await function(await source);
        }


#else // DebugThreads


        public static async Task<U> Bind<T, U>(this Task<T> source, Func<T, Task<U>> function)
        {
            Console.WriteLine($"Bind thread: {Thread.CurrentThread.ManagedThreadId}");
            var x = await source;
            Console.WriteLine($"First ContinueWith thread: {Thread.CurrentThread.ManagedThreadId}");
            Console.WriteLine($"Input value to monad is {x}");
            var result = await function(x);
            Console.WriteLine($"Second ContinueWith thread: {Thread.CurrentThread.ManagedThreadId}");
            Console.WriteLine($"Output value from monad is {result}");
            return result;
        }


#endif


#else // not AsyncAwaitVersion

        // Version of Bind function using ConinueWith


#if !DebugThreads

        public static Task<U> Bind<T, U>(this Task<T> source, Func<T, Task<U>> function)
        {
            var tcs = new TaskCompletionSource<U>();
            source.ContinueWith(
                (t) =>
                {
                    function(t.Result).ContinueWith(
                        (t) =>
                        {
                            tcs.SetResult(t.Result);
                        },
                        TaskScheduler.FromCurrentSynchronizationContext()
                    );
                },
                TaskScheduler.FromCurrentSynchronizationContext()
            );
            return tcs.Task;
        }

#else  // DebugThreads

        public static Task<U> Bind<T, U>(this Task<T> source, Func<T, Task<U>> function)
        {
            var tcs = new TaskCompletionSource<U>();
            Console.WriteLine($"Bind thread: {Thread.CurrentThread.ManagedThreadId}");
            source.ContinueWith(
                (t) =>
                {
                    Console.WriteLine($"First ContinueWith thread: {Thread.CurrentThread.ManagedThreadId}");
                    Console.WriteLine($"Input value to monad is {t.Result}");
                    function(t.Result).ContinueWith(
                        (t) =>
                        {
                            Console.WriteLine($"Second ContinueWith thread: {Thread.CurrentThread.ManagedThreadId}");
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


#endif

    }
}

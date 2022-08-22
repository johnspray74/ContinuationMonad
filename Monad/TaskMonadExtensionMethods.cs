// This source file implements the Task monad
// This monad is sometimes referred to as the ContinuationMonad and is built by adding Bind and Unit extension methods to Task<T> 
// Monads are a two-layer pattern, so we only use ProgrammingParadigms and Application layers.
// This file is not used by the ALA version

// #define DebugThreads // enable to get console.WriteLines of the input values, output values and thread IDs of the monads




using System;
using System.Threading.Tasks;



namespace Monad.AsynAwait
{


    public static class TaskMonadExtensionMethods
    {
        public static Task<T> ToTask<T>(this T value)
        {
            return new Task<T>(() => value);
        }


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
    }
}



namespace Monad.ContinueWith
{
    // Version of Bind function using ConinueWith
    // and using Unwrap instead of TaskCompetionource as the second version does


    public static class TaskMonadExtensionMethods
    {
        public static Task<T> ToTask<T>(this T value)
        {
            return new Task<T>(() => value);
        }



        public static Task<U> Bind<T, U>(this Task<T> source, Func<T, Task<U>> function)
        {
#if DebugThreads
            Console.WriteLine($"Bind thread: {Thread.CurrentThread.ManagedThreadId}");
#endif
            return source.ContinueWith(
                (t) =>
                {
#if DebugThreads
                    Console.WriteLine($"First ContinueWith thread: {Thread.CurrentThread.ManagedThreadId}");
                    Console.WriteLine($"Input value to monad is {t.Result}");
#endif
                    return function(t.Result).ContinueWith(
                        (t) =>
                        {
#if DebugThreads
                            Console.WriteLine($"Second ContinueWith thread: {Thread.CurrentThread.ManagedThreadId}");
                            Console.WriteLine($"Output value from monad is {t.Result}");
#endif
                            return t.Result;
                        }, TaskScheduler.FromCurrentSynchronizationContext()
                    );
                }, TaskScheduler.FromCurrentSynchronizationContext()
            ).Unwrap();
        }

    }
}





namespace Monad.ContinueWith2
{
    // Version of Bind function using ConinueWith
    // and using TaskCompetionource instead of Unwrap()


    public static class TaskMonadExtensionMethods
    {
        public static Task<T> ToTask<T>(this T value)
        {
            return new Task<T>(() => value);
        }



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
                    Console.WriteLine($"Input value to monad is {t.Result}");
#endif
                    function(t.Result).ContinueWith(
                        (t) =>
                        {
#if DebugThreads
                            Console.WriteLine($"Second ContinueWith thread: {Thread.CurrentThread.ManagedThreadId}");
                            Console.WriteLine($"Output value from monad is {t.Result}");
#endif
                            tcs.SetResult(t.Result);
                        },
                        TaskScheduler.FromCurrentSynchronizationContext()
                    );
                },
                TaskScheduler.FromCurrentSynchronizationContext()
            );
            return tcs.Task;
        }

    }
}

using ProgrammingParadigms;
using Foundation;
using System;
using System.Threading.Tasks;

namespace DomainAbstractions
{


    public class Continuation<T, U> : IContinuable<T>, IContinuation<T> // input port
    {
        readonly Func<T, Task<U>> function;
#pragma warning disable CS0649 // Field 'Continuation<T, U>.next' is never assigned to, and will always have its default value null
        private IContinuation<U> next; // output port
#pragma warning restore CS0649

        public Continuation(Func<T, Task<U>> function)
        {
            this.function = function;
        }

        async void IContinuation<T>.PushTask(Task<T> previousTask)
        {
            Console.WriteLine("PushTask");
            var result = await previousTask;
            Console.WriteLine($"PushTask {result}");
            next.PushTask(function(result));
            // next.PushTask(function(await previousTask));
        }
    }


    public static class BindExtensionMethod
    {

        public static IContinuable<U> Bind<T, U>(this IContinuable<T> source, Func<T, Task<U>> function)
        {
            return (IContinuable<U>)source.WireIn(new Continuation<T, U>(function));
        }
    }


}
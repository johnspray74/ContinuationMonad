using ProgrammingParadigms;
using Foundation;
using System;
using System.Threading.Tasks;

namespace DomainAbstractions
{

    // This is only used by the ALA version
    public class Continuation<T, U> : IBindable<T>, IContinuation<T> // input port
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
            var result = await previousTask;
            next.PushTask(function(result));
            // next.PushTask(function(await previousTask));
        }
    }


    public static class BindExtensionMethod
    {

        public static IBindable<U> Bind<T, U>(this IBindable<T> source, Func<T, Task<U>> function)
        {
            return (IBindable<U>)source.WireIn(new Continuation<T, U>(function));
        }
    }


}
using ProgrammingParadigms;
using Foundation;
using System;
using System.Threading.Tasks;

namespace DomainAbstractions
{

    // This is only used by the ALA version
    public class ContinuationFunction<T, U> : IBindable<T>, IDataflow<T> // input port
    {
        readonly Func<T, Task<U>> function;
#pragma warning disable CS0649 // Field 'Continuation<T, U>.next' is never assigned to, and will always have its default value null
        private IDataflow<U> next; // output port
#pragma warning restore CS0649

        public ContinuationFunction(Func<T, Task<U>> function)
        {
            this.function = function;
        }

        async void IDataflow<T>.Push(T data)
        {
            next.Push(await function(data));
        }
    }



    
    public static class BindExtensionMethod
    {

        public static IBindable<U> Bind<T, U>(this IBindable<T> source, Func<T, Task<U>> function)
        {
            return (IBindable<U>)source.WireIn(new ContinuationFunction<T, U>(function));
        }
    }


}
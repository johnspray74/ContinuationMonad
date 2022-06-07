using ProgrammingParadigms;
using Foundation;
using System;
using System.Threading.Tasks;

namespace DomainAbstractions
{
    // You can WireIn and instance of this class at the end of the chain of maonads in order to get a action delegate called with the value.


    public class ContinuationToValue<T> : IContinuation<T> // input port
    {
        private Action<T> action;

        public ContinuationToValue(Action<T> action)
        {
            this.action = action;
        }


        async void IContinuation<T>.PushTask(Task<T> previousTask)
        {
            action(await previousTask);
        }
    }


    // You can use this extension method to instantiate a ContinuationToValue and call WireIn. 
    // You would normally use this in ALA, it is only here to allow monad code to use ALA instead of monads.


    public static class ContinueWithExtensionMethod
    {
        public static ContinuationToValue<T> ToValue<T>(this IContinuable<T> source, Action<T> action)
        {
            return (ContinuationToValue<T>)source.WireIn(new ContinuationToValue<T>(action));
        }
    }
}
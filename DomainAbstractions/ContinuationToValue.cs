using ProgrammingParadigms;
using Foundation;
using System;
using System.Threading.Tasks;

namespace DomainAbstractions
{


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


    public static class ContinueWithExtensionMethod
    {
        public static ContinuationToValue<T> ToValue<T>(this IContinuable<T> source, Action<T> action)
        {
            return (ContinuationToValue<T>)source.WireIn(new ContinuationToValue<T>(action));
        }
    }
}
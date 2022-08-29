using ProgrammingParadigms;
using Foundation;
using System;
using System.Threading.Tasks;

namespace DomainAbstractions
{
    // You can WireIn and instance of this class at the end of the chain of maonads in order to get a action delegate called with the value.


    public class DataflowToAction<T> : IDataflow<T> // input port
    {
        private Action<T> action;

        public DataflowToAction(Action<T> action)
        {
            this.action = action;
        }


        async void IDataflow<T>.Push(T data)
        {
            action(data);
        }
    }




    // You can use this extension method to instantiate a dataflowToAction and call WireIn. 
    // You wouldn't normally use this in ALA, it is only for monad style.

    public static class ContinueWithExtensionMethod
    {
        public static DataflowToAction<T> ToAction<T>(this IBindable<T> source, Action<T> action)
        {
            return (DataflowToAction<T>)source.WireIn(new DataflowToAction<T>(action));
        }
    }
}
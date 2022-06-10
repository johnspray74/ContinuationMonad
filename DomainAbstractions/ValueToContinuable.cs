using ProgrammingParadigms;
using System.Threading.Tasks;

namespace DomainAbstractions
{

    public class ValueToContinuable<T> : IBindable<T>
    {
        readonly T value;
#pragma warning disable CS0649 // Field 'Continuation<T, U>.next' is never assigned to, and will always have its default value null
        private IContinuation<T> next; // output port
#pragma warning restore CS0649


        public ValueToContinuable(T value)
        {
            this.value = value;
        }

        public void Run()
        {
            var task = new Task<T>(() => value);
            next.PushTask(task);
            task.RunSynchronously();
        }
    }




    public static class ToContinuableExtensionMethods
    {
        public static ValueToContinuable<T> ToContinuable<T>(this T value)
        {
            return new ValueToContinuable<T>(value);
        }


    }
}
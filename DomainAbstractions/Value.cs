using ProgrammingParadigms;
using System.Threading.Tasks;

namespace DomainAbstractions
{
    public class Value<T> : IBindable<T>
    {
        readonly T value;
#pragma warning disable CS0649 // Field 'Continuation<T, U>.next' is never assigned to, and will always have its default value null
        private IDataflow<T> next; // output port
#pragma warning restore CS0649


        public Value(T value)
        {
            this.value = value;
        }

        public void Run()
        {
            next.Push(value);
        }
    }




    public static class ToContinuableExtensionMethods
    {
        public static Value<T> ToValue<T>(this T value)
        {
            return new Value<T>(value);
        }
    }
}
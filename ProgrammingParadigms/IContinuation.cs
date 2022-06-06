using System.Threading.Tasks;

namespace ProgrammingParadigms
{
    // Think of this interface as a bit like how IEnumarable is just used to get an IEnumerator, in this case we are going to get a Task.
    // Except this case the interface works the other way around and we will push the Task across.
    // That way WireTo works in the same direction as data flows, which is less confusing
    public interface IContinuation<T>
    {
        void PushTask(Task<T> task);
    }
}



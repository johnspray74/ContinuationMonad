using System.Threading.Tasks;

namespace ProgrammingParadigms
{
    // used by ALA programming paradigms for data flow
    // This is a push interface so is implemented by the destination
    // can be used for synchronous or async functions (async/await)
    // The WireIn/WireTo function wires up ports of this type. It wires them in the same direction as the dataflow
    public interface IDataflow<T>
    {
        void Push(T data);
    }
}



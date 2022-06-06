
namespace ProgrammingParadigms
{


    // The IContuable interface is only used as the type the Bind function.
    // Put it on all domain abstraction classes that are sources, that is can be wired to another IContinuable class using Bind.
    // The IContinuation interface is the one actually used to implement the Continuation dataflow programming paradigm
    // If instead of .Bind(function) you always use the longer full ALA syntax ".WireIn(new Continuation(function))" then you could delete the Bind function and this interface.
    // It would be nice if IConinuable was used as the programming paradigm interface, and iContinuation were deleted.
    // This could be done, but there are two disadvantes:
    // 1) WireIn would wire in the opposite direction of the dataflow - this is always confusing.
    // 2) The data sink end of the chain would be where you start the program - it will pull the Tasks through from source. Or you could use C# events but that is even more inconvenient.
    // So I really think IContinuation is the correct interface to use as the programming paradigm.
    // As I say, IContinuable is really only needed to support the Bind function to replicate the exact monad behaviour,
    // which we normally wouldn't use in ALA.
    // Normally in ALA we would use WireIn and have more specialized domain abstractions than Continuation(function)
    // which don't require you to have so much implemenation detail in the lambda expressions which are in the application layer.
    // see the abtractionlayerarchitecture.com online book chapter 3 section on monads for a more detailed discussion and comparison of the monad versus domain abstraction philosophies.

    public interface IContinuable<T> { };
}
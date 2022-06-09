
namespace ProgrammingParadigms
{
    // Used only by the ALA version, and only needed to support the Bind function to make the ALA version look like a monad

    // The IContuable interface is only used as the type for the Bind function. (The interface itself is empty)
    // Put it on all domain abstraction classes that are sources, that is can be wired using Bind.
    // For example Both the Continuation and ValueToContinuable classes use this interface.
    // The IContinuation interface is the one actually used to implement the ALA programming paradigm
    // If instead of .Bind(function) you always use the longer full ALA syntax ".WireIn(new Continuation(function))" then you could delete the Bind function and this interface.
    // We can consider using IConinuable as the main programming paradigm interface, and delete IContinuation.
    // There are two disadvantes:
    // 1) WireIn would wire in the opposite direction of the dataflow - this would be confusing.
    // 2) The data detination end of the chain would be where you start the program - it will pull the Tasks through from source. Or you could use C# events but that is even more inconvenient.
    // So I really think IContinuation is the correct interface to use as the programming paradigm.
    // As I say, IContinuable is really only needed to support the Bind function to replicate the exact monad behaviour,
    // which we normally wouldn't use in ALA. We are just show that ALA is versatile enough to compose functions juts like monads do.
    // Normally in ALA we would use WireIn and have more specialized domain abstractions than Continuation(function).
    // The ALA domain abstraction object would not require you to have so much implemenation detail in the lambda expressions which are in the application layer.
    // In other words the abstraction level of Domain abstraction can be a little more specific, taking some of the burden off the Application code, so it doesn't always need any actual code like lambda expressions.
    // see the abtractionlayerarchitecture.com online book chapter 3 section on monads for a more detailed discussion and comparison of the monad versus domain abstraction philosophies.

    public interface IContinuable<T> { };
}
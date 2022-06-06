#define AsyncAwaitVersion
// #define DebugThreads  // Write out the thread ID in different places so we can ensure everything runs on the same thread (Don't want multithreading!)
#define ALA

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using ProgrammingParadigms;
using DomainAbstractions;
using Foundation;
using System.Text;

namespace Application
{
    class Program
    {
        // Main just uses the Nito.AsyncEx package to proovde a dispatcher to enable console programs to use async/await on one thread.
        // It just calls another function called Application
        // (if you don't do this then Main will either complete immediately (ending the program before the asyncronous tasks finish)
        // or if you put in a ConsoleReadKey or Thread.Sleep at the end of Main, that will just block the main thread, causing the asynchronous tasks to run on other threads.
        // The program still works when it uses other threads, but I particular wanted to demonstrate this monad working on a single thread.


        static int Main()
        {
            try
            {
                Console.WriteLine("The application has started");
                AsyncContext.Run(() => Application());
                Console.WriteLine("The application has finished");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return -1;
            }
        }



        // The application function composes two functions using the Coninuation monad.
        // The Continuation monad consists of Task<T> plus the Totask extension method plus the Bind extension method (both of which are define in the layer below)
        // First it creates a source Task with the value 1. Then the first function adds 2, then the second function adds a number from the console. Then it prints the final result.
        // The thing is, because we are using the Coninuation monad, these two functions are allowed to take as much time as they want (be asynchronous).
        // To demo that, the first function does a delay, and the second function waits for Console input.
        // What the monad does is, despite these functions taking time, allows you to compose them like normal functions. You just have to use the Bind function to compose them.
        // And the function must return Task<U> instead of U.
        // The monad code in the lower layer takes care of making everything work.
        // There is no blocking of the main thread.
        // Everything runs on a single thread.

        // There are two versions - the first version uses async/await. If you are not familiar with async/await then use the second version which uses ContinueWith instead.



#if AsyncAwaitVersion
        // This is the version using async/await

#if !DebugThreads

#if !ALA

        static async Task Application()
        {
            var program = 1.ToTask();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            program.Bind(async (x) => { await Task.Delay(3000); return x + 2; })
            .Bind(async (x) =>
            {
                Console.WriteLine($"Value is {x}. Please enter a number to be added.");
                string line = null;
                await Task.Run(() => line = Console.ReadLine());
                return x + int.Parse(line);
            })
            .ContinueWith((x) => { Console.WriteLine($"Final result is {x.Result}."); }, TaskContinuationOptions.ExecuteSynchronously);
#pragma warning restore CS4014
            program.RunSynchronously();
            Console.WriteLine("Program will be running for 10s");
            await Task.Delay(10000); // Gives the asynchronous functions a chance to 
            Console.WriteLine("Press any key to finish program running");
            Console.ReadKey();  // This blocks the main thread, which is the one used for running the async tasks above, so don't execute this until other tasks are completed
        }

#else // ALA

        static async Task Application()
        {
            Wiring.diagnosticOutput += (s) => System.Diagnostics.Debug.WriteLine(s);
            var program = 1.ToContinuable();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            program
            .Bind(async (x) => { await Task.Delay(3000); return x + 2; })
            .Bind(async (x) =>
            {
                Console.WriteLine($"Value is {x}. Please enter a number to be added.");
                string line = null;
                await Task.Run(() => line = Console.ReadLine());
                return x + int.Parse(line);
            })
            .ContinueWith((x) => { Console.WriteLine($"Final result is {x}."); });
            Console.WriteLine(program);
#pragma warning restore CS4014
            program.Run();
            Console.WriteLine("Program will be running for 10s");
            await Task.Delay(10000); // Gives the asynchronous functions a chance to 
            Console.WriteLine("Press any key to finish program running");
            Console.ReadKey();  // This blocks the main thread, which is the one used for running the async tasks above, so don't execute this until other tasks are completed
        }


#endif

#else  // DebugThreads


        static async Task TaskMonadDemo()
        {
            Console.WriteLine($"Console thread: {Thread.CurrentThread.ManagedThreadId}");
            var program = 1.ToTask();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            program.Bind(async (x) =>
            {
                Console.WriteLine($"First function thread: {Thread.CurrentThread.ManagedThreadId}");
                await Task.Delay(3000);
                Console.WriteLine($"First function thread: {Thread.CurrentThread.ManagedThreadId}");
                return x + 2;
            })
            .Bind(async (x) =>
            {
                Console.WriteLine($"Second function thread: {Thread.CurrentThread.ManagedThreadId}");
                Console.WriteLine($"Value is {x}. Please enter a number to be added.");
                string line = null;
                await Task.Run(() => line = Console.ReadLine());
                Console.WriteLine($"Second function thread: {Thread.CurrentThread.ManagedThreadId}");
                return x + int.Parse(line);
            })
            .ContinueWith((x) =>
            {
                Console.WriteLine($"Final thread: {Thread.CurrentThread.ManagedThreadId}");
                Console.WriteLine($"Final result is {x.Result}.");
            }, TaskContinuationOptions.ExecuteSynchronously);
#pragma warning restore CS4014
            program.RunSynchronously();
            Console.WriteLine("Program will be running for 10s");
            await Task.Delay(10000);
            Console.WriteLine("Press any key to finish program running");
            Console.ReadKey();  // This blocks the main thread, which is the one used for running the async tasks above, so don't execute this until other tasks are completed
        }

#endif


#else // not AsyncAwaitVersion

        // This is the version using ContinueWith

#if !DebugThreads


        static async Task TaskMonadDemo()
        {
            var program = 1.ToTask();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            program.Bind((x) =>
            {
                var tcs = new TaskCompletionSource<int>();
                Task.Delay(3000).ContinueWith((t) =>
                {
                    tcs.SetResult(x + 4);
                });
                return tcs.Task;
            })
            .Bind((x) =>
            {
                var tcs = new TaskCompletionSource<int>();
                Console.WriteLine($"Value is {x}. Please enter a number to be added.");
                string line = null;
                Task.Run(() => line = Console.ReadLine()).ContinueWith((t)=>{
                    tcs.SetResult(x + int.Parse(line));
                });
                return tcs.Task;
            })
            .ContinueWith((x) =>
            {
                Console.WriteLine($"Final result is {x.Result}.");
            }, TaskContinuationOptions.ExecuteSynchronously);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            program.RunSynchronously();
            Console.WriteLine("Program will be running for 10s");
            await Task.Delay(10000);
            Console.WriteLine("Press any key to finish program running");
            Console.ReadKey();  // This blocks the main thread, which is the one used for running the async tasks above, so don't execute this until other tasks are completed
        }


#else  // DebugThreads


        static async Task TaskMonadDemo()
        {
            Console.WriteLine($"Console thread: {Thread.CurrentThread.ManagedThreadId}");
            var program = 1.ToTask();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            program.Bind((x) =>
            {
                var tcs = new TaskCompletionSource<int>();
                Console.WriteLine($"First function thread: {Thread.CurrentThread.ManagedThreadId}");
                Task.Delay(3000).ContinueWith((t) =>
                {
                    tcs.SetResult(x + 4);
                    Console.WriteLine($"First function thread: {Thread.CurrentThread.ManagedThreadId}");
                });
                Console.WriteLine($"First function thread: {Thread.CurrentThread.ManagedThreadId}");
                return tcs.Task;
            })
            .Bind((x) =>
            {
                Console.WriteLine($"Second function thread: {Thread.CurrentThread.ManagedThreadId}");
                var tcs = new TaskCompletionSource<int>();
                Console.WriteLine($"Value is {x}. Please enter a number to be added.");
                string line = null;
                Task.Run(() => line = Console.ReadLine()).ContinueWith((t)=>{
                    tcs.SetResult(x + int.Parse(line));
                    Console.WriteLine($"Second function thread: {Thread.CurrentThread.ManagedThreadId}");
                });
                return tcs.Task;
            })
            .ContinueWith((x) =>
            {
                Console.WriteLine($"Final thread: {Thread.CurrentThread.ManagedThreadId}");
                Console.WriteLine($"Final result is {x.Result}.");
            }, TaskContinuationOptions.ExecuteSynchronously);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            program.RunSynchronously();
            Console.WriteLine("Program will be running for 10s");
            await Task.Delay(10000);
            Console.WriteLine($"Program end thread: {Thread.CurrentThread.ManagedThreadId}");
            Console.WriteLine("Press any key to finish program running");
            Console.ReadKey();  // This blocks the main thread, which is the one used for running the async tasks above, so don't execute this until other tasks are completed
        }

#endif

#endif

    }

}


#if !ALA


// -----------------------------------------------------------------------------------
// Note that the monad itself is Not ImplementedException in the ProgrammingParadigms layer of ALA
// Normally this would be in its onw source file in i the ProgrammingParadigms fosubfolder.
// Monads are a two-layer pattern, so we only use ProgrammingParadigms and Application layers.



namespace ProgrammingParadigms
{
    // This monad is sometimes referred to as the ContinuationMonad and is built by adding Bind and Unit extension methods to Task<T> 


    public static class TaskMonadExtensionMethods
    {
        public static Task<T> ToContinuable<T>(this T value)
        {
            return new Task<T>(() => value);
        }


#if AsyncAwaitVersion


#if !DebugThreads

        // version of Bind function using async/await

        public static async Task<U> Bind<T, U>(this Task<T> source, Func<T, Task<U>> function)
        {
            return await function(await source);
        }


#else // DebugThreads


        public static async Task<U> Bind<T, U>(this Task<T> source, Func<T, Task<U>> function)
        {
            Console.WriteLine($"Bind thread: {Thread.CurrentThread.ManagedThreadId}");
            var x = await source;
            Console.WriteLine($"First ContinueWith thread: {Thread.CurrentThread.ManagedThreadId}");
            Console.WriteLine($"Input value to monad is {x}");
            var result = await function(x);
            Console.WriteLine($"Second ContinueWith thread: {Thread.CurrentThread.ManagedThreadId}");
            Console.WriteLine($"Output value from monad is {result}");
            return result;
        }


#endif


#else // not AsyncAwaitVersion

        // Version of Bind function using ConinueWith


#if !DebugThreads

        public static Task<U> Bind<T, U>(this Task<T> source, Func<T, Task<U>> function)
        {
            var tcs = new TaskCompletionSource<U>();
            source.ContinueWith(
                (t) =>
                {
                    function(t.Result).ContinueWith(
                        (t) =>
                        {
                            tcs.SetResult(t.Result);
                        },
                        TaskScheduler.FromCurrentSynchronizationContext()
                    );
                },
                TaskScheduler.FromCurrentSynchronizationContext()
            );
            return tcs.Task;
        }

#else  // DebugThreads

        public static Task<U> Bind<T, U>(this Task<T> source, Func<T, Task<U>> function)
        {
            var tcs = new TaskCompletionSource<U>();
            Console.WriteLine($"Bind thread: {Thread.CurrentThread.ManagedThreadId}");
            source.ContinueWith(
                (t) =>
                {
                    Console.WriteLine($"First ContinueWith thread: {Thread.CurrentThread.ManagedThreadId}");
                    Console.WriteLine($"Input value to monad is {t.Result}");
                    function(t.Result).ContinueWith(
                        (t) =>
                        {
                            Console.WriteLine($"Second ContinueWith thread: {Thread.CurrentThread.ManagedThreadId}");
                            Console.WriteLine($"Output value from monad is {t.Result}");
                            tcs.SetResult(t.Result);
                        },
                        TaskScheduler.FromCurrentSynchronizationContext()
                    );
                },
                TaskScheduler.FromCurrentSynchronizationContext()
            );
            return tcs.Task;
        }

#endif


#endif

    }
}


#else // ALA

namespace DomainAbstractions
{
    public static class TaskMonadExtensionMethods
    {
        public static ConstantContinuation<T> ToContinuable<T>(this T value)
        {
            return new ConstantContinuation<T>(value);
        }

        public static IContinuable<U> Bind<T, U>(this IContinuable<T> source, Func<T, Task<U>> function)
        {
            return (IContinuable<U>)source.WireIn(new Continuation<T, U>(function));
        }

        public static ContinuationToValue<T> ContinueWith<T>(this IContinuable<T> source, Action<T> action)
        {
            return (ContinuationToValue<T>)source.WireIn(new ContinuationToValue<T>(action));
        }
    }




    public class ConstantContinuation<T> : IContinuable<T> 
    {
        readonly T value;
#pragma warning disable CS0649 // Field 'Continuation<T, U>.next' is never assigned to, and will always have its default value null
        private IContinuation<T> next; // output port
#pragma warning restore CS0649


        public ConstantContinuation(T value)
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


    public class Continuation<T, U> : IContinuable<T>, IContinuation<T> // input port
    {
        readonly Func<T, Task<U>> function;
#pragma warning disable CS0649 // Field 'Continuation<T, U>.next' is never assigned to, and will always have its default value null
        private IContinuation<U> next; // output port
#pragma warning restore CS0649

        public Continuation(Func<T, Task<U>> function)
        {
            this.function = function;
        }

        async void IContinuation<T>.PushTask(Task<T> previousTask)
        {
            Console.WriteLine("PushTask");
            var result = await previousTask;
            Console.WriteLine($"PushTask {result}");
            next.PushTask(function(result));
            // next.PushTask(function(await previousTask));
        }
    }


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






namespace Foundation
{
    public static class Wiring
    {
        /// <Summary>
        /// Important method that wires and connects instances of classes that have ports by matching interfaces
        /// (with optional port name).
        /// WireTo is an extension method on the type object.
        /// If object A (this) has a private field of an interface, and object B implements the interface,
        /// then wire them together using reflection.
        /// The private field can also be a list.
        /// Returns this for fluent style programming.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="A">first object being wired</param>
        /// <param name="B">second object being wired</param>
        /// <param name="APortName">port fieldname in the A object (optional)</param>
        /// <returns>A</returns>
        /// ------------------------------------------------------------------------------------------------------------------
        /// WireTo method understanding what it does:
        /// <param name="A">
        /// The object on which the method is called is the object being wired from.
        /// It must have a private field of the interface type.
        /// </param> 
        /// <param name="B">
        /// The object being wired to. 
        /// It must implement the interface)
        /// </param> 
        /// <returns>this to support fluent programming style which allows multiple wiring to the same A object with .WireTo operators</returns>
        /// <remarks>
        /// 1. only wires compatible interfaces, e.g. A has a field of the type of an interface and B implements the interface
        /// 2. the field must be private (all publics are for use by the higher layer. This prevents confusion in the higher layer when when creating an instance of an abstraction - the ports should not be visible) 
        /// 3. can only wire a single matching port per call
        /// 4. Wires matching ports in the order they are decalared in class A (skips ports that are already wired)
        /// 5. looks for list as well (a list can block other ports of the same type lower down - they must be wired with an explict name)
        /// 6. you can overide the above order, or specify the port name explicitly, by giving the port field name in the WireTo method
        /// </remarks>
        public static T WireTo<T>(this T A, object B, string APortName = null)
        {
            // achieve the following via reflection
            // A.field = B; 
            // provided 1) field is private,
            // 2) field's type matches one of the implemented interfaces of B, and
            // 3) field is not yet assigned

            if (A == null)
            {
                throw new ArgumentException("A is null ");
            }
            if (B == null)
            {
                throw new ArgumentException("B is null ");
            }
            bool wired = false;


            // first get a list of private fields in object A (matching the name if given) and an array of implemented interfaces of object B
            // do the reflection once
            var BType = B.GetType();
            var AfieldInfos = A.GetType().GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Where(fi => fi.FieldType.IsInterface || fi.FieldType.IsGenericType && typeof(System.Collections.IEnumerable).IsAssignableFrom(fi.FieldType))   // filter for fields of interface type
                .Where(fi => APortName == null || fi.Name == APortName); // filter for given portname (if any) 
            var BinterfaceTypes = BType.GetInterfaces().ToList(); // do the reflection once


            // look through the private fields
            // (If multiple fields match, they are wired in the order they are declared)
            foreach (var AFieldInfo in AfieldInfos)
            {
                if (AFieldInfo.GetValue(A) == null)   // the private field is not yet assigned 
                {
                    // Is the field unassigned and type matches one of the interfaces of B
                    var BImplementedInterface = BinterfaceTypes.FirstOrDefault(interfaceType => AFieldInfo.FieldType == interfaceType);
                    if (BImplementedInterface != null)  // there is a matching interface
                    {
                        AFieldInfo.SetValue(A, B);  // do the wiring
                        wired = true;
                        diagnosticOutput?.Invoke(WiringToString(A, B, AFieldInfo));
                        break;
                    }
                }

                // Is the field a list whose generic type matches one of the interfaces of B
                var fieldType = AFieldInfo.FieldType;
                if (fieldType.IsGenericType && typeof(System.Collections.IEnumerable).IsAssignableFrom(fieldType))
                {
                    var AGenericArgument = AFieldInfo.FieldType.GetGenericArguments()[0];
                    var BImplementedInterface = BinterfaceTypes.FirstOrDefault(interfaceType => AGenericArgument.IsAssignableFrom(interfaceType));
                    if (BImplementedInterface != null)
                    {
                        var AListFieldValue = AFieldInfo.GetValue(A);
                        if (AListFieldValue == null)  // list not created yet
                        {
                            var listType = typeof(List<>);
                            Type[] listParam = { BImplementedInterface };
                            AListFieldValue = Activator.CreateInstance(listType.MakeGenericType(listParam));
                            AFieldInfo.SetValue(A, AListFieldValue);
                        }
                        // now add the B object to the list
                        AListFieldValue.GetType().GetMethod("Add").Invoke(AListFieldValue, new[] { B });
                        wired = true;
                        diagnosticOutput?.Invoke(WiringToString(A, B, AFieldInfo));
                        break;
                    }
                }
            }

            if (!wired) // throw exception
            {
                var AinstanceName = A.GetType().GetProperties().FirstOrDefault(f => f.Name == "InstanceName")?.GetValue(A);
                var BinstanceName = B.GetType().GetProperties().FirstOrDefault(f => f.Name == "InstanceName")?.GetValue(B);

                if (APortName != null)
                {
                    // a specific port was specified - see if the port was already wired
                    var AfieldInfo = AfieldInfos.FirstOrDefault();
                    if (AfieldInfo?.GetValue(A) != null) throw new Exception($"Port already wired {A.GetType().Name}[{AinstanceName}].{APortName} to {BType.Name}[{BinstanceName}]");
                }
                string AFieldsConsidered = string.Join(", ", AfieldInfos.Select(f => $"{f.Name}:{f.FieldType}, {(f.GetValue(A) == null ? "unassigned" : "assigned")}"));
                string BInterfacesConsidered = string.Join(", ", AfieldInfos.Select(f => $"{f.FieldType}"));
                throw new Exception($"Failed to wire {A.GetType().Name}[{AinstanceName}].\"{APortName}\" to {BType.Name}[{BinstanceName}]. Considered fields of A [{AFieldsConsidered}]. Considered interfaces of B [{BInterfacesConsidered}].");
            }
            return A;
        }

        /// <summary>
        /// Same as WireTo except that it return the second object to support composing a chain of instances of abstractions without nested syntax
        /// e.g. new A().WireIn(new B()).WireIn(new C());
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="A">first object being wired</param>
        /// <param name="B">second object being wired</param>
        /// <param name="APortName">port fieldname in the A object (optional)</param>
        /// <returns>B to support fluent programming style which allows wiring a chain of objects within .WireIn operators</returns>
        public static object WireIn<T>(this T A, object B, string APortName = null)
        {
            WireTo(A, B, APortName);
            return B;
        }



        private static string WiringToString(object A, object B, FieldInfo matchedField)
        {
            var AClassName = A.GetType().Name;
            var BClassName = B.GetType().Name;
            var AInstanceName = "No InstanceName";
            var BInstanceName = "No InstanceName";
            var AInstanceNameField = A.GetType().GetField("InstanceName");
            var BInstanceNameField = B.GetType().GetField("InstanceName");
            if (AInstanceNameField != null) AInstanceName = (string)AInstanceNameField.GetValue(A);
            if (AInstanceNameField != null) AInstanceName = (string)AInstanceNameField.GetValue(A);
            return $"WireTo {AClassName}[{AInstanceName}].{matchedField.Name} ---> {BClassName}[{BInstanceName}] : {matchedField.FieldType}";
        }


        // diagnostics output port
        // doesn't have to be wired anywhere
        public delegate void DiagnosticOutputDelegate(string output);
        public static event DiagnosticOutputDelegate diagnosticOutput;

    }






    public static class FoundationExtensionMethods
    {


        public static string ObjectToString<T>(this T instance) where T : class
        {

            if (instance == null)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            sb.Append("Class ");

            var type = instance.GetType();

            var typeName = type.Name;
            sb.Append(typeName);
            var instanceProperty = type.GetProperty("instanceName");
            if (instanceProperty != null)
            {
                sb.Append($" \"{instanceProperty.GetValue(instance)}\"");
            }
            if (instance.GetType().IsGenericType) sb.Append(" (Generic)");

            sb.AppendLine();
            sb.AppendLine(new string('=', sb.Length));


            var strListType = typeof(List<string>);
            var strArrType = typeof(string[]);

            var arrayTypes = new[] { strListType, strArrType };
            var handledTypes = new[] { typeof(Int32), typeof(String), typeof(bool), typeof(DateTime), typeof(double), typeof(decimal), strListType, strArrType };



            var propertyInfos = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var fieldInfos = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var methodInfos = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            /*
            var max = 0;
            if (propertyInfos.Length >  0)
            {
                max = propertyInfos.Select((p) => p.Name.Length).Max();
            }
            if (fieldInfos.Length > 0)
            {
                var max2 = fieldInfos.Select((p) => p.Name.Length).Max();
                if (max2 > max) max = max2;
            }
            */

            foreach (var fieldInfo in fieldInfos)
            {
                string[] strings = { "_methodBase", "_methodPtr", "_methodPtrAux" };

                var name = fieldInfo.Name;
                if (!strings.Contains(name))
                {
                    sb.Append("Field ");
                    sb.Append(fieldInfo.ToString() + " ");
                    // sb.Append(fieldInfo.FieldType + " ");
                    // Ssb.Append(name + " ");
                    if (handledTypes.Contains(fieldInfo.FieldType))
                    {

                        if (fieldInfo.GetValue(instance) != null)
                        {
                            var s = arrayTypes.Contains(fieldInfo.FieldType)
                                ? string.Join(", ", (IEnumerable<string>)fieldInfo.GetValue(instance))
                                : fieldInfo.GetValue(instance).ToString();
                            sb.AppendLine(s);
                        }
                        else
                        {
                            sb.AppendLine("null");
                        }
                    }
                    else if (typeof(object).IsAssignableFrom(fieldInfo.FieldType))
                    {
                        sb.AppendLine();
                        sb.Append(fieldInfo.GetValue(instance).ObjectToString().Indent());
                    }
                    else
                    {
                        sb.AppendLine("GetValue not supported");
                    }
                }
            }
            foreach (var propertyInfo in propertyInfos)
            {
                sb.Append("Property ");
                sb.Append(propertyInfo.ToString() + " ");
                try
                {

                    if (propertyInfo.GetValue(instance, null) == null)
                    {
                        sb.AppendLine("null");
                    }
                    else
                    {
                        var s = arrayTypes.Contains(propertyInfo.PropertyType)
                                ? string.Join(", ", (IEnumerable<string>)propertyInfo.GetValue(instance, null))
                                : propertyInfo.GetValue(instance, null);
                        sb.AppendLine();
                    }
                }
                catch
                {
                    sb.AppendLine("Exception getting value");
                }
            }
            foreach (var methodInfo in methodInfos)
            {
                if (methodInfo.Name[0] == '<')
                {
                    sb.Append("Method ");
                    sb.AppendLine(methodInfo.ToString());
                }
            }
            return sb.ToString();

        }


        public static string Indent(this string s)
        {
            bool first = true;
            StringBuilder sb = new StringBuilder();
            foreach (var line in s.Split(Environment.NewLine))
            {
                if (!first) sb.AppendLine();
                if (line != "")
                {
                    sb.Append("\t" + line);
                    first = false;
                }
            }
            return sb.ToString();
        }

        public static string Join(this IEnumerable<string> s, string separator)
        {
            return s.Aggregate(new StringBuilder(), (sb, s) => { if (sb.Length > 0) sb.Append(separator); sb.Append(s); return sb; }).ToString();
        }

    }
}



#endif // ALA




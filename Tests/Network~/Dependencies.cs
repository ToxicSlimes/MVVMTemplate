// Minimal stand-ins for Reflex's unrelated Unity, logging, injection and diagnostics APIs.
// The container, binding, resolver and disposal implementation is compiled from the real source.
using System;
using System.Collections.Generic;
using System.Linq;
namespace Reflex.Exceptions
{
    class ContractDefinitionException : Exception { public ContractDefinitionException(Type a, Type b) { } }
    class UnknownContractException : Exception { public UnknownContractException(Type a) { } }
}
namespace Reflex.Extensions
{
    static class ProbeExtensions
    {
        public static bool IsEnumerable(this Type type, out Type element) { element = null; return false; }
        public static IEnumerable<object> CastDynamic(this IEnumerable<object> items, Type type) => items;
        public static IEnumerable<T> Reversed<T>(this List<T> items) => Enumerable.Reverse(items);
    }
}
namespace Reflex.Injectors
{
    static class ConstructorInjector { public static object Construct(Type t, Reflex.Core.Container c) => throw new NotSupportedException(); }
    static class AttributeInjector { public static void Inject(object o, Reflex.Core.Container c) => throw new NotSupportedException(); }
}
namespace Reflex.Logging
{
    enum LogLevel { Info }
    static class ReflexLogger { public static void Log(string message, LogLevel level) { } }
}
static class Diagnosis
{
    public static void RegisterBuildCallSite(object value) { }
    public static void RegisterCallSite(object value) { }
    public static void IncrementResolutions(object value) { }
    public static void RegisterInstance(object value, object instance) { }
}

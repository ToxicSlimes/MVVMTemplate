namespace Scellecs.Morpeh.SourceGenerator.AspectGenerator
{
    public interface IAspect<T1, T2> : global::Scellecs.Morpeh.IAspect, global::Scellecs.Morpeh.IFilterExtension
        where T1 : struct, global::Scellecs.Morpeh.IComponent
        where T2 : struct, global::Scellecs.Morpeh.IComponent { }

    public interface IAspect<T1, T2, T3> : global::Scellecs.Morpeh.IAspect, global::Scellecs.Morpeh.IFilterExtension
        where T1 : struct, global::Scellecs.Morpeh.IComponent
        where T2 : struct, global::Scellecs.Morpeh.IComponent
        where T3 : struct, global::Scellecs.Morpeh.IComponent { }

    public interface IAspect<T1, T2, T3, T4> : global::Scellecs.Morpeh.IAspect, global::Scellecs.Morpeh.IFilterExtension
        where T1 : struct, global::Scellecs.Morpeh.IComponent
        where T2 : struct, global::Scellecs.Morpeh.IComponent
        where T3 : struct, global::Scellecs.Morpeh.IComponent
        where T4 : struct, global::Scellecs.Morpeh.IComponent { }

    public interface IAspect<T1, T2, T3, T4, T5> : global::Scellecs.Morpeh.IAspect, global::Scellecs.Morpeh.IFilterExtension
        where T1 : struct, global::Scellecs.Morpeh.IComponent
        where T2 : struct, global::Scellecs.Morpeh.IComponent
        where T3 : struct, global::Scellecs.Morpeh.IComponent
        where T4 : struct, global::Scellecs.Morpeh.IComponent
        where T5 : struct, global::Scellecs.Morpeh.IComponent { }

    public interface IAspect<T1, T2, T3, T4, T5, T6> : global::Scellecs.Morpeh.IAspect, global::Scellecs.Morpeh.IFilterExtension
        where T1 : struct, global::Scellecs.Morpeh.IComponent
        where T2 : struct, global::Scellecs.Morpeh.IComponent
        where T3 : struct, global::Scellecs.Morpeh.IComponent
        where T4 : struct, global::Scellecs.Morpeh.IComponent
        where T5 : struct, global::Scellecs.Morpeh.IComponent
        where T6 : struct, global::Scellecs.Morpeh.IComponent { }

    public interface IAspect<T1, T2, T3, T4, T5, T6, T7> : global::Scellecs.Morpeh.IAspect, global::Scellecs.Morpeh.IFilterExtension
        where T1 : struct, global::Scellecs.Morpeh.IComponent
        where T2 : struct, global::Scellecs.Morpeh.IComponent
        where T3 : struct, global::Scellecs.Morpeh.IComponent
        where T4 : struct, global::Scellecs.Morpeh.IComponent
        where T5 : struct, global::Scellecs.Morpeh.IComponent
        where T6 : struct, global::Scellecs.Morpeh.IComponent
        where T7 : struct, global::Scellecs.Morpeh.IComponent { }

    public interface IAspect<T1, T2, T3, T4, T5, T6, T7, T8> : global::Scellecs.Morpeh.IAspect, global::Scellecs.Morpeh.IFilterExtension
        where T1 : struct, global::Scellecs.Morpeh.IComponent
        where T2 : struct, global::Scellecs.Morpeh.IComponent
        where T3 : struct, global::Scellecs.Morpeh.IComponent
        where T4 : struct, global::Scellecs.Morpeh.IComponent
        where T5 : struct, global::Scellecs.Morpeh.IComponent
        where T6 : struct, global::Scellecs.Morpeh.IComponent
        where T7 : struct, global::Scellecs.Morpeh.IComponent
        where T8 : struct, global::Scellecs.Morpeh.IComponent { }

    public interface IAspect<T1, T2, T3, T4, T5, T6, T7, T8, T9> : global::Scellecs.Morpeh.IAspect, global::Scellecs.Morpeh.IFilterExtension
        where T1 : struct, global::Scellecs.Morpeh.IComponent
        where T2 : struct, global::Scellecs.Morpeh.IComponent
        where T3 : struct, global::Scellecs.Morpeh.IComponent
        where T4 : struct, global::Scellecs.Morpeh.IComponent
        where T5 : struct, global::Scellecs.Morpeh.IComponent
        where T6 : struct, global::Scellecs.Morpeh.IComponent
        where T7 : struct, global::Scellecs.Morpeh.IComponent
        where T8 : struct, global::Scellecs.Morpeh.IComponent
        where T9 : struct, global::Scellecs.Morpeh.IComponent { }

    public interface IAspect<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : global::Scellecs.Morpeh.IAspect, global::Scellecs.Morpeh.IFilterExtension
        where T1 : struct, global::Scellecs.Morpeh.IComponent
        where T2 : struct, global::Scellecs.Morpeh.IComponent
        where T3 : struct, global::Scellecs.Morpeh.IComponent
        where T4 : struct, global::Scellecs.Morpeh.IComponent
        where T5 : struct, global::Scellecs.Morpeh.IComponent
        where T6 : struct, global::Scellecs.Morpeh.IComponent
        where T7 : struct, global::Scellecs.Morpeh.IComponent
        where T8 : struct, global::Scellecs.Morpeh.IComponent
        where T9 : struct, global::Scellecs.Morpeh.IComponent
        where T10 : struct, global::Scellecs.Morpeh.IComponent { }

    public static class AspectFilterExtensions
    {
        public static global::Scellecs.Morpeh.FilterBuilder WithAspect<T>(
            this global::Scellecs.Morpeh.FilterBuilder filter)
            where T : struct, global::Scellecs.Morpeh.IAspect, global::Scellecs.Morpeh.IFilterExtension
        {
            return filter.Extend<T>();
        }
    }
}

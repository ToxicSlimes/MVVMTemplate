using System.Runtime.CompilerServices;
using Scellecs.Morpeh;

namespace Scellecs.Morpeh.SourceGenerator.FilterSystemGenerator
{
    public abstract class FilterSystem : ISystem
    {
        public World World { get; set; }

        private Filter Filter { get; set; }

        public virtual void OnAwake()
        {
            Initialize();
            Filter = GetFilter(World.Filter).Build();
        }

        protected virtual void Initialize()
        {
        }

        protected abstract FilterBuilder GetFilter(FilterBuilder builder);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnUpdate(float _)
        {
            Execute(Filter);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Execute(Filter entities)
        {
            foreach (var entity in entities)
            {
                Execute(entity);
            }
        }

        protected virtual void Execute(Entity entity)
        {
        }

        public virtual void Dispose()
        {
        }
    }
}

using Scellecs.Morpeh;

namespace Core.Infrastructure.Morpeh
{
    public abstract class EventBridgeSystem<TEvent> : ISystem
        where TEvent : struct, IComponent
    {
        private Filter _events;

        public World World { get; set; }

        public void OnAwake() => _events = World.Filter.With<TEvent>().Build();

        public void OnUpdate(float deltaTime)
        {
            foreach (var entity in _events)
            {
                Bridge(entity);
            }
        }

        public virtual void Dispose()
        {
        }

        protected abstract void Bridge(Entity entity);
    }
}

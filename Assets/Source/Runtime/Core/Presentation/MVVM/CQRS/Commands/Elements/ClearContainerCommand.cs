using Core.Mediator.Abstractions;
using UnityEngine;

namespace Core.Application.Commands.Elements
{
    public readonly struct ClearContainerCommand : IRequest<Nothing>
    {
        public readonly Transform Container;

        public ClearContainerCommand(Transform container)
        {
            Container = container;
        }
    }
}

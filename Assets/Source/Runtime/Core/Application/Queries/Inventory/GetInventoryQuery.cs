using System.Collections.Generic;
using Core.Domain.Models.Elements;
using Core.Mediator.Abstractions;

namespace Core.Application.Queries.Inventory
{
    public readonly struct GetInventoryQuery : IRequest<List<InventoryItemData>>
    {
    }
}

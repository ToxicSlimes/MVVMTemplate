using System.Collections.Generic;
using Core.Data.Domain.Models.Elements;
using Core.Infrastructure.Mediator.Abstractions;

namespace Core.Application.Queries.Inventory
{
    public readonly struct GetInventoryQuery : IRequest<List<InventoryItemData>>
    {
    }
}

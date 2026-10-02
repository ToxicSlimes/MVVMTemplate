using System.Collections.Generic;
using System.Threading;
using Core.Abstractions.MVVM.Models.Inventory;
using Core.Application.Queries.Inventory;
using Core.Domain.Models.Elements;
using Core.Mediator.Abstractions;
using Core.Mediator.Handlers;
using Cysharp.Threading.Tasks;

namespace Core.Application.Handlers.Inventory
{
    public sealed class GetInventoryHandler : IRequestHandler<GetInventoryQuery, List<InventoryItemData>>
    {
        private readonly IInventoryModel _model;

        public GetInventoryHandler(IInventoryModel model)
        {
            _model = model;
        }

        public UniTask<List<InventoryItemData>> Handle(GetInventoryQuery query, CancellationToken ct)
        {
            return UniTask.FromResult(_model.Items);
        }
    }
}

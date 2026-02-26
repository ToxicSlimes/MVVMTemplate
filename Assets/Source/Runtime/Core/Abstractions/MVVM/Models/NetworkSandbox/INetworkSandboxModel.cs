using System;
using Core.Abstractions.MVVM.Models;
using Core.Domain.Models.Workflow;

namespace Core.Abstractions.MVVM.Models.NetworkSandbox
{
    public interface INetworkSandboxModel : IModel
    {
        event Action<string> WorkflowIdChanged;
        event Action<string> ResponseTextChanged;
        event Action<string> StatusTextChanged;
        event Action<bool> IsLoadingChanged;
        event Action<WorkflowModel> CurrentWorkflowChanged;

        string WorkflowId { get; set; }
        string ResponseText { get; set; }
        string StatusText { get; set; }
        bool IsLoading { get; set; }
        WorkflowModel CurrentWorkflow { get; set; }
    }
}

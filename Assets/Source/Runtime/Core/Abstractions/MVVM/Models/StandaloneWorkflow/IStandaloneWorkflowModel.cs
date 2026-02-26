using System;
using System.Collections.Generic;
using Core.Domain.Models.Workflow;

namespace Core.Abstractions.MVVM.Models.StandaloneWorkflow
{
    public interface IStandaloneWorkflowModel : IModel
    {
        event Action<string> SelectedWorkflowIdChanged;
        event Action<List<WorkflowListItem>> AvailableWorkflowsChanged;
        event Action<bool> IsLoadingChanged;
        event Action<string> StatusTextChanged;
        event Action<string> ErrorTextChanged;
        event Action<StorageContextType> StorageTypeChanged;
        event Action<string> SessionIdChanged;

        string SelectedWorkflowId { get; set; }
        List<WorkflowListItem> AvailableWorkflows { get; set; }
        bool IsLoading { get; set; }
        string StatusText { get; set; }
        string ErrorText { get; set; }
        StorageContextType StorageType { get; set; }
        string SessionId { get; set; }
    }

    public class WorkflowListItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool HasStandaloneTrigger { get; set; }
        public string ParentWorkflowId { get; set; }
        public Dictionary<string, object> DefaultStorage { get; set; }
    }
}

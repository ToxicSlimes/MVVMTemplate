using System;

namespace Core.Domain.UI
{
    /// <summary>
    /// TEMPLATE EXAMPLE: Data for a confirmation popup with callbacks.
    /// Shows how to pass actions through popup data for user decisions.
    /// </summary>
    public class ConfirmPopupData : IPopupData
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string ConfirmText { get; set; } = "Confirm";
        public string CancelText { get; set; } = "Cancel";
        public Action OnConfirm { get; set; }
        public Action OnCancel { get; set; }
    }
}

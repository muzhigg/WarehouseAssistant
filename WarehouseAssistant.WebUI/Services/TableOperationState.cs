namespace WarehouseAssistant.WebUI.Services;

public class TableOperationState
{
    public bool IsInProgress { get; set; }
    
    public event Action OnChange;
    
    public void SetInProgress(bool inProgress)
    {
        IsInProgress = inProgress;
        NotifyStateChanged();
    }
    
    private void NotifyStateChanged() => OnChange?.Invoke();
}
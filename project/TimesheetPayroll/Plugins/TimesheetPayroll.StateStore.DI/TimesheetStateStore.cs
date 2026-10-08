namespace TimesheetPayroll.StateStore.DI;

public class StateStoreBase
{
    private Action? _listeners;
    public void AddStateChangeListeners(Action listener) => _listeners += listener;
    public void RemoveStateChangeListeners(Action listener) => _listeners -= listener;
    public void BroadcastStateChange() => _listeners?.Invoke();
}

public interface ITimesheetStateStore
{
    long SelectedPeriodId { get; set; }
    long CurrentEmployeeId { get; set; }
    void AddStateChangeListeners(Action listener);
    void RemoveStateChangeListeners(Action listener);
    void BroadcastStateChange();
}

public class TimesheetStateStore : StateStoreBase, ITimesheetStateStore
{
    public long SelectedPeriodId { get; set; } = 10; // Mặc định Tháng 10/2026
    public long CurrentEmployeeId { get; set; } = 1;  // Mặc định EMP001 (Nguyễn Văn A)
}

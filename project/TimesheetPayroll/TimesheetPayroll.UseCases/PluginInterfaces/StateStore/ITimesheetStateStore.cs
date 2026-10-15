namespace TimesheetPayroll.UseCases.PluginInterfaces.StateStore;

/// <summary>
/// Giao diện quản lý trạng thái phiên làm việc (In-Memory State Store)
/// lưu giữ thông tin kỳ lương và nhân viên đang được chọn thao tác trên UI.
/// </summary>
public interface ITimesheetStateStore
{
    /// <summary>
    /// Mã định danh kỳ lương đang được chọn.
    /// </summary>
    long SelectedPeriodId { get; set; }

    /// <summary>
    /// Mã định danh nhân viên hiện tại.
    /// </summary>
    long CurrentEmployeeId { get; set; }

    /// <summary>
    /// Đăng ký hàm lắng nghe sự kiện thay đổi trạng thái giao diện.
    /// </summary>
    void AddStateChangeListeners(Action listener);

    /// <summary>
    /// Hủy đăng ký hàm lắng nghe sự kiện.
    /// </summary>
    void RemoveStateChangeListeners(Action listener);

    /// <summary>
    /// Phát thông báo trạng thái thay đổi tới toàn bộ thành phần UI đang lắng nghe.
    /// </summary>
    void BroadcastStateChange();
}

using MauiApplication.Services.Application._interfaces;

namespace MauiApplication.Services.Application;

public class ShiftStateService : IShiftStateService
{
    public event Action ShiftWasChanged;

    public void ShiftChange()
    {
        ShiftWasChanged?.Invoke();
    }
}
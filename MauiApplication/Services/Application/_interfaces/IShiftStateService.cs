using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApplication.Services.Application._interfaces
{
    public interface IShiftStateService
    {
        public event Action ShiftWasChanged;

        public void ShiftChange();
    }
}

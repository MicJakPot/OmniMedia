using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OmniMedia.ViewModels.Base
{
    // Klasa bazowa dla wszystkich ViewModeli, dziedziczy z ReactiveObject dla obsługi reaktywności
    public class ViewModelBase : ReactiveObject, IDisposable
    {
        // Implementacja IDisposable jest przydatna do zarządzania zasobami, np. subskrypcjami
        protected CompositeDisposable Disposables { get; } = new CompositeDisposable();

        public virtual void Dispose()
        {
            Disposables.Dispose();
        }

        // Można dodać wspólne właściwości lub metody dla wszystkich ViewModeli tutaj w przyszłości
    }
}

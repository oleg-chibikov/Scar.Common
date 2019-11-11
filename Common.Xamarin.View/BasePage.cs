using System;
using Scar.Common.View.Contracts;
using Xamarin.Forms;

namespace Scar.Common.Xamarin.View
{
    class BasePage : ContentPage, IDisplayable
    {
        public event EventHandler SizeChanged;
        public event EventHandler Closed;
        public event EventHandler Loaded;

        private readonly IList<IDisposable> _associatedDisposables = new List<IDisposable>();

        public void AssociateDisposable(IDisposable disposable)
        {
            _associatedDisposables.Add(disposable);
        }

        public void Close()
        {
            Navigation.PopAsync();
        }

        public void Restore()
        {
            Show();
        }

        public void Show()
        {
            Navigation.PushAsync(this).Wait();
        }

        public bool? ShowDialog()
        {
            Navigation.PushModalAsync(this).Wait();
            return true;
        }

        public bool UnassociateDisposable(IDisposable disposable)
        {
            return _associatedDisposables.Remove(disposable);
        }
    }
}

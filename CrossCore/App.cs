using CrossCore.Interfaces;
using CrossCore.Services;
using CrossCore.ViewModels;
using MvvmCross;
using MvvmCross.ViewModels;

namespace CrossCore
{
    public class App : MvxApplication
    {
        public override void Initialize()
        {
            Mvx.IoCProvider.RegisterType<IWorker1, Worker1>();

            RegisterCustomAppStart<AppStart>();
        }
    }
}
using CrossCore.Interfaces;
using MvvmCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossCore.ViewModels
{
    public class DisplayViewModel : MvxViewModel
    {
        private string _fullName = "Bob";
        private readonly IWorker1 ServiceMethods;


        public DisplayViewModel(IWorker1 workers)
        {
            ServiceMethods = workers;
        }
        public string FullName
        {
            get { return _fullName; }
            set
            {
                _fullName = value;
                RaisePropertyChanged(() => FullName);
            }
        }

        public override async Task Initialize()
        {
            await base.Initialize();
        }

    }
}

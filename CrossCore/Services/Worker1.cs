using CrossCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossCore.Services
{
    public class Worker1 : IWorker1
    {
        public string DoWork1()
        {
            return "DoWork1 called";
        }
    }
}

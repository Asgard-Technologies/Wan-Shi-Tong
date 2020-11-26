using Autofac;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SSC.Cmdlets
{
    interface ICmdlet<CmdletOptions>
    {
        void RegisterServices(ContainerBuilder builder, CmdletOptions options);

        Task<int> Run(CmdletOptions options);
    }
}

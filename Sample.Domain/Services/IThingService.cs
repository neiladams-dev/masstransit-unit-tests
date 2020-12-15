using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Sample.Domain.Entities;

namespace Sample.Domain.Services
{
    public interface IThingService
    {
        Task CreateAsync(Thing thing);

        Task SendCreateThingCommandAsync(Thing thing);
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Sample.Domain.Contracts
{
    public interface IThingCreated
    {
        int Id { get; set; }
        string Name { get; set; }
    }
}

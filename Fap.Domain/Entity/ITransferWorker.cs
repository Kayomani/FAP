using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fap.Domain.Entity
{
    public interface ITransferWorker
    {
        long Length { get; }
        bool IsComplete { get; }
        long Speed { get; }
        string Status { get; }
        long Position { get; }
    }
}

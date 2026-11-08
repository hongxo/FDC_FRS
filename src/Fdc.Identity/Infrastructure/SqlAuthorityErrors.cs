using System.Data.Common;

namespace Fdc.Identity.Infrastructure;

internal static class SqlAuthorityErrors
{
    // EF may wrap transient SQL connection errors in InvalidOperationException.
    // Do not swallow unrelated programming errors or request cancellation.
    internal static bool IsUnavailable(Exception error)
    {
        for (Exception? current = error; current is not null; current = current.InnerException)
            if (current is DbException or TimeoutException) return true;
        return false;
    }
}

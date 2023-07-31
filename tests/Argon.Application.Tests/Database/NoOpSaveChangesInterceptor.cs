using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Argon.Application.Tests.Database;

public class NoOpSaveChangesInterceptor : SaveChangesInterceptor
{
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MicrosoftOrleans.Domain.Entities;

namespace MicrosoftOrleans.Application.Common.Interfaces;

public interface IOrleansDbContext
{
    DatabaseFacade GetDatabase();
}

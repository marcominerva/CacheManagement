using CacheManagement.DataAccessLayer;
using CacheManagement.Models;
using CacheManagement.Models.Requests;
using CacheManagement.Notifications.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using Microsoft.Extensions.Caching.Memory;

namespace CacheManagement.Endpoints;

public class PeopleEndpoint : IEndpointRouteHandlerBuilder
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var peopleEnpoint = endpoints.MapGroup("/api/people");

        peopleEnpoint.MapGet(string.Empty, GetListAsync)
            .Produces<IEnumerable<Person>>()
            .WithOpenApi();

        peopleEnpoint.MapGet("{id:guid}", GetAsync)
            .Produces<Person>()
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi()
            .WithName("GetPerson");

        peopleEnpoint.MapPost(string.Empty, InsertAsync)
            .Produces(StatusCodes.Status201Created)
            .WithOpenApi();

        peopleEnpoint.MapPut("{id:guid}", UpdateAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();

        peopleEnpoint.MapDelete("{id:guid}", DeleteAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();
    }
    private static async Task<IResult> GetListAsync(ApplicationDbContext dbContext, IMemoryCache cache)
    {
        var people = await cache.GetOrCreateAsync("People", async entry =>
        {
            var people = await dbContext.People.AsNoTracking()
                .OrderBy(p => p.FirstName).ThenBy(p => p.LastName)
                .Select(p => new Person(p.Id, p.FirstName, p.LastName, p.City.Name))
                .ToListAsync();

            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);

            return people;
        });

        return TypedResults.Ok(people);
    }

    private static async Task<IResult> GetAsync(Guid id, ApplicationDbContext dbContext, IMemoryCache cache)
    {
        var person = cache.Get<Person>($"Person-{id}");
        if (person is null)
        {
            var dbPerson = await dbContext.People.AsNoTracking()
                .Include(p => p.City).FirstOrDefaultAsync(p => p.Id == id);

            if (dbPerson is null)
            {
                return TypedResults.NotFound();
            }

            person = new Person(dbPerson.Id, dbPerson.FirstName, dbPerson.LastName, dbPerson.City.Name);
            cache.Set($"Person-{id}", person, TimeSpan.FromMinutes(5));
        }

        return TypedResults.Ok(person);
    }

    private static async Task<IResult> InsertAsync(SavePersonRequest request, ApplicationDbContext dbContext, IPublisher publisher, LinkGenerator linkGenerator)
    {
        var dbPerson = new DataAccessLayer.Entities.Person
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            CityId = request.CityId
        };

        dbContext.People.Add(dbPerson);
        await dbContext.SaveChangesAsync();

        await publisher.Publish(new PersonCreated(dbPerson.Id));

        var url = linkGenerator.GetPathByName("GetPerson", new { id = dbPerson.Id });
        return TypedResults.Created(url);
    }

    private static async Task<IResult> UpdateAsync(Guid id, SavePersonRequest request, ApplicationDbContext dbContext, IPublisher publisher)
    {
        var dbPerson = await dbContext.People.FirstOrDefaultAsync(p => p.Id == id);
        if (dbPerson is null)
        {
            return TypedResults.NotFound();
        }

        dbPerson.FirstName = request.FirstName;
        dbPerson.LastName = request.LastName;
        dbPerson.CityId = request.CityId;

        await dbContext.SaveChangesAsync();

        await publisher.Publish(new PersonUpdated(id));      

        return TypedResults.NoContent();
    }

    private static async Task<IResult> DeleteAsync(Guid id, ApplicationDbContext dbContext, IPublisher publisher)
    {
        var rowDeleted = await dbContext.People.Where(p => p.Id == id).ExecuteDeleteAsync();
        if (rowDeleted == 0)
        {
            return TypedResults.NotFound();
        }

        await publisher.Publish(new PersonDeleted(id));

        return TypedResults.NoContent();
    }
}

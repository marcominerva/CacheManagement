using CacheManagement.DataAccessLayer;
using CacheManagement.Models;
using CacheManagement.Models.Requests;
using Microsoft.EntityFrameworkCore;

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
    private static async Task<IResult> GetListAsync(ApplicationDbContext dbContext)
    {
        var people = await dbContext.People.AsNoTracking()
            .OrderBy(p => p.FirstName).ThenBy(p => p.LastName)
            .Select(p => new Person(p.Id, p.FirstName, p.LastName, p.City.Name))
            .ToListAsync();

        return TypedResults.Ok(people);
    }

    private static async Task<IResult> GetAsync(Guid id, ApplicationDbContext dbContext)
    {
        var dbPerson = await dbContext.People.AsNoTracking()
            .Include(p => p.City).FirstOrDefaultAsync(p => p.Id == id);

        if (dbPerson is null)
        {
            return TypedResults.NotFound();
        }

        var person = new Person(dbPerson.Id, dbPerson.FirstName, dbPerson.LastName, dbPerson.City.Name);
        return TypedResults.Ok(person);
    }

    private static async Task<IResult> InsertAsync(SavePersonRequest request, ApplicationDbContext dbContext, LinkGenerator linkGenerator)
    {
        var dbPerson = new DataAccessLayer.Entities.Person
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            CityId = request.CityId
        };

        dbContext.People.Add(dbPerson);
        await dbContext.SaveChangesAsync();

        var url = linkGenerator.GetPathByName("GetPerson", new { id = dbPerson.Id });
        return TypedResults.Created(url);
    }

    private static async Task<IResult> UpdateAsync(Guid id, SavePersonRequest request, ApplicationDbContext dbContext)
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

        return TypedResults.NoContent();
    }

    private static async Task<IResult> DeleteAsync(Guid id, ApplicationDbContext dbContext)
    {
        var rowDeleted = await dbContext.People.Where(p => p.Id == id).ExecuteDeleteAsync();
        if (rowDeleted == 0)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.NoContent();
    }
}

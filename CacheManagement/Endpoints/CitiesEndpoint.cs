using CacheManagement.DataAccessLayer;
using CacheManagement.Models;
using CacheManagement.Models.Requests;
using Microsoft.EntityFrameworkCore;

namespace CacheManagement.Endpoints;

public class CitiesEndpoint : IEndpointRouteHandlerBuilder
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var citiesEnpoint = endpoints.MapGroup("/api/cities");

        citiesEnpoint.MapGet(string.Empty, GetListAsync)
            .Produces<IEnumerable<City>>()
            .WithOpenApi();

        citiesEnpoint.MapGet("{id:guid}", GetAsync)
            .Produces<City>()
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi()
            .WithName("GetCity");

        citiesEnpoint.MapPost(string.Empty, InsertAsync)
            .Produces(StatusCodes.Status201Created)
            .WithOpenApi();

        citiesEnpoint.MapPut("{id:guid}", UpdateAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();

        citiesEnpoint.MapDelete("{id:guid}", DeleteAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();
    }
    private static async Task<IResult> GetListAsync(ApplicationDbContext dbContext)
    {
        var cities = await dbContext.Cities.AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new City(c.Id, c.Name))
            .ToListAsync();

        return TypedResults.Ok(cities);
    }

    private static async Task<IResult> GetAsync(Guid id, ApplicationDbContext dbContext)
    {
        var dbCity = await dbContext.Cities.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

        if (dbCity is null)
        {
            return TypedResults.NotFound();
        }

        var person = new City(dbCity.Id, dbCity.Name);
        return TypedResults.Ok(person);
    }

    private static async Task<IResult> InsertAsync(SaveCityRequest request, ApplicationDbContext dbContext, LinkGenerator linkGenerator)
    {
        var dbCity = new DataAccessLayer.Entities.City
        {
            Name = request.Name
        };

        dbContext.Cities.Add(dbCity);
        await dbContext.SaveChangesAsync();

        var url = linkGenerator.GetPathByName("GetCity", new { id = dbCity.Id });
        return TypedResults.Created(url);
    }

    private static async Task<IResult> UpdateAsync(Guid id, SaveCityRequest request, ApplicationDbContext dbContext)
    {
        var dbCity = await dbContext.Cities.FirstOrDefaultAsync(p => p.Id == id);
        if (dbCity is null)
        {
            return TypedResults.NotFound();
        }

        dbCity.Name = request.Name;

        await dbContext.SaveChangesAsync();

        return TypedResults.NoContent();
    }

    private static async Task<IResult> DeleteAsync(Guid id, ApplicationDbContext dbContext)
    {
        var rowDeleted = await dbContext.Cities.Where(c => c.Id == id).ExecuteDeleteAsync();
        if (rowDeleted == 0)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.NoContent();
    }
}

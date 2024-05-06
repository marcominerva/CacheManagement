namespace CacheManagement.Models.Requests;

public record class SavePersonRequest(string FirstName, string LastName, Guid CityId);
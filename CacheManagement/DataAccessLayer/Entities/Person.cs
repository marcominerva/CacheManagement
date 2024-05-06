namespace CacheManagement.DataAccessLayer.Entities;

public class Person
{
    public Guid Id { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public Guid CityId { get; set; }

    public virtual City City { get; set; }
}

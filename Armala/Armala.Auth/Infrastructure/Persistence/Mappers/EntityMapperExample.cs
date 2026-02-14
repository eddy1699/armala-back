namespace Armala.Armala.Auth.Infrastructure.Persistence.Mappers;

/// <summary>
/// Mapper entre entidades de dominio y entidades de EF Core
/// Ejemplo: Adaptar segun tus entidades reales
/// </summary>
public static class EntityMapperExample
{
    // Ejemplo: Mapear de EF Entity a Domain Entity
    // public static User ToDomain(this UserEntity entity)
    // {
    //     return new User(
    //         entity.Id,
    //         Email.Create(entity.Email),
    //         PhoneNumber.Create(entity.Phone),
    //         entity.CreatedAt
    //     );
    // }
    
    // Ejemplo: Mapear de Domain Entity a EF Entity
    // public static UserEntity ToEntity(this User domain)
    // {
    //     return new UserEntity
    //     {
    //         Id = domain.Id,
    //         Email = domain.Email.Value,
    //         Phone = domain.Phone.Value,
    //         CreatedAt = domain.CreatedAt
    //     };
    // }
}

using System.Reflection;

namespace IELTSLms.SharedKernel.Domain;

/// <summary>
/// Base class for smart enumerations: type-safe, with identity by <see cref="Id"/>.
/// </summary>
public abstract class Enumeration : IEquatable<Enumeration>
{
    protected Enumeration(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public int Id { get; }
    public string Name { get; }

    public static IEnumerable<T> GetAll<T>() where T : Enumeration =>
        typeof(T)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(f => f.FieldType == typeof(T))
            .Select(f => (T)f.GetValue(null)!)
            .OrderBy(e => e.Id);

    public static T FromId<T>(int id) where T : Enumeration =>
        GetAll<T>().FirstOrDefault(e => e.Id == id)
        ?? throw new InvalidOperationException($"No {typeof(T).Name} with id {id}.");

    public static T FromName<T>(string name) where T : Enumeration =>
        GetAll<T>().FirstOrDefault(e =>
            string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase))
        ?? throw new InvalidOperationException($"No {typeof(T).Name} with name '{name}'.");

    public bool Equals(Enumeration? other) =>
        other is not null && GetType() == other.GetType() && Id == other.Id;

    public override bool Equals(object? obj) => obj is Enumeration other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public override string ToString() => Name;
}

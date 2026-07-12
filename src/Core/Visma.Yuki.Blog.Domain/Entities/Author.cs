using Visma.Yuki.Blog.Domain.ValueObjects;

namespace Visma.Yuki.Blog.Domain.Entities;

public class Author
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Surname { get; private set; }

    public UniqueNameIdentifier UniqueNameIdentifier { get; private set; }

    public Author(Guid id, string name, string surname)
    {
        Id = id;
        Name = name;
        Surname = surname;
        UniqueNameIdentifier = UniqueNameIdentifier.Create(name, surname);
    }

    public Author(Guid id, string name, string surname, string uniqueNameIdentifier)
    {
        Id = id;
        Name = name;
        Surname = surname;
        
        UniqueNameIdentifier = UniqueNameIdentifier.FromString(uniqueNameIdentifier); 
    }
}
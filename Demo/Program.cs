using System;

var person = new TestRecord { Name = "Robin", Team = "Blue" };
Console.WriteLine(person.NameTeam);
var person2 = person with { Team = "Yellow" };
Console.WriteLine(person2.NameTeam);

public record TestRecord
{
    public string Name { get; init; }
    public string Team { get; init; }

    private string _nameTeam = null;
    public string NameTeam
    {
        get
        {
            if (_nameTeam == null) _nameTeam = $"{Name} (Team {Team})";
            return _nameTeam;
        }
    }
}
namespace StudyMatch.Models;

public record Course(string Code, string Name);
public record Student(int Id, string Name);
public record TimeSlot(DayOfWeek Day, TimeOnly Start, TimeOnly End)
{
    public bool Covers(TimeSlot other) => Day == other.Day && Start <= other.Start && End >= other.End;
    public override string ToString() => $"{Day switch { DayOfWeek.Monday => "Lundi", DayOfWeek.Tuesday => "Mardi", DayOfWeek.Wednesday => "Mercredi", DayOfWeek.Thursday => "Jeudi", DayOfWeek.Friday => "Vendredi", _ => Day.ToString() }} {Start:HH:mm}-{End:HH:mm}";
}
public record Availability(int StudentId, string CourseCode, TimeSlot Slot);
public record Room(string Name, IReadOnlyList<TimeSlot> AvailableSlots);
public class StudyGroup
{
    public int Id { get; set; }
    public required Course Course { get; init; }
    public required TimeSlot Slot { get; init; }
    public List<Student> Members { get; } = new();
    public string? RoomName { get; set; }
}
public class MatchRequest
{
    public string StudentName { get; set; } = "";
    public string CourseCode { get; set; } = "";
    public List<int> SlotIds { get; set; } = new();
}
public record MatchResult(StudyGroup? Group, string Message);

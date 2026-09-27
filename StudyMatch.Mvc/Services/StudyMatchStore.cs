using StudyMatch.Models;
namespace StudyMatch.Mvc.Services;
public class StudyMatchStore
{
    public IReadOnlyList<Course> Courses { get; } = new[] { new Course("420-TWS", "Technologie Web Serveur"), new Course("420-ALG", "Algorithmique"), new Course("420-BD", "Bases de données") };
    public IReadOnlyList<TimeSlot> Slots { get; } = new[] {
        new TimeSlot(DayOfWeek.Monday, new(9,0), new(11,0)), new TimeSlot(DayOfWeek.Monday, new(13,0), new(15,0)),
        new TimeSlot(DayOfWeek.Tuesday, new(9,0), new(11,0)), new TimeSlot(DayOfWeek.Wednesday, new(13,0), new(15,0)),
        new TimeSlot(DayOfWeek.Thursday, new(9,0), new(11,0)), new TimeSlot(DayOfWeek.Friday, new(13,0), new(15,0)) };
    public IReadOnlyList<Room> Rooms { get; }
    public List<Availability> Availabilities { get; } = new();
    private readonly List<StudyGroup> groups = new();
    private int nextStudentId = 5;
    private int nextGroupId = 3;
    private readonly object gate = new();
    public StudyMatchStore()
    {
        Rooms = new[] { new Room("B-201", new[] { Slots[0], Slots[1], Slots[2], Slots[3] }), new Room("B-202", new[] { Slots[0], Slots[2], Slots[4], Slots[5] }) };
        var alice = new Student(1,"Alice" ); var bob = new Student(2,"Bob"); var chloe = new Student(3,"Chloé"); var david = new Student(4,"David");
        Availabilities.AddRange(new[] { new Availability(1,"420-TWS",Slots[0]), new Availability(1,"420-TWS",Slots[1]), new Availability(2,"420-TWS",Slots[0]), new Availability(3,"420-ALG",Slots[2]), new Availability(4,"420-BD",Slots[4]) });
        var first = new StudyGroup { Id = 1, Course = Courses[0], Slot = Slots[0], RoomName = "B-201" }; first.Members.AddRange(new[] { alice, bob }); groups.Add(first);
        var second = new StudyGroup { Id = 2, Course = Courses[1], Slot = Slots[2] }; second.Members.Add(chloe); groups.Add(second);
        // David has availability but has not yet joined a group.
        Students.AddRange(new[] { alice, bob, chloe, david });
    }
    public List<Student> Students { get; } = new();
    public IReadOnlyList<StudyGroup> GetGroups() { lock (gate) return groups.Select(g => new StudyGroup { Id = g.Id, Course = g.Course, Slot = g.Slot, RoomName = g.RoomName }.WithMembers(g.Members)).ToList(); }
    public MatchResult Submit(MatchRequest request)
    {
        lock (gate)
        {
            var name = request.StudentName?.Trim();
            if (string.IsNullOrWhiteSpace(name) || name.Length > 80) return new(null,"Nom invalide.");
            var course = Courses.FirstOrDefault(c => c.Code == request.CourseCode);
            if (course is null || request.SlotIds is null || request.SlotIds.Count == 0 || request.SlotIds.Any(i => i < 0 || i >= Slots.Count)) return new(null,"Cours ou disponibilités invalides.");
            var student = new Student(nextStudentId++, name);
            Students.Add(student);
            var ids = request.SlotIds.Distinct().ToList();
            foreach (var i in ids) Availabilities.Add(new(student.Id, course.Code, Slots[i]));
            // Prefer a group for the same course and slot; otherwise create one at the slot
            // with the largest number of available classmates who are not already in another group.
            var existing = groups.Where(g => g.Course.Code == course.Code && ids.Any(i => Slots[i] == g.Slot))
                .OrderByDescending(g => g.Members.Count).ThenBy(g => g.Id).FirstOrDefault();
            if (existing is not null)
            {
                existing.Members.Add(student);
                AssignRoom(existing);
                return new(existing, existing.RoomName is null ? "Groupe rejoint. Aucun local disponible pour cet horaire pour l'instant." : $"Groupe rejoint. Local : {existing.RoomName}.");
            }
            var slot = ids.Select(i => Slots[i]).OrderByDescending(s => Availabilities.Count(a => a.CourseCode == course.Code && a.Slot == s && !groups.Any(g => g.Members.Any(m => m.Id == a.StudentId)))).ThenBy(s => ids.IndexOf(Slots.ToList().IndexOf(s))).First();
            var group = new StudyGroup { Id = nextGroupId++, Course = course, Slot = slot };
            group.Members.Add(student);
            var classmates = Availabilities.Where(a => a.CourseCode == course.Code && a.Slot == slot && a.StudentId != student.Id)
                .Select(a => Students.First(s => s.Id == a.StudentId)).DistinctBy(s => s.Id)
                .Where(s => !groups.Any(g => g.Members.Any(m => m.Id == s.Id))).ToList();
            group.Members.AddRange(classmates);
            AssignRoom(group);
            groups.Add(group);
            var message = group.Members.Count < 2 ? "Demande enregistrée. En attente d'un autre étudiant compatible." : group.RoomName is null ? "Groupe formé. Aucun local disponible pour cet horaire pour l'instant." : $"Groupe formé. Local : {group.RoomName}.";
            return new(group,message);
        }
    }
    private void AssignRoom(StudyGroup group)
    {
        if (group.Members.Count < 2 || group.RoomName is not null) return;
        group.RoomName = Rooms.FirstOrDefault(r => r.AvailableSlots.Any(s => s.Covers(group.Slot)) && !groups.Any(g => g.Id != group.Id && g.Slot == group.Slot && g.RoomName == r.Name))?.Name;
    }
}
internal static class GroupCopy { public static StudyGroup WithMembers(this StudyGroup group, IEnumerable<Student> members) { group.Members.AddRange(members); return group; } }

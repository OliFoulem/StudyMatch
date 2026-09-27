using Microsoft.AspNetCore.Mvc;
using StudyMatch.Mvc.Services;
namespace StudyMatch.Mvc.Controllers;
[Route("groups")]
public class GroupsController(StudyMatchStore store) : Controller
{
    [HttpGet("")] [HttpGet("/")] public IActionResult Index() => View(store.GetGroups());
}

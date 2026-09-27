using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using StudyMatch.Models;
using StudyMatch.Mvc.Services;
namespace StudyMatch.Mvc.Controllers;
[Route("match")]
public class MatchController(StudyMatchStore store) : Controller
{
    [HttpGet("")] public IActionResult Index() => View(store);
    [HttpGet("submit")] public IActionResult Submit(string payload) => View("Submit", payload);
    [HttpPost("submit")] [ValidateAntiForgeryToken]
    public IActionResult SubmitPost(string payload)
    {
        try
        {
            if (payload.Length > 4096) return BadRequest("Demande trop longue.");
            var request = JsonSerializer.Deserialize<MatchRequest>(Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(payload)));
            if (request is null) return BadRequest("Demande invalide.");
            return View("Result", store.Submit(request));
        }
        catch (Exception ex) when (ex is FormatException or JsonException or ArgumentException) { return BadRequest("Demande invalide."); }
    }
}

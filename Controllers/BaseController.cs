using Microsoft.AspNetCore.Mvc;

namespace StudyHub.Controllers
{
    public class BaseController : Controller
    {
        // =========================
        // USER SESSION HELPER
        // =========================

        protected bool TryGetUserId(out int userId)
        {
            int? sessionUserId = HttpContext.Session.GetInt32("UserId");

            if (sessionUserId == null)
            {
                userId = 0;
                return false;
            }

            userId = sessionUserId.Value;
            return true;
        }

        // =========================
        // LOGIN CHECK
        // =========================

        protected bool IsUserLoggedIn()
        {
            return HttpContext.Session.GetInt32("UserId") != null;
        }

        // =========================
        // REDIRECT HELPER
        // =========================

        protected IActionResult RedirectToLogin()
        {
            return RedirectToAction("Login", "Auth");
        }
    }
}
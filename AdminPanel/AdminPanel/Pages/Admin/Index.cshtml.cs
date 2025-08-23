using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdminPanel.Pages.Admin
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public int Minutes { get; set; }

        public string? Message { get; set; }

        public void OnGet()
        {
        }

        public void OnPostShutdown()
        {
            Message = "Shutdown requested (test).";
        }

        public void OnPostSchedule()
        {
            Message = $"Scheduled shutdown in {Minutes} minute(s) (test).";
        }
    }
}
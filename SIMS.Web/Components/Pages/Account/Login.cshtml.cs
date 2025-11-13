using Azure.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using System.ComponentModel.DataAnnotations;

namespace SIMS.Web.Components.Pages.Account
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public Credential Credential { get; set; }

        public void OnGet()
        {
            
        }

        public void OnPost() 
        { 

        }
    }

    public class Credential
    {
        [Required]
        [Display(Name = "User Name")]
      public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}

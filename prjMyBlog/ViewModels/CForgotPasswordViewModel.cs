using System.ComponentModel.DataAnnotations;

namespace prjMyBlog.ViewModels
{
    public class CForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}

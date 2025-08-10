using System.ComponentModel.DataAnnotations;

namespace prjMyBlog.ViewModels
{
    public class CResetPasswordViewModel
    {
        [Required]
        public string Token { get; set; }
        [Required(ErrorMessage ="請輸入新密碼")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "請再次輸入新密碼")]
        [Compare("NewPassword", ErrorMessage = "兩次輸入的密碼不一致")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}

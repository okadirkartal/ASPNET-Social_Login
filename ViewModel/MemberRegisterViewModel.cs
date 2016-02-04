using System.ComponentModel.DataAnnotations;

namespace ViewModel
{
    public class MemberRegisterViewModel
    {

        [RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$", ErrorMessageResourceName = "pleaseEntryValidEmailAddress")]
        public string Email { get; set; }

        public string Name { get; set; }

        public string SurName { get; set; }

        public int SourceId { get; set; }

        public string SourceIdValue { get; set; } 

    }
}

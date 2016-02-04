using Entity;
using System.Web.Mvc;
using ViewModel;

namespace Web.Controllers
{
    public class BaseController : Controller
    {
        protected void DoLogin(Member member, bool chkIsRemember)
        {

            Session[SessionKeys.MemberInfo] = new SessionUser()
            {
                Id = member.Id,
                NickName = member.NickName,
                Name = member.Name,
                SurName = member.Surname,
                Email = member.Email
            }; 
        }

        protected SessionUser GetSessionUser
        {
            get
            {
                return ((SessionUser)Session[SessionKeys.MemberInfo]);
            }
        }
    }
}
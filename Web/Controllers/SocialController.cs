using Business.Interface;
using Core.Constraints;
using Core.Enums.Types;
using DotNetOpenAuth.AspNet;
using Entity;
using Facebook;
using Microsoft.Web.WebPages.OAuth;
using System;
using System.Web.Mvc;
using TweetSharp;
using ViewModel;
using Web.App_Start;

namespace Web.Controllers
{
    public class SocialController : BaseController
    {
        private readonly IMemberBusiness _memberBusiness;  
        public SocialController(IMemberBusiness memberBusiness)
        {
            _memberBusiness = memberBusiness; 
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SocialLoginPage()
        {
            return View(RouteKeys.AfterSocialLoginPage);
        }


        #region  Social Login Operations

        private ActionResult GetMemberProfileInternalView()
        {
            return new RedirectResult(string.Format("/{0}/{1}?login=true",
                        RouteKeys.MemberController, "Index"), false);
        }


        public ActionResult SigninWithFacebook(FormCollection formCollection)
        {
            var referrer = Request.UrlReferrer;

            var resultMember = new Member();

            string accessToken = !string.IsNullOrEmpty(Request["accessToken"])
                ? Request["accessToken"]
                : string.Empty;

            string returnUrl = !string.IsNullOrEmpty(Request["ReturnUrl"])
                ? Request["ReturnUrl"]
                : string.Empty;

            var client = new FacebookClient(accessToken);
            dynamic result = client.Get("me");

            if (!string.IsNullOrEmpty(result.id))
            {

                Member  member = _memberBusiness.GetMemberByMemberSourceIdValue((int)MemberSourceTypes.Facebook, result.id);

                if (member!=null)
                {
                    member = _memberBusiness.GetMemberByEmail(result.email);
                    if (member!=null)
                    {
                        MemberRegisterViewModel model = new MemberRegisterViewModel()
                        {
                            Email = result.email,
                            Name = result.first_name,
                            SurName = result.last_name,
                            SourceId = (int)MemberSourceTypes.Facebook,
                            SourceIdValue = result.id 
                        }; 
                    }
                    else
                    {
                        member.SourceId = (int)MemberSourceTypes.Facebook;
                        member.SourceIdValue = result.id.ToString();
                        _memberBusiness.UpdateMember(member);
                    }
                }
                if (member!=null)
                {
                    return RedirectMemberProfile(member);
                }
            }

            return View();
        }

        #region Twitter Authorize

        public ActionResult TwitterAuthorize()
        {
            string _consumerKey = "Consumer Key";
            string _consumerSecret ="Secret Key";
            TwitterService service = new TwitterService(_consumerKey, _consumerSecret);
            var url = "CallBack Url";
            // GetTwitterToken();
            // This is the registered callback URL
            OAuthRequestToken requestToken = service.GetRequestToken(url);

            // Step 2 - Redirect to the OAuth Authorization URL
            Uri uri = service.GetAuthorizationUri(requestToken, url);
            return new RedirectResult(uri.ToString(), false /*permanent*/);

        }
        public ActionResult TwitterAuthorizeCallback(string oauth_token, string oauth_verifier)
        {
            var requestToken = new OAuthRequestToken { Token = oauth_token };
            string _consumerKey = "Consumer Key";
            string _consumerSecret ="Consumer Secret";

            //// Step 3 - Exchange the Request Token for an Access Token
            TwitterService service = new TwitterService(_consumerKey, _consumerSecret);
            ////OAuthAccessToken accessToken = service.GetAccessToken(requestToken, oauth_verifier);
            OAuthAccessToken accessToken = service.GetAccessToken(requestToken, oauth_verifier);
            //// Step 4 - User authenticates using the Access Token
            service.AuthenticateWith(accessToken.Token, accessToken.TokenSecret);
            TwitterUser user = service.VerifyCredentials(new VerifyCredentialsOptions() { IncludeEntities = true });
            if (user != null)
            {
                var member = _memberBusiness.GetMemberByMemberSourceIdValue((int)MemberSourceTypes.Twitter, user.Id.ToString());
                if (member!=null)
                {
                    //dynamic item = Json(user);

                    MemberRegisterViewModel model = new MemberRegisterViewModel()
                    {
                        //Email = result.ExtraData["email"],
                        Name = user.Name,
                        SourceId = (int)MemberSourceTypes.Twitter,
                        SourceIdValue = user.Id.ToString() 
                    };

                    return RedirectMemberProfile(member);

                }
                else
                {
                    member.SourceId = (int)MemberSourceTypes.Twitter;
                    member.SourceIdValue = user.Id.ToString();
                    _memberBusiness.UpdateMember(member);
                }
                if (member!=null)
                {
                    return RedirectMemberProfile(member);

                }
            }
            return RedirectToAction("ExternalLoginFailure");
        }
        #endregion

        #region Google Authorize
        public ActionResult GoogleAuthorizeLogin(string returnUrl)
        {
            return new ExternalLoginResult("GooglePlus", Url.Action("GoogleAuthorizeCallback", new { ReturnUrl = returnUrl }));
        }

        [AllowAnonymous]
        public ActionResult GoogleAuthorizeCallback(string returnUrl)
        {
            // Rewrite request before it gets passed on to the OAuth Web Security classes
            GooglePlusClient.RewriteRequest();

            AuthenticationResult result = OAuthWebSecurity.VerifyAuthentication(Url.Action("GoogleAuthorizeCallback", new { ReturnUrl = returnUrl }));
            if (!result.IsSuccessful)
            {
                return RedirectToAction("ExternalLoginFailure");
            }
            else
            {
                // User is new, ask for their desired membership name
                string loginData = OAuthWebSecurity.SerializeProviderUserId(result.Provider, result.ProviderUserId);
                ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(result.Provider).DisplayName;
                ViewBag.ReturnUrl = returnUrl;
                AuthenticationResult newResult = result;

                var member = _memberBusiness.GetMemberByMemberSourceIdValue((int)MemberSourceTypes.GooglePlus, result.ExtraData["id"].ToString());
                if (member!=null)
                {
                    member = _memberBusiness.GetMemberByEmail(result.ExtraData["email"]);

                    if (member!=null)
                    {
                        MemberRegisterViewModel model = new MemberRegisterViewModel()
                        {
                            Email = result.ExtraData["email"],
                            Name = result.ExtraData["given_name"],
                            SurName = result.ExtraData["family_name"],
                            //   NickName = result.ExtraData["family_name"],
                            SourceId = (int)MemberSourceTypes.GooglePlus,
                            SourceIdValue = result.ExtraData["id"]
                        };

                        return RedirectMemberProfile(member);
                    }
                    else
                    {
                        member.SourceId = (int)MemberSourceTypes.GooglePlus;
                        member.SourceIdValue = result.ExtraData["id"].ToString();
                        _memberBusiness.UpdateMember(member);
                    }

                }
                if (member!=null)
                {
                    return RedirectMemberProfile(member);
                }
            }
            return View();
        }

        private ActionResult RedirectMemberProfile(Member member)
        {
                 DoLogin(member, false);
                return GetMemberProfileInternalView();
        }

        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        #endregion

        #endregion
    }

    internal class ExternalLoginResult : ActionResult
    {
        public ExternalLoginResult(string provider, string returnUrl)
        {
            Provider = provider;
            ReturnUrl = returnUrl;
        }

        public string Provider { get; private set; }
        public string ReturnUrl { get; private set; }

        public override void ExecuteResult(ControllerContext context)
        {
            OAuthWebSecurity.RequestAuthentication(Provider, ReturnUrl);
        }
    }
}
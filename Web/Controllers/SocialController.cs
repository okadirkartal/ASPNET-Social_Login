using Business.Interface;
using Core.Enums.Types;
using DotNetOpenAuth.AspNet;
using Entity;
using Facebook;
using System;
using System.Web.Mvc;
using Core.Constants;
using TweetSharp;
using Web.App_Start;
using static Microsoft.Web.WebPages.OAuth.OAuthWebSecurity;

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


            string accessToken = !string.IsNullOrEmpty(Request["accessToken"])
                ? Request["accessToken"]
                : string.Empty;

            var client = new FacebookClient(accessToken);
            dynamic result = client.Get("me");

            if (!string.IsNullOrEmpty(result.id))
            {

                Member member = _memberBusiness.GetMemberByMemberSourceIdValue((int)MemberSourceTypes.Facebook, result.id);

                if (member != null)
                {
                    member = _memberBusiness.GetMemberByEmail(result.email);
                    if (member != null)
                    {

                        member.Email = result.email;
                        member.Name = result.first_name;
                        member.Surname = result.last_name;
                        member.SourceId = (int)MemberSourceTypes.Facebook;
                        member.SourceIdValue = result.id;
                    }

                }
                if (member != null)
                {
                    _memberBusiness.UpdateMember(member);
                    return RedirectMemberProfile(member);
                }
            }

            return View();
        }

        #region Twitter Authorize

        public ActionResult TwitterAuthorize()
        {
            string _consumerKey = "Consumer Key";
            string _consumerSecret = "Secret Key";
            TwitterService service = new TwitterService(_consumerKey, _consumerSecret);
            var url = "CallBack Url";
            // GetTwitterToken();
            // This is the registered callback URL
            OAuthRequestToken requestToken = service.GetRequestToken(url);

            // Step 2 - Redirect to the OAuth Authorization URL
            Uri uri = service.GetAuthorizationUri(requestToken, url);
            return new RedirectResult(uri.ToString(), false /*permanent*/);

        }
        public ActionResult TwitterAuthorizeCallback(string oauthToken, string oauthVerifier)
        {
            if (oauthToken == null) throw new ArgumentNullException(nameof(oauthToken));
            var requestToken = new OAuthRequestToken { Token = oauthToken };
            string _consumerKey = "Consumer Key";
            string _consumerSecret = "Consumer Secret";

            //// Step 3 - Exchange the Request Token for an Access Token
            TwitterService service = new TwitterService(_consumerKey, _consumerSecret);
            ////OAuthAccessToken accessToken = service.GetAccessToken(requestToken, oauth_verifier);
            OAuthAccessToken accessToken = service.GetAccessToken(requestToken, oauthVerifier);
            //// Step 4 - User authenticates using the Access Token
            service.AuthenticateWith(accessToken.Token, accessToken.TokenSecret);
            TwitterUser user = service.VerifyCredentials(new VerifyCredentialsOptions() { IncludeEntities = true });
            if (user != null)
            {
                var member = _memberBusiness.GetMemberByMemberSourceIdValue((int)MemberSourceTypes.Twitter, user.Id.ToString());
                if (member != null)
                {
                    //dynamic item = Json(user);


                    //Email = result.ExtraData["email"],
                    member.Name = user.Name;
                    member.SourceId = (int)MemberSourceTypes.Twitter;
                    member.SourceIdValue = user.Id.ToString();
                    _memberBusiness.UpdateMember(member);
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

            AuthenticationResult result = VerifyAuthentication(Url.Action("GoogleAuthorizeCallback", new { ReturnUrl = returnUrl }));
            if (!result.IsSuccessful)
            {
                return RedirectToAction("ExternalLoginFailure");
            }
            else
            {
                // User is new, ask for their desired membership name
                SerializeProviderUserId(result.Provider, result.ProviderUserId);
                ViewBag.ProviderDisplayName = GetOAuthClientData(result.Provider).DisplayName;
                ViewBag.ProviderDisplayName = GetOAuthClientData(result.Provider).DisplayName;
                ViewBag.ReturnUrl = returnUrl;

                var member = _memberBusiness.GetMemberByMemberSourceIdValue((int)MemberSourceTypes.GooglePlus, result.ExtraData["id"]);
                if (member != null)
                {
                    member = _memberBusiness.GetMemberByEmail(result.ExtraData["email"]);

                    if (member != null)
                    {

                        member.Email = result.ExtraData["email"];
                            member.Name = result.ExtraData["given_name"];
                        member.Surname = result.ExtraData["family_name"];
                        //   NickName = result.ExtraData["family_name"],
                        member.SourceId = (int)MemberSourceTypes.GooglePlus;
                        member.SourceIdValue = result.ExtraData["id"];
                        _memberBusiness.UpdateMember(member);
                        return RedirectMemberProfile(member);
                    }
                    

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
            RequestAuthentication(Provider, ReturnUrl);
        }
    }
}
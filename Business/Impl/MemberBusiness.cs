using System;
using Business.Interface;
using Entity;

namespace Business.Impl
{
    public class MemberBusiness : IMemberBusiness
    {
        Member IMemberBusiness.GetMemberByEmail(string email)
        {
            throw new NotImplementedException();
        }

        public Member GetMemberBySourceIdValue()
        {
            throw new NotImplementedException();
        }

        Member IMemberBusiness.GetMemberByMemberSourceIdValue(short sourceId, string sourceIdValue)
        {
            throw new NotImplementedException();
        }

        Member IMemberBusiness.GetMemberBySourceIdValue(string sourceIdValue)
        {
            throw new NotImplementedException();
        }

        Member IMemberBusiness.UpdateMember(Member member)
        {
            throw new NotImplementedException();
        }
    }
}

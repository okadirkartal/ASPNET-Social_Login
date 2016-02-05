using Entity;

namespace Business.Interface
{
    public interface IMemberBusiness

    {
        Member GetMemberByEmail(string email);
        Member GetMemberBySourceIdValue();
        Member GetMemberBySourceIdValue(string sourceIdValue);
        Member GetMemberByMemberSourceIdValue(short sourceId, string sourceIdValue);
        Member UpdateMember(Member member);
    }
}

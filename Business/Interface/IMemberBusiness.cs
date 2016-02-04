using Entity;

namespace Business.Interface
{
    public interface IMemberBusiness

    {
        Member GetMemberByEmail(string email);
        Member GetMemberBySourceIdValue(string SourceIdValue);
        Member GetMemberByMemberSourceIdValue(short sourceId, string sourceIdValue);
        Member UpdateMember(Member member);
    }
}

namespace Entity
{
    public class Member
    {
        public Member()
        {
            SourceId = 1;
        }
        public int Id { get; set; }
        public string Email { get; set; }
         public string NickName { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public short SourceId { get; set; }
        public string SourceIdValue { get; set; } 
    }
}

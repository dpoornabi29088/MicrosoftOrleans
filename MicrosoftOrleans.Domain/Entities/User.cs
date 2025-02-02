namespace MicrosoftOrleans.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public List<Address> Addresses { get; set; } = new List<Address>();
    }
}

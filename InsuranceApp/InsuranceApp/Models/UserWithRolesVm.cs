namespace InsuranceApp.Models
{
    internal class UserWithRolesVm
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public IList<string> Roles { get; set; }
    }
}
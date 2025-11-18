namespace MVC.Data
{
    public class EFRepositoryAPIInMemory : EFRepositoryAPI<ApplicationDbContextInMemory>
    {
        public EFRepositoryAPIInMemory(ApplicationDbContextInMemory context) : base(context) { }

    }
}

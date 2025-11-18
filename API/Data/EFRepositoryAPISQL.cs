namespace MVC.Data
{
    public class EFRepositoryAPISQL : EFRepositoryAPI<ApplicationDbContextSQL>
    {
        public EFRepositoryAPISQL(ApplicationDbContextSQL context) : base(context) { }

    }
}

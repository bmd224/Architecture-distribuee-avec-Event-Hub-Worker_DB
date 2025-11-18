using Microsoft.EntityFrameworkCore;
using MVC.Models;


namespace MVC.Data
{
    public class EFRepositoryAPINoSQL : EFRepositoryAPI<ApplicationDbContextNoSQL>
    {
        public EFRepositoryAPINoSQL(ApplicationDbContextNoSQL context) : base(context) { }

    }
}

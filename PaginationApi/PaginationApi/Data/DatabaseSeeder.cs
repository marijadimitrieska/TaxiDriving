//using Microsoft.EntityFrameworkCore;

//namespace PaginationApi.Data
//{
//    public class DatabaseSeeder
//    {
//        public static async Task SeedAsync(ApplicationDbContext context)
//        {
//            var needUpdate = await context.TaxiDrivings
//                .Where(t => t.updatedDate == null || t.createdDate == null)
//                .ToListAsync();

//            if (needUpdate.Any())
//            {
//                foreach (var r in needUpdate)
//                {
//                    if (r.createdDate == null)
//                    {
//                        r.createdDate = DateTime.Now;
//                    }
//                    if (r.updatedDate == null)
//                    {
//                        r.updatedDate = DateTime.Now;
//                    }
//                    await context.SaveChangesAsync();
//                }
//            }
//        }
//    }
//}

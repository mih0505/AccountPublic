using Account.DAL.Contexts;
using Account.DAL.Entities;
using Account.DAL.Interfaces;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Account.DAL.Repositories
{
    public class EFRPDItemRepository : EFBaseRepository<RPDItem>, IEFRPDItemRepository
    {
        public EFRPDItemRepository(AccountContext db) 
            : base(db) 
        { }

        public async Task<IEnumerable<RPDItem>> GetEducationStandartRPDItemsAsync(int educationStandartId, int documentTypeId)
        {
            return await _context.RPDItems.Where(RPDItem => RPDItem.EducationStandardId.Equals(educationStandartId) && RPDItem.DocumentTypeId.Equals(documentTypeId)).ToListAsync();
        }
    }
}
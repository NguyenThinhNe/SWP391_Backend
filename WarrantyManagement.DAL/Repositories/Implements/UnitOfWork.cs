using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantyManagement.DAL.Data.Context;
using WarrantyManagement.DAL.Repositories.Interfaces;

namespace WarrantyManagement.DAL.Repositories.Implements
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly WarrantyDbContext _context;
        public IUserRepository Users { get; }
        //public IServiceCenterRepository ServiceCenters { get; }

        public UnitOfWork(WarrantyDbContext context,
                          IUserRepository userRepo)
                          //IServiceCenterRepository serviceCenterRepo
        {
            _context = context;
            Users = userRepo;
            //ServiceCenters = serviceCenterRepo;
        }

        public async  Task<int> SaveChangesAsync()
        => await _context.SaveChangesAsync();
    }
}

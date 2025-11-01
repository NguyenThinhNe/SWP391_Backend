using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantyManagement.BLL.Services.Interfaces;
using WarrantyManagement.DAL.Data.Context;
using WarrantyManagement.DAL.Data.Entities;
using WarrantyManagement.DAL.Data.Response;
using WarrantyManagement.DAL.Repositories.Interfaces;

namespace WarrantyManagement.BLL.Services.Implements
{
    public class VehicleService : IVehicleService
    {
        private readonly IUnitOfWork<WarrantyDbContext> _unitOfWork;
        private readonly IMapper _mapper;

        public VehicleService(IUnitOfWork<WarrantyDbContext> unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Lấy toàn bộ danh sách xe (kèm thông tin Customer)
        /// </summary>
        public async Task<ICollection<VehicleResponse>> GetAllAsync()
        {
            var repo = _unitOfWork.GetRepository<CustomerVehicle>();
            var vehicles = await repo.GetListAsync(include: q => q.Include(x => x.Customer));

            return _mapper.Map<ICollection<VehicleResponse>>(vehicles);
        }

        /// <summary>
        /// Lấy thông tin xe theo VIN
        /// </summary>
        public async Task<VehicleResponse> GetByVinAsync(string vin)
        {
            var repo = _unitOfWork.GetRepository<CustomerVehicle>();
            var vehicle = await repo.FirstOrDefaultAsync(
                predicate: x => x.VIN == vin,
                include: q => q.Include(x => x.Customer)
            );

            return _mapper.Map<VehicleResponse>(vehicle);
        }

        /// <summary>
        /// Lấy danh sách xe theo tên khách hàng (FirstName hoặc LastName)
        /// </summary>
        public async Task<ICollection<VehicleResponse>> GetByCustomerNameAsync(string customerName)
        {
            var repo = _unitOfWork.GetRepository<CustomerVehicle>();
            var vehicles = await repo.GetListAsync(
                predicate: x =>
                    x.Customer.FirstName.Contains(customerName) ||
                    x.Customer.LastName.Contains(customerName),
                include: q => q.Include(x => x.Customer)
            );

            return _mapper.Map<ICollection<VehicleResponse>>(vehicles);
        }

        /// <summary>
        /// Lấy danh sách xe theo ID của khách hàng
        /// </summary>
        public async Task<ICollection<VehicleResponse>> GetByCustomerIdAsync(Guid customerId)
        {
            var repo = _unitOfWork.GetRepository<CustomerVehicle>();
            var vehicles = await repo.GetListAsync(
                predicate: x => x.CustomerId == customerId,
                include: q => q.Include(x => x.Customer)
            );

            return _mapper.Map<ICollection<VehicleResponse>>(vehicles);
        }
    }
}

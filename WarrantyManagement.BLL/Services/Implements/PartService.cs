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
    public class PartService : IPartService
    {
        private readonly IUnitOfWork<WarrantyDbContext> _unitOfWork;
        private readonly IMapper _mapper;

        public PartService(IUnitOfWork<WarrantyDbContext> unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PartResponseDto>> GetAllPartsAsync()
        {
            var vehiclePartRepo = _unitOfWork.GetRepository<VehiclePart>();

            var parts = await vehiclePartRepo.GetListAsync(
                include: query => query
                    .Include(vp => vp.Part)
                    .Include(vp => vp.Vehicle)
            );

            return _mapper.Map<IEnumerable<PartResponseDto>>(parts);
        }

        public async Task<IEnumerable<PartResponseDto>> GetPartsByVINAsync(string vin)
        {
            var vehiclePartRepo = _unitOfWork.GetRepository<VehiclePart>();
           

            var parts = await vehiclePartRepo.GetListAsync(
                predicate: vp => vp.VIN == vin,
                include: query => query
                    .Include(vp => vp.Part)
                    .Include(vp => vp.Vehicle)
            );
            foreach (var vp in parts)
            {
                if (vp.Part == null)
                    Console.WriteLine($"Missing Part for VehiclePartId: {vp.VehiclePartId}, PartId: {vp.PartId}");
            }
            return _mapper.Map<IEnumerable<PartResponseDto>>(parts);
        }

        public async Task<PartResponseDto> GetPartByIdAsync(Guid partId)
        {
            var vehiclePartRepo = _unitOfWork.GetRepository<VehiclePart>();

            var part = await vehiclePartRepo.FirstOrDefaultAsync(
                predicate: vp => vp.PartId == partId,
                include: query => query
                    .Include(vp => vp.Part)
                    .Include(vp => vp.Vehicle)
            );

            return _mapper.Map<PartResponseDto>(part);
        }
    }
}

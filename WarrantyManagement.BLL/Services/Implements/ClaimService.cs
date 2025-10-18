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
using WarrantyManagement.DAL.Data.Enums;
using WarrantyManagement.DAL.Data.Request;
using WarrantyManagement.DAL.Data.Response;
using WarrantyManagement.DAL.Repositories.Interfaces;

namespace WarrantyManagement.BLL.Services.Implements
{
    public class ClaimService: IClaimService
    {
        private readonly IUnitOfWork<WarrantyDbContext> _unitOfWork;
        private readonly IMapper _mapper;

        public ClaimService(IUnitOfWork<WarrantyDbContext> unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // -------------------------------
        // 1️⃣ Tạo yêu cầu bảo hành
        // -------------------------------
        public async Task<ClaimResponse> CreateClaimAsync(ClaimRequest request, Guid userId)
        {
            var claimRepo = _unitOfWork.GetRepository<WarrantyClaim>();
            var vehicleRepo = _unitOfWork.GetRepository<CustomerVehicle>();
            var policyRepo = _unitOfWork.GetRepository<WarrantyPolicy>();

            // Kiểm tra xe có tồn tại không
            var vehicle = await vehicleRepo.FirstOrDefaultAsync(
                predicate: v => v.VIN == request.VIN,
                orderBy: null,
                include: null
            );
            if (vehicle == null)
                throw new Exception($"Không tìm thấy xe có VIN: {request.VIN}");

            // (Tuỳ chọn) Tìm policy hiện hành cho xe
            var policy = await policyRepo.FirstOrDefaultAsync(
                  predicate: p => p.warrantyClaims.Any(c => c.VIN == request.VIN),
                  orderBy: null,
                  include: null
            );
            // Map sang entity
            var claimEntity = _mapper.Map<WarrantyClaim>(request);
            claimEntity.UserId = userId;
            claimEntity.CustomerVehicle = vehicle;
            claimEntity.VIN = request.VIN;
            claimEntity.PolicyId = policy?.PolicyId ?? Guid.Empty;

            // Thực thi trong transaction
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await claimRepo.InsertAsync(claimEntity);
            });

            return _mapper.Map<ClaimResponse>(claimEntity);
        }

        // -------------------------------
        // 2️⃣ Lấy tất cả yêu cầu bảo hành
        // -------------------------------
        public async Task<IEnumerable<ClaimResponse>> GetAllClaimsAsync()
        {
            var claimRepo = _unitOfWork.GetRepository<WarrantyClaim>();
            var claims = await claimRepo.GetListAsync(
                include: q => q.Include(c => c.CustomerVehicle)
            );

            return _mapper.Map<IEnumerable<ClaimResponse>>(claims);
        }

        // -------------------------------
        // 3️⃣ Lấy chi tiết 1 yêu cầu
        // -------------------------------
        public async Task<ClaimResponse> GetClaimByIdAsync(Guid claimId)
        {
            var claimRepo = _unitOfWork.GetRepository<WarrantyClaim>();

            var claim = await claimRepo.FirstOrDefaultAsync(
                predicate: c => c.ClaimId == claimId,
                include: q => q.Include(c => c.CustomerVehicle)
            );

            if (claim == null) return null;

            return _mapper.Map<ClaimResponse>(claim);
        }

        // -------------------------------
        // 4️⃣ Cập nhật trạng thái yêu cầu
        // -------------------------------
        //public async Task<bool> UpdateClaimStatusAsync(Guid claimId, WarrantyClaimStatus newStatus)
        //{
        //    var claimRepo = _unitOfWork.GetRepository<WarrantyClaim>();
        //    var claim = await claimRepo.FirstOrDefaultAsync(c => c.ClaimId == claimId);

        //    if (claim == null)
        //        return false;

        //    claim.Status = newStatus;
        //    claimRepo.UpdateAsync(claim);
        //    await _unitOfWork.SaveChangesAsync();

        //    return true;
        //}

        //// -------------------------------
        //// 5️⃣ Xoá yêu cầu bảo hành
        //// -------------------------------
        //public async Task<bool> DeleteClaimAsync(Guid claimId)
        //{
        //    var claimRepo = _unitOfWork.GetRepository<WarrantyClaim>();
        //    var claim = await claimRepo.FirstOrDefaultAsync(c => c.ClaimId == claimId);
        //    if (claim == null) return false;

        //    claimRepo.DeleteAsync(claim);
        //    await _unitOfWork.SaveChangesAsync();
        //    return true;
        //}
    }
}

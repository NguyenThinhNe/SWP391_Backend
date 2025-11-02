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
    public class CampaignService : ICampaignService
    {
        private readonly IUnitOfWork<WarrantyDbContext> _unitOfWork;
        private readonly IMapper _mapper;

        public CampaignService(IUnitOfWork<WarrantyDbContext> unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<CampaignResponse> CreateCampaignAsync(CreateCampainRequest request)
        {
            var _campaignRepo = _unitOfWork.GetRepository<Campaign>();

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Validate dates
            if (request.EndDate <= request.StartDate)
                throw new ArgumentException("End date must be after start date");

            var campaign = _mapper.Map<Campaign>(request);
            campaign.CampaignId = Guid.NewGuid();
            campaign.CreatedDate = DateTime.UtcNow;
            campaign.Status = CampaignStatus.Pending;

            await _campaignRepo.InsertAsync(campaign);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CampaignResponse>(campaign);
        }

        public async Task<CampaignResponse> UpdateCampaignAsync(Guid campaignId, UpdateCampaignRequest request)
        {
            var _campaignRepo = _unitOfWork.GetRepository<Campaign>();

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var campaign = await _campaignRepo.FirstOrDefaultAsync(
                predicate: c => c.CampaignId == campaignId,
                include: q => q.Include(c => c.CustomerVehicles));

            if (campaign == null)
                throw new KeyNotFoundException($"Campaign with ID {campaignId} not found");

            // Validate dates
            if (request.EndDate <= request.StartDate)
                throw new ArgumentException("End date must be after start date");

            // Update properties
            campaign.CampaignName = request.CampaignName;
            campaign.StartDate = request.StartDate;
            campaign.EndDate = request.EndDate;
            campaign.Description = request.Description;
            campaign.Status = request.Status;

            _campaignRepo.UpdateAsync(campaign);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CampaignResponse>(campaign);
        }

        public async Task<bool> DeleteCampaignAsync(Guid campaignId)
        {
            var _campaignRepo = _unitOfWork.GetRepository<Campaign>();
            var _vehicleRepo = _unitOfWork.GetRepository<CustomerVehicle>();

            var campaign = await _campaignRepo.FirstOrDefaultAsync(
                predicate: c => c.CampaignId == campaignId);

            if (campaign == null)
                return false;

            // Check if campaign has associated vehicles
            var hasVehicles = await _vehicleRepo.CountAsync(
                predicate: v => v.CampaignId == campaignId) > 0;

            if (hasVehicles)
                throw new InvalidOperationException("Cannot delete campaign with associated vehicles. Please remove all vehicles first.");

            _campaignRepo.DeleteAsync(campaign);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<CampaignResponse> GetCampaignByIdAsync(Guid campaignId)
        {
            var _campaignRepo = _unitOfWork.GetRepository<Campaign>();

            var campaign = await _campaignRepo.FirstOrDefaultAsync(
                predicate: c => c.CampaignId == campaignId,
                include: q => q.Include(c => c.CustomerVehicles));

            if (campaign == null)
                throw new KeyNotFoundException($"Campaign with ID {campaignId} not found");

            return _mapper.Map<CampaignResponse>(campaign);
        }

        public async Task<IEnumerable<CampaignResponse>> GetAllCampaignsAsync()
        {
            var _campaignRepo = _unitOfWork.GetRepository<Campaign>();

            var campaigns = await _campaignRepo.GetListAsync(
                include: q => q.Include(c => c.CustomerVehicles),
                orderBy: q => q.OrderByDescending(c => c.CreatedDate));

            return _mapper.Map<IEnumerable<CampaignResponse>>(campaigns);
        }

        public async Task<CampaignResponse> GetCampaignByStatusAsync(CampaignStatus status)
        {
            var _campaignRepo = _unitOfWork.GetRepository<Campaign>();

            var campaign = await _campaignRepo.FirstOrDefaultAsync(
                predicate: c => c.Status == status,
                include: q => q.Include(c => c.CustomerVehicles),
                orderBy: q => q.OrderByDescending(c => c.StartDate));

            if (campaign == null)
                throw new KeyNotFoundException($"No campaign found with status {status}");

            return _mapper.Map<CampaignResponse>(campaign);
        }

        public async Task<IEnumerable<CampaignResponse>> GetActiveCampaignsAsync()
        {
            var _campaignRepo = _unitOfWork.GetRepository<Campaign>();
            var now = DateTime.UtcNow;

            var campaigns = await _campaignRepo.GetListAsync(
                predicate: c => c.Status == CampaignStatus.Pending &&
                               c.StartDate <= now &&
                               c.EndDate >= now,
                include: q => q.Include(c => c.CustomerVehicles),
                orderBy: q => q.OrderBy(c => c.EndDate));

            return _mapper.Map<IEnumerable<CampaignResponse>>(campaigns);
        }

        public async Task<bool> UpdateCampaignStatusAsync(Guid campaignId, CampaignStatus newStatus)
        {
            var _campaignRepo = _unitOfWork.GetRepository<Campaign>();

            var campaign = await _campaignRepo.FirstOrDefaultAsync(
                predicate: c => c.CampaignId == campaignId);

            if (campaign == null)
                return false;

            campaign.Status = newStatus;
            _campaignRepo.UpdateAsync(campaign);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CampaignExistsAsync(Guid campaignId)
        {
            var _campaignRepo = _unitOfWork.GetRepository<Campaign>();
            return await _campaignRepo.CountAsync(c => c.CampaignId == campaignId) > 0;
        }

        public async Task<bool> AddVehiclesToCampaignAsync(Guid campaignId, List<string> vehicleVins)
        {
            if (vehicleVins == null || !vehicleVins.Any())
                throw new ArgumentException("Vehicle VINs list cannot be empty");

            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var _vehicleRepo = _unitOfWork.GetRepository<CustomerVehicle>();

                // Check if campaign exists
                var campaignExists = await CampaignExistsAsync(campaignId);
                if (!campaignExists)
                    throw new KeyNotFoundException($"Campaign with ID {campaignId} not found");

                // Get vehicles
                var vehicles = await _vehicleRepo.GetListAsync(
                    predicate: v => vehicleVins.Contains(v.VIN));

                if (vehicles.Count != vehicleVins.Count)
                {
                    var foundVins = vehicles.Select(v => v.VIN).ToList();
                    var notFoundVins = vehicleVins.Except(foundVins).ToList();
                    throw new KeyNotFoundException($"Vehicles not found: {string.Join(", ", notFoundVins)}");
                }

                // Update campaign ID for each vehicle
                foreach (var vehicle in vehicles)
                {
                    vehicle.CampaignId = campaignId;
                    _vehicleRepo.UpdateAsync(vehicle);
                }

                return true;
            });
        }

        public async Task<bool> RemoveVehiclesFromCampaignAsync(Guid campaignId, List<string> vehicleVins)
        {
            if (vehicleVins == null || !vehicleVins.Any())
                throw new ArgumentException("Vehicle VINs list cannot be empty");

            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var _vehicleRepo = _unitOfWork.GetRepository<CustomerVehicle>();

                var vehicles = await _vehicleRepo.GetListAsync(
                    predicate: v => v.CampaignId == campaignId && vehicleVins.Contains(v.VIN));

                if (!vehicles.Any())
                    return false;

                // NOTE: Since CampaignId is [Required], you have 2 options:
                // Option 1: Delete vehicles from database (uncomment below)
                foreach (var vehicle in vehicles)
                {
                    _vehicleRepo.DeleteAsync(vehicle);
                }

                // Option 2: If you want to keep vehicles but remove from campaign,
                // you need to make CampaignId nullable in CustomerVehicle entity first
                // Then use: vehicle.CampaignId = null;

                return true;
            });
        }
    }
}

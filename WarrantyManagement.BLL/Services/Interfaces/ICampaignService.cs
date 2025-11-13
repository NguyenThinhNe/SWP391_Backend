using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantyManagement.DAL.Data.Enums;
using WarrantyManagement.DAL.Data.Request;
using WarrantyManagement.DAL.Data.Response;

namespace WarrantyManagement.BLL.Services.Interfaces
{
    public interface ICampaignService
    {
        Task<CampaignResponse> CreateCampaignAsync(CreateCampainRequest request);

        /// <summary>
        /// Update an existing campaign
        /// </summary>
        Task<CampaignResponse> UpdateCampaignAsync(Guid campaignId, UpdateCampaignRequest request);

        /// <summary>
        /// Delete a campaign by ID
        /// </summary>
        Task<bool> DeleteCampaignAsync(Guid campaignId);

        /// <summary>
        /// Get campaign by ID with vehicles
        /// </summary>
        Task<CampaignResponse> GetCampaignByIdAsync(Guid campaignId);
        Task<IEnumerable<CampaignResponse>> GetAllCampaignsAsync();
        Task<IEnumerable<CampaignResponse>> GetCampaignByServiceCenterId(Guid serviceCenterId);
        Task<IEnumerable<CampaignResponse>> GetCampaignByUserId(Guid userId);
        Task<CampaignResponse> GetCampaignByStatusAsync(CampaignStatus status);
        Task<IEnumerable<CampaignResponse>> GetActiveCampaignsAsync();
        Task<bool> UpdateCampaignStatusAsync(Guid campaignId, CampaignStatus newStatus);
        Task<bool> AddVehiclesToCampaignAsync(Guid campaignId, List<string> vehicleVins);

        /// <summary>
        /// Remove vehicles from campaign
        /// </summary>
        Task<bool> RemoveVehiclesFromCampaignAsync(Guid campaignId, List<string> vehicleVins);
        Task<CampaignResponse> AssignTechnicianAsync(Guid campaignId, Guid technicianId);
    }
}

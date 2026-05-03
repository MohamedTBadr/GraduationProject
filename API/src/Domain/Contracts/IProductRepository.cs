
using Domain.Entities;
using Google.GenAI.Types;
using Shared;
using System.Linq.Expressions;
namespace Domain.Contracts
{

    public interface IServiceRepository
        {
        Task<Service> GetByIdAsync(Guid id, CancellationToken cancellationToken);

        Task<PaginatedResponse<Service>> GetAllAsync(
            PaginatedRequest request,
            Expression<Func<Service, bool>> visibilityFilter,
            CancellationToken ct);

        //Task<PaginatedResponse<Service>> GetByCategoryIdAsync(
        //    Guid categoryId,
        //    PaginatedRequest request,
        //    Expression<Func<Service, bool>> visibilityFilter,
        //    CancellationToken cancellationToken);
        Task<PaginatedResponse<Service>> GetByEventTypeIdAsync(Guid eventTypeId, PaginatedRequest request, Expression<Func<Service, bool>> visibilityFilter, CancellationToken cancellationToken);


        Task<List<ServiceImage>> GetServiceImagesAsync(Guid serviceId, CancellationToken cancellationToken);

        Task DeleteServiceImagesAsync(Guid serviceId, CancellationToken cancellationToken);
        Task<PaginatedResponse<Service>> GetByVendorIdAsync(
            Guid vendorId,
            PaginatedRequest request,
            Expression<Func<Service, bool>> visibilityFilter,
            CancellationToken cancellationToken);

        Task<PaginatedResponse<Service>> GetByServiceTypeIdAsync(
            Guid serviceTypeId,
            PaginatedRequest request,
            Expression<Func<Service, bool>> visibilityFilter,
            CancellationToken cancellationToken);
        Task<Service> CreateAsync(Service Service, CancellationToken cancellationToken);
            Task<Service> UpdateAsync(Service Service, CancellationToken cancellationToken);
            Task DeleteAsync(Guid id, CancellationToken cancellationToken);
            Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
        Task<bool> UpdateStatusAsync(Guid id, bool isActive, CancellationToken ct);
        Task AddRatingAsync(ServiceRating rating, CancellationToken cancellationToken);


        Task<bool> HasUserPurchasedAsync(Guid userId, Guid serviceId, CancellationToken cancellationToken);

        Task<List<Service>> AIFilterAsync(AIRequest AIRequest, CancellationToken cancellationToken);
    }
    }

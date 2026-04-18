using Domain.Contracts;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Registry;

namespace Infrastructure.Repositories
{
    public class EventTypeRespository(
        ApplicationDbContext context,
        ResiliencePipelineProvider<string> pipelineProvider) : IEventTypeRepository
    {
        private readonly ResiliencePipeline _pipeline = pipelineProvider.GetPipeline("db-pipeline");

        public async Task CreateAsync(EventType eventType, CancellationToken cancellationToken)
        {
            await _pipeline.ExecuteAsync(async token =>
            {
                await context.EventTypes.AddAsync(eventType, token);
                await context.SaveChangesAsync(token);
            }, cancellationToken);
        }

        public async Task DeleteAsync(EventType eventType, CancellationToken cancellationToken)
        {
            await _pipeline.ExecuteAsync(async token =>
            {
                context.EventTypes.Remove(eventType);
                await context.SaveChangesAsync(token);
            }, cancellationToken);
        }

        public async Task<List<EventType>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _pipeline.ExecuteAsync(async token =>
                await context.EventTypes
                    .AsNoTracking()
                    .ToListAsync(token),
                cancellationToken);
        }

        public async Task<EventType> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _pipeline.ExecuteAsync(async token =>
            {
                var type = await context.EventTypes.FirstOrDefaultAsync(t => t.Id == id, token);
                return type!;
            }, cancellationToken);
        }

        public async Task UpdateAsync(EventType eventType, CancellationToken cancellationToken)
        {
            await _pipeline.ExecuteAsync(async token =>
            {
                context.EventTypes.Update(eventType);
                await context.SaveChangesAsync(token);
            }, cancellationToken);
        }
    }
}
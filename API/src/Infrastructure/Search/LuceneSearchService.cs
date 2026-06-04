using Application.Interfaces;
using Domain.Entities;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Directory = Lucene.Net.Store.Directory;

namespace Infrastructure.Search
{
    public class LuceneSearchService : ISearchService, IDisposable
    {
        private const LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;
        private readonly string _indexPath;
        private readonly Directory _directory;
        private readonly StandardAnalyzer _analyzer;

        public LuceneSearchService(IConfiguration configuration)
        {
            _indexPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LuceneIndex");
            if (!System.IO.Directory.Exists(_indexPath))
                System.IO.Directory.CreateDirectory(_indexPath);

            _directory = FSDirectory.Open(_indexPath);
            _analyzer = new StandardAnalyzer(AppLuceneVersion);

            if (!DirectoryReader.IndexExists(_directory))
            {
                using var writer = CreateWriter();
                writer.Commit();
            }
        }

        public Task IndexVendorAsync(Vendor vendor)
        {
            using var writer = CreateWriter();
            var doc = new Document
            {
                new StringField("Id", vendor.UserId.ToString(), Field.Store.YES),
                new StringField("Type", "Vendor", Field.Store.YES),
                new TextField("BusinessName", vendor.BusinessName ?? "", Field.Store.YES),
                new TextField("Description", vendor.Description ?? "", Field.Store.YES),
                new StringField("VendorType", vendor.VendorType?.Name ?? "", Field.Store.YES),
                new TextField("City", vendor.Address?.City ?? "", Field.Store.YES),
                new TextField("State", vendor.Address?.State ?? "", Field.Store.YES),
                new StringField("IsVerified", vendor.IsVerified ? "true" : "false", Field.Store.YES)
            };

            writer.UpdateDocument(new Term("Id", vendor.UserId.ToString()), doc);
            writer.Commit();
            return Task.CompletedTask;
        }

        public Task IndexServiceAsync(Service service)
        {
            using var writer = CreateWriter();
            var doc = new Document
            {
                new StringField("Id", service.Id.ToString(), Field.Store.YES),
                new StringField("Type", "Service", Field.Store.YES),
                new TextField("Name", service.Name ?? "", Field.Store.YES),
                new TextField("Description", service.Description ?? "", Field.Store.YES),
                new StringField("ServiceType", service.ServiceType?.Name ?? "", Field.Store.YES),
                new StringField("ServiceTypeId", service.ServiceTypeId.ToString(), Field.Store.YES),
                new DoubleField("Price", (double)service.Price, Field.Store.YES),
                new StringField("VendorId", service.VendorId.ToString(), Field.Store.YES),
                new TextField("VendorName", service.Vendor?.BusinessName ?? "", Field.Store.YES)
            };

            writer.UpdateDocument(new Term("Id", service.Id.ToString()), doc);
            writer.Commit();
            return Task.CompletedTask;
        }

        public Task RemoveVendorAsync(Guid userId) => RemoveDocumentAsync(userId.ToString());
        public Task RemoveServiceAsync(Guid serviceId) => RemoveDocumentAsync(serviceId.ToString());

        private Task RemoveDocumentAsync(string id)
        {
            using var writer = CreateWriter();
            writer.DeleteDocuments(new Term("Id", id));
            writer.Commit();
            return Task.CompletedTask;
        }

        public Task RebuildIndexAsync()
        {
            // This would normally be handled by a background task that fetches all from DB
            // For now, we clear the index
            using var writer = CreateWriter();
            writer.DeleteAll();
            writer.Commit();
            return Task.CompletedTask;
        }

        public Task ClearIndexAsync()
        {
            using var writer = CreateWriter();
            writer.DeleteAll();
            writer.Commit();
            return Task.CompletedTask;
        }

        public Task IndexVendorsBatchAsync(IEnumerable<Vendor> vendors)
        {
            using var writer = CreateWriter();
            foreach (var vendor in vendors)
            {
                var doc = new Document
                {
                    new StringField("Id", vendor.UserId.ToString(), Field.Store.YES),
                    new StringField("Type", "Vendor", Field.Store.YES),
                    new TextField("BusinessName", vendor.BusinessName ?? "", Field.Store.YES),
                    new TextField("Description", vendor.Description ?? "", Field.Store.YES),
                    new StringField("VendorType", vendor.VendorType?.Name ?? "", Field.Store.YES),
                    new TextField("City", vendor.Address?.City ?? "", Field.Store.YES),
                    new TextField("State", vendor.Address?.State ?? "", Field.Store.YES),
                    new StringField("IsVerified", vendor.IsVerified ? "true" : "false", Field.Store.YES)
                };
                writer.UpdateDocument(new Term("Id", vendor.UserId.ToString()), doc);
            }
            writer.Commit();
            return Task.CompletedTask;
        }

        public Task IndexServicesBatchAsync(IEnumerable<Service> services)
        {
            using var writer = CreateWriter();
            foreach (var service in services)
            {
                var doc = new Document
                {
                    new StringField("Id", service.Id.ToString(), Field.Store.YES),
                    new StringField("Type", "Service", Field.Store.YES),
                    new TextField("Name", service.Name ?? "", Field.Store.YES),
                    new TextField("Description", service.Description ?? "", Field.Store.YES),
                    new StringField("ServiceType", service.ServiceType?.Name ?? "", Field.Store.YES),
                    new StringField("ServiceTypeId", service.ServiceTypeId.ToString(), Field.Store.YES),
                    new DoubleField("Price", (double)service.Price, Field.Store.YES),
                    new StringField("VendorId", service.VendorId.ToString(), Field.Store.YES),
                    new TextField("VendorName", service.Vendor?.BusinessName ?? "", Field.Store.YES)
                };
                writer.UpdateDocument(new Term("Id", service.Id.ToString()), doc);
            }
            writer.Commit();
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Guid>> SearchVendorsAsync(string query, string? category = null, string? location = null, bool includeUnverified = false)
        {
            if (string.IsNullOrWhiteSpace(query) && string.IsNullOrWhiteSpace(category) && string.IsNullOrWhiteSpace(location))
                return Task.FromResult(Enumerable.Empty<Guid>());

            using var reader = DirectoryReader.Open(_directory);
            var searcher = new IndexSearcher(reader);
            var booleanQuery = new BooleanQuery();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var parser = new MultiFieldQueryParser(AppLuceneVersion, new[] { "BusinessName", "Description" }, _analyzer);
                parser.FuzzyMinSim = 0.7f;
                var fuzzyQuery = parser.Parse(query + "~");
                booleanQuery.Add(fuzzyQuery, Occur.MUST);
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                booleanQuery.Add(new TermQuery(new Term("VendorType", category)), Occur.MUST);
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                var locParser = new MultiFieldQueryParser(AppLuceneVersion, new[] { "City", "State" }, _analyzer);
                booleanQuery.Add(locParser.Parse(location), Occur.MUST);
            }

            if (!includeUnverified)
            {
                booleanQuery.Add(new TermQuery(new Term("IsVerified", "true")), Occur.MUST);
            }

            booleanQuery.Add(new TermQuery(new Term("Type", "Vendor")), Occur.MUST);

            var hits = searcher.Search(booleanQuery, 50).ScoreDocs;
            var results = hits.Select(hit => Guid.Parse(searcher.Doc(hit.Doc).Get("Id"))).ToList();

            return Task.FromResult<IEnumerable<Guid>>(results);
        }

        public Task<IEnumerable<Guid>> SearchServicesAsync(string query, Guid? serviceTypeId = null, decimal? minPrice = null, decimal? maxPrice = null)
        {
            using var reader = DirectoryReader.Open(_directory);
            var searcher = new IndexSearcher(reader);
            var booleanQuery = new BooleanQuery();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var parser = new MultiFieldQueryParser(AppLuceneVersion, new[] { "Name", "Description", "VendorName" }, _analyzer);
                booleanQuery.Add(parser.Parse(query + "~"), Occur.MUST);
            }

            if (serviceTypeId.HasValue)
            {
                booleanQuery.Add(new TermQuery(new Term("ServiceTypeId", serviceTypeId.Value.ToString())), Occur.MUST);
            }

            if (minPrice.HasValue || maxPrice.HasValue)
            {
                var min = minPrice.HasValue ? (double)minPrice.Value : double.MinValue;
                var max = maxPrice.HasValue ? (double)maxPrice.Value : double.MaxValue;
                booleanQuery.Add(NumericRangeQuery.NewDoubleRange("Price", min, max, true, true), Occur.MUST);
            }

            booleanQuery.Add(new TermQuery(new Term("Type", "Service")), Occur.MUST);

            var hits = searcher.Search(booleanQuery, 50).ScoreDocs;
            var results = hits.Select(hit => Guid.Parse(searcher.Doc(hit.Doc).Get("Id"))).ToList();

            return Task.FromResult<IEnumerable<Guid>>(results);
        }

        public Task IndexUserProfilesBatchAsync(IEnumerable<(Guid UserId, string BookedVendorIds, string BookedCategories)> userProfiles)
        {
            using var writer = CreateWriter();
            foreach (var profile in userProfiles)
            {
                var doc = new Document
                {
                    new StringField("Id", profile.UserId.ToString(), Field.Store.YES),
                    new StringField("Type", "UserProfile", Field.Store.YES),
                    new TextField("BookedVendors", profile.BookedVendorIds ?? "", Field.Store.YES),
                    new TextField("BookedCategories", profile.BookedCategories ?? "", Field.Store.YES)
                };
                writer.UpdateDocument(new Term("Id", profile.UserId.ToString()), doc);
            }
            writer.Commit();
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Guid>> SearchSimilarUsersAsync(string vendorIds, string categories, int limit = 10)
        {
            if (string.IsNullOrWhiteSpace(vendorIds) && string.IsNullOrWhiteSpace(categories))
                return Task.FromResult(Enumerable.Empty<Guid>());

            using var reader = DirectoryReader.Open(_directory);
            var searcher = new IndexSearcher(reader);
            var booleanQuery = new BooleanQuery();

            if (!string.IsNullOrWhiteSpace(vendorIds))
            {
                var parser = new MultiFieldQueryParser(AppLuceneVersion, new[] { "BookedVendors" }, _analyzer);
                // Allow matches on any overlapping vendor
                var q = parser.Parse(QueryParserBase.Escape(vendorIds));
                booleanQuery.Add(q, Occur.SHOULD);
            }

            if (!string.IsNullOrWhiteSpace(categories))
            {
                var parser = new MultiFieldQueryParser(AppLuceneVersion, new[] { "BookedCategories" }, _analyzer);
                var q = parser.Parse(QueryParserBase.Escape(categories));
                booleanQuery.Add(q, Occur.SHOULD);
            }

            // Must be a user profile
            booleanQuery.Add(new TermQuery(new Term("Type", "UserProfile")), Occur.MUST);

            var hits = searcher.Search(booleanQuery, limit).ScoreDocs;
            var results = hits.Select(hit => Guid.Parse(searcher.Doc(hit.Doc).Get("Id"))).ToList();

            return Task.FromResult<IEnumerable<Guid>>(results);
        }

        private IndexWriter CreateWriter()
        {
            var config = new IndexWriterConfig(AppLuceneVersion, _analyzer);
            return new IndexWriter(_directory, config);
        }

        public void Dispose()
        {
            _analyzer?.Dispose();
            _directory?.Dispose();
        }
    }
}

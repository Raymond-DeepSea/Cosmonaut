﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Cosmonaut.Extensions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Cosmonaut.Storage
{
    public class CosmosCollectionCreator : ICollectionCreator
    {
        private readonly IDocumentClient _documentClient;

        public CosmosCollectionCreator(IDocumentClient documentClient)
        {
            _documentClient = documentClient;
        }

        public async Task<bool> EnsureCreatedAsync(Type entityType, 
            Database database, 
            int collectionThroughput,
            IndexingPolicy indexingPolicy = null)
        {
            var collectionName = entityType.GetCollectionName();
            var collection = _documentClient
                .CreateDocumentCollectionQuery(database.SelfLink)
                .ToArray()
                .FirstOrDefault(c => c.Id == collectionName);

            if (collection != null)
                return true;

            collection = new DocumentCollection
            {
                Id = collectionName
            };
            var partitionKey = entityType.GetPartitionKeyForEntity();

            if (partitionKey != null)
                collection.PartitionKey = partitionKey;

            if (indexingPolicy != null)
                collection.IndexingPolicy = indexingPolicy;

            collection = await _documentClient.CreateDocumentCollectionAsync(database.SelfLink, collection, new RequestOptions
            {
                OfferThroughput = collectionThroughput
            });

            return collection != null;
        }
    }
}
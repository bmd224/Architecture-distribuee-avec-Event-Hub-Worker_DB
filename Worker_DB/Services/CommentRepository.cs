using Microsoft.Azure.Cosmos;
using System;
using System.Threading.Tasks;
using Worker_DB.Models;

namespace Worker_DB.Services
{
    public class CommentRepository
    {
        private readonly Container _container;

        public CommentRepository(CosmosClient cosmosClient, string databaseName, string containerName)
        {
            _container = cosmosClient.GetContainer(databaseName, containerName);
        }

        public async Task AddCommentAsync(Comment comment)
        {
            comment.Id = Guid.NewGuid();
            comment.Created = DateTime.UtcNow;
            await _container.CreateItemAsync(comment, new PartitionKey(comment.PostId.ToString()));
        }

        public async Task PatchCommentAsync(Guid commentId, Guid postId, PatchOperation[] patchOps)
        {
            await _container.PatchItemAsync<Comment>(
                id: commentId.ToString(),
                partitionKey: new PartitionKey(postId.ToString()),
                patchOperations: patchOps);
        }

    }
}

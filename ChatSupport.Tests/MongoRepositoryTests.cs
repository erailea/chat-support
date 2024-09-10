using ChatSupport.Data;
using ChatSupport.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace ChatSupport.Tests
{

    public class MongoRepositoryTests
    {
        private readonly Mock<IMongoCollection<TestEntity>> _mockCollection;
        private readonly TestMongoRepository _repository;

        public MongoRepositoryTests()
        {
            var mockDatabase = new Mock<IMongoDatabase>();
            _mockCollection = new Mock<IMongoCollection<TestEntity>>();
            mockDatabase.Setup(db => db.GetCollection<TestEntity>(It.IsAny<string>(), null))
                        .Returns(_mockCollection.Object);

            _repository = new TestMongoRepository(mockDatabase.Object);
        }

        [Fact]
        public async Task AddAsync_ShouldInsertEntity()
        {
            var entity = new TestEntity { Id = ObjectId.GenerateNewId(), Name = "Test" };

            await _repository.AddAsync(entity);

            _mockCollection.Verify(c => c.InsertOneAsync(entity, null, default), Times.Once);
        }
        /*
                [Fact]
                public async Task GetAllAsync_ShouldReturnAllEntities()
                {
                    var entities = new List<TestEntity>
                {
                    new TestEntity { Id = ObjectId.GenerateNewId(), Name = "Test1" },
                    new TestEntity { Id = ObjectId.GenerateNewId(), Name = "Test2" }
                };

                    _mockCollection.Setup(c => c.Find(
                            It.IsAny<FilterDefinition<TestEntity>>(),
                            default(FindOptions)
                            )
                        )
                        .Returns(new FakeFindFluent<TestEntity, TestEntity>(entities));

                    var result = await _repository.GetAllAsync();

                    Assert.Equal(entities.Count, result.Count);
                }

                [Fact]
                public async Task GetByIdAsync_ShouldReturnEntity_WhenExists()
                {
                    var entityId = ObjectId.GenerateNewId();
                    var entity = new TestEntity { Id = entityId, Name = "TestEntity" };

                    _mockCollection.Setup(c => c.Find(Builders<TestEntity>.Filter.Eq("_id", entityId), null))
                                   .Returns(new FakeFindFluent<TestEntity, TestEntity>(new List<TestEntity> { entity }));

                    var result = await _repository.GetByIdAsync(entityId);

                    Assert.Equal(entityId, result.Id);
                }

                [Fact]
                public async Task UpdateAsync_ShouldUpdateEntity()
                {
                    var entityId = ObjectId.GenerateNewId();
                    var entity = new TestEntity { Id = entityId, Name = "Updated Test" };

                    await _repository.UpdateAsync(entityId, entity);

                    _mockCollection.Verify(c => c.ReplaceOneAsync(
                        Builders<TestEntity>.Filter.Eq("_id", entityId),
                        entity,
                        default(ReplaceOptions),
                        default(CancellationToken)),
                        Times.Once);
                }

                [Fact]
                public async Task DeleteAsync_ShouldDeleteEntity()
                {
                    var entityId = ObjectId.GenerateNewId();

                    await _repository.DeleteAsync(entityId);

                    _mockCollection.Verify(c => c.DeleteOneAsync(
                        Builders<TestEntity>.Filter.Eq("_id", entityId),
                        It.IsAny<CancellationToken>()),
                        Times.Once);
                }*/

        [Fact]
        public async Task CountAsync_ShouldReturnCountOfEntities()
        {
            _mockCollection.Setup(c => c.CountDocumentsAsync(It.IsAny<FilterDefinition<TestEntity>>(), null, default))
                           .ReturnsAsync(2);

            var count = await _repository.CountAsync();

            Assert.Equal(2, count);
        }

        [Fact]
        public async Task DeleteAllAsync_ShouldDeleteAllEntities()
        {
            await _repository.DeleteAllAsync();

            _mockCollection.Verify(c => c.DeleteManyAsync(It.IsAny<FilterDefinition<TestEntity>>(), default), Times.Once);
        }

    }

    [CollectionName("test_collection")]
    public class TestEntity
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; }
    }

    public class TestMongoRepository : MongoRepository<TestEntity>

    {
        public TestMongoRepository(IMongoDatabase database) : base(database) { }
    }

    public class FakeFindFluent<TEntity, TProjection> : IFindFluent<TEntity, TEntity>
    {
        private readonly IEnumerable<TEntity> _items;

        public FakeFindFluent(IEnumerable<TEntity> items)
        {
            _items = items ?? Enumerable.Empty<TEntity>();
        }

        public FilterDefinition<TEntity> Filter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public FindOptions<TEntity, TEntity> Options => throw new NotImplementedException();

        public IFindFluent<TEntity, TResult> As<TResult>(MongoDB.Bson.Serialization.IBsonSerializer<TResult> resultSerializer = null)
        {
            throw new NotImplementedException();
        }

        public long Count(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<long> CountAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public long CountDocuments(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<long> CountDocumentsAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public IFindFluent<TEntity, TEntity> Limit(int? limit)
        {
            throw new NotImplementedException();
        }

        public IFindFluent<TEntity, TNewProjection> Project<TNewProjection>(ProjectionDefinition<TEntity, TNewProjection> projection)
        {
            throw new NotImplementedException();
        }

        public IFindFluent<TEntity, TEntity> Skip(int? skip)
        {
            throw new NotImplementedException();
        }

        public IFindFluent<TEntity, TEntity> Sort(SortDefinition<TEntity> sort)
        {
            throw new NotImplementedException();
        }

        public IAsyncCursor<TEntity> ToCursor(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IAsyncCursor<TEntity>> ToCursorAsync(CancellationToken cancellationToken = default)
        {
            IAsyncCursor<TEntity> cursor = new FakeAsyncCursor<TEntity>(_items);
            var task = Task.FromResult(cursor);

            return task;
        }
    }


    public class FakeAsyncCursor<TEntity> : IAsyncCursor<TEntity>
    {
        private IEnumerable<TEntity> items;

        public FakeAsyncCursor(IEnumerable<TEntity> items)
        {
            this.items = items;
        }

        public IEnumerable<TEntity> Current => items;

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public bool MoveNext(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> MoveNextAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }
    }

}
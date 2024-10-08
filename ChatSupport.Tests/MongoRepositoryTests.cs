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


public class MongoRepositoryTests
{
    private readonly Mock<IMongoCollection<MyEntity>> _mockCollection;
    private MongoRepository<MyEntity> _repository;
    private readonly Mock<IAsyncCursor<MyEntity>> _mockCursor;
    private readonly Mock<IMongoDatabase> _mockDatabase;
    private readonly Mock<IMongoCollection<EntityWithoutCollectionAttribute>> _mockCollectionWithoutAttribute;


    public MongoRepositoryTests()
    {
        _mockCollection = new Mock<IMongoCollection<MyEntity>>();
        _mockCollectionWithoutAttribute = new Mock<IMongoCollection<EntityWithoutCollectionAttribute>>();
        _mockCursor = new Mock<IAsyncCursor<MyEntity>>();
        _mockDatabase = new Mock<IMongoDatabase>();

        _mockDatabase.Setup(d => d.GetCollection<MyEntity>(
            It.IsAny<string>(),
            (MongoCollectionSettings)null))
        .Returns(_mockCollection.Object);

        _repository = new MongoRepository<MyEntity>(_mockDatabase.Object);
    }

    [Fact]
    public async Task Cotr_Should_Throw_Exception_When_Entity_Attribute_Not_Found()
    {
        var exception = Assert.Throws<Exception>(() => new MongoRepository<EntityWithoutCollectionAttribute>(null));
        Assert.Equal("CollectionNameAttribute is missing", exception.Message);
    }

    [Fact]
    public async Task GetById_Should_Return_Entity()
    {
        var id = ObjectId.GenerateNewId();
        var entity = new MyEntity { Name = "Test" };

        _mockCollection.Setup(c => c.FindSync(
            It.IsAny<FilterDefinition<MyEntity>>(),
            It.IsAny<FindOptions<MyEntity>>(),
            default))
        .Returns(_mockCursor.Object);

        _mockCursor.SetupSequence(_async => _async.MoveNext(default)).Returns(true).Returns(false);

        _mockCursor.SetupGet(_async => _async.Current).Returns(new List<MyEntity> { entity });

        var result = _repository.GetById(id);

        Assert.Equal(entity, result);
    }

    [Fact]
    public async Task GetAll_With_Argument_Should_Return_List_Of_Entities()
    {
        // Arrange
        var expected = new List<MyEntity> { new MyEntity { Name = "Entity1" } };

        _mockCursor.SetupSequence(_async => _async.MoveNext(default)).Returns(true).Returns(false);

        _mockCursor.SetupGet(_async => _async.Current).Returns(expected);

        _mockCollection.Setup(c => c.FindSync(
            It.IsAny<FilterDefinition<MyEntity>>(),
            It.IsAny<FindOptions<MyEntity>>(),
            default))
        .Returns(_mockCursor.Object);


        // Act
        var result = _repository.GetAll(e => e.Name == "Entity1");

        // Assert
        Assert.Equal(expected, result);

    }



    [Fact]
    public async Task GetAll_Should_Return_List_Of_Entities()
    {
        // Arrange
        var entities = new List<MyEntity> { new MyEntity { Name = "Entity1" }, new MyEntity { Name = "Entity2" } };

        _mockCursor.SetupSequence(_async => _async.MoveNext(default)).Returns(true).Returns(false);

        _mockCursor.SetupGet(_async => _async.Current).Returns(entities);

        _mockCollection.Setup(c => c.FindSync(
            Builders<MyEntity>.Filter.Empty,
            It.IsAny<FindOptions<MyEntity>>(),
            default))
        .Returns(_mockCursor.Object);


        // Act
        var result = _repository.GetAll();

        // Assert
        Assert.Equal(entities, result);
    }


    [Fact]
    public async Task DeleteAllAsync_ShouldDeleteAllEntities()
    {
        await _repository.DeleteAllAsync();

        _mockCollection.Verify(c => c.DeleteManyAsync(It.IsAny<FilterDefinition<MyEntity>>(), default), Times.Once);
    }


    [Fact]
    public async Task DeleteAsync_ShouldDeleteAllEntities()
    {
        var id = ObjectId.GenerateNewId();
        await _repository.DeleteAsync(id);

        _mockCollection.Verify(c => c.DeleteOneAsync(It.IsAny<FilterDefinition<MyEntity>>(), default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateEntity()
    {
        var id = ObjectId.GenerateNewId();
        var entity = new MyEntity { Name = "Test" };

        await _repository.UpdateAsync(id, entity);

        _mockCollection.Verify(c => c.ReplaceOneAsync(It.IsAny<FilterDefinition<MyEntity>>(), entity, (ReplaceOptions)null, default), Times.Once);
    }

    [Fact]
    public async Task CountAsync_ShouldReturnCountOfEntities()
    {
        _mockCollection.Setup(c => c.CountDocumentsAsync(It.IsAny<FilterDefinition<MyEntity>>(), null, default))
                       .ReturnsAsync(2);

        var count = await _repository.CountAsync();

        Assert.Equal(2, count);
    }


    [Fact]
    public async Task AddAsync_ShouldInsertEntity()
    {
        var entity = new MyEntity { Name = "Test" };

        await _repository.AddAsync(entity);

        _mockCollection.Verify(c => c.InsertOneAsync(entity, null, default), Times.Once);
    }


}


public class MyRepository
{
    private readonly IMongoCollection<MyEntity> _collection;

    public MyRepository(IMongoCollection<MyEntity> collection)
    {
        _collection = collection;
    }

    public async Task<IList<MyEntity>> GetAll()
    {
        return await _collection.Find(new BsonDocument()).ToListAsync();
    }
}

[CollectionName("test_collection")]
public class MyEntity
{
    public string Name { get; set; }
}

public class EntityWithoutCollectionAttribute
{
    public string Name { get; set; }
}


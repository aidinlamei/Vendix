using Vendix.Domain.Common;

namespace Vendix.Domain.Tests.Common;

/// <summary>
/// Unit tests for <see cref="BaseEntity"/> class.
/// </summary>
public class BaseEntityTests
{
    /// <summary>
    /// Concrete implementation of BaseEntity for testing purposes.
    /// </summary>
    private sealed class TestEntity : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public TestEntity() : base()
        {
        }

        public TestEntity(Guid id) : base(id)
        {
        }
    }

    /// <summary>
    /// Another concrete implementation to test cross-type equality.
    /// </summary>
    private sealed class OtherTestEntity : BaseEntity
    {
        public OtherTestEntity() : base()
        {
        }

        public OtherTestEntity(Guid id) : base(id)
        {
        }
    }

    [Fact]
    public void Constructor_WithNoId_GeneratesNewGuid()
    {
        // Act
        var entity = new TestEntity();

        // Assert
        entity.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_WithValidId_SetsId()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var entity = new TestEntity(id);

        // Assert
        entity.Id.Should().Be(id);
    }

    [Fact]
    public void Constructor_WithEmptyId_ThrowsArgumentException()
    {
        // Arrange
        var emptyId = Guid.Empty;

        // Act
        var act = () => new TestEntity(emptyId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("id")
            .WithMessage("*cannot be empty*");
    }

    [Fact]
    public void Equals_SameId_ReturnsTrue()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id) { Name = "Entity 1" };
        var entity2 = new TestEntity(id) { Name = "Entity 2" };

        // Act & Assert
        entity1.Equals(entity2).Should().BeTrue();
        (entity1 == entity2).Should().BeTrue();
        (entity1 != entity2).Should().BeFalse();
    }

    [Fact]
    public void Equals_DifferentId_ReturnsFalse()
    {
        // Arrange
        var entity1 = new TestEntity(Guid.NewGuid());
        var entity2 = new TestEntity(Guid.NewGuid());

        // Act & Assert
        entity1.Equals(entity2).Should().BeFalse();
        (entity1 == entity2).Should().BeFalse();
        (entity1 != entity2).Should().BeTrue();
    }

    [Fact]
    public void Equals_SameReference_ReturnsTrue()
    {
        // Arrange
        var entity = new TestEntity();

        // Act & Assert
        entity.Equals(entity).Should().BeTrue();
#pragma warning disable CS1718 // Comparison made to same variable
        (entity == entity).Should().BeTrue();
#pragma warning restore CS1718
    }

    [Fact]
    public void Equals_Null_ReturnsFalse()
    {
        // Arrange
        var entity = new TestEntity();

        // Act & Assert
        entity.Equals(null).Should().BeFalse();
        (entity == null).Should().BeFalse();
        (null == entity).Should().BeFalse();
    }

    [Fact]
    public void Equals_BothNull_ReturnsTrue()
    {
        // Arrange
        TestEntity? entity1 = null;
        TestEntity? entity2 = null;

        // Act & Assert
        (entity1 == entity2).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentType_ReturnsFalse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id);
        var entity2 = new OtherTestEntity(id);

        // Act & Assert
        entity1.Equals(entity2).Should().BeFalse();
    }

    [Fact]
    public void Equals_NonEntityObject_ReturnsFalse()
    {
        // Arrange
        var entity = new TestEntity();
        var notAnEntity = "not an entity";

        // Act & Assert
        entity.Equals(notAnEntity).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_SameId_ReturnsSameHashCode()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id);
        var entity2 = new TestEntity(id);

        // Act & Assert
        entity1.GetHashCode().Should().Be(entity2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentId_ReturnsDifferentHashCode()
    {
        // Arrange
        var entity1 = new TestEntity(Guid.NewGuid());
        var entity2 = new TestEntity(Guid.NewGuid());

        // Act & Assert
        entity1.GetHashCode().Should().NotBe(entity2.GetHashCode());
    }

    [Fact]
    public void TwoNewEntities_HaveDifferentIds()
    {
        // Act
        var entity1 = new TestEntity();
        var entity2 = new TestEntity();

        // Assert
        entity1.Id.Should().NotBe(entity2.Id);
        entity1.Equals(entity2).Should().BeFalse();
    }
}

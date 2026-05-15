using AlgoSphere.Application.Features.Topics.Queries.GetTopics;
using FluentAssertions;
using Xunit;

namespace AlgoSphere.UnitTests;

public class TopicTests
{
    [Fact]
    public void TopicDto_ShouldStoreDataCorrectly()
    {
        // Arrange
        var id = 1;
        var name = "Sorting";
        var desc = "Algorithms for sorting data";
        var order = 10;

        // Act
        var dto = new TopicDto(id, name, desc, order);

        // Assert
        Assert.Equal(id, dto.Id);
        Assert.Equal(name, dto.Name);
        Assert.Equal(desc, dto.Description);
        Assert.Equal(order, dto.OrderIndex);
    }
}

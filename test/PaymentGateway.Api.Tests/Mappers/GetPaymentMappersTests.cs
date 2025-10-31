using Mapster;

using MapsterMapper;

using PaymentGateway.Api.Mappers;

namespace PaymentGateway.Api.Tests.Mappers;

public class GetPaymentMappersTests : MapperTests
{
    public GetPaymentMappersTests() : base(() => new GetPaymentMappers())
    {
       
    }

    // [Fact]
    // public void GetPaymentMappers_IsValid()
    // {
    //     // Arrange & Act
    //     var act = () => Sut.Config.Compile();
    //     // Assert
    //     act.Should().NotThrow();
    // }
}

public abstract class MapperTests
{
    protected readonly IMapper Sut;
    protected MapperTests(params Func<IRegister>[] mapperFactories)
    {
        var config = new TypeAdapterConfig { RequireDestinationMemberSource = true };
        foreach (var mapperFactory in mapperFactories)
        {
            var mapper =  mapperFactory();
            mapper.Register(config);
        }
        Sut = new Mapper(config);
    }

    [Fact]
    public void Mapper_Is_Valid()
    {
        // Arrange & Act
        var act = () => Sut.Config.Compile();
        // Assert
        act.Should().NotThrow();
    }
}
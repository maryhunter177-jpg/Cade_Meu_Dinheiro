using CadeMeuDinheiro.Infrastructure;
using FluentAssertions;
namespace CadeMeuDinheiro.UnitTests;
public sealed class PasswordServiceTests
{
    [Fact]
    public void HashIsSaltedAndVerifiable()
    {
        var service = new PasswordService();
        var first = service.Hash("SenhaSegura123"); var second = service.Hash("SenhaSegura123");
        first.Should().NotBe(second); service.Verify("SenhaSegura123", first).Should().BeTrue();
        service.Verify("SenhaErrada123", first).Should().BeFalse();
    }
}

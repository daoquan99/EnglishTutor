using EnglishTutor.BuildingBlocks.Domain.ValueObjects;
using FluentAssertions;

namespace EnglishTutor.BuildingBlocks.UnitTests;

public class ValueObjectTests
{
    private class TestValueObject : ValueObject
    {
        public string Value1 { get; }
        public int Value2 { get; }

        public TestValueObject(string value1, int value2)
        {
            Value1 = value1;
            Value2 = value2;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value1;
            yield return Value2;
        }
    }

    /// <summary>Edge-case value object with zero components (e.g. Unit).</summary>
    private class EmptyValueObject : ValueObject
    {
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield break;
        }
    }

    [Fact]
    public void ValueObject_With_Same_Values_Should_Be_Equal()
    {
        var vo1 = new TestValueObject("test", 42);
        var vo2 = new TestValueObject("test", 42);

        vo1.Should().Be(vo2);
        vo1.GetHashCode().Should().Be(vo2.GetHashCode());
    }

    [Fact]
    public void ValueObject_With_Different_Values_Should_Not_Be_Equal()
    {
        var vo1 = new TestValueObject("test1", 42);
        var vo2 = new TestValueObject("test2", 42);

        vo1.Should().NotBe(vo2);
    }

    [Fact]
    public void ValueObject_Should_Not_Be_Equal_To_Null()
    {
        var vo = new TestValueObject("test", 42);

        vo.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Equality_Operator_Should_Work_Correctly()
    {
        var vo1 = new TestValueObject("test", 42);
        var vo2 = new TestValueObject("test", 42);

        (vo1 == vo2).Should().BeTrue();
        (vo1 != vo2).Should().BeFalse();
    }

    [Fact]
    public void ValueObject_With_Null_Component_Should_Be_Handled()
    {
        var vo1 = new TestValueObject(null!, 42);
        var vo2 = new TestValueObject(null!, 42);

        vo1.Should().Be(vo2);
    }

    // ===== Fix #4: GetHashCode must NOT throw with empty components =====

    [Fact]
    public void GetHashCode_With_No_Equality_Components_Should_Not_Throw()
    {
        // Regression: previous implementation called Aggregate on an empty sequence,
        // which threw InvalidOperationException.
        var empty = new EmptyValueObject();

        var act = () => _ = empty.GetHashCode();
        act.Should().NotThrow();
    }

    [Fact]
    public void GetHashCode_With_No_Equality_Components_Should_Be_Stable()
    {
        var vo1 = new EmptyValueObject();
        var vo2 = new EmptyValueObject();

        vo1.GetHashCode().Should().Be(vo2.GetHashCode());
    }

    [Fact]
    public void EmptyValueObjects_Should_Be_Equal()
    {
        var vo1 = new EmptyValueObject();
        var vo2 = new EmptyValueObject();

        vo1.Should().Be(vo2);
    }
}

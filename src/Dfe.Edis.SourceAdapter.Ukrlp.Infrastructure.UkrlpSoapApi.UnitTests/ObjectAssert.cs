using NUnit.Framework;

namespace Dfe.Edis.SourceAdapter.Ukrlp.Infrastructure.UkrlpSoapApi.UnitTests
{
    public static class ObjectAssert
    {
        public static void AreEqual<T>(T expected, T actual)
        {
            var properties = typeof(T).GetProperties();
            foreach (var property in properties)
            {
                var expectedValue = property.GetValue(expected);
                var actualValue = property.GetValue(actual);

                if (property.PropertyType.IsClass)
                {
                    AreEqual(expectedValue, actualValue);

                    continue;
                }

                if (expectedValue == null && actualValue != null ||
                    expectedValue != null && actualValue == null)
                {
                    throw new AssertionException($"Expected {property.Name} of {typeof(T).FullName} to be {expectedValue} but was {actualValue}");
                }

                if (expectedValue != null && !expectedValue.Equals(actualValue))
                {
                    throw new AssertionException($"Expected {property.Name} of {typeof(T).FullName} to be {expectedValue} but was {actualValue}");
                }
            }
        }
    }
}
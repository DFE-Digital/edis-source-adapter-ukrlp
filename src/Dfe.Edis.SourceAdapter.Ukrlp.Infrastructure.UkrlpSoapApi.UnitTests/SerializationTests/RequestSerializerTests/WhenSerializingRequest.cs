using System;
using System.Linq;
using System.Xml.Linq;
using AutoFixture.NUnit3;
using Dfe.Edis.SourceAdapter.Ukrlp.Infrastructure.UkrlpSoapApi.Models;
using Dfe.Edis.SourceAdapter.Ukrlp.Infrastructure.UkrlpSoapApi.Serialization;
using NUnit.Framework;

namespace Dfe.Edis.SourceAdapter.Ukrlp.Infrastructure.UkrlpSoapApi.UnitTests.SerializationTests.RequestSerializerTests
{
    public class WhenSerializingRequest
    {
        [Test, AutoData]
        public void ThenItShouldReturnValidXml(ProviderQueryRequest request)
        {
            var serializer = new RequestSerializer();

            var actual = serializer.Serialize(request);

            XDocument doc = null;
            Assert.DoesNotThrow(() => doc = XDocument.Parse(actual));
            Assert.IsNotNull(doc);
        }

        [Test, AutoData]
        public void ThenItShouldWrapTheRequestInASoapPackage(ProviderQueryRequest request)
        {
            var serializer = new RequestSerializer();

            var actual = serializer.Serialize(request);

            XDocument doc = XDocument.Parse(actual);
            Assert.IsNotNull(doc.Root.Name == RequestSerializer.SoapNs + "Envelope");
            Assert.IsNotNull(doc?
                .Element(RequestSerializer.SoapNs + "Envelope")?
                .Element(RequestSerializer.SoapNs + "Header"));
            Assert.IsNotNull(doc?
                .Element(RequestSerializer.SoapNs + "Envelope")?
                .Element(RequestSerializer.SoapNs + "Body"));
            Assert.IsNotNull(doc?
                .Element(RequestSerializer.SoapNs + "Envelope")?
                .Element(RequestSerializer.SoapNs + "Body")?
                .Element(RequestSerializer.UkrlpNs + "ProviderQueryRequest"));
        }

        [Test, AutoData]
        public void ThenItShouldIncludeTheRequestQueryId(ProviderQueryRequest request)
        {
            var serializer = new RequestSerializer();

            var actual = serializer.Serialize(request);

            var doc = XDocument.Parse(actual);
            var requestElement = doc?
                .Element(RequestSerializer.SoapNs + "Envelope")?
                .Element(RequestSerializer.SoapNs + "Body")?
                .Element(RequestSerializer.UkrlpNs + "ProviderQueryRequest");
            var expectedElement = requestElement?.Element("QueryId");

            Assert.IsNotNull(expectedElement);
            Assert.AreEqual(request.QueryId.ToString(), expectedElement.Value);
        }

        [Test, AutoData]
        public void ThenItShouldIncludeCriteriaCondition(ProviderQueryRequest request)
        {
            var serializer = new RequestSerializer();

            var actual = serializer.Serialize(request);

            var doc = XDocument.Parse(actual);
            var requestElement = doc?
                .Element(RequestSerializer.SoapNs + "Envelope")?
                .Element(RequestSerializer.SoapNs + "Body")?
                .Element(RequestSerializer.UkrlpNs + "ProviderQueryRequest");
            var expectedElement = requestElement?
                .Element("SelectionCriteria")?
                .Element("CriteriaCondition");

            Assert.IsNotNull(expectedElement);
            Assert.AreEqual(request.SelectionCriteria.CriteriaCondition.ToString(), expectedElement.Value);
        }

        [Test, AutoData]
        public void ThenItShouldIncludeApprovedProvidersOnly(ProviderQueryRequest request)
        {
            var serializer = new RequestSerializer();

            var actual = serializer.Serialize(request);

            var doc = XDocument.Parse(actual);
            var requestElement = doc?
                .Element(RequestSerializer.SoapNs + "Envelope")?
                .Element(RequestSerializer.SoapNs + "Body")?
                .Element(RequestSerializer.UkrlpNs + "ProviderQueryRequest");
            var expectedElement = requestElement?
                .Element("SelectionCriteria")?
                .Element("ApprovedProvidersOnly");

            Assert.IsNotNull(expectedElement);
            Assert.AreEqual(request.SelectionCriteria.ApprovedProvidersOnly.ToString(), expectedElement.Value);
        }

        [Test, AutoData]
        public void ThenItShouldIncludeProviderStatus(ProviderQueryRequest request)
        {
            var serializer = new RequestSerializer();

            var actual = serializer.Serialize(request);

            var doc = XDocument.Parse(actual);
            var requestElement = doc?
                .Element(RequestSerializer.SoapNs + "Envelope")?
                .Element(RequestSerializer.SoapNs + "Body")?
                .Element(RequestSerializer.UkrlpNs + "ProviderQueryRequest");
            var expectedElement = requestElement?
                .Element("SelectionCriteria")?
                .Element("ProviderStatus");

            Assert.IsNotNull(expectedElement);
            Assert.AreEqual(request.SelectionCriteria.ProviderStatus.ToString(), expectedElement.Value);
        }

        [Test, AutoData]
        public void ThenItShouldIncludeStakeholderId(ProviderQueryRequest request)
        {
            var serializer = new RequestSerializer();

            var actual = serializer.Serialize(request);

            var doc = XDocument.Parse(actual);
            var requestElement = doc?
                .Element(RequestSerializer.SoapNs + "Envelope")?
                .Element(RequestSerializer.SoapNs + "Body")?
                .Element(RequestSerializer.UkrlpNs + "ProviderQueryRequest");
            var expectedElement = requestElement?
                .Element("SelectionCriteria")?
                .Element("StakeholderId");

            Assert.IsNotNull(expectedElement);
            Assert.AreEqual(request.SelectionCriteria.StakeholderId.ToString(), expectedElement.Value);
        }

        [Test, AutoData]
        public void ThenItShouldIncludeUkprnsWhenSpecifiedInQuery(long ukprn1, long ukprn2)
        {
            var serializer = new RequestSerializer();
            var request = new ProviderQueryRequest
            {
                SelectionCriteria = new SelectionCriteria
                {
                    CriteriaCondition = CriteriaConditionEnum.OR,
                    ProviderStatus = ProviderStatusEnum.A,
                    StakeholderId = 123,
                    ApprovedProvidersOnly = ApprovedProvidersOnlyEnum.Yes,
                    UnitedKingdomProviderReferenceNumberList = new[] {ukprn1, ukprn2},
                },
                QueryId = "123213",
            };

            var actual = serializer.Serialize(request);

            var doc = XDocument.Parse(actual);
            var requestElement = doc?
                .Element(RequestSerializer.SoapNs + "Envelope")?
                .Element(RequestSerializer.SoapNs + "Body")?
                .Element(RequestSerializer.UkrlpNs + "ProviderQueryRequest");
            var expectedElement = requestElement?
                .Element("SelectionCriteria")?
                .Element("UnitedKingdomProviderReferenceNumberList");

            Assert.IsNotNull(expectedElement);
            Assert.AreEqual(2, expectedElement.Elements().Count());
            Assert.AreEqual("UnitedKingdomProviderReferenceNumber", expectedElement.Elements().ElementAt(0).Name.LocalName);
            Assert.AreEqual(ukprn1.ToString(), expectedElement.Elements().ElementAt(0).Value);
            Assert.AreEqual("UnitedKingdomProviderReferenceNumber", expectedElement.Elements().ElementAt(1).Name.LocalName);
            Assert.AreEqual(ukprn2.ToString(), expectedElement.Elements().ElementAt(1).Value);
        }

        [Test, AutoData]
        public void ThenItShouldIncludeProviderUpdatedSinceWhenSpecifiedInQuery(DateTime providerUpdatedSince)
        {
            var serializer = new RequestSerializer();
            var request = new ProviderQueryRequest
            {
                SelectionCriteria = new SelectionCriteria
                {
                    CriteriaCondition = CriteriaConditionEnum.OR,
                    ProviderStatus = ProviderStatusEnum.A,
                    StakeholderId = 123,
                    ApprovedProvidersOnly = ApprovedProvidersOnlyEnum.Yes,
                    ProviderUpdatedSince = providerUpdatedSince,
                },
                QueryId = "123213",
            };

            var actual = serializer.Serialize(request);

            var doc = XDocument.Parse(actual);
            var requestElement = doc?
                .Element(RequestSerializer.SoapNs + "Envelope")?
                .Element(RequestSerializer.SoapNs + "Body")?
                .Element(RequestSerializer.UkrlpNs + "ProviderQueryRequest");
            var expectedElement = requestElement?
                .Element("SelectionCriteria")?
                .Element("ProviderUpdatedSince");

            Assert.IsNotNull(expectedElement);
            Assert.AreEqual(providerUpdatedSince.ToUniversalTime().ToString("O"), expectedElement.Value);
        }
    }
}
using System.Linq;
using System.Xml.Linq;
using Dfe.Edis.SourceAdapter.Ukrlp.Infrastructure.UkrlpSoapApi.Models;

namespace Dfe.Edis.SourceAdapter.Ukrlp.Infrastructure.UkrlpSoapApi.Serialization
{
    public interface IRequestSerializer
    {
        string Serialize(ProviderQueryRequest request);
    }

    public class RequestSerializer : IRequestSerializer
    {
        internal static readonly XNamespace SoapNs = "http://schemas.xmlsoap.org/soap/envelope/";
        internal static readonly XNamespace UkrlpNs = "http://ukrlp.co.uk.server.ws.v3";

        public string Serialize(ProviderQueryRequest request)
        {
            var requestElement = SerializeRequest(request);
            var envelopeElement = WrapRequestInSoapEnvelope(requestElement);

            return envelopeElement.ToString();
        }

        private XElement SerializeRequest(ProviderQueryRequest request)
        {
            var selectionCriteriaElement = SerializeSelectionCriteria(request.SelectionCriteria);

            var requestElement = new XElement(UkrlpNs + "ProviderQueryRequest",
                selectionCriteriaElement,
                new XElement("QueryId", request.QueryId));

            return requestElement;
        }
        
        private XElement SerializeSelectionCriteria(SelectionCriteria selectionCriteria)
        {
            var selectionCriteriaElement = new XElement("SelectionCriteria",
                new XElement("CriteriaCondition", selectionCriteria.CriteriaCondition.ToString()),
                new XElement("ApprovedProvidersOnly", selectionCriteria.ApprovedProvidersOnly.ToString()),
                new XElement("ProviderStatus", selectionCriteria.ProviderStatus.ToString()),
                new XElement("StakeholderId", selectionCriteria.StakeholderId));

            if (selectionCriteria.UnitedKingdomProviderReferenceNumberList != null &&
                selectionCriteria.UnitedKingdomProviderReferenceNumberList.Length > 0)
            {
                var ukprnElements = selectionCriteria.UnitedKingdomProviderReferenceNumberList
                    .Select(ukprn => new XElement("UnitedKingdomProviderReferenceNumber", ukprn))
                    .ToArray();
                selectionCriteriaElement.Add(
                    new XElement("UnitedKingdomProviderReferenceNumberList", ukprnElements));
            }

            if (selectionCriteria.ProviderUpdatedSince.HasValue)
            {
                var providerUpdatedSinceValue = selectionCriteria.ProviderUpdatedSince.Value.ToUniversalTime().ToString("O");
                selectionCriteriaElement.Add(new XElement("ProviderUpdatedSince", providerUpdatedSinceValue));
            }

            return selectionCriteriaElement;
        }

        private XElement WrapRequestInSoapEnvelope(XElement requestElement)
        {
            var bodyElement = new XElement(SoapNs + "Body", requestElement);
            var headerElement = new XElement(SoapNs + "Header", requestElement);
            var envelopeElement = new XElement(SoapNs + "Envelope",
                new XAttribute(XNamespace.Xmlns + "soapenv", SoapNs.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "ukrlp", UkrlpNs.NamespaceName),
                headerElement,
                bodyElement);

            return envelopeElement;
        }
    }
}
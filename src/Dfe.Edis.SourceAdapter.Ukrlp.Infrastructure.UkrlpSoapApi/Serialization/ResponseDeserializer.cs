using System;
using System.Linq;
using System.Xml.Linq;
using Dfe.Edis.SourceAdapter.Ukrlp.Domain.UkrlpApi;

namespace Dfe.Edis.SourceAdapter.Ukrlp.Infrastructure.UkrlpSoapApi.Serialization
{
    public interface IResponseDeserializer
    {
        Provider[] DeserializeResponse(string responseXml);
    }

    public class ResponseDeserializer : IResponseDeserializer
    {
        internal static readonly XNamespace SoapNs = "http://schemas.xmlsoap.org/soap/envelope/";
        internal static readonly XNamespace UkrlpNs = "http://ukrlp.co.uk.server.ws.v3";
        internal static readonly XNamespace GovTalkNs = "http://www.govtalk.gov.uk/people/PersonDescriptives";

        public Provider[] DeserializeResponse(string responseXml)
        {
            XDocument doc;
            try
            {
                doc = XDocument.Parse(responseXml);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Response is not valid XML", ex);
            }

            var body = doc?
                .Element(SoapNs + "Envelope")?
                .Element(SoapNs + "Body");
            if (body == null)
            {
                throw new SoapException("Response XML does not appear to be a valid SOAP response");
            }

            var faultElement = body.Element(SoapNs + "Fault");
            if (faultElement != null)
            {
                var faultCode = faultElement.Element("faultcode")?.Value;
                var faultString = faultElement.Element("faultstring")?.Value;
                throw new SoapException(faultCode, faultString);
            }

            var matchingRecordElements = body?
                .Element(UkrlpNs + "ProviderQueryResponse")?
                .Elements("MatchingProviderRecords")
                .ToArray();

            return matchingRecordElements?.Select(DeserializeMatchingProviderRecord).ToArray();
        }

        private Provider DeserializeMatchingProviderRecord(XElement matchingProviderElement)
        {
            var provider = new Provider();

            provider.UnitedKingdomProviderReferenceNumber = long.Parse(matchingProviderElement.Element("UnitedKingdomProviderReferenceNumber").Value);
            provider.ProviderName = matchingProviderElement.Element("ProviderName")?.Value;
            provider.AccessibleProviderName = matchingProviderElement.Element("AccessibleProviderName")?.Value;

            provider.ProviderContacts = matchingProviderElement.Elements("ProviderContact").Select(DeserializeProviderContent).ToArray();

            provider.ProviderVerificationDate = matchingProviderElement.Element("ProviderVerificationDate") != null
                ? (DateTime?) DateTime.Parse(matchingProviderElement.Element("ProviderVerificationDate").Value)
                : null;
            provider.ExpiryDate = matchingProviderElement.Element("ExpiryDate") != null
                ? (DateTime?) DateTime.Parse(matchingProviderElement.Element("ExpiryDate").Value)
                : null;
            provider.ProviderStatus = matchingProviderElement.Element("ProviderStatus").Value;

            provider.VerificationDetails = matchingProviderElement.Elements("VerificationDetails").Select(DeserializeVerificationDetails).ToArray();

            return provider;
        }

        private ProviderContact DeserializeProviderContent(XElement providerContactElement)
        {
            if (providerContactElement == null)
            {
                return null;
            }
            
            var contactAddressElement = providerContactElement.Element("ContactAddress");
            var contactPersonalDetailsElement = providerContactElement.Element("ContactPersonalDetails");
            
            var providerContact = new ProviderContact
            {
                ContactAddress = new AddressStructure(),
                ContactPersonalDetails = new PersonNameStructure(),
            };

            providerContact.ContactType = providerContactElement.Element("ContactType").Value;
            
            providerContact.ContactAddress.Address1 = contactAddressElement?.Element("Address1")?.Value;
            providerContact.ContactAddress.Address2 = contactAddressElement?.Element("Address2")?.Value;
            providerContact.ContactAddress.Address3 = contactAddressElement?.Element("Address3")?.Value;
            providerContact.ContactAddress.Address4 = contactAddressElement?.Element("Address4")?.Value;
            providerContact.ContactAddress.Town = contactAddressElement?.Element("Town")?.Value;
            providerContact.ContactAddress.County = contactAddressElement?.Element("County")?.Value;
            providerContact.ContactAddress.PostCode = contactAddressElement?.Element("PostCode")?.Value;

            providerContact.ContactPersonalDetails.PersonNameTitle = contactPersonalDetailsElement?.Element(GovTalkNs + "PersonNameTitle")?.Value;
            providerContact.ContactPersonalDetails.PersonGivenName = contactPersonalDetailsElement?.Element(GovTalkNs + "PersonGivenName")?.Value;
            providerContact.ContactPersonalDetails.PersonFamilyName = contactPersonalDetailsElement?.Element(GovTalkNs + "PersonFamilyName")?.Value;
            providerContact.ContactPersonalDetails.PersonNameSuffix = contactPersonalDetailsElement?.Element(GovTalkNs + "PersonNameSuffix")?.Value;
            providerContact.ContactPersonalDetails.PersonRequestedName = contactPersonalDetailsElement?.Element(GovTalkNs + "PersonRequestedName")?.Value;

            providerContact.ContactRole = providerContactElement.Element("ContactRole")?.Value;
            providerContact.ContactTelephone1 = providerContactElement.Element("ContactTelephone1")?.Value;
            providerContact.ContactTelephone2 = providerContactElement.Element("ContactTelephone2")?.Value;
            providerContact.ContactFax = providerContactElement.Element("ContactFax")?.Value;
            providerContact.ContactWebsiteAddress = providerContactElement.Element("ContactWebsiteAddress")?.Value;
            providerContact.ContactEmail = providerContactElement.Element("ContactEmail")?.Value;
            providerContact.LastUpdated = providerContactElement.Element("LastUpdated") != null
                ? (DateTime?) DateTime.Parse(providerContactElement.Element("LastUpdated").Value)
                : null;

            return providerContact;
        }

        private VerificationDetails DeserializeVerificationDetails(XElement verificationDetailsElement)
        {
            var verificationDetails = new VerificationDetails();

            verificationDetails.VerificationID = verificationDetailsElement?.Element("VerificationID")?.Value;
            verificationDetails.VerificationAuthority = verificationDetailsElement?.Element("VerificationAuthority")?.Value;

            return verificationDetails;
        }
    }
}
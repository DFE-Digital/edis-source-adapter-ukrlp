using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using AutoFixture.NUnit3;
using Dfe.Edis.SourceAdapter.Ukrlp.Domain.UkrlpApi;
using Dfe.Edis.SourceAdapter.Ukrlp.Infrastructure.UkrlpSoapApi.Serialization;
using NUnit.Framework; 

namespace Dfe.Edis.SourceAdapter.Ukrlp.Infrastructure.UkrlpSoapApi.UnitTests.SerializationTests.ResponseDeserializerTests
{
    public class WhenDeserializingResponse
    {
        [Test, AutoData]
        public void ThenItShouldThrowExceptionIfResponseIsNotXml(string faultCode, string faultString)
        {
            var deserializer = new ResponseDeserializer();
            var response = "no-xml-in-here";

            Assert.Throws<ArgumentException>(() => deserializer.DeserializeResponse(response));
        }
        
        [Test, AutoData]
        public void ThenItShouldThrowExceptionIfResponseIsNotSoapResponse(string faultCode, string faultString)
        {
            var deserializer = new ResponseDeserializer();
            var response = "<nosoap>just xml</nosoap>";

            var actual = Assert.Throws<SoapException>(() => deserializer.DeserializeResponse(response));
            Assert.AreEqual("Response XML does not appear to be a valid SOAP response", actual.Message);
        }
        
        [Test, AutoData]
        public void ThenItShouldThrowSoapExceptionIfResponseIsFault(string faultCode, string faultString)
        {
            var deserializer = new ResponseDeserializer();
            var response = GetFaultResponse(faultCode, faultString);

            var actual = Assert.Throws<SoapException>(() => deserializer.DeserializeResponse(response));
            Assert.AreEqual(faultCode, actual.FaultCode);
            Assert.AreEqual(faultString, actual.FaultString);
        }

        [Test, AutoData]
        public void ThenItShouldParseProvidersFromResponse(Provider provider1, Provider provider2)
        {
            
            var deserializer = new ResponseDeserializer();
            var response = GetQueryResponse(provider1, provider2);

            var actual = deserializer.DeserializeResponse(response);
            
            Assert.IsNotNull(actual);
            Assert.AreEqual(2, actual.Length);
            ObjectAssert.AreEqual(provider1, actual[0]);
            ObjectAssert.AreEqual(provider2, actual[1]);
        }

        private string GetQueryResponse(params Provider[] providers)
        {
            var matchingRecords = providers.Select(GetMatchingRecord).ToArray();
            var providerQueryResponse = new XElement(ResponseDeserializer.UkrlpNs + "ProviderQueryResponse",
                matchingRecords);
            return WrapBody(providerQueryResponse.ToString());
        }
        private XElement GetMatchingRecord(Provider provider)
        {
            var matchingRecord = new XElement("MatchingProviderRecords");
            
            matchingRecord.Add(new XElement("UnitedKingdomProviderReferenceNumber", provider.UnitedKingdomProviderReferenceNumber));
            matchingRecord.Add(new XElement("ProviderName", provider.ProviderName));
            matchingRecord.Add(new XElement("ProviderStatus", provider.ProviderStatus));

            foreach (var providerContact in provider.ProviderContacts)
            {
                var address = new XElement("ContactAddress",
                    new XElement("Address1", providerContact.ContactAddress.Address1),
                    new XElement("Address2", providerContact.ContactAddress.Address2),
                    new XElement("Address3", providerContact.ContactAddress.Address3),
                    new XElement("Address4", providerContact.ContactAddress.Address4),
                    new XElement("Town", providerContact.ContactAddress.Town),
                    new XElement("County", providerContact.ContactAddress.County),
                    new XElement("PostCode", providerContact.ContactAddress.PostCode));
                var personalDetails = new XElement("ContactPersonalDetails",
                    new XElement(ResponseDeserializer.GovTalkNs + "PersonGivenName", providerContact.ContactPersonalDetails.PersonGivenName),
                    new XElement(ResponseDeserializer.GovTalkNs + "PersonFamilyName", providerContact.ContactPersonalDetails.PersonFamilyName),
                    new XElement(ResponseDeserializer.GovTalkNs + "PersonNameSuffix", providerContact.ContactPersonalDetails.PersonNameSuffix),
                    new XElement(ResponseDeserializer.GovTalkNs + "PersonNameTitle", providerContact.ContactPersonalDetails.PersonNameTitle),
                    new XElement(ResponseDeserializer.GovTalkNs + "PersonRequestedName", providerContact.ContactPersonalDetails.PersonRequestedName));
                
                matchingRecord.Add(new XElement("ProviderContact",
                    new XElement("ContactType", providerContact.ContactType),
                    address,
                    personalDetails,
                    new XElement("ContactRole", providerContact.ContactTelephone1),
                    new XElement("ContactTelephone1", providerContact.ContactTelephone1),
                    new XElement("ContactTelephone2", providerContact.ContactTelephone2),
                    new XElement("ContactFax", providerContact.ContactFax),
                    new XElement("ContactWebsiteAddress", providerContact.ContactWebsiteAddress),
                    new XElement("ContactEmail", providerContact.ContactEmail),
                    new XElement("LastUpdated", providerContact.LastUpdated)));
            }
            
            matchingRecord.Add(new XElement("ProviderVerificationDate", provider.ProviderVerificationDate));
            matchingRecord.Add(new XElement("ExpiryDate", provider.ExpiryDate));
            matchingRecord.Add(new XElement("ProviderStatus", provider.ProviderStatus));

            foreach (var providerVerification in provider.VerificationDetails)
            {
                matchingRecord.Add(new XElement("VerificationDetails",
                    new XElement("VerificationAuthority", providerVerification.VerificationAuthority),
                    new XElement("VerificationID", providerVerification.VerificationID)));
            }

            return matchingRecord;
        }
        private string GetFaultResponse(string faultCode, string faultString)
        {
            var response = new StringBuilder("<ns0:Fault " +
                                             "xmlns:ns1=\"http://www.w3.org/2003/05/soap-envelope\" " +
                                             "xmlns:ns0=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            response.Append($"<faultcode>{faultCode}</faultcode>");
            response.Append($"<faultstring>{faultString}</faultstring>");
            response.Append("</ns0:Fault>");
            return WrapBody(response.ToString());
        }
        private string WrapBody(string bodyXml)
        {
            const string prefix = "<?xml version='1.0' encoding='UTF-8'?>" +
                                  "<S:Envelope xmlns:S=\"http://schemas.xmlsoap.org/soap/envelope/\">" +
                                  "<S:Body>";
            const string suffix = "</S:Body>" +
                                  "</S:Envelope>";

            return $"{prefix}{bodyXml}{suffix}";
        }
    }
}
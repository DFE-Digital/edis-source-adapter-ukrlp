using System;

namespace Dfe.Edis.SourceAdapter.Ukrlp.Infrastructure.UkrlpSoapApi
{
    public class SoapException : Exception
    {
        public SoapException(string faultCode, string faultString)
            : base($"Error quering providers in UKRLP SOAP API. Fault code={faultCode}, Fault string={faultString}")
        {
            FaultCode = faultCode;
            FaultString = faultString;
        }

        public SoapException(string message)
            : base(message)
        {
        }

        public string FaultCode { get; }
        public string FaultString { get; }
    }
}
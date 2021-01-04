using System;

namespace Dfe.Edis.SourceAdapter.Ukrlp.Infrastructure.UkrlpSoapApi.Models
{
    public class SelectionCriteria
    {
        public long[] UnitedKingdomProviderReferenceNumberList { get; set; }
        public DateTime? ProviderUpdatedSince { get; set; }
        public CriteriaConditionEnum CriteriaCondition { get; set; }
        public ApprovedProvidersOnlyEnum ApprovedProvidersOnly { get; set; }
        public ProviderStatusEnum ProviderStatus { get; set; }
        public int StakeholderId { get; set; }
    }

    public enum CriteriaConditionEnum
    {
        AND,
        OR
    }

    public enum ApprovedProvidersOnlyEnum
    {
        Yes,
        No
    }

    public enum ProviderStatusEnum
    {
        A,
        V,
        PD1,
        PD2
    }
}
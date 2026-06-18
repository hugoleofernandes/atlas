namespace Atlas.Staff.Application.StaffMembers.Queries.GetByPartyId;

public sealed record GetStaffMemberByPartyIdQuery(Guid PartyId, Guid TenantId);

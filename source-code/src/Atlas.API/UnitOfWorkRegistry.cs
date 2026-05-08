//using Atlas.Identity.Application.Abstractions;
//using Atlas.SharedKernel.Application;
//using Atlas.Staff.Application.Abstractions;

//namespace Atlas.API;

//public sealed class UnitOfWorkRegistry : IUnitOfWorkRegistry
//{
//    private readonly IIdentityUnitOfWork _identity;
//    private readonly IStaffUnitOfWork _staff;

//    public UnitOfWorkRegistry(
//        IIdentityUnitOfWork identity,
//        IStaffUnitOfWork staff)
//    {
//        _identity = identity;
//        _staff = staff;
//    }

//    public IUnitOfWork? Resolve(object request)
//    {
//        return request switch
//        {
//            IIdentityCommand => _identity,
//            IStaffCommand => _staff,
//            _ => null
//        };
//    }
//}

//todo: deletar
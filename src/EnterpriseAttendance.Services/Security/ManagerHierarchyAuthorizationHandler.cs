using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using EnterpriseAttendance.Core.Interfaces;

namespace EnterpriseAttendance.Services.Security
{
    public class ManagerScopeRequirement : IAuthorizationRequirement
    {
    }

    public class ManagerHierarchyAuthorizationHandler : AuthorizationHandler<ManagerScopeRequirement, (int ManagerId, int TargetEmployeeId)>
    {
        private readonly IOrgHierarchyService _orgHierarchyService;

        public ManagerHierarchyAuthorizationHandler(IOrgHierarchyService orgHierarchyService)
        {
            _orgHierarchyService = orgHierarchyService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ManagerScopeRequirement requirement, (int ManagerId, int TargetEmployeeId) resource)
        {
            if (resource.ManagerId == resource.TargetEmployeeId)
            {
                context.Succeed(requirement);
                return;
            }

            bool isInSubtree = await _orgHierarchyService.IsEmployeeInManagerSubtreeAsync(resource.ManagerId, resource.TargetEmployeeId);
            if (isInSubtree)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
        }
    }
}

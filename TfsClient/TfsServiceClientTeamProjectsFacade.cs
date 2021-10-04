using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TfsClient
{
    internal class TfsServiceClientTeamProjectsFacade : ITfsServiceClientTeamProjectsFacade
    {
        private readonly ITfsServiceClient _tfsService;

        public TfsServiceClientTeamProjectsFacade(ITfsServiceClient tfsService)
        {
            _tfsService = tfsService;
        }

        public IEnumerable<ITfsTeam> GetAllTfsTeams(bool currentUser = false) =>
            _tfsService.GetAllTfsTeams(currentUser);

        public IEnumerable<ITfsTeamMember> GetProjectTeamMembers(ITfsTeamProject tfsProject, ITfsTeam tfsTeam) =>
            _tfsService.GetProjectTeamMembers(tfsProject, tfsTeam);

        public IEnumerable<ITfsTeam> GetProjectTeams(ITfsTeamProject tfsProject, bool currentUser = false) =>
            _tfsService.GetProjectTeams(tfsProject, currentUser);

        public IEnumerable<ITfsTeamProject> GetTeamProjects(int skip = 0) =>
            _tfsService.GetTeamProjects(skip);
    }

    internal class AsyncTfsServiceClientTeamProjectsFacade : IAsyncTfsServiceClientTeamProjectsFacade
    {
        private readonly ITfsServiceClient _tfsService;

        public AsyncTfsServiceClientTeamProjectsFacade(ITfsServiceClient tfsService)
        {
            _tfsService = tfsService;
        }

        public Task<IEnumerable<ITfsTeam>> GetAllTfsTeamsAsync(bool currentUser = false) =>
            _tfsService.GetAllTfsTeamsAsync(currentUser);

        public Task<IEnumerable<ITfsTeamMember>> GetProjectTeamMembersAsync(
            ITfsTeamProject tfsProject, ITfsTeam tfsTeam) =>
            _tfsService.GetProjectTeamMembersAsync(tfsProject, tfsTeam);

        public Task<IEnumerable<ITfsTeam>> GetProjectTeamsAsync(ITfsTeamProject tfsProject, bool currentUser = false) =>
            _tfsService.GetProjectTeamsAsync(tfsProject, currentUser);

        public Task<IEnumerable<ITfsTeamProject>> GetTeamProjectsAsync(int skip = 0) =>
            _tfsService.GetTeamProjectsAsync(skip);
    }
}

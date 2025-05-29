using Apps.Sitecore.Api;
using Apps.Sitecore.Invocables;
using Apps.SitecoreXmCloud.Models.Responses.Workflows;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Sitecore.DataSourceHandlers;

public class WorkflowStateDataHandler(InvocationContext invocationContext) : SitecoreInvocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var request = new SitecoreRequest("/Workflows", Method.Get, Creds);
        var response = await Client.ExecuteWithErrorHandling<List<WorkflowResponse>>(request);

        return response
            .SelectMany(workflow => workflow.States.Select(state => new DataSourceItem(
                state.StateId,
                GetStateName(workflow, state))))
            .Where(item => context.SearchString is null ||
                           item.DisplayName.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .DistinctBy(item => item.Value);
    }

    private string GetStateName(WorkflowResponse workflow, WorkflowState state)
    {
        return $"[{workflow.WorkflowName}] {state.StateName}";
    }
}
using Apps.Sitecore.Actions;
using Apps.SitecoreXmCloud.Models.Requests.Workflows;
using Tests.Sitecore.Base;

namespace Tests.Sitecore;

[TestClass]
public class WorkflowActionsTests : TestBase
{
    [TestMethod]
    public async Task UpdateWorkflowState_ValidData_ShouldUpdateWorkflowState()
    {
        // Arrange
        var request = new UpdateWorkflowStateRequest
        {
            ItemId = "481b20da-bcbf-4676-aaa7-3844824521f7",
            WorkflowCommandId = "65EF04A8-E6E0-45D3-B28B-69793F264193",
            Version = "2",
            Locale = "de"
        };

        var action = new WorkflowActions(InvocationContext);

        // Act
        var response = await action.UpdateWorkflowState(request);

        // Assert
        Assert.IsNotNull(response);
    }
}